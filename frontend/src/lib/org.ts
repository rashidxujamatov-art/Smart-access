import { api } from './api';

export interface Department {
  id: string;
  name: string;
  groupCount: number;
}

export interface Group {
  id: string;
  name: string;
  departmentId: string;
  departmentName: string;
}

/** CLAUDE.md 7-bo'lim — Org API. */
export const orgApi = {
  departments: () => api.get<Department[]>('/departments'),
  createDepartment: (name: string) => api.post<Department>('/departments', { name }),
  renameDepartment: (id: string, name: string) => api.patch<Department>(`/departments/${id}`, { name }),
  deleteDepartment: (id: string) => api.delete<void>(`/departments/${id}`),
  groups: (departmentId?: string) =>
    api.get<Group[]>(departmentId ? `/groups?department=${encodeURIComponent(departmentId)}` : '/groups'),
  createGroup: (departmentId: string, name: string) => api.post<Group>('/groups', { departmentId, name }),
  deleteGroup: (id: string) => api.delete<void>(`/groups/${id}`),
};
