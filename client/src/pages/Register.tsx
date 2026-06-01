import { useState, type FormEvent } from "react";
import { useNavigate, Link } from "react-router-dom";
import api from "../api/client";
import type { RegisterRequest } from "../types/api";

export default function Register() {
  type FormState = {
  firstName: string;
  lastName: string;
  username: string;
  email: string;
  password: string;
  age: string;
  monthlyIncome: string;
};

const [form, setForm] = useState<FormState>({
  firstName: "",
  lastName: "",
  username: "",
  email: "",
  password: "",
  age: "",
  monthlyIncome: "",
});
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  function update<K extends keyof FormState>(key: K, value: string) {
  setForm((f) => ({ ...f, [key]: value }));
}

  async function handleSubmit(e: FormEvent) {
  e.preventDefault();
  setError(null);

  const age = parseInt(form.age);
  const monthlyIncome = parseFloat(form.monthlyIncome);

  if (isNaN(age) || age < 18) {
    setError("You must be at least 18 to register.");
    return;
  }
  if (isNaN(monthlyIncome) || monthlyIncome < 0) {
    setError("Monthly income must be a non-negative number.");
    return;
  }

  setLoading(true);
  try {
    const payload: RegisterRequest = {
      firstName: form.firstName,
      lastName: form.lastName,
      username: form.username,
      email: form.email,
      password: form.password,
      age,
      monthlyIncome,
    };
    await api.post("/auth/register", payload);
    navigate("/login");
  } catch (err: any) {
    const data = err.response?.data;
    setError(data?.Message ?? data?.message ?? (typeof data === "string" ? data : "Registration failed"));
  } finally {
    setLoading(false);
  }
}

  return (
    <div className="min-h-screen flex items-center justify-center px-6 py-12">
      <div className="w-full max-w-md">
        <h1 className="font-serif text-4xl mb-8">Create account</h1>
        <form onSubmit={handleSubmit} className="space-y-5">
          <div className="grid grid-cols-2 gap-4">
            <Field label="First name" value={form.firstName} onChange={(v) => update("firstName", v)} />
            <Field label="Last name" value={form.lastName} onChange={(v) => update("lastName", v)} />
          </div>
          <Field label="Username" value={form.username} onChange={(v) => update("username", v)} />
          <Field label="Email" type="email" value={form.email} onChange={(v) => update("email", v)} />
          <Field label="Password" type="password" value={form.password} onChange={(v) => update("password", v)} />
          <div className="grid grid-cols-2 gap-4">
            <Field label="Age" type="number" value={form.age} onChange={(v) => update("age", v)} />
            <Field label="Monthly income" type="number" value={form.monthlyIncome} onChange={(v) => update("monthlyIncome", v)} />          </div>
          {error && <p className="text-sm text-accent">{error}</p>}
          <button
            type="submit"
            disabled={loading}
            className="w-full border border-ink py-2 hover:bg-ink hover:text-bg transition disabled:opacity-50"
          >
            {loading ? "Creating…" : "Create account"}
          </button>
        </form>
        <p className="text-sm text-muted mt-6">
          Already registered? <Link to="/login" className="text-ink underline">Sign in</Link>
        </p>
      </div>
    </div>
  );
}

function Field({
  label,
  value,
  onChange,
  type = "text",
  min
}: {
  label: string;
  value: string;
  onChange: (v: string) => void;
  type?: string;
  min?: number;
}) {
  return (
    <div>
      <label className="block text-xs uppercase tracking-wider text-muted mb-1">{label}</label>
      <input
        type={type}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        min={min}
        className="w-full border-b border-line bg-transparent py-2 focus:outline-none focus:border-ink"
      />
    </div>
  );
}