import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import type { ReactNode } from "react";

export default function Layout({ children }: { children: ReactNode }) {
  const { username, role, logout } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate("/login");
  }

  return (
    <div className="min-h-screen flex flex-col">
      <header className="border-b border-line">
        <div className="max-w-5xl mx-auto px-6 py-4 flex items-center justify-between">
          <Link to="/" className="font-serif text-xl">Loan API</Link>
          <div className="flex items-center gap-6 text-sm">
            <span className="text-muted">
              <span className="font-mono text-ink">{username}</span>
              <span className="mx-2">·</span>
              <span>{role}</span>
            </span>
            <button onClick={handleLogout} className="text-ink hover:underline">
              Sign out
            </button>
          </div>
        </div>
      </header>
      <main className="flex-1 max-w-5xl w-full mx-auto px-6 py-10">
        {children}
      </main>
    </div>
  );
}