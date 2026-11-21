import { useState, type FormEvent } from 'react';
import { Navigate } from 'react-router-dom';
import { QRCodeSVG } from 'qrcode.react';
import { errorText, t } from '../i18n';
import { ApiError } from '../lib/api';
import { isRestricted, useAuth } from '../lib/auth';
import { DEMO_MODE, MIN_PASSWORD_LENGTH } from '../lib/config';
import { homePath } from '../app/menu';
import { Icon } from '../ui/Icon';
import { Modal } from '../ui/Modal';

const DEMO_ACCOUNTS = ['qorovul', 'admin', 'mas_ul'] as const;

function messageFor(err: unknown): string {
  if (err instanceof ApiError) {
    if (err.code === 'account_locked' && typeof err.details?.lockedUntil === 'string') {
      const time = new Date(err.details.lockedUntil).toLocaleTimeString('uz-UZ', { hour: '2-digit', minute: '2-digit' });
      return errorText(err.code, { time });
    }
    return errorText(err.code, { min: MIN_PASSWORD_LENGTH });
  }
  return errorText(undefined);
}

/** D01 — Login (UI-SPEC.md). */
export function LoginPage() {
  const auth = useAuth();
  const [tenant, setTenant] = useState(DEMO_MODE ? 'KORXONA-001' : '');
  const [login, setLogin] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [remember, setRemember] = useState(true);
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);
  const [challenge, setChallenge] = useState<string | null>(null);

  if (auth.status === 'authenticated' && auth.user && !isRestricted(auth.user)) {
    return <Navigate to={homePath(auth.user.role)} replace />;
  }

  async function submit(e: FormEvent) {
    e.preventDefault();
    setBusy(true);
    setError('');
    try {
      const r = await auth.login(tenant, login, password, remember);
      if (r.stage === '2fa' && r.challengeToken) setChallenge(r.challengeToken);
    } catch (err) {
      setError(messageFor(err));
    } finally {
      setBusy(false);
      setPassword('');
    }
  }

  const fillDemo = (account: string) => {
    setLogin(account);
    setPassword('demo');
    setError('');
  };

  return (
    <div className="login">
      <div className="lg-l">
        <div className="row">
          <Icon name="shield" size={36} />
          <h2 style={{ fontSize: 20 }}>{t('app.name')}</h2>
        </div>
        <h1 style={{ fontSize: 30, lineHeight: 1.2 }}>{t('login.headline')}</h1>
        <div className="col" style={{ gap: 8, color: '#C9D3E0' }}>
          <div>{t('login.feature1')}</div>
          <div>{t('login.feature2')}</div>
          <div>{t('login.feature3')}</div>
        </div>
        {DEMO_MODE && <div className="sm" style={{ color: '#9FB0C6' }}>{t('login.demoNote')}</div>}
      </div>

      <div className="lg-r">
        <form className="card col" style={{ width: '100%', maxWidth: 400, gap: 14 }} onSubmit={submit} noValidate>
          <h2>{t('login.title')}</h2>
          <div>
            <label className="lbl" htmlFor="corp">{t('login.tenant')}</label>
            <input id="corp" className="inp" value={tenant} onChange={(e) => setTenant(e.target.value)} autoComplete="organization" required />
          </div>
          <div>
            <label className="lbl" htmlFor="lg">{t('login.login')}</label>
            <input id="lg" className="inp" value={login} onChange={(e) => setLogin(e.target.value)} autoComplete="username" required />
          </div>
          <div>
            <label className="lbl" htmlFor="ps">{t('login.password')}</label>
            <div className="row" style={{ gap: 8 }}>
              <input id="ps" className="inp" type={showPassword ? 'text' : 'password'} value={password}
                onChange={(e) => setPassword(e.target.value)} autoComplete="current-password" required />
              <button type="button" className="btn ico" onClick={() => setShowPassword((v) => !v)}
                aria-label={showPassword ? t('login.hidePassword') : t('login.showPassword')} aria-pressed={showPassword}>
                <Icon name="eye" />
              </button>
            </div>
          </div>
          <label className="row" style={{ gap: 8 }}>
            <input type="checkbox" checked={remember} onChange={(e) => setRemember(e.target.checked)} /> {t('login.remember')}
          </label>
          {error && <div className="ban b-danger" role="alert">{error}</div>}
          <button type="submit" className="btn pri" style={{ minHeight: 44 }} disabled={busy}>
            {busy ? t('login.submitting') : t('login.submit')}
          </button>
          <div className="sm muted">{t('login.note')}</div>
          {DEMO_MODE && (
            <>
              <div className="sm" style={{ fontWeight: 600 }}>{t('login.demoAccounts')}</div>
              <div className="row wrap" style={{ gap: 8 }}>
                {DEMO_ACCOUNTS.map((a) => (
                  <button key={a} type="button" className="chip" onClick={() => fillDemo(a)}>{a}</button>
                ))}
              </div>
            </>
          )}
        </form>
      </div>

      {challenge && <TwoFactorModal challenge={challenge} onCancel={() => setChallenge(null)} />}
      {auth.user?.mustChangePassword && <ChangePasswordModal />}
      {auth.user && !auth.user.mustChangePassword && auth.user.mustSetup2fa && <SetupTwoFactorModal />}
    </div>
  );
}

/** BR-AUTH-06: parol to'g'ri bo'lgach 6 xonali kod. */
function TwoFactorModal({ challenge, onCancel }: { challenge: string; onCancel: () => void }) {
  const auth = useAuth();
  const [code, setCode] = useState('');
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);

  async function submit(e: FormEvent) {
    e.preventDefault();
    setBusy(true);
    setError('');
    try {
      await auth.verify2fa(challenge, code);
    } catch (err) {
      setError(messageFor(err));
      if (err instanceof ApiError && err.code !== 'invalid_2fa_code') onCancel();
    } finally {
      setBusy(false);
    }
  }

  return (
    <Modal title={t('twoFactor.title')} onClose={onCancel}>
      <form className="col" onSubmit={submit} style={{ gap: 14 }}>
        <div className="muted">{t('twoFactor.text')}</div>
        <div>
          <label className="lbl" htmlFor="otp">{t('twoFactor.code')}</label>
          <input id="otp" className="inp code-inp" inputMode="numeric" autoComplete="one-time-code" maxLength={6}
            value={code} onChange={(e) => setCode(e.target.value.replace(/\D/g, ''))} aria-invalid={!!error} />
        </div>
        {error && <div className="ban b-danger" role="alert">{error}</div>}
        <div className="row" style={{ justifyContent: 'flex-end', gap: 8 }}>
          <button type="button" className="btn" onClick={onCancel}>{t('twoFactor.cancel')}</button>
          <button type="submit" className="btn pri" disabled={busy || code.length !== 6}>{t('twoFactor.confirm')}</button>
        </div>
      </form>
    </Modal>
  );
}

/** BR-USR-02: birinchi kirishda parolni almashtirish majburiy. */
function ChangePasswordModal() {
  const auth = useAuth();
  const [current, setCurrent] = useState('');
  const [next, setNext] = useState('');
  const [repeat, setRepeat] = useState('');
  const [fields, setFields] = useState<Record<string, string>>({});
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);

  async function submit(e: FormEvent) {
    e.preventDefault();
    setError('');
    if (next !== repeat) {
      setFields({ repeat: t('changePassword.mismatch') });
      return;
    }
    setFields({});
    setBusy(true);
    try {
      await auth.changePassword(current, next);
    } catch (err) {
      if (err instanceof ApiError && err.fields) {
        setFields(Object.fromEntries(Object.entries(err.fields).map(([k, v]) => [k, errorText(v, { min: MIN_PASSWORD_LENGTH })])));
      } else {
        setError(messageFor(err));
      }
    } finally {
      setBusy(false);
    }
  }

  const field = (id: string, key: 'currentPassword' | 'newPassword' | 'repeat', label: string, value: string,
    set: (v: string) => void, auto: string) => (
    <div>
      <label className="lbl" htmlFor={id}>{label}</label>
      <input id={id} className="inp" type="password" value={value} onChange={(e) => set(e.target.value)}
        autoComplete={auto} aria-invalid={!!fields[key]} aria-describedby={fields[key] ? `${id}-err` : undefined} />
      {fields[key] && <div id={`${id}-err`} className="ferr">{fields[key]}</div>}
    </div>
  );

  return (
    <Modal title={t('changePassword.title')} onClose={() => void auth.logout()}>
      <form className="col" onSubmit={submit} style={{ gap: 14 }}>
        <div className="muted">{t('changePassword.text')}</div>
        {field('cp-cur', 'currentPassword', t('changePassword.current'), current, setCurrent, 'current-password')}
        {field('cp-new', 'newPassword', t('changePassword.next'), next, setNext, 'new-password')}
        <div className="sm muted" style={{ marginTop: -8 }}>{t('changePassword.hint', { min: MIN_PASSWORD_LENGTH })}</div>
        {field('cp-rep', 'repeat', t('changePassword.repeat'), repeat, setRepeat, 'new-password')}
        {error && <div className="ban b-danger" role="alert">{error}</div>}
        <div className="row" style={{ justifyContent: 'flex-end', gap: 8 }}>
          <button type="button" className="btn" onClick={() => void auth.logout()}>{t('twoFactor.cancel')}</button>
          <button type="submit" className="btn pri" disabled={busy}>{t('changePassword.save')}</button>
        </div>
      </form>
    </Modal>
  );
}

/** BR-AUTH-06 + security.require_2fa_for_admin: administrator 2FA ni yoqmaguncha ilovaga kira olmaydi. */
function SetupTwoFactorModal() {
  const auth = useAuth();
  const [setup, setSetup] = useState<{ secret: string; otpauthUri: string } | null>(null);
  const [code, setCode] = useState('');
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);

  async function start() {
    setError('');
    try {
      setSetup(await auth.begin2faSetup());
    } catch (err) {
      setError(messageFor(err));
    }
  }

  async function submit(e: FormEvent) {
    e.preventDefault();
    setBusy(true);
    setError('');
    try {
      await auth.enable2fa(code);
    } catch (err) {
      setError(messageFor(err));
    } finally {
      setBusy(false);
    }
  }

  return (
    <Modal title={t('twoFactor.setupTitle')} onClose={() => void auth.logout()}>
      <form className="col" onSubmit={submit} style={{ gap: 14 }}>
        <div className="muted">{t('twoFactor.setupText')}</div>
        {setup ? (
          <>
            <div className="qr"><QRCodeSVG value={setup.otpauthUri} size={180} /></div>
            <div className="sm muted">{t('twoFactor.secret')}</div>
            <code style={{ wordBreak: 'break-all', fontWeight: 600 }}>{setup.secret}</code>
            <div>
              <label className="lbl" htmlFor="otp-setup">{t('twoFactor.code')}</label>
              <input id="otp-setup" className="inp code-inp" inputMode="numeric" autoComplete="one-time-code" maxLength={6}
                value={code} onChange={(e) => setCode(e.target.value.replace(/\D/g, ''))} aria-invalid={!!error} />
            </div>
          </>
        ) : null}
        {error && <div className="ban b-danger" role="alert">{error}</div>}
        <div className="row" style={{ justifyContent: 'flex-end', gap: 8 }}>
          <button type="button" className="btn" onClick={() => void auth.logout()}>{t('twoFactor.cancel')}</button>
          {setup ? (
            <button type="submit" className="btn pri" disabled={busy || code.length !== 6}>{t('twoFactor.enable')}</button>
          ) : (
            <button type="button" className="btn pri" onClick={() => void start()}>{t('twoFactor.confirm')}</button>
          )}
        </div>
      </form>
    </Modal>
  );
}
