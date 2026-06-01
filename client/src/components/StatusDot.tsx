import type { LoanStatus } from "../types/api";

const config: Record<LoanStatus, { color: string; label: string }> = {
  InProcess: { color: "#C99A2E", label: "In process" },
  Approved: { color: "#3A7D44", label: "Approved" },
  Rejected: { color: "#9B3B3B", label: "Rejected" },
};

export default function StatusDot({ status }: { status: LoanStatus }) {
  const { color, label } = config[status];
  return (
    <span className="inline-flex items-center gap-2">
      <span className="inline-block w-2 h-2 rounded-full" style={{ backgroundColor: color }} />
      <span>{label}</span>
    </span>
  );
}