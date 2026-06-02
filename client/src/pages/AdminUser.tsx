import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import api from "../api/client";
import type { User } from "../types/api";
import StatusDot from "../components/StatusDot";
import { formatMoney, formatLoanType } from "../lib/format";

export default function AdminUser() {
  const { id } = useParams<{ id: string }>();
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [days, setDays] = useState("7");
  const [working, setWorking] = useState(false);

  async function loadUser() {
    try {
      const res = await api.get<User>(`/auth/user/${id}`);
      setUser(res.data);
    } catch (err: any) {
      const data = err.response?.data;
      setError(data?.Message ?? data?.message ?? "Failed to load user.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadUser();
  }, [id]);

  async function block() {
    const numDays = parseInt(days);
    if (isNaN(numDays) || numDays < 1) {
      setError("Days must be a positive number.");
      return;
    }
    setWorking(true);
    setError(null);
    try {
      await api.patch(`/accountant/block-user/${id}`, { days: numDays });
      await loadUser();
    } catch (err: any) {
      const data = err.response?.data;
      setError(data?.Message ?? data?.message ?? "Failed to block user.");
    } finally {
      setWorking(false);
    }
  }

  async function unblock() {
    setWorking(true);
    setError(null);
    try {
      await api.patch(`/accountant/unblock-user/${id}`);
      await loadUser();
    } catch (err: any) {
      const data = err.response?.data;
      setError(data?.Message ?? data?.message ?? "Failed to unblock user.");
    } finally {
      setWorking(false);
    }
  }

  return (
    <div>
      <Link to="/admin" className="text-sm text-muted hover:text-ink">← Back to all loans</Link>

      {loading && <p className="font-serif italic text-muted mt-4">Loading…</p>}
      {error && <p className="text-accent mt-4">{error}</p>}

      {!loading && user && (
        <>
          <h1 className="font-serif text-4xl mt-4 mb-8">{user.firstName} {user.lastName}</h1>

          <div className="grid grid-cols-2 gap-x-12 gap-y-3 max-w-xl mb-10 text-sm">
            <Detail label="Username" value={user.userName} mono />
            <Detail label="Email" value={user.email} />
            <Detail label="Age" value={String(user.age)} />
            <Detail label="Monthly income" value={user.monthlyIncome.toLocaleString()} mono />
            <Detail label="Role" value={user.role} />
            <Detail
              label="Block status"
              value={user.blockedUntil ? `Blocked until ${new Date(user.blockedUntil).toLocaleDateString()}` : (user.isBlocked ? "Blocked" : "Active")}
            />
          </div>

          <div className="border-t border-line pt-6 mb-10">
            <h2 className="font-serif text-2xl mb-4">Access control</h2>
            <div className="flex items-end gap-4">
              <div>
                <label className="block text-xs uppercase tracking-wider text-muted mb-1">Block for (days)</label>
                <input
                  type="number"
                  value={days}
                  min={1}
                  onChange={(e) => setDays(e.target.value)}
                  className="w-32 border-b border-line bg-transparent py-2 focus:outline-none focus:border-ink"
                />
              </div>
              <button
                onClick={block}
                disabled={working}
                className="border border-accent text-accent py-2 px-4 text-sm hover:bg-accent hover:text-bg transition disabled:opacity-50"
              >
                {working ? "Working…" : "Block"}
              </button>
              <button
                onClick={unblock}
                disabled={working}
                className="border border-line py-2 px-4 text-sm hover:border-ink transition disabled:opacity-50"
              >
                Unblock
              </button>
            </div>
          </div>

          <div className="border-t border-line pt-6">
            <h2 className="font-serif text-2xl mb-4">Loans</h2>
            {user.loans && user.loans.length > 0 ? (
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-line text-left text-muted">
                    <th className="py-2 font-medium font-mono">ID</th>
                    <th className="py-2 font-medium">Type</th>
                    <th className="py-2 font-medium">Amount</th>
                    <th className="py-2 font-medium">Period</th>
                    <th className="py-2 font-medium">Status</th>
                  </tr>
                </thead>
                <tbody>
                  {user.loans.map((loan) => (
                    <tr key={loan.id} className="border-b border-line">
                      <td className="py-3 font-mono">{loan.id}</td>
                      <td className="py-3">{formatLoanType(loan.loanType)}</td>
                      <td className="py-3 font-mono">{formatMoney(loan.amount, loan.currency)}</td>
                      <td className="py-3">{loan.periodInMonths} mo</td>
                      <td className="py-3"><StatusDot status={loan.status} /></td>
                    </tr>
                  ))}
                </tbody>
              </table>
            ) : (
              <p className="font-serif italic text-muted">No loans.</p>
            )}
          </div>
        </>
      )}
    </div>
  );
}

function Detail({ label, value, mono }: { label: string; value: string; mono?: boolean }) {
  return (
    <div>
      <div className="text-xs uppercase tracking-wider text-muted mb-1">{label}</div>
      <div className={mono ? "font-mono" : ""}>{value}</div>
    </div>
  );
}