import { apiRequest } from "./apiClient";

export async function getFamilyMembers(familyId, token) {
  return apiRequest(`/familymember/${familyId}`, { token });
}

export async function createFamilyMember(familyId, payload, token) {
  return apiRequest(`/familymember/${familyId}/familymember`, {
    method: "POST",
    token,
    payload
  });
}

export async function updateFamilyMember(familyId, id, payload, token) {
  return apiRequest(`/familymember/${familyId}/familymember/${id}`, {
    method: "PUT",
    token,
    payload: {
      ...payload,
      id,
      familyId
    }
  });
}

export async function deleteFamilyMember(familyId, id, token) {
  return apiRequest(`/familymember/${familyId}/${id}`, {
    method: "DELETE",
    token
  });
}
