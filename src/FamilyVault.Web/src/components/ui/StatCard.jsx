import React from "react";
export default function StatCard({ title, value, hint }) {
  return (
    <article className="stat-card">
      <small>{title}</small>
      <h3>{value}</h3>
      <p>{hint}</p>
    </article>
  );
}

