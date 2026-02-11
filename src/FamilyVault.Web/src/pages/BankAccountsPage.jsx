import React, { useEffect, useMemo, useState } from "react";
import { Link, useParams } from "react-router-dom";
import CrudTable from "../components/ui/CrudTable";
import FormModal from "../components/ui/FormModal";
import { useAuth } from "../context/AuthContext";
import { createBankAccount, deleteBankAccount, getBankAccounts, updateBankAccount } from "../services/bankAccountService";
import { accountTypeOptions, optionLabelByValue } from "../utils/options";
import { unwrapData } from "../utils/response";

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
  const { token } = useAuth();
  const [accounts, setAccounts] = useState([]);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingAccount, setEditingAccount] = useState(null);

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
  }, [memberId, token]);

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
    const shouldDelete = window.confirm(`Delete account from "${account.bankName}"?`);
    if (!shouldDelete) {
      return;
    }

    try {
      await deleteBankAccount(memberId, account.id, token);
      await loadAccounts();
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  return (
    <section>
      <header className="page-header">
        <div>
          <h2>Bank Accounts</h2>
          <p className="subtle">
            Family: {familyId} | Member: {memberId}
          </p>
        </div>
        <button type="button" className="btn" onClick={openCreate}>
          Add Account
        </button>
      </header>

      {error && <p className="error-text">{error}</p>}
      {loading ? <p>Loading bank accounts...</p> : <CrudTable columns={columns} rows={accounts} onEdit={openEdit} onDelete={handleDelete} />}

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
        onClose={closeModal}
        onSubmit={handleSubmit}
      />

      <Link className="inline-link" to={`/families/${familyId}/members`}>
        Back to members
      </Link>
    </section>
  );
}

