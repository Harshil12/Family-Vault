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
import { getFinancialDetails } from "../services/financialDetailsService";
import { unwrapData } from "../utils/response";

const FINANCIAL_CATEGORIES = ["bank-accounts", "fd", "life-insurance", "mediclaim", "demat-accounts", "mutual-funds"];

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

function isBlank(value) {
  return value == null || String(value).trim() === "";
}

function toDisplayMemberName(member) {
  const fullName = `${member.firstName || ""} ${member.lastName || ""}`.trim();
  return fullName || `Member ${member.id}`;
}

export default function DashboardPage() {
  const { token, userId, isPreviewMode } = useAuth();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [insightSourceFilter, setInsightSourceFilter] = useState("all");
  const [showAllRenewals, setShowAllRenewals] = useState(false);
  const [showAllAlerts, setShowAllAlerts] = useState(false);
  const [summary, setSummary] = useState({
    families: [],
    members: [],
    documents: [],
    bankAccounts: [],
    financialDetails: FINANCIAL_CATEGORIES.reduce((acc, category) => {
      acc[category] = [];
      return acc;
    }, {})
  });

  useEffect(() => {
    const loadDashboard = async () => {
      if (isPreviewMode) {
        setSummary({
          families: [{ id: "preview-family" }],
          members: [
            { id: "preview-member-1", familyId: "preview-family", firstName: "Aarav", lastName: "Shah" },
            { id: "preview-member-2", familyId: "preview-family", firstName: "Neha", lastName: "Shah" }
          ],
          documents: [
            {
              id: "preview-doc-1",
              documentNumber: "P1234567",
              expiryDate: "2030-01-01T00:00:00Z",
              memberId: "preview-member-1",
              familyId: "preview-family",
              memberName: "Aarav Shah"
            }
          ],
          bankAccounts: [
            {
              id: "preview-account-1",
              bankName: "State Bank",
              nomineeName: "",
              memberId: "preview-member-1",
              familyId: "preview-family",
              memberName: "Aarav Shah"
            }
          ],
          financialDetails: {
            "bank-accounts": [{ id: "preview-account-1", nomineeName: "", memberName: "Aarav Shah" }],
            fd: [{ id: "preview-fd-1", institutionName: "HDFC Bank", maturityDate: "", nomineeName: "", memberName: "Aarav Shah" }],
            "life-insurance": [{ id: "preview-life-1", insurerName: "LIC", policyEndDate: "2026-04-15", nomineeName: "", memberName: "Aarav Shah" }],
            mediclaim: [{ id: "preview-med-1", insurerName: "Star Health", policyEndDate: "", tpaName: "", memberName: "Neha Shah" }],
            "demat-accounts": [{ id: "preview-demat-1", brokerName: "Zerodha", nomineeName: "", memberName: "Aarav Shah" }],
            "mutual-funds": [{ id: "preview-mf-1", amcName: "HDFC MF", nomineeName: "", memberName: "Neha Shah" }]
          }
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
            return unwrapData(memberResponse).map((member) => ({ ...member, familyId: family.id }));
          })
        );

        const members = membersPerFamily.flat();

        const documentPerMember = await Promise.all(
          members.map(async (member) => {
            const docsResponse = await getDocuments(member.id, token);
            return unwrapData(docsResponse).map((item) => ({
              ...item,
              memberId: member.id,
              familyId: member.familyId,
              memberName: toDisplayMemberName(member)
            }));
          })
        );

        const bankAccountPerMember = await Promise.all(
          members.map(async (member) => {
            const bankResponse = await getBankAccounts(member.id, token);
            return unwrapData(bankResponse).map((item) => ({
              ...item,
              memberId: member.id,
              familyId: member.familyId,
              memberName: toDisplayMemberName(member)
            }));
          })
        );

        const financialPerMember = await Promise.all(
          members.map(async (member) => {
            const perCategory = await Promise.all(
              FINANCIAL_CATEGORIES.map(async (category) => {
                const financialResponse = await getFinancialDetails(member.id, category, token);
                return {
                  category,
                  records: unwrapData(financialResponse).map((item) => ({
                    ...item,
                    memberId: member.id,
                    familyId: member.familyId,
                    memberName: toDisplayMemberName(member)
                  }))
                };
              })
            );
            return perCategory;
          })
        );

        const financialDetails = FINANCIAL_CATEGORIES.reduce((acc, category) => {
          acc[category] = [];
          return acc;
        }, {});
        financialPerMember.forEach((perCategory) => {
          perCategory.forEach(({ category, records }) => {
            financialDetails[category].push(...records);
          });
        });

        setSummary({
          families,
          members,
          documents: documentPerMember.flat(),
          bankAccounts: bankAccountPerMember.flat(),
          financialDetails
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
  const upcomingRenewals = useMemo(() => {
    const now = new Date();
    const next60 = new Date();
    next60.setDate(now.getDate() + 60);
    const inWindow = (dateValue) => dateValue && dateValue >= now && dateValue <= next60;

    const documentItems = summary.documents
      .filter((item) => item.expiryDate)
      .map((item) => {
        const date = new Date(item.expiryDate);
        return inWindow(date)
          ? {
              id: `doc-${item.id}`,
              type: "Document",
              title: item.documentNumber || "Document",
              memberName: item.memberName || "-",
              source: "documents",
              date,
              daysLeft: daysBetween(now, date),
              to: item.memberId && item.familyId ? `/families/${item.familyId}/members/${item.memberId}/documents` : "/families"
            }
          : null;
      })
      .filter(Boolean);

    const fdItems = (summary.financialDetails.fd || [])
      .filter((item) => item.maturityDate)
      .map((item) => {
        const date = new Date(item.maturityDate);
        return inWindow(date)
          ? {
              id: `fd-${item.id}`,
              type: "FD",
              title: item.institutionName || "Fixed Deposit",
              memberName: item.memberName || "-",
              source: "financial",
              date,
              daysLeft: daysBetween(now, date),
              to: item.memberId && item.familyId ? `/families/${item.familyId}/members/${item.memberId}/financial-details?category=fd` : "/families"
            }
          : null;
      })
      .filter(Boolean);

    const lifeItems = (summary.financialDetails["life-insurance"] || [])
      .filter((item) => item.policyEndDate || item.maturityDate)
      .map((item) => {
        const date = new Date(item.policyEndDate || item.maturityDate);
        return inWindow(date)
          ? {
              id: `life-${item.id}`,
              type: "Life Insurance",
              title: item.policyNumberLast4 ? `Policy ****${item.policyNumberLast4}` : (item.policyNumber || "Policy"),
              memberName: item.memberName || "-",
              source: "financial",
              date,
              daysLeft: daysBetween(now, date),
              to: item.memberId && item.familyId ? `/families/${item.familyId}/members/${item.memberId}/financial-details?category=life-insurance` : "/families"
            }
          : null;
      })
      .filter(Boolean);

    const mediclaimItems = (summary.financialDetails.mediclaim || [])
      .filter((item) => item.policyEndDate)
      .map((item) => {
        const date = new Date(item.policyEndDate);
        return inWindow(date)
          ? {
              id: `med-${item.id}`,
              type: "Mediclaim",
              title: item.policyNumberLast4 ? `Policy ****${item.policyNumberLast4}` : (item.policyNumber || "Policy"),
              memberName: item.memberName || "-",
              source: "financial",
              date,
              daysLeft: daysBetween(now, date),
              to: item.memberId && item.familyId ? `/families/${item.familyId}/members/${item.memberId}/financial-details?category=mediclaim` : "/families"
            }
          : null;
      })
      .filter(Boolean);

    return [...documentItems, ...fdItems, ...lifeItems, ...mediclaimItems]
      .sort((a, b) => a.date - b.date);
  }, [summary.documents, summary.financialDetails]);

  const missingDetailAlerts = useMemo(() => {
    const alerts = [];

    summary.documents.forEach((item) => {
      if (isBlank(item.expiryDate)) {
        alerts.push({
          id: `doc-missing-${item.id}`,
          message: `${item.memberName || "Member"}: document ${item.documentNumber || item.id} has no expiry date`,
          source: "documents",
          severity: "critical",
          to: item.memberId && item.familyId ? `/families/${item.familyId}/members/${item.memberId}/documents` : "/families"
        });
      }
    });

    (summary.financialDetails["bank-accounts"] || []).forEach((item) => {
      if (isBlank(item.nomineeName)) {
        alerts.push({
          id: `bank-nominee-${item.id}`,
          message: `${item.memberName || "Member"}: bank account missing nominee`,
          source: "financial",
          severity: "warning",
          to: item.memberId && item.familyId ? `/families/${item.familyId}/members/${item.memberId}/financial-details?category=bank-accounts` : "/families"
        });
      }
    });

    (summary.financialDetails.fd || []).forEach((item) => {
      if (isBlank(item.nomineeName)) {
        alerts.push({
          id: `fd-nominee-${item.id}`,
          message: `${item.memberName || "Member"}: FD missing nominee`,
          source: "financial",
          severity: "warning",
          to: item.memberId && item.familyId ? `/families/${item.familyId}/members/${item.memberId}/financial-details?category=fd` : "/families"
        });
      }
      if (isBlank(item.maturityDate)) {
        alerts.push({
          id: `fd-maturity-${item.id}`,
          message: `${item.memberName || "Member"}: FD missing maturity date`,
          source: "financial",
          severity: "critical",
          to: item.memberId && item.familyId ? `/families/${item.familyId}/members/${item.memberId}/financial-details?category=fd` : "/families"
        });
      }
    });

    (summary.financialDetails["life-insurance"] || []).forEach((item) => {
      if (isBlank(item.nomineeName)) {
        alerts.push({
          id: `life-nominee-${item.id}`,
          message: `${item.memberName || "Member"}: life insurance missing nominee`,
          source: "financial",
          severity: "warning",
          to: item.memberId && item.familyId ? `/families/${item.familyId}/members/${item.memberId}/financial-details?category=life-insurance` : "/families"
        });
      }
      if (isBlank(item.policyEndDate) && isBlank(item.maturityDate)) {
        alerts.push({
          id: `life-end-${item.id}`,
          message: `${item.memberName || "Member"}: life insurance missing end or maturity date`,
          source: "financial",
          severity: "critical",
          to: item.memberId && item.familyId ? `/families/${item.familyId}/members/${item.memberId}/financial-details?category=life-insurance` : "/families"
        });
      }
    });

    (summary.financialDetails.mediclaim || []).forEach((item) => {
      if (isBlank(item.policyEndDate)) {
        alerts.push({
          id: `med-end-${item.id}`,
          message: `${item.memberName || "Member"}: mediclaim missing policy end date`,
          source: "financial",
          severity: "critical",
          to: item.memberId && item.familyId ? `/families/${item.familyId}/members/${item.memberId}/financial-details?category=mediclaim` : "/families"
        });
      }
      if (isBlank(item.tpaName)) {
        alerts.push({
          id: `med-tpa-${item.id}`,
          message: `${item.memberName || "Member"}: mediclaim missing TPA name`,
          source: "financial",
          severity: "warning",
          to: item.memberId && item.familyId ? `/families/${item.familyId}/members/${item.memberId}/financial-details?category=mediclaim` : "/families"
        });
      }
    });

    (summary.financialDetails["demat-accounts"] || []).forEach((item) => {
      if (isBlank(item.nomineeName)) {
        alerts.push({
          id: `demat-nominee-${item.id}`,
          message: `${item.memberName || "Member"}: demat account missing nominee`,
          source: "financial",
          severity: "warning",
          to: item.memberId && item.familyId ? `/families/${item.familyId}/members/${item.memberId}/financial-details?category=demat-accounts` : "/families"
        });
      }
    });

    (summary.financialDetails["mutual-funds"] || []).forEach((item) => {
      if (isBlank(item.nomineeName)) {
        alerts.push({
          id: `mf-nominee-${item.id}`,
          message: `${item.memberName || "Member"}: mutual fund missing nominee`,
          source: "financial",
          severity: "warning",
          to: item.memberId && item.familyId ? `/families/${item.familyId}/members/${item.memberId}/financial-details?category=mutual-funds` : "/families"
        });
      }
    });

    return alerts;
  }, [summary.documents, summary.financialDetails]);

  const filteredUpcomingRenewals = useMemo(() => {
    if (insightSourceFilter === "all") {
      return upcomingRenewals;
    }
    return upcomingRenewals.filter((item) => item.source === insightSourceFilter);
  }, [upcomingRenewals, insightSourceFilter]);

  const filteredMissingAlerts = useMemo(() => {
    if (insightSourceFilter === "all") {
      return missingDetailAlerts;
    }
    return missingDetailAlerts.filter((item) => item.source === insightSourceFilter);
  }, [missingDetailAlerts, insightSourceFilter]);

  const visibleRenewals = showAllRenewals ? filteredUpcomingRenewals : filteredUpcomingRenewals.slice(0, 6);
  const visibleAlerts = showAllAlerts ? filteredMissingAlerts : filteredMissingAlerts.slice(0, 8);

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

      <section className="panel dashboard-top-timeline">
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

      <div className="stats-grid dashboard-stats-grid">
        <StatCard title="Families" value={summary.families.length} hint="Households onboarded" />
        <StatCard title="Family Members" value={summary.members.length} hint="People in your vault" />
        <StatCard title="Documents" value={summary.documents.length} hint="Identity and legal records" />
        <StatCard title="Bank Accounts" value={summary.bankAccounts.length} hint="Financial profiles tracked" />
      </div>

      <div className="insights-filter-row">
        {["all", "documents", "financial"].map((filterKey) => (
          <button
            key={filterKey}
            type="button"
            className={`filter-pill ${insightSourceFilter === filterKey ? "active" : ""}`}
            onClick={() => setInsightSourceFilter(filterKey)}
          >
            {filterKey === "all" ? "All" : filterKey === "documents" ? "Documents" : "Financial"}
          </button>
        ))}
      </div>

      <div className="dashboard-insights-grid">
        <section className="panel">
          <div className="panel-head">
            <h3>Upcoming Renewals (Next 60 Days)</h3>
            <div className="panel-head-actions">
              <span className="subtle">{filteredUpcomingRenewals.length} items</span>
              {filteredUpcomingRenewals.length > 6 && (
                <button type="button" className="inline-link panel-link-btn" onClick={() => setShowAllRenewals((prev) => !prev)}>
                  {showAllRenewals ? "Show less" : "View all"}
                </button>
              )}
            </div>
          </div>
          {!visibleRenewals.length ? (
            <p className="empty-state">No upcoming renewals in the next 60 days.</p>
          ) : (
            <ul className="simple-list">
              {visibleRenewals.map((item) => (
                <li key={item.id}>
                  <strong>{item.type}: {item.title}</strong>
                  <p className="subtle">
                    {item.memberName} | {formatDate(item.date)} | {item.daysLeft} day(s) left
                  </p>
                  <Link className="inline-link" to={item.to}>Open</Link>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section className="panel">
          <div className="panel-head">
            <h3>Missing Details Alerts</h3>
            <div className="panel-head-actions">
              <span className="subtle">{filteredMissingAlerts.length} issues</span>
              {filteredMissingAlerts.length > 8 && (
                <button type="button" className="inline-link panel-link-btn" onClick={() => setShowAllAlerts((prev) => !prev)}>
                  {showAllAlerts ? "Show less" : "View all"}
                </button>
              )}
            </div>
          </div>
          {!visibleAlerts.length ? (
            <p className="empty-state">No missing detail alerts right now.</p>
          ) : (
            <ul className="simple-list">
              {visibleAlerts.map((alert) => (
                <li key={alert.id}>
                  <span className={`alert-severity ${alert.severity}`}>{alert.severity}</span>
                  <span>{alert.message}</span>
                  <Link className="inline-link" to={alert.to}>Fix now</Link>
                </li>
              ))}
            </ul>
          )}
        </section>
      </div>

    </section>
  );
}


