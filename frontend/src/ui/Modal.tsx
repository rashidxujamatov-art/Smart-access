import { useEffect, useId, useRef, type ReactNode } from 'react';

/** Tasdiqlash oynasi — ekran markazida (UI-SPEC A.2). Esc — yopadi, fokus oyna ichida. */
export function Modal({ title, children, onClose }: { title: string; children: ReactNode; onClose?: () => void }) {
  const titleId = useId();
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    ref.current?.querySelector<HTMLElement>('input, button')?.focus();
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose?.();
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [onClose]);

  return (
    <div className="modal" role="presentation">
      <div ref={ref} className="card col" role="dialog" aria-modal="true" aria-labelledby={titleId}
        style={{ width: '100%', maxWidth: 420, gap: 14 }}>
        <h2 id={titleId}>{title}</h2>
        {children}
      </div>
    </div>
  );
}
