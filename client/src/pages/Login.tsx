import { useState, type FormEvent } from "react";
import { useNavigate, Link } from "react-router-dom";
import api from "../api/client";
import { useAuth } from "../context/AuthContext";
import type { LoginResponse } from "../types/api";

export default function Login() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const res = await api.post<LoginResponse>("/auth/login", { username, password });
      login(res.data.token);
      navigate("/");
    } catch (err: any) {
        const data = err.response?.data;
        setError(data?.Message ?? data?.message ?? (typeof data === "string" ? data : "Login failed"));    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center px-6">
      <div className="w-full max-w-sm">
        <h1 className="font-serif text-4xl mb-8">Sign in</h1>
        <form onSubmit={handleSubmit} className="space-y-5">
          <div>
            <label className="block text-xs uppercase tracking-wider text-muted mb-1">Username</label>
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              className="w-full border-b border-line bg-transparent py-2 focus:outline-none focus:border-ink"
              autoFocus
            />
          </div>
          <div>
            <label className="block text-xs uppercase tracking-wider text-muted mb-1">Password</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full border-b border-line bg-transparent py-2 focus:outline-none focus:border-ink"
            />
          </div>
          {error && <p className="text-sm text-accent">{error}</p>}
          <button
            type="submit"
            disabled={loading}
            className="w-full border border-ink py-2 hover:bg-ink hover:text-bg transition disabled:opacity-50"
          >
            {loading ? "Signing in…" : "Sign in"}
          </button>
        </form>
        <p className="text-sm text-muted mt-6">
          No account? <Link to="/register" className="text-ink underline">Register</Link>
        </p>
      </div>
    </div>
  );
}