import { useEffect, useState } from "react";
import { useParams, useNavigate, Link } from "react-router-dom";
import api from "../api/client";
import LoanForm, { type LoanFormValues } from "../components/LoanForm";
import type { Loan } from "../types/api";

export default function EditLoan() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [initial, setInitial] = useState<LoanFormValues | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    async function load() {
      try {
        const res = await api.get<Loan>(`/loan/${id}`);
        if (!cancelled) {
          setInitial({
            loanType: res.data.loanType,
            amount: String(res.data.amount),
            currency: res.data.currency,
            periodInMonths: String(res.data.periodInMonths),
          });
        }
      } catch (err: any) {
        if (!cancelled) {
          const data = err.response?.data;
          setError(data?.Message ?? data?.message ?? "Failed to load loan.");
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    }
    load();
    return () => { cancelled = true; };
  }, [id]);

  return (
    <div>
      <Link to="/loans" className="text-sm text-muted hover:text-ink">← Back to loans</Link>
      <h1 className="font-serif text-4xl mt-4 mb-8">Edit loan</h1>

      {loading && <p className="font-serif italic text-muted">Loading…</p>}
      {error && <p className="text-accent">{error}</p>}

      {!loading && !error && initial && (
        <LoanForm
          initial={initial}
          submitLabel="Save changes"
          onSubmit={async (values) => {
            await api.put(`/loan/${id}`, values);
            navigate("/loans");
          }}
        />
      )}
    </div>
  );
}