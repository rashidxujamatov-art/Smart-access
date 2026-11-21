import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { AppShell } from './app/AppShell';
import { homePath, MENUS } from './app/menu';
import { AuthProvider, useAuth } from './lib/auth';
import { useTheme } from './lib/theme';
import { LoginPage } from './pages/LoginPage';
import { GroupsPage } from './pages/admin/GroupsPage';
import { ToastProvider } from './ui/Toast';

/** Qurilgan ekranlar (manzil → komponent). Qolgan menyu bandlari — tegishli bandlarda (docs/progress.md). */
const SCREENS: Record<string, React.ReactNode> = {
  '/admin/groups': <GroupsPage />,
};

/** Barcha rollarning menyu manzillari (takrorlanmasdan). Ekranlar tegishli bandlarda qo'shiladi (docs/progress.md). */
const ALL_PATHS = [...new Set(Object.values(MENUS).flat().map((m) => m.path))];

function RootRedirect() {
  const { status, user } = useAuth();
  if (status === 'loading') return null;
  return <Navigate to={user ? homePath(user.role) : '/login'} replace />;
}

function ThemeRoot({ children }: { children: React.ReactNode }) {
  useTheme();
  return <div className="root">{children}</div>;
}

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <ThemeRoot>
          <ToastProvider>
            <Routes>
              <Route path="/login" element={<LoginPage />} />
              <Route element={<AppShell />}>
                {ALL_PATHS.map((p) => (
                  <Route key={p} path={p} element={SCREENS[p] ?? null} />
                ))}
              </Route>
              <Route path="*" element={<RootRedirect />} />
            </Routes>
          </ToastProvider>
        </ThemeRoot>
      </BrowserRouter>
    </AuthProvider>
  );
}
