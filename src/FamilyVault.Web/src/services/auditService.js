import { apiRequest } from "./apiClient";
import { unwrapData } from "../utils/response";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7037";

export async function getActivityLog(token, { days = 30, take = 200 } = {}) {
  const response = await apiRequest(`/audit/activity?days=${days}&take=${take}`, { token });
  return unwrapData(response);
}

export async function getDownloadHistory(token, { days = 30, take = 200 } = {}) {
  const response = await apiRequest(`/audit/downloads?days=${days}&take=${take}`, { token });
  return unwrapData(response);
}

export async function exportAuditReport(token, { days = 30 } = {}) {
  const response = await fetch(`${API_BASE_URL}/audit/export?days=${days}`, {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });

  if (!response.ok) {
    throw new Error(`Unable to export audit report (${response.status})`);
  }

  const blob = await response.blob();
  const objectUrl = URL.createObjectURL(blob);
  const anchor = document.createElement("a");
  anchor.href = objectUrl;
  anchor.download = `audit-report-${new Date().toISOString().replace(/[:.]/g, "-")}.csv`;
  document.body.appendChild(anchor);
  anchor.click();
  anchor.remove();
  URL.revokeObjectURL(objectUrl);
}
