# FamilyVault.Web Repo Split Checklist

Use this when moving `src/FamilyVault.Web` into a separate repository.

## 1. Prepare UI project

1. Ensure `.env.example` is current and includes `VITE_API_BASE_URL`.
2. Ensure `README.md` has run steps (`npm install`, `npm run dev`, `npm run build`).
3. Keep API endpoint list in `README.md` aligned with backend routes.
4. Keep Postman files in `postman/` updated.

## 2. Define API contract boundary

1. Freeze endpoint paths used by UI:
   1. `/register`
   2. `/login`
   3. `/family/{userId}/*`
   4. `/familymember/{familyId}/*`
   5. `/documents/{familyMemberId}/*`
   6. `/bankaccounts/{familyMemberId}/*`
2. Confirm auth contract:
   1. JWT `sub` claim must be user id.
   2. Protected APIs require `Authorization: Bearer <token>`.
3. Confirm response envelope compatibility (`ApiResponse<T>` for protected CRUD).

## 3. Extract repository

1. Copy/move `src/FamilyVault.Web` to new repo root.
2. Preserve `.gitignore`, `package.json`, `vite.config.js`, `postman/`.
3. Initialize git history for new repo.

## 4. CI/CD for UI repo

1. Add pipeline for:
   1. `npm ci`
   2. `npm run build`
2. Publish static artifacts from `dist/`.
3. Set environment variable per stage:
   1. Dev `VITE_API_BASE_URL`
   2. QA `VITE_API_BASE_URL`
   3. Prod `VITE_API_BASE_URL`

## 5. CORS and deployment alignment

1. Add deployed UI origin to backend `Cors:AllowedOrigins`.
2. Keep `Cors:AllowedMethods` consistent with UI verbs (`GET, POST, PUT, DELETE`).
3. Verify HTTPS base URL in production.

## 6. Versioning and release flow

1. Tag UI releases independently.
2. Track backend compatibility per UI release in release notes.
3. If breaking API changes happen, version API or ship backward compatibility.

## 7. Post-split smoke test

1. Register user.
2. Login user.
3. Create family.
4. Create family member.
5. Create document.
6. Create bank account.
7. Verify dashboard counts and expiry list.
