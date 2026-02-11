import { apiRequest } from "./apiClient";

export async function getFamilies(userId, token) {
  return apiRequest(`/family/${userId}`, { token });
}

export async function createFamily(payload, userId, token) {
  return apiRequest(`/family/${userId}/family`, {
    method: "POST",
    token,
    payload
  });
}

export async function updateFamily(id, payload, userId, token) {
  return apiRequest(`/family/${userId}/family/${id}`, {
    method: "PUT",
    token,
    payload: {
      ...payload,
      id
    }
  });
}

export async function deleteFamily(id, userId, token) {
  return apiRequest(`/family/${userId}/${id}`, {
    method: "DELETE",
    token
  });
}
