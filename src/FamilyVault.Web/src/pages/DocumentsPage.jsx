import React, { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import CrudTable from "../components/ui/CrudTable";
import FormModal from "../components/ui/FormModal";
import ConfirmModal from "../components/ui/ConfirmModal";
import { BackIcon, DocumentIcon, PlusIcon } from "../components/ui/Icons";
import { useAuth } from "../context/AuthContext";
import { createDocument, deleteDocument, downloadDocumentFile, getDocuments, previewDocumentFile, updateDocument, uploadDocument } from "../services/documentService";
import { documentTypeOptions, optionLabelByValue } from "../utils/options";
import { unwrapData } from "../utils/response";
import { validateDocument } from "../utils/validation";

const blankDocument = {
  documentType: "",
  documentNumber: "",
  issueDate: "",
  expiryDate: "",
  file: null
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
    expiryDate: document.expiryDate ? document.expiryDate.slice(0, 10) : "",
    file: null
  };
}

export default function DocumentsPage() {
  const { familyId, memberId } = useParams();
  const { token, isPreviewMode } = useAuth();
  const [documents, setDocuments] = useState([]);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingDocument, setEditingDocument] = useState(null);
  const [deletingDocument, setDeletingDocument] = useState(null);
  const [searchText, setSearchText] = useState("");
  const [documentTypeFilter, setDocumentTypeFilter] = useState("");
  const [expiringOnly, setExpiringOnly] = useState(false);

  const columns = [
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
    },
    {
      key: "file",
      header: "File",
      render: (row) => (
        row.savedLocation ? (
          <div className="inline-actions">
            <button
              type="button"
              className="icon-link"
              title="Preview file"
              aria-label="Preview file"
              onClick={() => handlePreview(row)}
            >
              <DocumentIcon />
            </button>
            <button
              type="button"
              className="icon-link"
              title="Download file"
              aria-label="Download file"
              onClick={() => handleDownload(row)}
            >
              <span className="download-glyph">D</span>
            </button>
          </div>
        ) : (
          <span className="subtle">-</span>
        )
      )
    }
  ];

  const loadDocuments = async () => {
    if (isPreviewMode) {
      setDocuments([{ id: "preview-doc-1", documentType: 1, documentNumber: "P1234567", expiryDate: "2030-01-01T00:00:00Z" }]);
      setLoading(false);
      return;
    }
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
  }, [memberId, token, isPreviewMode]);

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
    if (isPreviewMode) {
      window.alert("Preview mode: login to create or edit records.");
      return;
    }
    const payload = toPayload(values, memberId);

    try {
      if (editingDocument) {
        payload.savedLocation = editingDocument.savedLocation ?? null;
        await updateDocument(memberId, editingDocument.id, payload, token);
      } else {
        if (values.file) {
          await uploadDocument(memberId, { ...payload, file: values.file }, token);
        } else {
          await createDocument(memberId, payload, token);
        }
      }

      closeModal();
      await loadDocuments();
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const handleDelete = async (document) => {
    if (isPreviewMode) {
      window.alert("Preview mode: login to delete records.");
      return;
    }
    setDeletingDocument(document);
  };

  const handlePreview = async (document) => {
    try {
      await previewDocumentFile(memberId, document.id, token);
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const handleDownload = async (document) => {
    try {
      await downloadDocumentFile(memberId, document.id, token, document.documentNumber || "document");
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const filteredDocuments = documents.filter((document) => {
    const search = searchText.trim().toLowerCase();
    const matchesSearch = !search || (document.documentNumber || "").toLowerCase().includes(search);
    const matchesType = documentTypeFilter === "" || String(document.documentType) === documentTypeFilter;
    const matchesExpiring = !expiringOnly || (() => {
      if (!document.expiryDate) {
        return false;
      }
      const now = new Date();
      const next45 = new Date();
      next45.setDate(now.getDate() + 45);
      const expiry = new Date(document.expiryDate);
      return expiry >= now && expiry <= next45;
    })();

    return matchesSearch && matchesType && matchesExpiring;
  });

  const confirmDelete = async () => {
    if (!deletingDocument) {
      return;
    }
    try {
      await deleteDocument(memberId, deletingDocument.id, token);
      setDeletingDocument(null);
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
          <span className="btn-icon"><PlusIcon /></span>
          <span>Add Document</span>
        </button>
      </header>

      {error && <p className="error-text">{error}</p>}
      {isPreviewMode && <p className="subtle">Preview mode is on. Login to enable CRUD.</p>}
      <div className="toolbar">
        <input
          type="text"
          placeholder="Search by document number..."
          value={searchText}
          onChange={(event) => setSearchText(event.target.value)}
        />
        <select value={documentTypeFilter} onChange={(event) => setDocumentTypeFilter(event.target.value)}>
          <option value="">All types</option>
          {documentTypeOptions.map((option) => (
            <option key={`${option.label}-${option.value}`} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
        <label className="inline-check">
          <input type="checkbox" checked={expiringOnly} onChange={(event) => setExpiringOnly(event.target.checked)} />
          <span>Expiring in 45 days</span>
        </label>
      </div>
      {loading ? <p>Loading documents...</p> : <CrudTable columns={columns} rows={filteredDocuments} onEdit={openEdit} onDelete={handleDelete} />}

      <FormModal
        title={editingDocument ? "Edit Document" : "Add Document"}
        isOpen={modalOpen}
        initialValues={toFormValues(editingDocument)}
        fields={[
          { name: "documentType", label: "Type", type: "select", options: documentTypeOptions, required: true },
          { name: "documentNumber", label: "Document Number", required: true },
          { name: "issueDate", label: "Issue Date", type: "date" },
          { name: "expiryDate", label: "Expiry Date", type: "date" },
          ...(editingDocument
            ? []
            : [{ name: "file", label: "Upload File", type: "file", accept: ".pdf,.doc,.docx,.xls,.xlsx,.png,.jpg,.jpeg,.gif,.bmp,.webp" }])
        ]}
        validate={(values) => validateDocument(values, { requireFile: !editingDocument })}
        onClose={closeModal}
        onSubmit={handleSubmit}
      />
      <ConfirmModal
        isOpen={Boolean(deletingDocument)}
        title="Delete Document"
        message={deletingDocument ? `Are you sure you want to delete document "${deletingDocument.documentNumber}"?` : ""}
        onCancel={() => setDeletingDocument(null)}
        onConfirm={confirmDelete}
      />

      <Link className="inline-link back-link" to={`/families/${familyId}/members`}>
        <span className="btn-icon"><BackIcon /></span>
        <span>Back to Members</span>
      </Link>
    </section>
  );
}

