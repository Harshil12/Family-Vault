import React, { useEffect, useMemo, useState } from "react";
import { Link, useParams } from "react-router-dom";
import CrudTable from "../components/ui/CrudTable";
import FormModal from "../components/ui/FormModal";
import ConfirmModal from "../components/ui/ConfirmModal";
import HelpTip from "../components/ui/HelpTip";
import { BackIcon, PlusIcon } from "../components/ui/Icons";
import { useAuth } from "../context/AuthContext";
import { createBankAccount, deleteBankAccount, getBankAccounts, updateBankAccount } from "../services/bankAccountService";
import { accountTypeOptions, optionLabelByValue } from "../utils/options";
import { unwrapData } from "../utils/response";
import { validateBankAccount } from "../utils/validation";

const blankAccount = {
  bankName: "",
  accountNumber: "",
  accountType: "",
  accountHolderName: "",
  ifsc: "",
  branch: ""
};

function toPayload(values, memberId) {
  return {
    bankName: values.bankName,
    accountNumber: values.accountNumber,
    accountType: Number(values.accountType),
    accountHolderName: values.accountHolderName || null,
    ifsc: values.ifsc || null,
    branch: values.branch || null,
    familyMemberId: memberId
  };
}

function toFormValues(account) {
  if (!account) {
    return blankAccount;
  }

  return {
    bankName: account.bankName ?? "",
    accountNumber: account.accountNumber ?? "",
    accountType: String(account.accountType ?? ""),
    accountHolderName: account.accountHolderName ?? "",
    ifsc: account.ifsc ?? "",
    branch: account.branch ?? ""
  };
}

export default function BankAccountsPage() {
  const { familyId, memberId } = useParams();
  const { token, isPreviewMode } = useAuth();
  const [accounts, setAccounts] = useState([]);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingAccount, setEditingAccount] = useState(null);
  const [deletingAccount, setDeletingAccount] = useState(null);
  const [searchText, setSearchText] = useState("");
  const [accountTypeFilter, setAccountTypeFilter] = useState("");

  const columns = useMemo(
    () => [
      { key: "bankName", header: "Bank" },
      {
        key: "accountType",
        header: "Type",
        render: (row) => optionLabelByValue(accountTypeOptions, row.accountType)
      },
      {
        key: "accountMask",
        header: "Account",
        render: (row) => row.accountNumberLast4 ? `****${row.accountNumberLast4}` : row.accountNumber
      },
      { key: "ifsc", header: "IFSC" }
    ],
    []
  );

  const loadAccounts = async () => {
    if (isPreviewMode) {
      setAccounts([{ id: "preview-account-1", bankName: "State Bank", accountType: 1, accountNumberLast4: "9012", ifsc: "SBIN0001234" }]);
      setLoading(false);
      return;
    }
    setError("");
    setLoading(true);

    try {
      const response = await getBankAccounts(memberId, token);
      setAccounts(unwrapData(response));
    } catch (requestError) {
      setError(requestError.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadAccounts();
  }, [memberId, token, isPreviewMode]);

  const openCreate = () => {
    setEditingAccount(null);
    setModalOpen(true);
  };

  const openEdit = (account) => {
    setEditingAccount(account);
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
      if (editingAccount) {
        await updateBankAccount(memberId, editingAccount.id, payload, token);
      } else {
        await createBankAccount(memberId, payload, token);
      }

      closeModal();
      await loadAccounts();
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const handleDelete = async (account) => {
    if (isPreviewMode) {
      window.alert("Preview mode: login to delete records.");
      return;
    }
    setDeletingAccount(account);
  };

  const confirmDelete = async () => {
    if (!deletingAccount) {
      return;
    }
    try {
      await deleteBankAccount(memberId, deletingAccount.id, token);
      setDeletingAccount(null);
      await loadAccounts();
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const filteredAccounts = accounts.filter((account) => {
    const search = searchText.trim().toLowerCase();
    const matchesSearch =
      !search ||
      (account.bankName || "").toLowerCase().includes(search) ||
      (account.ifsc || "").toLowerCase().includes(search) ||
      (account.accountNumberLast4 || "").toLowerCase().includes(search);
    const matchesType = accountTypeFilter === "" || String(account.accountType) === accountTypeFilter;
    return matchesSearch && matchesType;
  });

  return (
    <section>
      <header className="page-header">
        <div>
          <div className="heading-with-help">
            <h2>Banking Records</h2>
            <HelpTip text="Only masked account details are shown in lists for extra privacy." />
          </div>
          <p className="subtle page-intro">
            Records for Family {familyId} and Member {memberId}
          </p>
        </div>
        <button type="button" className="btn" onClick={openCreate}>
          <span className="btn-icon"><PlusIcon /></span>
          <span>Add Account</span>
        </button>
      </header>

      {error && <p className="error-text">{error}</p>}
      {isPreviewMode && <p className="subtle">Preview mode is on. Login to enable CRUD.</p>}
      <div className="toolbar">
        <input
          type="text"
          placeholder="Search bank / IFSC / last4..."
          value={searchText}
          onChange={(event) => setSearchText(event.target.value)}
        />
        <select value={accountTypeFilter} onChange={(event) => setAccountTypeFilter(event.target.value)}>
          <option value="">All account types</option>
          {accountTypeOptions.map((option) => (
            <option key={`${option.label}-${option.value}`} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </div>
      {loading ? <p>Loading bank accounts...</p> : <CrudTable columns={columns} rows={filteredAccounts} onEdit={openEdit} onDelete={handleDelete} />}

      <FormModal
        title={editingAccount ? "Edit Bank Account" : "Add Bank Account"}
        isOpen={modalOpen}
        initialValues={toFormValues(editingAccount)}
        fields={[
          { name: "bankName", label: "Bank Name", required: true },
          { name: "accountNumber", label: "Account Number", required: true },
          { name: "accountType", label: "Account Type", type: "select", options: accountTypeOptions, required: true },
          { name: "accountHolderName", label: "Account Holder Name" },
          { name: "ifsc", label: "IFSC" },
          { name: "branch", label: "Branch" }
        ]}
        validate={validateBankAccount}
        onClose={closeModal}
        onSubmit={handleSubmit}
      />
      <ConfirmModal
        isOpen={Boolean(deletingAccount)}
        title="Delete Bank Account"
        message={deletingAccount ? `Are you sure you want to delete account from "${deletingAccount.bankName}"?` : ""}
        onCancel={() => setDeletingAccount(null)}
        onConfirm={confirmDelete}
      />

      <Link className="inline-link back-link" to={`/families/${familyId}/members`}>
        <span className="btn-icon"><BackIcon /></span>
        <span>Back to Members</span>
      </Link>
    </section>
  );
}


