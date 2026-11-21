import type { TKey } from '../i18n';
import type { Role } from '../lib/api';
import type { IconName } from '../ui/Icon';

export interface MenuItem {
  path: string;
  label: TKey;
  icon: IconName;
}

/** CLAUDE.md 3-bo'lim — menyular dizayndagi tartibda (BR-RBAC-02). */
export const MENUS: Record<Role, MenuItem[]> = {
  guard: [
    { path: '/guard', label: 'nav.guardDashboard', icon: 'dash' },
    { path: '/guard/cameras', label: 'nav.guardCameras', icon: 'video' },
    { path: '/guard/gate', label: 'nav.guardGate', icon: 'gateClosed' },
    { path: '/guard/player', label: 'nav.guardPlayer', icon: 'play' },
    { path: '/archive', label: 'nav.archive', icon: 'film' },
  ],
  admin: [
    { path: '/admin', label: 'nav.adminDashboard', icon: 'dash' },
    { path: '/admin/employees', label: 'nav.employees', icon: 'users' },
    { path: '/admin/groups', label: 'nav.groups', icon: 'layers' },
    { path: '/admin/schedules', label: 'nav.schedules', icon: 'cal' },
    { path: '/admin/cameras', label: 'nav.cameras', icon: 'video' },
    { path: '/admin/map', label: 'nav.map', icon: 'map' },
    { path: '/admin/gates', label: 'nav.gates', icon: 'gateClosed' },
    { path: '/admin/face', label: 'nav.face', icon: 'face' },
    { path: '/admin/anpr', label: 'nav.anpr', icon: 'car' },
    { path: '/attendance', label: 'nav.attendanceAdmin', icon: 'clock' },
    { path: '/archive', label: 'nav.archive', icon: 'film' },
    { path: '/reports', label: 'nav.reports', icon: 'file' },
    { path: '/admin/users', label: 'nav.users', icon: 'shield' },
    { path: '/admin/settings', label: 'nav.settings', icon: 'gear' },
    { path: '/admin/audit', label: 'nav.audit', icon: 'log' },
  ],
  manager: [
    { path: '/manager', label: 'nav.managerDashboard', icon: 'dash' },
    { path: '/attendance', label: 'nav.attendance', icon: 'clock' },
    { path: '/reports', label: 'nav.reports', icon: 'file' },
  ],
};

/** BR-AUTH-08: rolga qarab boshlang'ich ekran. */
export function homePath(role: Role): string {
  return MENUS[role][0].path;
}

/** Joriy manzilga mos menyu bandi (eng uzun mos prefiks — ichki ekranlarda ota band faol turadi). */
export function activeItem(role: Role, pathname: string): MenuItem | undefined {
  return MENUS[role]
    .filter((m) => pathname === m.path || pathname.startsWith(m.path + '/'))
    .sort((a, b) => b.path.length - a.path.length)[0];
}
