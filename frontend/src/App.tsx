import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { AppShell } from './app/AppShell';
import { homePath, MENUS } from './app/menu';
import { AuthProvider, useAuth } from './lib/auth';
import { useTheme } from './lib/theme';
import { LoginPage } from './pages/LoginPage';

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
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route element={<AppShell />}>
              {ALL_PATHS.map((p) => (
                <Route key={p} path={p} element={null} />
              ))}
            </Route>
            <Route path="*" element={<RootRedirect />} />
          </Routes>
        </ThemeRoot>
      </BrowserRouter>
    </AuthProvider>
  );
}
