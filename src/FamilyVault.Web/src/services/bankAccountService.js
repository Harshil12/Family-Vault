import { apiRequest } from "./apiClient";

export async function getBankAccounts(memberId, token) {
  return apiRequest(`/bankaccounts/${memberId}`, { token });
}

export async function createBankAccount(memberId, payload, token) {
  return apiRequest(`/bankaccounts/${memberId}/bankaccounts`, {
    method: "POST",
    token,
    payload
  });
}

export async function updateBankAccount(memberId, id, payload, token) {
  return apiRequest(`/bankaccounts/${memberId}/bankaccounts/${id}`, {
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
  return apiRequest(`/bankaccounts/${memberId}/${id}`, {
    method: "DELETE",
    token
  });
}
