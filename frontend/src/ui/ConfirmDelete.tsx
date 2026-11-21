import { useState } from 'react';
import { t } from '../i18n';
import { Modal } from './Modal';

/**
 * O'chirishni tasdiqlash (UI-SPEC A.2, dizayn: "O'chirishni tasdiqlang").
 * onConfirm xato tashlasa, oyna yopiladi — xabarni chaqiruvchi toast orqali ko'rsatadi.
 */
export function ConfirmDelete({ label, kind, onConfirm, onClose }: {
  label: string;
  kind: string;
  onConfirm: () => Promise<void>;
  onClose: () => void;
}) {
  const [busy, setBusy] = useState(false);

  async function confirm() {
    setBusy(true);
    try {
      await onConfirm();
    } finally {
      setBusy(false);
      onClose();
    }
  }

  return (
    <Modal title={t('common.confirmDeleteTitle')} onClose={onClose}>
      <div>{t('common.confirmDeleteText', { label, kind })}</div>
      <div className="row" style={{ justifyContent: 'flex-end', gap: 8 }}>
        <button type="button" className="btn" onClick={onClose}>{t('common.cancel')}</button>
        <button type="button" className="btn danger" onClick={() => void confirm()} disabled={busy}>{t('common.delete')}</button>
      </div>
    </Modal>
  );
}
