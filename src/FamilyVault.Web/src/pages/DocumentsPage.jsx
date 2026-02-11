import React, { useEffect, useMemo, useState } from "react";
import { Link, useParams } from "react-router-dom";
import CrudTable from "../components/ui/CrudTable";
import FormModal from "../components/ui/FormModal";
import { useAuth } from "../context/AuthContext";
import { createDocument, deleteDocument, getDocuments, updateDocument } from "../services/documentService";
import { documentTypeOptions, optionLabelByValue } from "../utils/options";
import { unwrapData } from "../utils/response";

const blankDocument = {
  documentType: "",
  documentNumber: "",
  issueDate: "",
  expiryDate: ""
};

function toPayload(values, memberId) {
  return {
    documentType: Number(values.documentType),
    documentNumber: values.documentNumber,
    issueDate: values.issueDate || null,
    expiryDate: values.expiryDate || null,
    familyMemberId: memberId
  };
}

function toFormValues(document) {
  if (!document) {
    return blankDocument;
  }

  return {
    documentType: String(document.documentType ?? ""),
    documentNumber: document.documentNumber ?? "",
    issueDate: document.issueDate ? document.issueDate.slice(0, 10) : "",
    expiryDate: document.expiryDate ? document.expiryDate.slice(0, 10) : ""
  };
}

export default function DocumentsPage() {
  const { familyId, memberId } = useParams();
  const { token } = useAuth();
  const [documents, setDocuments] = useState([]);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingDocument, setEditingDocument] = useState(null);

  const columns = useMemo(
    () => [
      {
        key: "documentType",
        header: "Type",
        render: (row) => optionLabelByValue(documentTypeOptions, row.documentType)
      },
      { key: "documentNumber", header: "Document Number" },
      {
        key: "expiryDate",
        header: "Expiry",
        render: (row) => (row.expiryDate ? new Date(row.expiryDate).toLocaleDateString() : "-")
      }
    ],
    []
  );

  const loadDocuments = async () => {
    setError("");
    setLoading(true);

    try {
      const response = await getDocuments(memberId, token);
      setDocuments(unwrapData(response));
    } catch (requestError) {
      setError(requestError.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadDocuments();
  }, [memberId, token]);

  const openCreate = () => {
    setEditingDocument(null);
    setModalOpen(true);
  };

  const openEdit = (document) => {
    setEditingDocument(document);
    setModalOpen(true);
  };

  const closeModal = () => {
    setModalOpen(false);
  };

  const handleSubmit = async (values) => {
    const payload = toPayload(values, memberId);

    try {
      if (editingDocument) {
        await updateDocument(memberId, editingDocument.id, payload, token);
      } else {
        await createDocument(memberId, payload, token);
      }

      closeModal();
      await loadDocuments();
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const handleDelete = async (document) => {
    const shouldDelete = window.confirm(`Delete document "${document.documentNumber}"?`);
    if (!shouldDelete) {
      return;
    }

    try {
      await deleteDocument(memberId, document.id, token);
      await loadDocuments();
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  return (
    <section>
      <header className="page-header">
        <div>
          <h2>Documents</h2>
          <p className="subtle">
            Family: {familyId} | Member: {memberId}
          </p>
        </div>
        <button type="button" className="btn" onClick={openCreate}>
          Add Document
        </button>
      </header>

      {error && <p className="error-text">{error}</p>}
      {loading ? <p>Loading documents...</p> : <CrudTable columns={columns} rows={documents} onEdit={openEdit} onDelete={handleDelete} />}

      <FormModal
        title={editingDocument ? "Edit Document" : "Add Document"}
        isOpen={modalOpen}
        initialValues={toFormValues(editingDocument)}
        fields={[
          { name: "documentType", label: "Type", type: "select", options: documentTypeOptions, required: true },
          { name: "documentNumber", label: "Document Number", required: true },
          { name: "issueDate", label: "Issue Date", type: "date" },
          { name: "expiryDate", label: "Expiry Date", type: "date" }
        ]}
        onClose={closeModal}
        onSubmit={handleSubmit}
      />

      <Link className="inline-link" to={`/families/${familyId}/members`}>
        Back to members
      </Link>
    </section>
  );
}

