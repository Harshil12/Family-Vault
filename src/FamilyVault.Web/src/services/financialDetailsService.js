import { apiRequest } from "./apiClient";

export async function getFinancialDetails(memberId, category, token) {
  return apiRequest(`/financial-details/${memberId}/${category}`, { token });
}

export async function createFinancialDetail(memberId, category, payload, token) {
  return apiRequest(`/financial-details/${memberId}/${category}`, {
    method: "POST",
    token,
    payload
  });
}

export async function updateFinancialDetail(memberId, category, id, payload, token) {
  return apiRequest(`/financial-details/${memberId}/${category}/${id}`, {
    method: "PUT",
    token,
    payload
  });
}

export async function deleteFinancialDetail(memberId, category, id, token) {
  return apiRequest(`/financial-details/${memberId}/${category}/${id}`, {
    method: "DELETE",
    token
  });
}
