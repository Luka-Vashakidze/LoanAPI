import type { Currency, LoanType } from "../types/api";

const currencySymbol: Record<Currency, string> = {
  GEL: "₾",
  USD: "$",
  EUR: "€",
};

export function formatMoney(amount: number, currency: Currency): string {
  return `${currencySymbol[currency]}${amount.toLocaleString()}`;
}

const loanTypeLabel: Record<LoanType, string> = {
  Fast: "Fast loan",
  Auto: "Auto loan",
  Installment: "Installment",
};

export function formatLoanType(type: LoanType): string {
  return loanTypeLabel[type];
}