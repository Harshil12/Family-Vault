import React, { useEffect, useState } from "react";
import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { useTheme } from "../context/ThemeContext";
import { LogoutIcon, MoonIcon, SunIcon } from "./ui/Icons";
import logo from "../assets/familyvault-logo-option-4.svg";

const links = [
  { to: "/", label: "Dashboard", end: true },
  { to: "/families", label: "Family Profiles" },
  { to: "/audit", label: "Audit" }
];

export default function AppLayout() {
  const { userId, logout, isPreviewMode } = useAuth();
  const { isDark, toggleTheme } = useTheme();
  const [isSidebarVisible, setIsSidebarVisible] = useState(true);

  useEffect(() => {
    const stored = window.localStorage.getItem("fv_sidebar_visible");
    if (stored === "false") {
      setIsSidebarVisible(false);
    }
  }, []);

  useEffect(() => {
    window.localStorage.setItem("fv_sidebar_visible", String(isSidebarVisible));
  }, [isSidebarVisible]);

  return (
    <div className={`app-shell ${isSidebarVisible ? "" : "sidebar-hidden"}`}>
      <aside className="sidebar" aria-hidden={!isSidebarVisible}>
        <div className="brand-wrap">
          <img className="brand-logo" src={logo} alt="Family Vault" />
        </div>
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

      <button
        type="button"
        className="panel-arrow-toggle"
        onClick={() => setIsSidebarVisible((current) => !current)}
        aria-label={isSidebarVisible ? "Hide side panel" : "Show side panel"}
        title={isSidebarVisible ? "Hide side panel" : "Show side panel"}
      >
        <span className="panel-arrow-glyph" aria-hidden="true">
          {isSidebarVisible ? "❮" : "❯"}
        </span>
      </button>

      <main className="content">
        <Outlet />
      </main>
    </div>
  );
}


