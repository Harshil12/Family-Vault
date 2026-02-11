import React from "react";
import { Navigate, Route, Routes } from "react-router-dom";
import AppLayout from "./components/AppLayout";
import ProtectedRoute from "./components/ProtectedRoute";
import DashboardPage from "./pages/DashboardPage";
import FamiliesPage from "./pages/FamiliesPage";
import FamilyMembersPage from "./pages/FamilyMembersPage";
import DocumentsPage from "./pages/DocumentsPage";
import BankAccountsPage from "./pages/BankAccountsPage";
import AuditPage from "./pages/AuditPage";
import LoginPage from "./pages/LoginPage";
import SignUpPage from "./pages/SignUpPage";
import NotFoundPage from "./pages/NotFoundPage";

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/signup" element={<SignUpPage />} />
      <Route
        path="/"
        element={
          <ProtectedRoute>
            <AppLayout />
          </ProtectedRoute>
        }
      >
        <Route index element={<DashboardPage />} />
        <Route path="families" element={<FamiliesPage />} />
        <Route path="audit" element={<AuditPage />} />
        <Route path="families/:familyId/members" element={<FamilyMembersPage />} />
        <Route path="families/:familyId/members/:memberId/documents" element={<DocumentsPage />} />
        <Route path="families/:familyId/members/:memberId/accounts" element={<BankAccountsPage />} />
      </Route>
      <Route path="*" element={<NotFoundPage />} />
      <Route path="/home" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

