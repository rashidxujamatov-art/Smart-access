import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import { api, onUnauthorized, refreshSession, setAccessToken, type AuthResponse, type UserInfo } from './api';

type Status = 'loading' | 'anonymous' | 'authenticated';

interface AuthState {
  status: Status;
  user: UserInfo | null;
  login: (tenantCode: string, login: string, password: string, rememberMe: boolean) => Promise<AuthResponse>;
  verify2fa: (challengeToken: string, code: string) => Promise<void>;
  changePassword: (currentPassword: string, newPassword: string) => Promise<void>;
  begin2faSetup: () => Promise<{ secret: string; otpauthUri: string }>;
  enable2fa: (code: string) => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthState | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [status, setStatus] = useState<Status>('loading');
  const [user, setUser] = useState<UserInfo | null>(null);

  const apply = useCallback((r: AuthResponse) => {
    if (r.stage === 'ok' && r.accessToken && r.user) {
      setAccessToken(r.accessToken);
      setUser(r.user);
      setStatus('authenticated');
    }
  }, []);

  const clear = useCallback(() => {
    setAccessToken(null);
    setUser(null);
    setStatus('anonymous');
  }, []);

  // Sahifa yangilanganda: cookie'dagi refresh token orqali sessiyani tiklash.
  useEffect(() => {
    onUnauthorized(clear);
    refreshSession().then((r) => (r ? apply(r) : clear()));
  }, [apply, clear]);

  // Access token 15 daqiqa yashaydi — muddati tugashidan oldin yangilanadi.
  useEffect(() => {
    if (status !== 'authenticated') return;
    const id = window.setInterval(() => {
      refreshSession().then((r) => (r ? apply(r) : clear()));
    }, 10 * 60 * 1000);
    return () => window.clearInterval(id);
  }, [status, apply, clear]);

  const value = useMemo<AuthState>(() => ({
    status,
    user,
    async login(tenantCode, login, password, rememberMe) {
      const r = await api.post<AuthResponse>('/auth/login', { tenantCode, login, password, rememberMe });
      apply(r);
      return r;
    },
    async verify2fa(challengeToken, code) {
      apply(await api.post<AuthResponse>('/auth/2fa', { challengeToken, code }));
    },
    async changePassword(currentPassword, newPassword) {
      apply(await api.post<AuthResponse>('/auth/change-password', { currentPassword, newPassword }));
    },
    begin2faSetup: () => api.post<{ secret: string; otpauthUri: string }>('/auth/2fa/setup'),
    async enable2fa(code) {
      apply(await api.post<AuthResponse>('/auth/2fa/enable', { code }));
    },
    async logout() {
      try {
        await api.post('/auth/logout');
      } finally {
        clear();
      }
    },
  }), [status, user, apply, clear]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth(): AuthState {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth AuthProvider ichida ishlatilishi kerak');
  return ctx;
}

/** Cheklangan sessiya: parol almashtirilmagan yoki 2FA sozlanmagan — ilovaga kirish yo'q. */
export function isRestricted(user: UserInfo | null): boolean {
  return !!user && (user.mustChangePassword || user.mustSetup2fa);
}
