import React from "react";
export default function StatCard({ title, value, hint }) {
  return (
    <article className="stat-card">
      <div className="stat-card-head">
        <small className="stat-label">{title}</small>
      </div>
      <h3 className="stat-value">{value}</h3>
      <p className="stat-hint">{hint}</p>
    </article>
  );
}

