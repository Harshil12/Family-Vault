import React, { useEffect, useMemo, useState } from "react";
import { Link, useParams } from "react-router-dom";
import CrudTable from "../components/ui/CrudTable";
import FormModal from "../components/ui/FormModal";
import { useAuth } from "../context/AuthContext";
import { createFamilyMember, deleteFamilyMember, getFamilyMembers, updateFamilyMember } from "../services/familyMemberService";
import { bloodGroupOptions, optionLabelByValue, relationshipOptions } from "../utils/options";
import { unwrapData } from "../utils/response";

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
  const { token } = useAuth();
  const [members, setMembers] = useState([]);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingMember, setEditingMember] = useState(null);

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
          <Link className="inline-link" to={`/families/${familyId}/members/${row.id}/documents`}>
            Manage
          </Link>
        )
      },
      {
        key: "accounts",
        header: "Bank Accounts",
        render: (row) => (
          <Link className="inline-link" to={`/families/${familyId}/members/${row.id}/accounts`}>
            Manage
          </Link>
        )
      }
    ],
    [familyId]
  );

  const loadMembers = async () => {
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
  }, [familyId, token]);

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
    const shouldDelete = window.confirm(`Delete member "${member.firstName}"?`);
    if (!shouldDelete) {
      return;
    }

    try {
      await deleteFamilyMember(familyId, member.id, token);
      await loadMembers();
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  return (
    <section>
      <header className="page-header">
        <div>
          <h2>Family Members</h2>
          <p className="subtle">Family ID: {familyId}</p>
        </div>
        <button type="button" className="btn" onClick={openCreate}>
          Add Member
        </button>
      </header>

      {error && <p className="error-text">{error}</p>}
      {loading ? <p>Loading family members...</p> : <CrudTable columns={columns} rows={members} onEdit={openEdit} onDelete={handleDelete} />}

      <FormModal
        title={editingMember ? "Edit Family Member" : "Add Family Member"}
        isOpen={modalOpen}
        initialValues={toFormValues(editingMember)}
        fields={[
          { name: "firstName", label: "First Name", required: true },
          { name: "lastName", label: "Last Name" },
          { name: "countryCode", label: "Country Code" },
          { name: "mobile", label: "Mobile" },
          { name: "relationshipType", label: "Relationship", type: "select", options: relationshipOptions, required: true },
          { name: "dateOfBirth", label: "Date Of Birth", type: "date" },
          { name: "bloodGroup", label: "Blood Group", type: "select", options: bloodGroupOptions },
          { name: "email", label: "Email", type: "email" },
          { name: "pan", label: "PAN" },
          { name: "aadhar", label: "Aadhar" }
        ]}
        onClose={closeModal}
        onSubmit={handleSubmit}
      />
    </section>
  );
}

