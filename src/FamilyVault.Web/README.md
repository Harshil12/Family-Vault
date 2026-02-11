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
  - Bank Accounts (`/families/:familyId/members/:memberId/accounts`)

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
- `PUT /documents/{familyMemberId}/documents/{id}`
- `DELETE /documents/{familyMemberId}/{id}`
- `GET /bankaccounts/{familyMemberId}`
- `POST /bankaccounts/{familyMemberId}/bankaccounts`
- `PUT /bankaccounts/{familyMemberId}/bankaccounts/{id}`
- `DELETE /bankaccounts/{familyMemberId}/{id}`

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
