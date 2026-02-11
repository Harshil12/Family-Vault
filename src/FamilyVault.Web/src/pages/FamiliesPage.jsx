import React, { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import CrudTable from "../components/ui/CrudTable";
import FormModal from "../components/ui/FormModal";
import ConfirmModal from "../components/ui/ConfirmModal";
import { PlusIcon } from "../components/ui/Icons";
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

  const columns = useMemo(
    () => [
      { key: "name", header: "Family Name" },
      {
        key: "members",
        header: "Flow",
        render: (row) => (
          <Link className="inline-link" to={`/families/${row.id}/members`}>
            Open members
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

  return (
    <section>
      <header className="page-header">
        <div>
          <h2>Families</h2>
          <p className="subtle">Create and manage each family unit.</p>
        </div>
        <button type="button" className="btn" onClick={openCreate}>
          <span className="btn-icon"><PlusIcon /></span>
          <span>Add Family</span>
        </button>
      </header>

      {error && <p className="error-text">{error}</p>}
      {isPreviewMode && <p className="subtle">Preview mode is on. Login to enable CRUD.</p>}
      {loading ? <p>Loading families...</p> : <CrudTable columns={columns} rows={families} onEdit={openEdit} onDelete={handleDelete} />}

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

