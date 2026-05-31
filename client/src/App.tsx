import { Routes, Route, Navigate } from "react-router-dom";
import Login from "./pages/Login";
import { useAuth } from "./context/AuthContext";

function Home() {
  const { username, role, logout } = useAuth();
  return (
    <div className="min-h-screen p-12">
      <h1 className="font-serif text-4xl mb-4">Logged in</h1>
      <p className="text-muted">Username: <span className="font-mono text-ink">{username}</span></p>
      <p className="text-muted">Role: <span className="font-mono text-ink">{role}</span></p>
      <button
        onClick={logout}
        className="mt-6 border border-ink py-2 px-4 hover:bg-ink hover:text-bg transition"
      >
        Sign out
      </button>
    </div>
  );
}

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { token } = useAuth();
  return token ? <>{children}</> : <Navigate to="/login" replace />;
}

function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route path="/" element={<ProtectedRoute><Home /></ProtectedRoute>} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

export default App;