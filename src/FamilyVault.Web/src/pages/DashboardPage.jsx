import React, { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import StatCard from "../components/ui/StatCard";
import HelpTip from "../components/ui/HelpTip";
import { ArrowRightIcon } from "../components/ui/Icons";
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

function daysBetween(fromDate, toDate) {
  const msPerDay = 1000 * 60 * 60 * 24;
  const from = new Date(fromDate.getFullYear(), fromDate.getMonth(), fromDate.getDate());
  const to = new Date(toDate.getFullYear(), toDate.getMonth(), toDate.getDate());
  return Math.floor((to - from) / msPerDay);
}

export default function DashboardPage() {
  const { token, userId, isPreviewMode } = useAuth();
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
      if (isPreviewMode) {
        setSummary({
          families: [{ id: "preview-family" }],
          members: [{ id: "preview-member-1" }, { id: "preview-member-2" }],
          documents: [{ id: "preview-doc-1", documentNumber: "P1234567", expiryDate: "2030-01-01T00:00:00Z" }],
          bankAccounts: [{ id: "preview-account-1" }]
        });
        setLoading(false);
        return;
      }
      if (!userId) {
        setLoading(false);
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
  }, [token, userId, isPreviewMode]);

  const timelineItems = useMemo(() => {
    const now = new Date();
    const next60 = new Date();
    next60.setDate(now.getDate() + 60);

    const documentEvents = summary.documents
      .filter((doc) => doc.expiryDate)
      .map((doc) => {
        const expiryDate = new Date(doc.expiryDate);
        const dayDiff = daysBetween(now, expiryDate);
        const category = dayDiff <= 30 ? "Expiring Soon" : "Renewal";
        return {
          id: `doc-${doc.id}`,
          date: expiryDate,
          title: `Document: ${doc.documentNumber}`,
          category
        };
      })
      .filter((event) => event.date >= now && event.date <= next60);

    const birthdayEvents = summary.members
      .filter((member) => member.dateOfBirth)
      .map((member) => {
        const dob = new Date(member.dateOfBirth);
        const nextBirthday = new Date(now.getFullYear(), dob.getMonth(), dob.getDate());
        if (nextBirthday < now) {
          nextBirthday.setFullYear(nextBirthday.getFullYear() + 1);
        }

        return {
          id: `dob-${member.id}`,
          date: nextBirthday,
          title: `Birthday: ${member.firstName} ${member.lastName || ""}`.trim(),
          category: "Birthday"
        };
      })
      .filter((event) => event.date >= now && event.date <= next60);

    return [...documentEvents, ...birthdayEvents]
      .sort((a, b) => a.date - b.date)
      .slice(0, 8);
  }, [summary.documents, summary.members]);

  const expiringSoonCount = timelineItems.filter((item) => item.category === "Expiring Soon").length;

  if (loading) {
    return <p>Loading dashboard...</p>;
  }

  return (
    <section>
      <header className="page-header">
        <div>
          <div className="heading-with-help">
            <h2>Dashboard</h2>
            <HelpTip text="This summary rolls up families, members, documents, and bank records linked to your account." />
          </div>
          <p className="subtle page-intro">A clear view of your family’s members, documents, and financial details.</p>
        </div>
        <Link className="btn" to="/families">
          <span>Open Family Profiles</span>
          <span className="btn-icon"><ArrowRightIcon /></span>
        </Link>
      </header>

      {error && <p className="error-text">{error}</p>}
      {isPreviewMode && <p className="subtle">Preview mode is on. Login to load real API data.</p>}

      <div className="stats-grid">
        <StatCard title="Families" value={summary.families.length} hint="Households onboarded" />
        <StatCard title="Family Members" value={summary.members.length} hint="People in your vault" />
        <StatCard title="Documents" value={summary.documents.length} hint="Identity and legal records" />
        <StatCard title="Bank Accounts" value={summary.bankAccounts.length} hint="Financial profiles tracked" />
      </div>

      <section className="panel">
        <div className="panel-head">
          <h3>Heads Up, Family! Don&apos;t Miss These Dates (60-Day View)</h3>
          <span className="timeline-chip">{expiringSoonCount} expiring soon</span>
        </div>
        {!timelineItems.length ? (
          <p className="empty-state">No upcoming events in next 60 days.</p>
        ) : (
          <ul className="timeline-list">
            {timelineItems.map((item) => (
              <li key={item.id}>
                <span
                  className={`timeline-dot ${
                    item.category === "Birthday"
                      ? "birthday"
                      : item.category === "Expiring Soon"
                        ? "expiring"
                        : "renewal"
                  }`}
                />
                <div>
                  <strong>{item.title}</strong>
                  <p className="subtle">
                    {item.category} | {formatDate(item.date)}
                  </p>
                </div>
              </li>
            ))}
          </ul>
        )}
      </section>
    </section>
  );
}


