import React, { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import StatCard from "../components/ui/StatCard";
import { useAuth } from "../context/AuthContext";
import { getFamilies } from "../services/familyService";
import { getFamilyMembers } from "../services/familyMemberService";
import { getDocuments } from "../services/documentService";
import { getBankAccounts } from "../services/bankAccountService";
import { unwrapData } from "../utils/response";

function formatDate(value) {
  if (!value) {
    return "-";
  }

  return new Date(value).toLocaleDateString();
}

export default function DashboardPage() {
  const { token, userId } = useAuth();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [summary, setSummary] = useState({
    families: [],
    members: [],
    documents: [],
    bankAccounts: []
  });

  useEffect(() => {
    const loadDashboard = async () => {
      if (!userId) {
        setLoading(false);
        setError("JWT token does not contain user id in sub claim.");
        return;
      }

      setError("");
      setLoading(true);

      try {
        const familyResponse = await getFamilies(userId, token);
        const families = unwrapData(familyResponse);

        const membersPerFamily = await Promise.all(
          families.map(async (family) => {
            const memberResponse = await getFamilyMembers(family.id, token);
            return unwrapData(memberResponse);
          })
        );

        const members = membersPerFamily.flat();

        const documentPerMember = await Promise.all(
          members.map(async (member) => {
            const docsResponse = await getDocuments(member.id, token);
            return unwrapData(docsResponse);
          })
        );

        const bankAccountPerMember = await Promise.all(
          members.map(async (member) => {
            const bankResponse = await getBankAccounts(member.id, token);
            return unwrapData(bankResponse);
          })
        );

        setSummary({
          families,
          members,
          documents: documentPerMember.flat(),
          bankAccounts: bankAccountPerMember.flat()
        });
      } catch (requestError) {
        setError(requestError.message);
      } finally {
        setLoading(false);
      }
    };

    loadDashboard();
  }, [token, userId]);

  const expiringDocuments = useMemo(() => {
    const now = new Date();
    const next30Days = new Date(now);
    next30Days.setDate(now.getDate() + 30);

    return summary.documents.filter((doc) => {
      if (!doc.expiryDate) {
        return false;
      }
      const expiry = new Date(doc.expiryDate);
      return expiry >= now && expiry <= next30Days;
    });
  }, [summary.documents]);

  if (loading) {
    return <p>Loading dashboard...</p>;
  }

  return (
    <section>
      <header className="page-header">
        <div>
          <h2>Dashboard</h2>
          <p className="subtle">A quick view of your family profiles, IDs, and bank records.</p>
        </div>
        <Link className="btn" to="/families">
          Manage Families
        </Link>
      </header>

      {error && <p className="error-text">{error}</p>}

      <div className="stats-grid">
        <StatCard title="Families" value={summary.families.length} hint="Households onboarded" />
        <StatCard title="Family Members" value={summary.members.length} hint="People in your vault" />
        <StatCard title="Documents" value={summary.documents.length} hint="Identity and legal records" />
        <StatCard title="Bank Accounts" value={summary.bankAccounts.length} hint="Financial profiles tracked" />
      </div>

      <section className="panel">
        <h3>Documents expiring in 30 days</h3>
        {!expiringDocuments.length ? (
          <p className="empty-state">No near-expiry documents.</p>
        ) : (
          <ul className="simple-list">
            {expiringDocuments.map((doc) => (
              <li key={doc.id}>
                <strong>{doc.documentNumber}</strong>
                <span>Expiry: {formatDate(doc.expiryDate)}</span>
              </li>
            ))}
          </ul>
        )}
      </section>
    </section>
  );
}

