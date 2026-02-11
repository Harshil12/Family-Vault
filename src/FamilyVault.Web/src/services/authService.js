import { apiRequest } from "./apiClient";

export async function loginRequest(credentials) {
  return apiRequest("/login", {
    method: "POST",
    payload: credentials
  });
}

export async function signUpRequest(payload) {
  return apiRequest("/register", {
    method: "POST",
    payload
  });
}
