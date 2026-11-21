import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { errorText, t } from '../../i18n';
import { ApiError } from '../../lib/api';
import { orgApi, type Department, type Group } from '../../lib/org';
import { ConfirmDelete } from '../../ui/ConfirmDelete';
import { Modal } from '../../ui/Modal';
import { useToast } from '../../ui/Toast';

const ALL = '';

function apiMessage(err: unknown): string {
  if (err instanceof ApiError) {
    const field = err.fields && Object.values(err.fields)[0];
    return errorText(field ?? err.code);
  }
  return errorText(undefined);
}

/**
 * D11 — Guruhlar va bo'limlar (UI-SPEC.md).
 * Guruh kartasidagi "N xodim" va "Guruh grafigi" — 4-band (EMP) va 5-band (SCH) bilan qo'shiladi.
 */
export function GroupsPage() {
  const toast = useToast();
  const [departments, setDepartments] = useState<Department[]>([]);
  const [groups, setGroups] = useState<Group[] | null>(null);
  const [loadError, setLoadError] = useState(false);
  const [filter, setFilter] = useState(ALL);
  const [newName, setNewName] = useState('');
  const [nameError, setNameError] = useState('');
  const [busy, setBusy] = useState(false);
  const [deleting, setDeleting] = useState<Group | null>(null);
  const [showDepartments, setShowDepartments] = useState(false);

  const load = useCallback(async () => {
    setLoadError(false);
    try {
      const [deps, grps] = await Promise.all([orgApi.departments(), orgApi.groups()]);
      setDepartments(deps);
      setGroups(grps);
    } catch {
      setLoadError(true);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  // Filtrdagi bo'lim o'chirilgan bo'lsa — "Hammasi"ga qaytish.
  useEffect(() => {
    if (filter !== ALL && !departments.some((d) => d.id === filter)) setFilter(ALL);
  }, [departments, filter]);

  async function createGroup(e: FormEvent) {
    e.preventDefault();
    if (filter === ALL) {
      setNameError(t('groups.chooseDepartment'));
      return;
    }
    if (!newName.trim()) {
      setNameError(errorText('required'));
      return;
    }
    setBusy(true);
    setNameError('');
    try {
      await orgApi.createGroup(filter, newName);
      setNewName('');
      toast(t('groups.created'));
      await load();
    } catch (err) {
      setNameError(apiMessage(err));
    } finally {
      setBusy(false);
    }
  }

  async function deleteGroup(g: Group) {
    try {
      await orgApi.deleteGroup(g.id);
      toast(t('groups.deleted'));
      await load();
    } catch (err) {
      toast(apiMessage(err));
    }
  }

  const visible = groups?.filter((g) => filter === ALL || g.departmentId === filter) ?? [];

  return (
    <>
      <form className="card row wrap" onSubmit={createGroup} noValidate>
        <div className="muted" style={{ flex: '1 1 300px' }}>{t('groups.hierarchy')}</div>
        <select className="inp" style={{ flex: '0 1 200px', maxWidth: 220 }} value={filter}
          onChange={(e) => { setFilter(e.target.value); setNameError(''); }} aria-label={t('groups.departmentFilter')}>
          <option value={ALL}>{t('groups.departmentFilter')}: {t('common.all')}</option>
          {departments.map((d) => <option key={d.id} value={d.id}>{d.name}</option>)}
        </select>
        <button type="button" className="btn" onClick={() => setShowDepartments(true)}>{t('groups.addDepartment')}</button>
        <div style={{ flex: '1 1 200px', maxWidth: 260 }}>
          <input className="inp" placeholder={t('groups.newGroupName')} aria-label={t('groups.newGroupName')}
            value={newName} onChange={(e) => { setNewName(e.target.value); setNameError(''); }}
            aria-invalid={!!nameError} aria-describedby={nameError ? 'grp-name-err' : undefined} maxLength={100} />
          {nameError && <div id="grp-name-err" className="ferr">{nameError}</div>}
        </div>
        <button type="submit" className="btn pri" disabled={busy}>{t('groups.create')}</button>
      </form>

      {loadError ? (
        <div className="ban b-danger" role="alert">
          {t('common.loadError')}
          <button type="button" className="btn" style={{ marginLeft: 'auto' }} onClick={() => void load()}>{t('common.retry')}</button>
        </div>
      ) : groups === null ? (
        <div className="muted" role="status">{t('common.loading')}</div>
      ) : visible.length === 0 ? (
        <div className="card muted">{groups.length === 0 ? t('groups.empty') : t('common.empty')}</div>
      ) : (
        <div className="g2">
          {visible.map((g) => (
            <div key={g.id} className="card col">
              <div className="row">
                <h2>{g.name}</h2>
                <span className="badge b-neutral" style={{ marginLeft: 'auto' }}>{g.departmentName}</span>
                <button type="button" className="btn" style={{ minHeight: 32, color: '#B91C1C' }} onClick={() => setDeleting(g)}>
                  {t('common.delete')}
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {deleting && (
        <ConfirmDelete label={deleting.name} kind={t('groups.kind')}
          onConfirm={() => deleteGroup(deleting)} onClose={() => setDeleting(null)} />
      )}
      {showDepartments && (
        <DepartmentsModal departments={departments} onChanged={load} onClose={() => setShowDepartments(false)} />
      )}
    </>
  );
}

/** Bo'limlar CRUD (BR-ORG). UI-SPEC D11: "Bo'lim qo'shish" — joyi toolbar'da; boshqaruv — shu oynada (13-bo'lim, 15-savol). */
function DepartmentsModal({ departments, onChanged, onClose }: {
  departments: Department[];
  onChanged: () => Promise<void>;
  onClose: () => void;
}) {
  const toast = useToast();
  const [name, setName] = useState('');
  const [error, setError] = useState('');
  const [editing, setEditing] = useState<{ id: string; name: string; error: string } | null>(null);
  const [deleting, setDeleting] = useState<Department | null>(null);

  async function create(e: FormEvent) {
    e.preventDefault();
    setError('');
    try {
      await orgApi.createDepartment(name);
      setName('');
      toast(t('groups.departmentCreated'));
      await onChanged();
    } catch (err) {
      setError(apiMessage(err));
    }
  }

  async function rename(e: FormEvent) {
    e.preventDefault();
    if (!editing) return;
    try {
      await orgApi.renameDepartment(editing.id, editing.name);
      setEditing(null);
      toast(t('groups.departmentRenamed'));
      await onChanged();
    } catch (err) {
      setEditing({ ...editing, error: apiMessage(err) });
    }
  }

  async function remove(d: Department) {
    try {
      await orgApi.deleteDepartment(d.id);
      toast(t('groups.departmentDeleted'));
      await onChanged();
    } catch (err) {
      toast(apiMessage(err));
    }
  }

  if (deleting) {
    return <ConfirmDelete label={deleting.name} kind={t('groups.departmentKind')}
      onConfirm={() => remove(deleting)} onClose={() => setDeleting(null)} />;
  }

  return (
    <Modal title={t('groups.departmentsTitle')} onClose={onClose}>
      <form className="row" style={{ alignItems: 'flex-start', gap: 8 }} onSubmit={create} noValidate>
        <div style={{ flex: 1 }}>
          <label className="lbl" htmlFor="dep-new">{t('groups.departmentName')}</label>
          <input id="dep-new" className="inp" value={name} maxLength={100}
            onChange={(e) => { setName(e.target.value); setError(''); }}
            aria-invalid={!!error} aria-describedby={error ? 'dep-new-err' : undefined} />
          {error && <div id="dep-new-err" className="ferr">{error}</div>}
        </div>
        <button type="submit" className="btn pri" style={{ marginTop: 22 }}>{t('groups.departmentAdd')}</button>
      </form>

      <div className="col" style={{ gap: 8, maxHeight: 320, overflowY: 'auto' }}>
        {departments.length === 0 && <div className="muted">{t('groups.departmentsEmpty')}</div>}
        {departments.map((d) =>
          editing?.id === d.id ? (
            <form key={d.id} className="col" style={{ gap: 4 }} onSubmit={rename} noValidate>
              <div className="row" style={{ gap: 8 }}>
                <input className="inp" value={editing.name} maxLength={100} aria-label={t('groups.departmentName')}
                  onChange={(e) => setEditing({ ...editing, name: e.target.value, error: '' })} aria-invalid={!!editing.error} />
                <button type="button" className="btn" onClick={() => setEditing(null)}>{t('common.cancel')}</button>
                <button type="submit" className="btn pri">{t('common.save')}</button>
              </div>
              {editing.error && <div className="ferr">{editing.error}</div>}
            </form>
          ) : (
            <div key={d.id} className="row" style={{ gap: 8 }}>
              <div style={{ flex: 1, minWidth: 0 }}>
                <b>{d.name}</b> <span className="sm muted">· {t('groups.departmentGroups', { n: d.groupCount })}</span>
              </div>
              <button type="button" className="btn" onClick={() => setEditing({ id: d.id, name: d.name, error: '' })}>
                {t('common.edit')}
              </button>
              <button type="button" className="btn" style={{ color: '#B91C1C' }} onClick={() => setDeleting(d)}>
                {t('common.delete')}
              </button>
            </div>
          ),
        )}
      </div>

      <div className="row" style={{ justifyContent: 'flex-end' }}>
        <button type="button" className="btn" onClick={onClose}>{t('groups.close')}</button>
      </div>
    </Modal>
  );
}
