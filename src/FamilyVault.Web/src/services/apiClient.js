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
    throw new Error(getErrorMessage(responseData, `Request failed (${response.status})`));
  }

  return responseData;
}
