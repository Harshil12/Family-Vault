import React, { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import CrudTable from "../components/ui/CrudTable";
import FormModal from "../components/ui/FormModal";
import { useAuth } from "../context/AuthContext";
import { createFamily, deleteFamily, getFamilies, updateFamily } from "../services/familyService";
import { unwrapData } from "../utils/response";

const blankFamily = {
  familyName: ""
};

export default function FamiliesPage() {
  const { token, userId } = useAuth();
  const [families, setFamilies] = useState([]);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingFamily, setEditingFamily] = useState(null);

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
    if (!userId) {
      setLoading(false);
      setError("Cannot load families: user id not found in token.");
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
  }, [token, userId]);

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
    if (!userId) {
      return;
    }

    const shouldDelete = window.confirm(`Delete family "${row.name}"?`);
    if (!shouldDelete) {
      return;
    }

    try {
      await deleteFamily(row.id, userId, token);
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
          Add Family
        </button>
      </header>

      {error && <p className="error-text">{error}</p>}
      {loading ? <p>Loading families...</p> : <CrudTable columns={columns} rows={families} onEdit={openEdit} onDelete={handleDelete} />}

      <FormModal
        title={editingFamily ? "Edit Family" : "Create Family"}
        isOpen={modalOpen}
        initialValues={{ familyName: editingFamily?.name ?? blankFamily.familyName }}
        fields={[{ name: "familyName", label: "Family name", required: true }]}
        onClose={closeModal}
        onSubmit={handleSubmit}
      />
    </section>
  );
}

