import React from "react";
import { Link } from "react-router-dom";

export default function NotFoundPage() {
  return (
    <div className="auth-shell">
      <section className="auth-card">
        <h1>Page not found</h1>
        <p>This route does not exist.</p>
        <Link to="/" className="btn">
          Go Home
        </Link>
      </section>
    </div>
  );
}

