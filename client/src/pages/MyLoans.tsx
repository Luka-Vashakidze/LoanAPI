import { useEffect, useState, Fragment } from "react";
import { Link } from "react-router-dom";
import api from "../api/client";
import type { Loan } from "../types/api";
import StatusDot from "../components/StatusDot";
import { formatMoney, formatLoanType } from "../lib/format";

export default function MyLoans() {
    const [loans, setLoans] = useState<Loan[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [confirmingId, setConfirmingId] = useState<number | null>(null);
    const [deletingId, setDeletingId] = useState<number | null>(null);

  
    async function loadLoans() {
    try {
        const res = await api.get<Loan[]>("/loan/my-loans");
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

    async function handleDelete(id: number) {
    setDeletingId(id);
    try {
        await api.delete(`/loan/${id}`);
        setConfirmingId(null);
        await loadLoans();
    } catch (err: any) {
        const data = err.response?.data;
        setError(data?.Message ?? data?.message ?? "Failed to delete loan.");
    } finally {
        setDeletingId(null);
    }
    }

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
  <Fragment key={loan.id}>
    <tr className="border-b border-line">
      <td className="py-3 font-mono">{loan.id}</td>
      <td className="py-3">{formatLoanType(loan.loanType)}</td>
      <td className="py-3 font-mono">{formatMoney(loan.amount, loan.currency)}</td>
      <td className="py-3">{loan.periodInMonths} mo</td>
      <td className="py-3"><StatusDot status={loan.status} /></td>
      <td className="py-3 text-right">
        {loan.status === "InProcess" ? (
          <span className="space-x-3">
            <Link to={`/loans/${loan.id}/edit`} className="text-ink underline">Edit</Link>
            <button onClick={() => setConfirmingId(loan.id)} className="text-accent underline">Delete</button>
          </span>
        ) : (
          <span className="text-muted">—</span>
        )}
      </td>
    </tr>
        {confirmingId === loan.id && (
        <tr className="border-b border-line bg-line/30">
            <td colSpan={6} className="py-3 px-1">
            <div className="flex items-center justify-between">
                <span className="text-sm">Delete loan #{loan.id}? This cannot be undone.</span>
                <span className="space-x-3">
                <button
                    onClick={() => handleDelete(loan.id)}
                    disabled={deletingId === loan.id}
                    className="border border-accent text-accent py-1 px-3 text-sm hover:bg-accent hover:text-bg transition disabled:opacity-50"
                >
                    {deletingId === loan.id ? "Deleting…" : "Confirm delete"}
                </button>
                <button
                    onClick={() => setConfirmingId(null)}
                    className="border border-line py-1 px-3 text-sm hover:border-ink transition"
                >
                    Cancel
                </button>
                </span>
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