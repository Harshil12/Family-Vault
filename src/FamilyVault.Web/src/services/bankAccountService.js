import { apiRequest } from "./apiClient";

export async function getBankAccounts(memberId, token) {
  return apiRequest(`/financial-details/${memberId}/bank-accounts`, { token });
}

export async function createBankAccount(memberId, payload, token) {
  return apiRequest(`/financial-details/${memberId}/bank-accounts`, {
    method: "POST",
    token,
    payload
  });
}

export async function updateBankAccount(memberId, id, payload, token) {
  return apiRequest(`/financial-details/${memberId}/bank-accounts/${id}`, {
    method: "PUT",
    token,
    payload: {
      ...payload,
      id,
      familyMemberId: memberId
    }
  });
}

export async function deleteBankAccount(memberId, id, token) {
  return apiRequest(`/financial-details/${memberId}/bank-accounts/${id}`, {
    method: "DELETE",
    token
  });
}
