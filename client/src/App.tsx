import { Routes, Route, Navigate } from "react-router-dom";
import Login from "./pages/Login";
import Register from "./pages/Register";
import Layout from "./components/Layout";
import { useAuth } from "./context/AuthContext";
import type { ReactNode } from "react";
import MyLoans from "./pages/MyLoans";
import CreateLoan from "./pages/CreateLoan";
import EditLoan from "./pages/EditLoan";
import AdminLoans from "./pages/AdminLoans";


function ProtectedRoute({ children }: { children: ReactNode }) {
  const { token, initializing } = useAuth();
  if (initializing) return <p className="p-12 font-serif italic text-muted">Loading…</p>;
  return token ? <>{children}</> : <Navigate to="/login" replace />;
}

function RoleRedirect() {
  const { role, initializing } = useAuth();
   if (initializing) return <p className="p-12 font-serif italic text-muted">Loading…</p>;
  if (role === "Accountant") return <Navigate to="/admin" replace />;
  return <Navigate to="/loans" replace />;
}

function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />
      <Route
        path="/"
        element={
          <ProtectedRoute>
            <RoleRedirect />
          </ProtectedRoute>
        }
      />
      <Route
        path="/loans"
        element={
          <ProtectedRoute>
            <Layout><MyLoans /></Layout>
          </ProtectedRoute>
        }
      />
      <Route
  path="/loans/new"
  element={
    <ProtectedRoute>
      <Layout><CreateLoan /></Layout>
    </ProtectedRoute>
  }
/>
<Route
  path="/loans/:id/edit"
  element={
    <ProtectedRoute>
      <Layout><EditLoan /></Layout>
    </ProtectedRoute>
  }
/>
      <Route
        path="/admin"
        element={
          <ProtectedRoute>
            <Layout><AdminLoans /></Layout>
          </ProtectedRoute>
        }
      />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

export default App;