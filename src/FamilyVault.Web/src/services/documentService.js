import { apiRequest } from "./apiClient";

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
