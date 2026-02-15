import React, { useEffect, useMemo, useState } from "react";
import { Link, useSearchParams, useParams } from "react-router-dom";
import DataGridTable from "../components/ui/DataGridTable";
import FormModal from "../components/ui/FormModal";
import ConfirmModal from "../components/ui/ConfirmModal";
import HelpTip from "../components/ui/HelpTip";
import { BackIcon, PlusIcon } from "../components/ui/Icons";
import { useAuth } from "../context/AuthContext";
import {
  createFinancialDetail,
  deleteFinancialDetail,
  getFinancialDetails,
  updateFinancialDetail
} from "../services/financialDetailsService";
import {
  accountTypeOptions,
  depositoryTypeOptions,
  financialCategoryOptions,
  fixedDepositTypeOptions,
  holdingPatternTypeOptions,
  investmentModeTypeOptions,
  lifeInsurancePolicyTypeOptions,
  mediclaimPolicyTypeOptions,
  mutualFundOptionTypeOptions,
  mutualFundPlanTypeOptions,
  mutualFundSchemeTypeOptions,
  optionLabelByValue,
  policyStatusOptions,
  premiumFrequencyOptions
} from "../utils/options";
import { unwrapData } from "../utils/response";
import {
  validateBankAccount,
  validateDematAccount,
  validateFixedDeposit,
  validateLifeInsurance,
  validateMediclaim,
  validateMutualFund
} from "../utils/validation";

const CATEGORY_CONFIG = {
  "bank-accounts": {
    title: "Bank Accounts",
    addButtonText: "Add Bank Account",
    empty: {
      bankName: "",
      accountNumber: "",
      accountType: "",
      accountHolderName: "",
      ifsc: "",
      branch: "",
      nomineeName: ""
    },
    columns: [
      { key: "bankName", header: "Bank" },
      { key: "accountType", header: "Type", render: (row) => optionLabelByValue(accountTypeOptions, row.accountType) },
      { key: "accountMask", header: "Account", render: (row) => (row.accountNumberLast4 ? `****${row.accountNumberLast4}` : row.accountNumber) },
      { key: "accountHolderName", header: "Holder" },
      { key: "ifsc", header: "IFSC" },
      { key: "branch", header: "Branch" },
      { key: "nomineeName", header: "Nominee" }
    ],
    fields: [
      { name: "bankName", label: "Bank Name", required: true },
      { name: "accountNumber", label: "Account Number", required: true },
      { name: "accountType", label: "Account Type", type: "select", options: accountTypeOptions, required: true },
      { name: "accountHolderName", label: "Account Holder Name" },
      { name: "ifsc", label: "IFSC" },
      { name: "branch", label: "Branch" },
      { name: "nomineeName", label: "Nominee Name" }
    ],
    validate: validateBankAccount,
    toPayload: (values, memberId) => ({
      bankName: values.bankName,
      accountNumber: values.accountNumber,
      accountType: Number(values.accountType),
      accountHolderName: values.accountHolderName || null,
      ifsc: values.ifsc || null,
      branch: values.branch || null,
      nomineeName: values.nomineeName || null,
      familyMemberId: memberId
    }),
    toFormValues: (item) => ({
      bankName: item.bankName ?? "",
      accountNumber: item.accountNumber ?? "",
      accountType: String(item.accountType ?? ""),
      accountHolderName: item.accountHolderName ?? "",
      ifsc: item.ifsc ?? "",
      branch: item.branch ?? "",
      nomineeName: item.nomineeName ?? ""
    })
  },
  fd: {
    title: "Fixed Deposits",
    addButtonText: "Add FD",
    empty: {
      institutionName: "",
      depositNumber: "",
      depositType: "",
      principalAmount: "",
      interestRate: "",
      startDate: "",
      maturityDate: "",
      maturityAmount: "",
      isAutoRenewal: "false",
      nomineeName: ""
    },
    columns: [
      { key: "institutionName", header: "Institution" },
      { key: "depositType", header: "Type", render: (row) => optionLabelByValue(fixedDepositTypeOptions, row.depositType) },
      { key: "depositMask", header: "Deposit #", render: (row) => (row.depositNumberLast4 ? `****${row.depositNumberLast4}` : row.depositNumber) },
      { key: "principalAmount", header: "Principal" },
      { key: "interestRate", header: "Rate %" },
      { key: "startDate", header: "Start" },
      { key: "maturityDate", header: "Maturity" },
      { key: "maturityAmount", header: "Maturity Amt" },
      { key: "isAutoRenewal", header: "Auto Renewal", render: (row) => (row.isAutoRenewal ? "Yes" : "No") },
      { key: "nomineeName", header: "Nominee" }
    ],
    fields: [
      { name: "institutionName", label: "Institution Name", required: true },
      { name: "depositNumber", label: "Deposit Number", required: true },
      { name: "depositType", label: "Deposit Type", type: "select", options: fixedDepositTypeOptions, required: true },
      { name: "principalAmount", label: "Principal Amount", type: "number", required: true },
      { name: "interestRate", label: "Interest Rate (%)", type: "number", required: true },
      { name: "startDate", label: "Start Date", type: "date", required: true },
      { name: "maturityDate", label: "Maturity Date", type: "date", required: true },
      { name: "maturityAmount", label: "Maturity Amount", type: "number" },
      { name: "isAutoRenewal", label: "Auto Renewal", type: "select", options: [{ label: "No", value: "false" }, { label: "Yes", value: "true" }] },
      { name: "nomineeName", label: "Nominee Name" }
    ],
    validate: validateFixedDeposit,
    toPayload: (values, memberId) => ({
      institutionName: values.institutionName,
      depositNumber: values.depositNumber,
      depositType: Number(values.depositType),
      principalAmount: Number(values.principalAmount),
      interestRate: Number(values.interestRate),
      startDate: values.startDate,
      maturityDate: values.maturityDate,
      maturityAmount: values.maturityAmount ? Number(values.maturityAmount) : null,
      isAutoRenewal: values.isAutoRenewal === "true",
      nomineeName: values.nomineeName || null,
      familyMemberId: memberId
    }),
    toFormValues: (item) => ({
      institutionName: item.institutionName ?? "",
      depositNumber: item.depositNumber ?? "",
      depositType: String(item.depositType ?? ""),
      principalAmount: item.principalAmount ?? "",
      interestRate: item.interestRate ?? "",
      startDate: item.startDate ?? "",
      maturityDate: item.maturityDate ?? "",
      maturityAmount: item.maturityAmount ?? "",
      isAutoRenewal: String(Boolean(item.isAutoRenewal)),
      nomineeName: item.nomineeName ?? ""
    })
  },
  "life-insurance": {
    title: "Life Insurance",
    addButtonText: "Add Policy",
    empty: {
      insurerName: "",
      policyNumber: "",
      policyType: "",
      planName: "",
      coverAmount: "",
      premiumAmount: "",
      premiumFrequency: "",
      policyStartDate: "",
      policyEndDate: "",
      maturityDate: "",
      nomineeName: "",
      agentName: "",
      status: ""
    },
    columns: [
      { key: "insurerName", header: "Insurer" },
      { key: "policyType", header: "Type", render: (row) => optionLabelByValue(lifeInsurancePolicyTypeOptions, row.policyType) },
      { key: "policyMask", header: "Policy #", render: (row) => (row.policyNumberLast4 ? `****${row.policyNumberLast4}` : row.policyNumber) },
      { key: "planName", header: "Plan" },
      { key: "coverAmount", header: "Cover" },
      { key: "premiumAmount", header: "Premium" },
      { key: "premiumFrequency", header: "Frequency", render: (row) => optionLabelByValue(premiumFrequencyOptions, row.premiumFrequency) },
      { key: "policyStartDate", header: "Start" },
      { key: "policyEndDate", header: "End" },
      { key: "maturityDate", header: "Maturity" },
      { key: "nomineeName", header: "Nominee" },
      { key: "agentName", header: "Agent" },
      { key: "status", header: "Status", render: (row) => optionLabelByValue(policyStatusOptions, row.status) }
    ],
    fields: [
      { name: "insurerName", label: "Insurer Name", required: true },
      { name: "policyNumber", label: "Policy Number", required: true },
      { name: "policyType", label: "Policy Type", type: "select", options: lifeInsurancePolicyTypeOptions, required: true },
      { name: "planName", label: "Plan Name" },
      { name: "coverAmount", label: "Cover Amount", type: "number", required: true },
      { name: "premiumAmount", label: "Premium Amount", type: "number", required: true },
      { name: "premiumFrequency", label: "Premium Frequency", type: "select", options: premiumFrequencyOptions, required: true },
      { name: "policyStartDate", label: "Policy Start Date", type: "date", required: true },
      { name: "policyEndDate", label: "Policy End Date", type: "date" },
      { name: "maturityDate", label: "Maturity Date", type: "date" },
      { name: "nomineeName", label: "Nominee Name" },
      { name: "agentName", label: "Agent Name" },
      { name: "status", label: "Status", type: "select", options: policyStatusOptions, required: true }
    ],
    validate: validateLifeInsurance,
    toPayload: (values, memberId) => ({
      insurerName: values.insurerName,
      policyNumber: values.policyNumber,
      policyType: Number(values.policyType),
      planName: values.planName || null,
      coverAmount: Number(values.coverAmount),
      premiumAmount: Number(values.premiumAmount),
      premiumFrequency: Number(values.premiumFrequency),
      policyStartDate: values.policyStartDate,
      policyEndDate: values.policyEndDate || null,
      maturityDate: values.maturityDate || null,
      nomineeName: values.nomineeName || null,
      agentName: values.agentName || null,
      status: Number(values.status),
      familyMemberId: memberId
    }),
    toFormValues: (item) => ({
      insurerName: item.insurerName ?? "",
      policyNumber: item.policyNumber ?? "",
      policyType: String(item.policyType ?? ""),
      planName: item.planName ?? "",
      coverAmount: item.coverAmount ?? "",
      premiumAmount: item.premiumAmount ?? "",
      premiumFrequency: String(item.premiumFrequency ?? ""),
      policyStartDate: item.policyStartDate ?? "",
      policyEndDate: item.policyEndDate ?? "",
      maturityDate: item.maturityDate ?? "",
      nomineeName: item.nomineeName ?? "",
      agentName: item.agentName ?? "",
      status: String(item.status ?? "")
    })
  },
  mediclaim: {
    title: "Mediclaim",
    addButtonText: "Add Mediclaim",
    empty: {
      insurerName: "",
      policyNumber: "",
      policyType: "",
      planName: "",
      sumInsured: "",
      premiumAmount: "",
      policyStartDate: "",
      policyEndDate: "",
      tpaName: "",
      hospitalNetworkUrl: "",
      status: ""
    },
    columns: [
      { key: "insurerName", header: "Insurer" },
      { key: "policyType", header: "Type", render: (row) => optionLabelByValue(mediclaimPolicyTypeOptions, row.policyType) },
      { key: "policyMask", header: "Policy #", render: (row) => (row.policyNumberLast4 ? `****${row.policyNumberLast4}` : row.policyNumber) },
      { key: "planName", header: "Plan" },
      { key: "sumInsured", header: "Sum Insured" },
      { key: "premiumAmount", header: "Premium" },
      { key: "policyStartDate", header: "Start" },
      { key: "policyEndDate", header: "End" },
      { key: "tpaName", header: "TPA" },
      { key: "hospitalNetworkUrl", header: "Hospital Network" },
      { key: "status", header: "Status", render: (row) => optionLabelByValue(policyStatusOptions, row.status) }
    ],
    fields: [
      { name: "insurerName", label: "Insurer Name", required: true },
      { name: "policyNumber", label: "Policy Number", required: true },
      { name: "policyType", label: "Policy Type", type: "select", options: mediclaimPolicyTypeOptions, required: true },
      { name: "planName", label: "Plan Name" },
      { name: "sumInsured", label: "Sum Insured", type: "number", required: true },
      { name: "premiumAmount", label: "Premium Amount", type: "number", required: true },
      { name: "policyStartDate", label: "Policy Start Date", type: "date", required: true },
      { name: "policyEndDate", label: "Policy End Date", type: "date", required: true },
      { name: "tpaName", label: "TPA Name" },
      { name: "hospitalNetworkUrl", label: "Hospital Network URL" },
      { name: "status", label: "Status", type: "select", options: policyStatusOptions, required: true }
    ],
    validate: validateMediclaim,
    toPayload: (values, memberId) => ({
      insurerName: values.insurerName,
      policyNumber: values.policyNumber,
      policyType: Number(values.policyType),
      planName: values.planName || null,
      sumInsured: Number(values.sumInsured),
      premiumAmount: Number(values.premiumAmount),
      policyStartDate: values.policyStartDate,
      policyEndDate: values.policyEndDate,
      tpaName: values.tpaName || null,
      hospitalNetworkUrl: values.hospitalNetworkUrl || null,
      status: Number(values.status),
      familyMemberId: memberId
    }),
    toFormValues: (item) => ({
      insurerName: item.insurerName ?? "",
      policyNumber: item.policyNumber ?? "",
      policyType: String(item.policyType ?? ""),
      planName: item.planName ?? "",
      sumInsured: item.sumInsured ?? "",
      premiumAmount: item.premiumAmount ?? "",
      policyStartDate: item.policyStartDate ?? "",
      policyEndDate: item.policyEndDate ?? "",
      tpaName: item.tpaName ?? "",
      hospitalNetworkUrl: item.hospitalNetworkUrl ?? "",
      status: String(item.status ?? "")
    })
  },
  "demat-accounts": {
    title: "Demat Accounts",
    addButtonText: "Add Demat",
    empty: {
      brokerName: "",
      depository: "",
      dpId: "",
      clientId: "",
      boId: "",
      holdingPattern: "",
      nomineeName: ""
    },
    columns: [
      { key: "brokerName", header: "Broker" },
      { key: "depository", header: "Depository", render: (row) => optionLabelByValue(depositoryTypeOptions, row.depository) },
      { key: "clientMask", header: "Client ID", render: (row) => (row.clientIdLast4 ? `****${row.clientIdLast4}` : row.clientId) },
      { key: "boIdMask", header: "BO ID", render: (row) => (row.boIdLast4 ? `****${row.boIdLast4}` : row.boId) },
      { key: "dpId", header: "DP ID" },
      { key: "holdingPattern", header: "Pattern", render: (row) => optionLabelByValue(holdingPatternTypeOptions, row.holdingPattern) },
      { key: "nomineeName", header: "Nominee" }
    ],
    fields: [
      { name: "brokerName", label: "Broker Name", required: true },
      { name: "depository", label: "Depository", type: "select", options: depositoryTypeOptions, required: true },
      { name: "dpId", label: "DP ID", required: true },
      { name: "clientId", label: "Client ID", required: true },
      { name: "boId", label: "BO ID" },
      { name: "holdingPattern", label: "Holding Pattern", type: "select", options: holdingPatternTypeOptions, required: true },
      { name: "nomineeName", label: "Nominee Name" }
    ],
    validate: validateDematAccount,
    toPayload: (values, memberId) => ({
      brokerName: values.brokerName,
      depository: Number(values.depository),
      dpId: values.dpId,
      clientId: values.clientId,
      boId: values.boId || null,
      holdingPattern: Number(values.holdingPattern),
      nomineeName: values.nomineeName || null,
      familyMemberId: memberId
    }),
    toFormValues: (item) => ({
      brokerName: item.brokerName ?? "",
      depository: String(item.depository ?? ""),
      dpId: item.dpId ?? "",
      clientId: item.clientId ?? "",
      boId: item.boId ?? "",
      holdingPattern: String(item.holdingPattern ?? ""),
      nomineeName: item.nomineeName ?? ""
    })
  },
  "mutual-funds": {
    title: "Mutual Funds",
    addButtonText: "Add Mutual Fund",
    empty: {
      amcName: "",
      folioNumber: "",
      schemeName: "",
      schemeType: "",
      planType: "",
      optionType: "",
      investmentMode: "",
      units: "",
      investedAmount: "",
      currentValue: "",
      startDate: "",
      nomineeName: ""
    },
    columns: [
      { key: "amcName", header: "AMC" },
      { key: "schemeName", header: "Scheme" },
      { key: "folioMask", header: "Folio", render: (row) => (row.folioNumberLast4 ? `****${row.folioNumberLast4}` : row.folioNumber) },
      { key: "schemeType", header: "Scheme Type", render: (row) => optionLabelByValue(mutualFundSchemeTypeOptions, row.schemeType) },
      { key: "planType", header: "Plan", render: (row) => optionLabelByValue(mutualFundPlanTypeOptions, row.planType) },
      { key: "optionType", header: "Option", render: (row) => optionLabelByValue(mutualFundOptionTypeOptions, row.optionType) },
      { key: "investmentMode", header: "Mode", render: (row) => optionLabelByValue(investmentModeTypeOptions, row.investmentMode) },
      { key: "units", header: "Units" },
      { key: "investedAmount", header: "Invested" },
      { key: "currentValue", header: "Current Value" },
      { key: "startDate", header: "Start" },
      { key: "nomineeName", header: "Nominee" }
    ],
    fields: [
      { name: "amcName", label: "AMC Name", required: true },
      { name: "folioNumber", label: "Folio Number", required: true },
      { name: "schemeName", label: "Scheme Name", required: true },
      { name: "schemeType", label: "Scheme Type", type: "select", options: mutualFundSchemeTypeOptions, required: true },
      { name: "planType", label: "Plan Type", type: "select", options: mutualFundPlanTypeOptions, required: true },
      { name: "optionType", label: "Option Type", type: "select", options: mutualFundOptionTypeOptions, required: true },
      { name: "investmentMode", label: "Investment Mode", type: "select", options: investmentModeTypeOptions, required: true },
      { name: "units", label: "Units", type: "number" },
      { name: "investedAmount", label: "Invested Amount", type: "number" },
      { name: "currentValue", label: "Current Value", type: "number" },
      { name: "startDate", label: "Start Date", type: "date" },
      { name: "nomineeName", label: "Nominee Name" }
    ],
    validate: validateMutualFund,
    toPayload: (values, memberId) => ({
      amcName: values.amcName,
      folioNumber: values.folioNumber,
      schemeName: values.schemeName,
      schemeType: Number(values.schemeType),
      planType: Number(values.planType),
      optionType: Number(values.optionType),
      investmentMode: Number(values.investmentMode),
      units: values.units ? Number(values.units) : null,
      investedAmount: values.investedAmount ? Number(values.investedAmount) : null,
      currentValue: values.currentValue ? Number(values.currentValue) : null,
      startDate: values.startDate || null,
      nomineeName: values.nomineeName || null,
      familyMemberId: memberId
    }),
    toFormValues: (item) => ({
      amcName: item.amcName ?? "",
      folioNumber: item.folioNumber ?? "",
      schemeName: item.schemeName ?? "",
      schemeType: String(item.schemeType ?? ""),
      planType: String(item.planType ?? ""),
      optionType: String(item.optionType ?? ""),
      investmentMode: String(item.investmentMode ?? ""),
      units: item.units ?? "",
      investedAmount: item.investedAmount ?? "",
      currentValue: item.currentValue ?? "",
      startDate: item.startDate ?? "",
      nomineeName: item.nomineeName ?? ""
    })
  }
};

const PREVIEW_DATA = {
  "bank-accounts": [{ id: "preview-bank-1", bankName: "State Bank", accountType: 1, accountNumberLast4: "9012", ifsc: "SBIN0001234" }],
  fd: [{ id: "preview-fd-1", institutionName: "HDFC Bank", depositType: 1, depositNumberLast4: "1122", maturityDate: "2027-03-31" }],
  "life-insurance": [{ id: "preview-life-1", insurerName: "LIC", policyType: 1, policyNumberLast4: "9988", status: 1 }],
  mediclaim: [{ id: "preview-med-1", insurerName: "Star Health", policyType: 2, policyNumberLast4: "4455", status: 1 }],
  "demat-accounts": [{ id: "preview-demat-1", brokerName: "Zerodha", depository: 2, clientIdLast4: "1020", dpId: "12012300" }],
  "mutual-funds": [{ id: "preview-mf-1", amcName: "HDFC MF", schemeName: "Balanced Advantage", folioNumberLast4: "7890", planType: 1 }]
};

export default function FinancialDetailsPage() {
  const { familyId, memberId } = useParams();
  const [searchParams] = useSearchParams();
  const { token, isPreviewMode } = useAuth();
  const initialCategory = searchParams.get("category");
  const hasCategory = financialCategoryOptions.some((option) => option.value === initialCategory);
  const [category, setCategory] = useState(hasCategory ? initialCategory : "bank-accounts");
  const [records, setRecords] = useState([]);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingRecord, setEditingRecord] = useState(null);
  const [deletingRecord, setDeletingRecord] = useState(null);
  const [searchText, setSearchText] = useState("");

  const config = CATEGORY_CONFIG[category];

  const loadRecords = async () => {
    if (isPreviewMode) {
      setRecords(PREVIEW_DATA[category] ?? []);
      setLoading(false);
      return;
    }
    setError("");
    setLoading(true);

    try {
      const response = await getFinancialDetails(memberId, category, token);
      setRecords(unwrapData(response));
    } catch (requestError) {
      setError(requestError.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    setEditingRecord(null);
    setDeletingRecord(null);
    setModalOpen(false);
    loadRecords();
  }, [category, memberId, token, isPreviewMode]);

  useEffect(() => {
    const queryCategory = searchParams.get("category");
    const isValidQueryCategory = financialCategoryOptions.some((option) => option.value === queryCategory);
    if (isValidQueryCategory && queryCategory !== category) {
      setCategory(queryCategory);
    }
  }, [searchParams, category]);

  const openCreate = () => {
    setEditingRecord(null);
    setModalOpen(true);
  };

  const openEdit = (item) => {
    setEditingRecord(item);
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
    const payload = config.toPayload(values, memberId);

    try {
      if (editingRecord) {
        await updateFinancialDetail(memberId, category, editingRecord.id, { ...payload, id: editingRecord.id }, token);
      } else {
        await createFinancialDetail(memberId, category, payload, token);
      }
      closeModal();
      await loadRecords();
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const handleDelete = async (item) => {
    if (isPreviewMode) {
      window.alert("Preview mode: login to delete records.");
      return;
    }
    setDeletingRecord(item);
  };

  const confirmDelete = async () => {
    if (!deletingRecord) {
      return;
    }
    try {
      await deleteFinancialDetail(memberId, category, deletingRecord.id, token);
      setDeletingRecord(null);
      await loadRecords();
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const filteredRecords = useMemo(() => {
    const search = searchText.trim().toLowerCase();
    if (!search) {
      return records;
    }
    return records.filter((row) => JSON.stringify(row).toLowerCase().includes(search));
  }, [records, searchText]);

  return (
    <section>
      <header className="page-header">
        <div>
          <div className="heading-with-help">
            <h2>Financial Details</h2>
            <HelpTip text="Switch category to manage different financial records for this member." />
          </div>
          <p className="subtle page-intro">Records for Family {familyId} and Member {memberId}</p>
        </div>
        <button type="button" className="btn" onClick={openCreate}>
          <span className="btn-icon"><PlusIcon /></span>
          <span>{config.addButtonText}</span>
        </button>
      </header>

      {error && <p className="error-text">{error}</p>}
      {isPreviewMode && <p className="subtle">Preview mode is on. Login to enable CRUD.</p>}

      <div className="toolbar">
        <select value={category} onChange={(event) => setCategory(event.target.value)}>
          {financialCategoryOptions.map((option) => (
            <option key={option.value} value={option.value}>{option.label}</option>
          ))}
        </select>
        <input
          type="text"
          placeholder={`Search ${config.title.toLowerCase()}...`}
          value={searchText}
          onChange={(event) => setSearchText(event.target.value)}
        />
      </div>

      <h3 className="section-grid-title">{config.title}</h3>
      {loading ? <p>Loading {config.title.toLowerCase()}...</p> : <DataGridTable columns={config.columns} rows={filteredRecords} onEdit={openEdit} onDelete={handleDelete} />}

      <FormModal
        title={editingRecord ? `Edit ${config.title}` : config.addButtonText}
        isOpen={modalOpen}
        initialValues={editingRecord ? config.toFormValues(editingRecord) : config.empty}
        fields={config.fields}
        validate={config.validate}
        onClose={closeModal}
        onSubmit={handleSubmit}
      />

      <ConfirmModal
        isOpen={Boolean(deletingRecord)}
        title={`Delete ${config.title}`}
        message={deletingRecord ? "Are you sure you want to delete this record?" : ""}
        onCancel={() => setDeletingRecord(null)}
        onConfirm={confirmDelete}
      />

      <Link className="inline-link back-link" to={`/families/${familyId}/members`}>
        <span className="btn-icon"><BackIcon /></span>
        <span>Back to Members</span>
      </Link>
    </section>
  );
}
