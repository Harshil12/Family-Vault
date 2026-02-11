export function unwrapData(response, fallback = []) {
  if (!response) {
    return fallback;
  }

  if (response.data !== undefined) {
    return response.data ?? fallback;
  }

  return response;
}
