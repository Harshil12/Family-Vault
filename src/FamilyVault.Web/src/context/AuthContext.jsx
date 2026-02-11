import React, { createContext, useContext, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getUserIdFromToken } from "../utils/jwt";

const TOKEN_KEY = "familyvault_token";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [token, setToken] = useState(() => localStorage.getItem(TOKEN_KEY) ?? "");
  const navigate = useNavigate();

  const userId = useMemo(() => getUserIdFromToken(token), [token]);

  const login = (nextToken) => {
    localStorage.setItem(TOKEN_KEY, nextToken);
    setToken(nextToken);
  };

  const logout = () => {
    localStorage.removeItem(TOKEN_KEY);
    setToken("");
    navigate("/login", { replace: true });
  };

  const value = {
    token,
    userId,
    isAuthenticated: Boolean(token),
    login,
    logout
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);

  if (!ctx) {
    throw new Error("useAuth must be used inside AuthProvider");
  }

  return ctx;
}

