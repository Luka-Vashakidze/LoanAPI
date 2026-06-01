import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../api/client";
import type { Loan } from "../types/api";
import StatusDot from "../components/StatusDot";
import { formatMoney, formatLoanType } from "../lib/format";

export default function MyLoans() {
  const [loans, setLoans] = useState<Loan[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    async function load() {
      try {
        const res = await api.get<Loan[]>("/loan/my-loans");
        if (!cancelled) setLoans(res.data);
      } catch (err: any) {
        if (!cancelled) {
          const data = err.response?.data;
          setError(data?.Message ?? data?.message ?? "Failed to load loans.");
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    }
    load();
    return () => { cancelled = true; };
  }, []);

  return (
    <div>
      <div className="flex items-center justify-between mb-8">
        <h1 className="font-serif text-4xl">My loans</h1>
        <Link
          to="/loans/new"
          className="border border-ink py-2 px-4 text-sm hover:bg-ink hover:text-bg transition"
        >
          New loan
        </Link>
      </div>

      {loading && <p className="font-serif italic text-muted">Loading…</p>}
      {error && <p className="text-accent">{error}</p>}

      {!loading && !error && loans.length === 0 && (
        <p className="font-serif italic text-muted">No loans yet.</p>
      )}

      {!loading && !error && loans.length > 0 && (
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-line text-left text-muted">
              <th className="py-2 font-medium font-mono">ID</th>
              <th className="py-2 font-medium">Type</th>
              <th className="py-2 font-medium">Amount</th>
              <th className="py-2 font-medium">Period</th>
              <th className="py-2 font-medium">Status</th>
              <th className="py-2 font-medium text-right">Actions</th>
            </tr>
          </thead>
          <tbody>
            {loans.map((loan) => (
              <tr key={loan.id} className="border-b border-line">
                <td className="py-3 font-mono">{loan.id}</td>
                <td className="py-3">{formatLoanType(loan.loanType)}</td>
                <td className="py-3 font-mono">{formatMoney(loan.amount, loan.currency)}</td>
                <td className="py-3">{loan.periodInMonths} mo</td>
                <td className="py-3"><StatusDot status={loan.status} /></td>
                <td className="py-3 text-right">
                  {loan.status === "InProcess" ? (
                    <Link to={`/loans/${loan.id}/edit`} className="text-ink underline">Edit</Link>
                  ) : (
                    <span className="text-muted">—</span>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}