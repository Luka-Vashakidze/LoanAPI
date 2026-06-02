import { createContext, useContext, useState, useEffect,  type ReactNode } from "react";

interface AuthState {
  token: string | null;
  userId: number | null;
  username: string | null;
  role: "User" | "Accountant" | null;
}

interface AuthContextValue extends AuthState {
  initializing: boolean;
  login: (token: string) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function decodeToken(token: string): Omit<AuthState, "token"> {
  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    return {
      userId: parseInt(payload["nameid"] ?? payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"]),
      username: payload["unique_name"] ?? payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"],
      role: payload["role"] ?? payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"],
    };
  } catch {
    return { userId: null, username: null, role: null };
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>({
    token: null,
    userId: null,
    username: null,
    role: null,
  });
  const [initializing, setInitializing] = useState(true);
  useEffect(() => {
    const token = localStorage.getItem("token");
    if (token) {
      setState({ token, ...decodeToken(token) });
    }
    setInitializing(false);
  }, []);

  const login = (token: string) => {
    localStorage.setItem("token", token);
    setState({ token, ...decodeToken(token) });
  };

  const logout = () => {
    localStorage.removeItem("token");
    setState({ token: null, userId: null, username: null, role: null });
  };

  return (
    <AuthContext.Provider value={{ ...state, initializing, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used inside AuthProvider");
  return ctx;
}