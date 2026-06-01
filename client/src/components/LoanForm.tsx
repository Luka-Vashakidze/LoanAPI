import { useState, type FormEvent } from "react";
import type { LoanType, Currency } from "../types/api";

export interface LoanFormValues {
  loanType: LoanType;
  amount: string;
  currency: Currency;
  periodInMonths: string;
}

interface Props {
  initial?: LoanFormValues;
  submitLabel: string;
  onSubmit: (values: { loanType: LoanType; amount: number; currency: Currency; periodInMonths: number }) => Promise<void>;
}

const defaults: LoanFormValues = {
  loanType: "Fast",
  amount: "",
  currency: "GEL",
  periodInMonths: "",
};

export default function LoanForm({ initial, submitLabel, onSubmit }: Props) {
  const [values, setValues] = useState<LoanFormValues>(initial ?? defaults);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  function update<K extends keyof LoanFormValues>(key: K, value: LoanFormValues[K]) {
    setValues((v) => ({ ...v, [key]: value }));
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);

    const amount = parseFloat(values.amount);
    const period = parseInt(values.periodInMonths);

    if (isNaN(amount) || amount <= 0) {
      setError("Amount must be greater than 0.");
      return;
    }
    if (isNaN(period) || period < 3 || period > 60) {
      setError("Period must be between 3 and 60 months.");
      return;
    }

    setLoading(true);
    try {
      await onSubmit({
        loanType: values.loanType,
        amount,
        currency: values.currency,
        periodInMonths: period,
      });
    } catch (err: any) {
      const data = err.response?.data;
      setError(data?.Message ?? data?.message ?? "Something went wrong.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="max-w-md space-y-5">
      <div>
        <label className="block text-xs uppercase tracking-wider text-muted mb-1">Loan type</label>
        <select
          value={values.loanType}
          onChange={(e) => update("loanType", e.target.value as LoanType)}
          className="w-full border-b border-line bg-transparent py-2 focus:outline-none focus:border-ink"
        >
          <option value="Fast">Fast loan</option>
          <option value="Auto">Auto loan</option>
          <option value="Installment">Installment</option>
        </select>
      </div>

      <div>
        <label className="block text-xs uppercase tracking-wider text-muted mb-1">Amount</label>
        <input
          type="number"
          value={values.amount}
          onChange={(e) => update("amount", e.target.value)}
          min={1}
          className="w-full border-b border-line bg-transparent py-2 focus:outline-none focus:border-ink"
        />
      </div>

      <div>
        <label className="block text-xs uppercase tracking-wider text-muted mb-1">Currency</label>
        <select
          value={values.currency}
          onChange={(e) => update("currency", e.target.value as Currency)}
          className="w-full border-b border-line bg-transparent py-2 focus:outline-none focus:border-ink"
        >
          <option value="GEL">GEL (₾)</option>
          <option value="USD">USD ($)</option>
          <option value="EUR">EUR (€)</option>
        </select>
      </div>

      <div>
        <label className="block text-xs uppercase tracking-wider text-muted mb-1">Period (months)</label>
        <input
          type="number"
          value={values.periodInMonths}
          onChange={(e) => update("periodInMonths", e.target.value)}
          min={3}
          max={60}
          className="w-full border-b border-line bg-transparent py-2 focus:outline-none focus:border-ink"
        />
      </div>

      {error && <p className="text-sm text-accent">{error}</p>}

      <button
        type="submit"
        disabled={loading}
        className="border border-ink py-2 px-6 hover:bg-ink hover:text-bg transition disabled:opacity-50"
      >
        {loading ? "Saving…" : submitLabel}
      </button>
    </form>
  );
}