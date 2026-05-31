export type LoanType = "Fast" | "Auto" | "Installment";
export type Currency = "GEL" | "USD" | "EUR";
export type LoanStatus = "InProcess" | "Approved" | "Rejected";
export type Role = "User" | "Accountant";

export interface User {
  id: number;
  firstName: string;
  lastName: string;
  userName: string;
  age: number;
  email: string;
  monthlyIncome: number;
  isBlocked: boolean;
  blockedUntil: string | null;
  role: Role;
}

export interface Loan {
  id: number;
  loanType: LoanType;
  amount: number;
  currency: Currency;
  periodInMonths: number;
  status: LoanStatus;
  userId: number;
  user?: User;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  username: string;
  email: string;
  password: string;
  age: number;
  monthlyIncome: number;
}

export interface LoginResponse {
  token: string;
}