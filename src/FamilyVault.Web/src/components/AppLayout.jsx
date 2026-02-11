import React from "react";
import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { LogoutIcon } from "./ui/Icons";

const links = [
  { to: "/", label: "Dashboard", end: true },
  { to: "/families", label: "Families" }
];

export default function AppLayout() {
  const { userId, logout, isPreviewMode } = useAuth();

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <h1 className="brand">FamilyVault</h1>
        <p className="subtle">{isPreviewMode ? "Preview mode (login skipped)." : "Protect documents and financial records."}</p>
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

