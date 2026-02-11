import React from "react";
import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

const links = [
  { to: "/", label: "Dashboard", end: true },
  { to: "/families", label: "Families" }
];

export default function AppLayout() {
  const { userId, logout } = useAuth();

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <h1 className="brand">FamilyVault</h1>
        <p className="subtle">Protect documents and financial records.</p>
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
          <small className="subtle">User ID: {userId ?? "unknown"}</small>
          <button type="button" className="btn ghost" onClick={logout}>
            Logout
          </button>
        </div>
      </aside>

      <main className="content">
        <Outlet />
      </main>
    </div>
  );
}

