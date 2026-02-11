import { apiRequest } from "./apiClient";
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7037";

export async function getDocuments(memberId, token) {
  return apiRequest(`/documents/${memberId}`, { token });
}

export async function createDocument(memberId, payload, token) {
  return apiRequest(`/documents/${memberId}/documents`, {
    method: "POST",
    token,
    payload
  });
}

export async function uploadDocument(memberId, payload, token) {
  const formData = new FormData();
  formData.append("file", payload.file);
  formData.append("documentType", String(payload.documentType));
  formData.append("documentNumber", payload.documentNumber);
  if (payload.issueDate) {
    formData.append("issueDate", payload.issueDate);
  }
  if (payload.expiryDate) {
    formData.append("expiryDate", payload.expiryDate);
  }

  return apiRequest(`/documents/${memberId}/documents/upload`, {
    method: "POST",
    token,
    payload: formData
  });
}

export async function replaceDocumentFile(memberId, documentId, payload, token) {
  const formData = new FormData();
  formData.append("file", payload.file);
  if (payload.documentType !== undefined && payload.documentType !== null) {
    formData.append("documentType", String(payload.documentType));
  }
  if (payload.documentNumber) {
    formData.append("documentNumber", payload.documentNumber);
  }
  if (payload.issueDate) {
    formData.append("issueDate", payload.issueDate);
  }
  if (payload.expiryDate) {
    formData.append("expiryDate", payload.expiryDate);
  }

  return apiRequest(`/documents/${memberId}/documents/${documentId}/file`, {
    method: "PUT",
    token,
    payload: formData
  });
}

export async function updateDocument(memberId, id, payload, token) {
  return apiRequest(`/documents/${memberId}/documents/${id}`, {
    method: "PUT",
    token,
    payload: {
      ...payload,
      id,
      familyMemberId: memberId
    }
  });
}

export async function deleteDocument(memberId, id, token) {
  return apiRequest(`/documents/${memberId}/${id}`, {
    method: "DELETE",
    token
  });
}

function getDocumentFileUrl(memberId, documentId, mode = "preview") {
  const download = mode === "download" ? "true" : "false";
  return `${API_BASE_URL}/documents/${memberId}/${documentId}/file?download=${download}`;
}

async function fetchDocumentBlob(memberId, documentId, token, mode = "preview") {
  const response = await fetch(getDocumentFileUrl(memberId, documentId, mode), {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });

  if (!response.ok) {
    throw new Error(`Unable to open file (${response.status})`);
  }

  const blob = await response.blob();
  return { blob, contentType: response.headers.get("content-type") ?? "application/octet-stream" };
}

export async function previewDocumentFile(memberId, documentId, token) {
  const { blob, contentType } = await fetchDocumentBlob(memberId, documentId, token, "preview");
  const objectUrl = URL.createObjectURL(blob);
  return { objectUrl, contentType };
}

export async function downloadDocumentFile(memberId, documentId, token, suggestedName = "document") {
  const { blob, contentType } = await fetchDocumentBlob(memberId, documentId, token, "download");
  const extension = contentType.includes("pdf")
    ? ".pdf"
    : contentType.includes("word")
      ? ".docx"
      : contentType.includes("sheet")
        ? ".xlsx"
        : contentType.includes("image/")
          ? `.${contentType.split("/")[1]}`
          : "";

  const objectUrl = URL.createObjectURL(blob);
  const anchor = document.createElement("a");
  anchor.href = objectUrl;
  anchor.download = `${suggestedName}${extension}`;
  document.body.appendChild(anchor);
  anchor.click();
  anchor.remove();
  URL.revokeObjectURL(objectUrl);
}
