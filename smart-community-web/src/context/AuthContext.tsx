import { createContext, useContext, useState, type ReactNode } from "react";

export interface AuthUser {
  userId: number;
  userName: string;
  unitNumber: string;
  isAdmin: boolean;
}

interface AuthContextType {
  user: AuthUser | null;
  token: string | null;
  login: (user: AuthUser, token: string) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(() => {
    const stored = localStorage.getItem("sc_user");
    return stored ? (JSON.parse(stored) as AuthUser) : null;
  });

  const [token, setToken] = useState<string | null>(() =>
    localStorage.getItem("sc_token")
  );

  const login = (user: AuthUser, token: string) => {
    localStorage.setItem("sc_user", JSON.stringify(user));
    localStorage.setItem("sc_token", token);
    setUser(user);
    setToken(token);
  };

  const logout = () => {
    localStorage.removeItem("sc_user");
    localStorage.removeItem("sc_token");
    setUser(null);
    setToken(null);
  };

  return (
    <AuthContext.Provider value={{ user, token, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextType {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}
