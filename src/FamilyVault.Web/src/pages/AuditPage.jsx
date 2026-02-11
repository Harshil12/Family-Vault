import React, { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";
import { exportAuditReport, getActivityLog, getDownloadHistory } from "../services/auditService";

function formatDateTime(value) {
  if (!value) {
    return "-";
  }
  return new Date(value).toLocaleString();
}

function AuditTable({ title, rows, emptyMessage }) {
  return (
    <section className="panel">
      <div className="panel-head">
        <h3>{title}</h3>
      </div>
      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              <th>When</th>
              <th>Action</th>
              <th>Entity</th>
              <th>Description</th>
              <th>By</th>
            </tr>
          </thead>
          <tbody>
            {rows.length ? (
              rows.map((row) => (
                <tr key={row.id}>
                  <td>{formatDateTime(row.createdAt)}</td>
                  <td>{row.action}</td>
                  <td>{row.entityType}</td>
                  <td>{row.description || "-"}</td>
                  <td>{row.createdBy || row.userId}</td>
                </tr>
              ))
            ) : (
              <tr>
                <td className="empty-row" colSpan={5}>
                  {emptyMessage}
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </section>
  );
}

export default function AuditPage() {
  const { token, isPreviewMode } = useAuth();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [activityRows, setActivityRows] = useState([]);
  const [downloadRows, setDownloadRows] = useState([]);
  const [days, setDays] = useState("30");

  const loadAudit = async () => {
    if (isPreviewMode) {
      const now = new Date().toISOString();
      setActivityRows([
        { id: "a1", createdAt: now, action: "Create", entityType: "Family", description: "Created family Demo Family", createdBy: "preview-user" },
        { id: "a2", createdAt: now, action: "Update", entityType: "Document", description: "Updated document P1234567", createdBy: "preview-user" }
      ]);
      setDownloadRows([
        { id: "d1", createdAt: now, action: "Download", entityType: "Document", description: "Downloaded file for document P1234567", createdBy: "preview-user" }
      ]);
      setLoading(false);
      return;
    }

    setError("");
    setLoading(true);
    try {
      const windowDays = Number(days) || 30;
      const [activity, downloads] = await Promise.all([
        getActivityLog(token, { days: windowDays, take: 300 }),
        getDownloadHistory(token, { days: windowDays, take: 300 })
      ]);
      setActivityRows(activity);
      setDownloadRows(downloads);
    } catch (requestError) {
      setError(requestError.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadAudit();
  }, [token, isPreviewMode]);

  const handleRefresh = async () => {
    await loadAudit();
  };

  const handleExport = async () => {
    try {
      await exportAuditReport(token, { days: Number(days) || 30 });
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  return (
    <section>
      <header className="page-header">
        <div>
          <h2>Audit & Activity</h2>
          <p className="subtle page-intro">Track who changed records, file downloads, and export audit reports.</p>
        </div>
      </header>

      {error && <p className="error-text">{error}</p>}
      <div className="toolbar">
        <select value={days} onChange={(event) => setDays(event.target.value)}>
          <option value="7">Last 7 days</option>
          <option value="30">Last 30 days</option>
          <option value="60">Last 60 days</option>
          <option value="90">Last 90 days</option>
        </select>
        <button type="button" className="btn ghost" onClick={handleRefresh}>
          Refresh
        </button>
        <button type="button" className="btn" onClick={handleExport} disabled={isPreviewMode}>
          Export Audit Report
        </button>
      </div>

      {loading ? (
        <p>Loading audit logs...</p>
      ) : (
        <>
          <AuditTable title="Activity Log" rows={activityRows} emptyMessage="No activity available for selected period." />
          <AuditTable title="Document Download History" rows={downloadRows} emptyMessage="No document downloads found for selected period." />
        </>
      )}
    </section>
  );
}
