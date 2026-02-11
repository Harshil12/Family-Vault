import React, { useEffect, useMemo, useState } from "react";
import { Link, useParams } from "react-router-dom";
import CrudTable from "../components/ui/CrudTable";
import FormModal from "../components/ui/FormModal";
import ConfirmModal from "../components/ui/ConfirmModal";
import { BackIcon, BankIcon, DocumentIcon, PlusIcon } from "../components/ui/Icons";
import { useAuth } from "../context/AuthContext";
import useCountryCodes from "../hooks/useCountryCodes";
import { createFamilyMember, deleteFamilyMember, getFamilyMembers, updateFamilyMember } from "../services/familyMemberService";
import { bloodGroupOptions, optionLabelByValue, relationshipOptions } from "../utils/options";
import { unwrapData } from "../utils/response";
import { validateFamilyMember } from "../utils/validation";

const blankMember = {
  firstName: "",
  lastName: "",
  countryCode: "+91",
  mobile: "",
  relationshipType: "",
  dateOfBirth: "",
  bloodGroup: "",
  email: "",
  pan: "",
  aadhar: ""
};

function toPayload(values, familyId) {
  return {
    firstName: values.firstName,
    lastName: values.lastName || null,
    familyId,
    countryCode: values.countryCode || null,
    mobile: values.mobile || null,
    relationshipType: Number(values.relationshipType),
    dateOfBirth: values.dateOfBirth || null,
    bloodGroup: values.bloodGroup === "" ? null : Number(values.bloodGroup),
    email: values.email || null,
    pan: values.pan || null,
    aadhar: values.aadhar || null
  };
}

function toFormValues(member) {
  if (!member) {
    return blankMember;
  }

  return {
    firstName: member.firstName ?? "",
    lastName: member.lastName ?? "",
    countryCode: member.countryCode ?? "+91",
    mobile: member.mobile ?? "",
    relationshipType: String(member.relationshipType ?? ""),
    dateOfBirth: member.dateOfBirth ? member.dateOfBirth.slice(0, 10) : "",
    bloodGroup: member.bloodGroup === null || member.bloodGroup === undefined ? "" : String(member.bloodGroup),
    email: member.email ?? "",
    pan: member.pan ?? "",
    aadhar: member.aadhar ?? ""
  };
}

export default function FamilyMembersPage() {
  const { familyId } = useParams();
  const { token, isPreviewMode } = useAuth();
  const countryCodeOptions = useCountryCodes();
  const [members, setMembers] = useState([]);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingMember, setEditingMember] = useState(null);
  const [deletingMember, setDeletingMember] = useState(null);
  const [searchText, setSearchText] = useState("");
  const [relationFilter, setRelationFilter] = useState("");

  const columns = useMemo(
    () => [
      {
        key: "fullName",
        header: "Name",
        render: (row) => `${row.firstName} ${row.lastName ?? ""}`.trim()
      },
      {
        key: "relationshipType",
        header: "Relation",
        render: (row) => optionLabelByValue(relationshipOptions, row.relationshipType)
      },
      { key: "email", header: "Email" },
      {
        key: "documents",
        header: "Documents",
        render: (row) => (
          <Link
            className="icon-link"
            to={`/families/${familyId}/members/${row.id}/documents`}
            title="View documents"
            aria-label="View documents"
          >
            <DocumentIcon />
          </Link>
        )
      },
      {
        key: "accounts",
        header: "Bank Accounts",
        render: (row) => (
          <Link
            className="icon-link"
            to={`/families/${familyId}/members/${row.id}/accounts`}
            title="View bank accounts"
            aria-label="View bank accounts"
          >
            <BankIcon />
          </Link>
        )
      }
    ],
    [familyId]
  );

  const loadMembers = async () => {
    if (isPreviewMode) {
      setMembers([
        { id: "preview-member-1", firstName: "Jane", lastName: "Doe", relationshipType: 1, email: "jane@example.com" }
      ]);
      setLoading(false);
      return;
    }
    setError("");
    setLoading(true);

    try {
      const response = await getFamilyMembers(familyId, token);
      setMembers(unwrapData(response));
    } catch (requestError) {
      setError(requestError.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadMembers();
  }, [familyId, token, isPreviewMode]);

  const openCreate = () => {
    setEditingMember(null);
    setModalOpen(true);
  };

  const openEdit = (member) => {
    setEditingMember(member);
    setModalOpen(true);
  };

  const closeModal = () => {
    setModalOpen(false);
  };

  const handleSubmit = async (values) => {
    if (isPreviewMode) {
      window.alert("Preview mode: login to create or edit records.");
      return;
    }
    const payload = toPayload(values, familyId);

    try {
      if (editingMember) {
        await updateFamilyMember(familyId, editingMember.id, payload, token);
      } else {
        await createFamilyMember(familyId, payload, token);
      }

      closeModal();
      await loadMembers();
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const handleDelete = async (member) => {
    if (isPreviewMode) {
      window.alert("Preview mode: login to delete records.");
      return;
    }
    setDeletingMember(member);
  };

  const confirmDelete = async () => {
    if (!deletingMember) {
      return;
    }
    try {
      await deleteFamilyMember(familyId, deletingMember.id, token);
      setDeletingMember(null);
      await loadMembers();
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const filteredMembers = members.filter((member) => {
    const search = searchText.trim().toLowerCase();
    const matchesSearch =
      !search ||
      `${member.firstName || ""} ${member.lastName || ""}`.toLowerCase().includes(search) ||
      (member.email || "").toLowerCase().includes(search);
    const matchesRelation = relationFilter === "" || String(member.relationshipType) === relationFilter;
    return matchesSearch && matchesRelation;
  });

  return (
    <section>
      <header className="page-header">
        <div>
          <h2>Family Members</h2>
          <p className="subtle">Family ID: {familyId}</p>
        </div>
        <button type="button" className="btn" onClick={openCreate}>
          <span className="btn-icon"><PlusIcon /></span>
          <span>Add Member</span>
        </button>
      </header>

      {error && <p className="error-text">{error}</p>}
      {isPreviewMode && <p className="subtle">Preview mode is on. Login to enable CRUD.</p>}
      <div className="toolbar">
        <input
          type="text"
          placeholder="Search members by name/email..."
          value={searchText}
          onChange={(event) => setSearchText(event.target.value)}
        />
        <select value={relationFilter} onChange={(event) => setRelationFilter(event.target.value)}>
          <option value="">All relations</option>
          {relationshipOptions.map((option) => (
            <option key={`${option.label}-${option.value}`} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </div>
      {loading ? <p>Loading family members...</p> : <CrudTable columns={columns} rows={filteredMembers} onEdit={openEdit} onDelete={handleDelete} />}

      <FormModal
        title={editingMember ? "Edit Family Member" : "Add Family Member"}
        isOpen={modalOpen}
        initialValues={toFormValues(editingMember)}
        fields={[
          { name: "firstName", label: "First Name", required: true },
          { name: "lastName", label: "Last Name" },
          { name: "countryCode", label: "Country Code", type: "select", options: countryCodeOptions },
          { name: "mobile", label: "Mobile" },
          { name: "relationshipType", label: "Relationship", type: "select", options: relationshipOptions, required: true },
          { name: "dateOfBirth", label: "Date Of Birth", type: "date" },
          { name: "bloodGroup", label: "Blood Group", type: "select", options: bloodGroupOptions },
          { name: "email", label: "Email", type: "email" },
          { name: "pan", label: "PAN" },
          { name: "aadhar", label: "Aadhar" }
        ]}
        validate={validateFamilyMember}
        onClose={closeModal}
        onSubmit={handleSubmit}
      />
      <Link className="inline-link back-link" to="/families">
        <span className="btn-icon"><BackIcon /></span>
        <span>Back to Families</span>
      </Link>
      <ConfirmModal
        isOpen={Boolean(deletingMember)}
        title="Delete Family Member"
        message={deletingMember ? `Are you sure you want to delete member "${deletingMember.firstName}"?` : ""}
        onCancel={() => setDeletingMember(null)}
        onConfirm={confirmDelete}
      />
    </section>
  );
}

