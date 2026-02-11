const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7037";

function toJsonBody(payload) {
  return {
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify(payload)
  };
}

function isFormData(value) {
  return typeof FormData !== "undefined" && value instanceof FormData;
}

function getErrorMessage(errorPayload, fallbackMessage) {
  if (!errorPayload) {
    return fallbackMessage;
  }

  if (typeof errorPayload === "string") {
    return errorPayload;
  }

  if (errorPayload.message) {
    return errorPayload.message;
  }

  if (errorPayload.title) {
    return errorPayload.title;
  }

  return fallbackMessage;
}

function toCamelCaseKey(key) {
  if (!key || typeof key !== "string") {
    return key;
  }
  return key.charAt(0).toLowerCase() + key.slice(1);
}

function normalizeFieldErrors(errorPayload) {
  // New API shape from global validation handler
  if (errorPayload?.errorCode === "VALIDATION_ERROR" && errorPayload?.data && typeof errorPayload.data === "object") {
    return Object.entries(errorPayload.data).reduce((acc, [k, v]) => {
      acc[toCamelCaseKey(k)] = Array.isArray(v) ? v[0] : String(v);
      return acc;
    }, {});
  }

  // ASP.NET ValidationProblem fallback shape
  if (errorPayload?.errors && typeof errorPayload.errors === "object") {
    return Object.entries(errorPayload.errors).reduce((acc, [k, v]) => {
      acc[toCamelCaseKey(k)] = Array.isArray(v) ? v[0] : String(v);
      return acc;
    }, {});
  }

  return null;
}

export async function apiRequest(path, { method = "GET", token, payload } = {}) {
  const headers = {
    Accept: "application/json"
  };

  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  const requestInit = {
    method,
    headers
  };

  if (payload !== undefined) {
    if (isFormData(payload)) {
      requestInit.body = payload;
    } else {
      const bodyData = toJsonBody(payload);
      requestInit.headers = { ...requestInit.headers, ...bodyData.headers };
      requestInit.body = bodyData.body;
    }
  }

  const response = await fetch(`${API_BASE_URL}${path}`, requestInit);
  const contentType = response.headers.get("content-type") || "";
  const responseData = contentType.includes("application/json") ? await response.json() : null;

  if (!response.ok) {
    const error = new Error(getErrorMessage(responseData, `Request failed (${response.status})`));
    error.status = response.status;
    error.payload = responseData;
    error.fieldErrors = normalizeFieldErrors(responseData);
    throw error;
  }

  return responseData;
}
