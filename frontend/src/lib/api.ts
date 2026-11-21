/**
 * Backend bilan aloqa. Access token faqat xotirada; refresh token — httpOnly cookie (BR-AUTH-05),
 * shuning uchun JavaScript uni o'qiy olmaydi. 401 bo'lsa bir marta yangilashga urinadi.
 */

export class ApiError extends Error {
  readonly status: number;
  readonly code: string;
  readonly fields?: Record<string, string>;
  readonly details?: Record<string, unknown>;

  constructor(status: number, code: string, fields?: Record<string, string>, details?: Record<string, unknown>) {
    super(code);
    this.status = status;
    this.code = code;
    this.fields = fields ?? undefined;
    this.details = details ?? undefined;
  }
}

export type Role = 'guard' | 'admin' | 'manager';

export interface UserInfo {
  id: string;
  login: string;
  fullName: string;
  role: Role;
  home: 'guard_dashboard' | 'admin_dashboard' | 'manager_dashboard';
  twoFactorEnabled: boolean;
  mustChangePassword: boolean;
  mustSetup2fa: boolean;
}

export interface AuthResponse {
  stage: 'ok' | '2fa';
  accessToken: string | null;
  accessTokenExpiresAt: string | null;
  user: UserInfo | null;
  challengeToken: string | null;
}

const BASE = '/api/v1';
const NO_RETRY = ['/auth/login', '/auth/2fa', '/auth/refresh'];

let accessToken: string | null = null;
let refreshing: Promise<AuthResponse | null> | null = null;

export function setAccessToken(token: string | null) {
  accessToken = token;
}

async function send(method: string, path: string, body?: unknown): Promise<Response> {
  const headers: Record<string, string> = { Accept: 'application/json' };
  if (body !== undefined) headers['Content-Type'] = 'application/json';
  if (accessToken) headers.Authorization = `Bearer ${accessToken}`;
  try {
    return await fetch(BASE + path, {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
      credentials: 'same-origin',
    });
  } catch {
    throw new ApiError(0, 'network');
  }
}

async function parse<T>(res: Response): Promise<T> {
  if (res.status === 204) return undefined as T;
  const data = await res.json().catch(() => null);
  if (!res.ok) {
    throw new ApiError(res.status, data?.code ?? 'unknown', data?.fields ?? undefined, data?.details ?? undefined);
  }
  return data as T;
}

/** Refresh — bir vaqtda faqat bitta so'rov (single-flight). */
export function refreshSession(): Promise<AuthResponse | null> {
  refreshing ??= (async () => {
    try {
      const res = await send('POST', '/auth/refresh', {});
      if (!res.ok) {
        setAccessToken(null);
        return null;
      }
      const data = await parse<AuthResponse>(res);
      setAccessToken(data.accessToken);
      return data;
    } catch {
      return null;
    } finally {
      refreshing = null;
    }
  })();
  return refreshing;
}

let unauthorizedHandler: (() => void) | null = null;

/** Sessiya butunlay tugaganda (refresh ham ishlamasa) chaqiriladi — login ekraniga qaytarish uchun. */
export function onUnauthorized(handler: () => void) {
  unauthorizedHandler = handler;
}

async function request<T>(method: string, path: string, body?: unknown): Promise<T> {
  let res = await send(method, path, body);
  if (res.status === 401 && !NO_RETRY.includes(path) && accessToken) {
    const refreshed = await refreshSession();
    if (refreshed) {
      res = await send(method, path, body);
    } else {
      unauthorizedHandler?.();
    }
  }
  return parse<T>(res);
}

export const api = {
  get: <T>(path: string) => request<T>('GET', path),
  post: <T>(path: string, body?: unknown) => request<T>('POST', path, body ?? {}),
  delete: <T>(path: string) => request<T>('DELETE', path),
};
