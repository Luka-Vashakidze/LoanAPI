import { useEffect, useState, Fragment } from "react";
import api from "../api/client";
import type { Loan, LoanStatus } from "../types/api";
import StatusDot from "../components/StatusDot";
import { formatMoney, formatLoanType } from "../lib/format";
import { Link } from "react-router-dom";

const statuses: LoanStatus[] = ["InProcess", "Approved", "Rejected"];

export default function AdminLoans() {
  const [loans, setLoans] = useState<Loan[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [savingId, setSavingId] = useState<number | null>(null);

  async function loadLoans() {
    try {
      const res = await api.get<Loan[]>("/accountant/all-loans");
      setLoans(res.data);
    } catch (err: any) {
      const data = err.response?.data;
      setError(data?.Message ?? data?.message ?? "Failed to load loans.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadLoans();
  }, []);

  async function changeStatus(loanId: number, newStatus: LoanStatus) {
    setSavingId(loanId);
    try {
      await api.patch(`/accountant/loan/${loanId}/status`, { newStatus });
      setEditingId(null);
      await loadLoans();
    } catch (err: any) {
      const data = err.response?.data;
      setError(data?.Message ?? data?.message ?? "Failed to change status.");
    } finally {
      setSavingId(null);
    }
  }

  return (
    <div>
      <h1 className="font-serif text-4xl mb-8">All loans</h1>

      {loading && <p className="font-serif italic text-muted">Loading…</p>}
      {error && <p className="text-accent">{error}</p>}

      {!loading && !error && loans.length === 0 && (
        <p className="font-serif italic text-muted">No loans in the system.</p>
      )}

      {!loading && !error && loans.length > 0 && (
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-line text-left text-muted">
              <th className="py-2 font-medium font-mono">ID</th>
              <th className="py-2 font-medium">User</th>
              <th className="py-2 font-medium">Type</th>
              <th className="py-2 font-medium">Amount</th>
              <th className="py-2 font-medium">Period</th>
              <th className="py-2 font-medium">Status</th>
              <th className="py-2 font-medium text-right">Actions</th>
            </tr>
          </thead>
          <tbody>
            {loans.map((loan) => (
              <Fragment key={loan.id}>
                <tr className="border-b border-line">
                  <td className="py-3 font-mono">{loan.id}</td>
                    <td className="py-3">
                        <Link to={`/admin/users/${loan.userId}`} className="text-ink underline">
                         {loan.user ? loan.user.userName : `#${loan.userId}`}
                        </Link>
                    </td>
                    <td className="py-3">{formatLoanType(loan.loanType)}</td>
                    <td className="py-3 font-mono">{formatMoney(loan.amount, loan.currency)}</td>
                    <td className="py-3">{loan.periodInMonths} mo</td>
                    <td className="py-3"><StatusDot status={loan.status} /></td>
                  <td className="py-3 text-right">
                    <button
                      onClick={() => setEditingId(editingId === loan.id ? null : loan.id)}
                      className="text-ink underline"
                    >
                      Change status
                    </button>
                  </td>
                </tr>
                {editingId === loan.id && (
                  <tr className="border-b border-line bg-line/30">
                    <td colSpan={7} className="py-3 px-1">
                      <div className="flex items-center gap-3">
                        <span className="text-sm text-muted">Set status:</span>
                        {statuses.map((s) => (
                          <button
                            key={s}
                            onClick={() => changeStatus(loan.id, s)}
                            disabled={savingId === loan.id || s === loan.status}
                            className="border border-line py-1 px-3 text-sm hover:border-ink transition disabled:opacity-40 disabled:cursor-not-allowed"
                          >
                            {s === "InProcess" ? "In process" : s}
                          </button>
                        ))}
                        <button
                          onClick={() => setEditingId(null)}
                          className="ml-auto text-sm text-muted hover:text-ink"
                        >
                          Cancel
                        </button>
                      </div>
                    </td>
                  </tr>
                )}
              </Fragment>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}