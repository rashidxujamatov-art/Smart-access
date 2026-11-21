import { useEffect, useState } from 'react';
import { Link, Navigate, Outlet, useLocation } from 'react-router-dom';
import { t } from '../i18n';
import { isRestricted } from '../lib/api';
import { useAuth } from '../lib/auth';
import { DEMO_MODE } from '../lib/config';
import { useTheme } from '../lib/theme';
import { Icon } from '../ui/Icon';
import { activeItem, homePath, MENUS } from './menu';

const HEALTH_INTERVAL_MS = 30_000;

/** Server holati — haqiqiy /health so'rovi bo'yicha (UI-SPEC A.1, top bar). */
function useServerOnline(): boolean {
  const [online, setOnline] = useState(true);
  useEffect(() => {
    let alive = true;
    const check = () =>
      fetch('/api/v1/health')
        .then((r) => alive && setOnline(r.ok))
        .catch(() => alive && setOnline(false));
    check();
    const id = window.setInterval(check, HEALTH_INTERVAL_MS);
    return () => {
      alive = false;
      window.clearInterval(id);
    };
  }, []);
  return online;
}

function initials(name: string): string {
  return name.split(/\s+/).filter(Boolean).slice(0, 2).map((p) => p[0]!.toUpperCase()).join('');
}

/** UI-SPEC A.1 — desktop/web karkasi: chapda sidebar, tepada top bar. */
export function AppShell() {
  const { status, user, logout } = useAuth();
  const { pathname } = useLocation();
  const [theme, toggleTheme] = useTheme();
  const online = useServerOnline();

  if (status === 'loading') return null;
  if (!user || isRestricted(user)) return <Navigate to="/login" replace />;

  const current = activeItem(user.role, pathname);
  // BR-RBAC-02: boshqa rolning ekraniga kirishga urinish — o'z bosh ekraniga qaytaradi.
  if (!current) return <Navigate to={homePath(user.role)} replace />;

  const roleLabel = t(`roles.${user.role}`);
  const title = t(current.label);

  return (
    <>
      <aside className="side">
        <div className="row" style={{ padding: '4px 8px 12px', color: '#fff' }}>
          <Icon name="shield" size={28} />
          <div>
            <div style={{ fontWeight: 700, letterSpacing: '.06em' }}>{t('app.name')}</div>
            <div className="sm" style={{ color: '#9FB0C6' }}>{roleLabel}</div>
          </div>
        </div>
        <nav className="col" style={{ gap: 2 }}>
          {MENUS[user.role].map((m) => (
            <Link key={m.path} to={m.path} className={`nv${m === current ? ' on' : ''}`}
              aria-current={m === current ? 'page' : undefined}>
              <Icon name={m.icon} />
              <span>{t(m.label)}</span>
            </Link>
          ))}
        </nav>
        <div style={{ flex: 1 }} />
        <div className="row" style={{ padding: 8, borderTop: '1px solid #2D3E58', marginTop: 8, color: '#C9D3E0' }}>
          <span className="av" style={{ background: '#334155' }}>{initials(user.fullName)}</span>
          <div style={{ flex: 1, minWidth: 0 }}>
            <div style={{ fontWeight: 600, color: '#fff' }}>{user.fullName}</div>
            <div className="sm">{online ? t('shell.serverOnline') : t('shell.serverOffline')}</div>
          </div>
          <button type="button" className="btn ico" style={{ background: 'transparent', borderColor: '#2D3E58', color: '#C9D3E0' }}
            aria-label={t('shell.logout')} title={t('shell.logout')} onClick={() => void logout()}>
            <Icon name="out" />
          </button>
        </div>
      </aside>
      <main className="main">
        <header className="top">
          <div>
            <div className="crumb">{roleLabel} / {title}</div>
            <h1>{title}</h1>
          </div>
          <div className="row wrap" style={{ marginLeft: 'auto', gap: 8 }}>
            {DEMO_MODE && <span className="badge b-warn" title={t('app.demoTitle')}>{t('app.demo')}</span>}
            <span className={`badge ${online ? 'b-ok' : 'b-warn'}`} role="status">
              <span className="dot" />
              {online ? t('shell.serverOnline') : t('shell.serverOffline')}
            </span>
            <button type="button" className="btn ico" aria-label={t('shell.theme')} title={t('shell.theme')} onClick={toggleTheme}>
              <Icon name={theme === 'dark' ? 'sun' : 'moon'} />
            </button>
          </div>
        </header>
        <Outlet />
      </main>
    </>
  );
}
