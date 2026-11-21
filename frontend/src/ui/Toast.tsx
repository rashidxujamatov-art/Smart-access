import { createContext, useCallback, useContext, useEffect, useRef, useState, type ReactNode } from 'react';

const DURATION_MS = 2800;

const ToastContext = createContext<(text: string) => void>(() => {});

/** Toast — ekranning pastki markazida, 2.8 s (UI-SPEC A.2). */
export function ToastProvider({ children }: { children: ReactNode }) {
  const [text, setText] = useState('');
  const timer = useRef<number | undefined>(undefined);

  const show = useCallback((t: string) => {
    setText(t);
    window.clearTimeout(timer.current);
    timer.current = window.setTimeout(() => setText(''), DURATION_MS);
  }, []);

  useEffect(() => () => window.clearTimeout(timer.current), []);

  return (
    <ToastContext.Provider value={show}>
      {children}
      {text && <div className="toast" role="status" aria-live="polite">{text}</div>}
    </ToastContext.Provider>
  );
}

// eslint-disable-next-line react-refresh/only-export-components
export function useToast() {
  return useContext(ToastContext);
}
