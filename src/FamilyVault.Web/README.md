# FamilyVault React App

Separate React frontend project for the existing FamilyVault API.

## Implemented flow

- Login page (`/login`)
- Signup page (`/signup`)
- Dashboard first page (`/`) with:
  - Families count
  - Members count
  - Documents count
  - Bank accounts count
  - Expiring documents in next 30 days
- CRUD pages:
  - Families (`/families`)
  - Family Members (`/families/:familyId/members`)
  - Documents (`/families/:familyId/members/:memberId/documents`)
  - Financial Details (`/families/:familyId/members/:memberId/financial-details`)

## API mapping

This app is mapped to current backend routes:

- `POST /login`
- `POST /register`
- `GET /family/{userId}`
- `POST /family/{userId}/family`
- `PUT /family/{userId}/family/{id}`
- `DELETE /family/{userId}/{id}`
- `GET /familymember/{familyId}`
- `POST /familymember/{familyId}/familymember`
- `PUT /familymember/{familyId}/familymember/{id}`
- `DELETE /familymember/{familyId}/{id}`
- `GET /documents/{familyMemberId}`
- `POST /documents/{familyMemberId}/documents`
- `POST /documents/{familyMemberId}/documents/upload` (multipart/form-data)
- `GET /documents/{familyMemberId}/{id}/file?download=true|false`
- `PUT /documents/{familyMemberId}/documents/{id}/file` (multipart/form-data, replace file)
- `PUT /documents/{familyMemberId}/documents/{id}`
- `DELETE /documents/{familyMemberId}/{id}`
- `GET /financial-details/{familyMemberId}/bank-accounts`
- `POST /financial-details/{familyMemberId}/bank-accounts`
- `PUT /financial-details/{familyMemberId}/bank-accounts/{id}`
- `DELETE /financial-details/{familyMemberId}/bank-accounts/{id}`
- `GET /financial-details/{familyMemberId}/fd`
- `POST /financial-details/{familyMemberId}/fd`
- `PUT /financial-details/{familyMemberId}/fd/{id}`
- `DELETE /financial-details/{familyMemberId}/fd/{id}`
- `GET /financial-details/{familyMemberId}/life-insurance`
- `POST /financial-details/{familyMemberId}/life-insurance`
- `PUT /financial-details/{familyMemberId}/life-insurance/{id}`
- `DELETE /financial-details/{familyMemberId}/life-insurance/{id}`
- `GET /financial-details/{familyMemberId}/mediclaim`
- `POST /financial-details/{familyMemberId}/mediclaim`
- `PUT /financial-details/{familyMemberId}/mediclaim/{id}`
- `DELETE /financial-details/{familyMemberId}/mediclaim/{id}`
- `GET /financial-details/{familyMemberId}/demat-accounts`
- `POST /financial-details/{familyMemberId}/demat-accounts`
- `PUT /financial-details/{familyMemberId}/demat-accounts/{id}`
- `DELETE /financial-details/{familyMemberId}/demat-accounts/{id}`
- `GET /financial-details/{familyMemberId}/mutual-funds`
- `POST /financial-details/{familyMemberId}/mutual-funds`
- `PUT /financial-details/{familyMemberId}/mutual-funds/{id}`
- `DELETE /financial-details/{familyMemberId}/mutual-funds/{id}`

## Setup

1. Copy `.env.example` to `.env` and adjust `VITE_API_BASE_URL`.
2. Install dependencies:

```bash
npm install
```

3. Run app:

```bash
npm run dev
```

## Important backend note

`/register` is anonymous for self-signup.
`/User/*` routes remain protected and require JWT.

## Document Upload

- Allowed file types: PDF, Word (`.doc/.docx`), Excel (`.xls/.xlsx`), images (`.png/.jpg/.jpeg/.gif/.bmp/.webp`)
- Max file size: 10 MB
- Files are saved on server under:
  - `uploads/{userId}/{familyName}/{generatedFileName}`
- Relative path is stored in document `SavedLocation`.
