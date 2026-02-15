import React, { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import DataGridTable from "../components/ui/DataGridTable";
import FormModal from "../components/ui/FormModal";
import ConfirmModal from "../components/ui/ConfirmModal";
import HelpTip from "../components/ui/HelpTip";
import { MembersIcon, PlusIcon } from "../components/ui/Icons";
import { useAuth } from "../context/AuthContext";
import { createFamily, deleteFamily, getFamilies, updateFamily } from "../services/familyService";
import { unwrapData } from "../utils/response";
import { validateFamily } from "../utils/validation";

const blankFamily = {
  familyName: ""
};

export default function FamiliesPage() {
  const { token, userId, isPreviewMode } = useAuth();
  const [families, setFamilies] = useState([]);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingFamily, setEditingFamily] = useState(null);
  const [deletingFamily, setDeletingFamily] = useState(null);
  const [searchText, setSearchText] = useState("");

  const columns = useMemo(
    () => [
      { key: "name", header: "Family Name" },
      {
        key: "createdAt",
        header: "Created",
        render: (row) => (row.createdAt ? new Date(row.createdAt).toLocaleString() : "-")
      },
      {
        key: "updatedAt",
        header: "Updated",
        render: (row) => (row.updatedAt ? new Date(row.updatedAt).toLocaleString() : "-")
      },
      {
        key: "members",
        header: "Members",
        render: (row) => (
          <Link className="icon-link" to={`/families/${row.id}/members`} title="View members" aria-label="View members">
            <MembersIcon />
          </Link>
        )
      }
    ],
    []
  );

  const loadFamilies = async () => {
    if (isPreviewMode) {
      setFamilies([{ id: "preview-family", name: "Demo Family" }]);
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
      const response = await getFamilies(userId, token);
      setFamilies(unwrapData(response));
    } catch (requestError) {
      setError(requestError.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadFamilies();
  }, [token, userId, isPreviewMode]);

  const openCreate = () => {
    setEditingFamily(null);
    setModalOpen(true);
  };

  const openEdit = (row) => {
    setEditingFamily(row);
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
    if (!userId) {
      return;
    }

    const payload = {
      familyName: values.familyName
    };

    try {
      if (editingFamily) {
        await updateFamily(editingFamily.id, payload, userId, token);
      } else {
        await createFamily(payload, userId, token);
      }

      closeModal();
      await loadFamilies();
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const handleDelete = async (row) => {
    if (isPreviewMode) {
      window.alert("Preview mode: login to delete records.");
      return;
    }
    if (!userId) {
      return;
    }
    setDeletingFamily(row);
  };

  const confirmDelete = async () => {
    if (!deletingFamily || !userId) {
      return;
    }
    try {
      await deleteFamily(deletingFamily.id, userId, token);
      setDeletingFamily(null);
      await loadFamilies();
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const filteredFamilies = families.filter((family) => {
    const search = searchText.trim().toLowerCase();
    return !search || (family.name || "").toLowerCase().includes(search);
  });

  return (
    <section>
      <header className="page-header">
        <div>
          <div className="heading-with-help">
            <h2>Family Profiles</h2>
            <HelpTip text="Create a household first, then open members to manage documents and bank records." />
          </div>
          <p className="subtle page-intro">Organize each household with members, documents, and accounts.</p>
        </div>
        <button type="button" className="btn" onClick={openCreate}>
          <span className="btn-icon"><PlusIcon /></span>
          <span>Add Family</span>
        </button>
      </header>

      {error && <p className="error-text">{error}</p>}
      {isPreviewMode && <p className="subtle">Preview mode is on. Login to enable CRUD.</p>}
      <div className="toolbar">
        <input
          type="text"
          placeholder="Search families..."
          value={searchText}
          onChange={(event) => setSearchText(event.target.value)}
        />
      </div>
      {loading ? <p>Loading families...</p> : <DataGridTable columns={columns} rows={filteredFamilies} onEdit={openEdit} onDelete={handleDelete} />}

      <FormModal
        title={editingFamily ? "Edit Family" : "Create Family"}
        isOpen={modalOpen}
        initialValues={{ familyName: editingFamily?.name ?? blankFamily.familyName }}
        fields={[{ name: "familyName", label: "Family name", required: true }]}
        validate={validateFamily}
        onClose={closeModal}
        onSubmit={handleSubmit}
      />
      <ConfirmModal
        isOpen={Boolean(deletingFamily)}
        title="Delete Family"
        message={deletingFamily ? `Are you sure you want to delete family "${deletingFamily.name}"?` : ""}
        onCancel={() => setDeletingFamily(null)}
        onConfirm={confirmDelete}
      />
    </section>
  );
}


