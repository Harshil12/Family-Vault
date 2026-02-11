import React from "react";
import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { useTheme } from "../context/ThemeContext";
import { LogoutIcon, MoonIcon, SunIcon } from "./ui/Icons";

const links = [
  { to: "/", label: "Dashboard", end: true },
  { to: "/families", label: "Family Profiles" },
  { to: "/audit", label: "Audit" }
];

export default function AppLayout() {
  const { userId, logout, isPreviewMode } = useAuth();
  const { isDark, toggleTheme } = useTheme();

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <h1 className="brand">Family Vault</h1>
        <p className="subtle">{isPreviewMode ? "Preview mode (login skipped)." : "Your family’s secure records hub."}</p>
        <nav className="nav-links">
          {links.map((link) => (
            <NavLink
              key={link.to}
              to={link.to}
              end={link.end}
              className={({ isActive }) => (isActive ? "nav-link active" : "nav-link")}
            >
              {link.label}
            </NavLink>
          ))}
        </nav>
        <div className="sidebar-footer">
          <button type="button" className="btn ghost theme-toggle" onClick={toggleTheme}>
            <span className="btn-icon">{isDark ? <SunIcon /> : <MoonIcon />}</span>
            <span>{isDark ? "Light Theme" : "Dark Theme"}</span>
          </button>
          <small className="subtle">User ID: {userId ?? "preview-user"}</small>
          {!isPreviewMode && (
            <button type="button" className="btn ghost" onClick={logout}>
              <span className="btn-icon"><LogoutIcon /></span>
              <span>Logout</span>
            </button>
          )}
        </div>
      </aside>

      <main className="content">
        <Outlet />
      </main>
    </div>
  );
}


