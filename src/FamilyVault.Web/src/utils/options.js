export const relationshipOptions = [
  { label: "Self", value: 0 },
  { label: "Spouse", value: 1 },
  { label: "Parent", value: 2 },
  { label: "Child", value: 3 },
  { label: "Sibling", value: 4 },
  { label: "Partner", value: 10 },
  { label: "Other", value: 99 }
];

export const bloodGroupOptions = [
  { label: "A+", value: 0 },
  { label: "A-", value: 1 },
  { label: "B+", value: 2 },
  { label: "B-", value: 3 },
  { label: "AB+", value: 4 },
  { label: "AB-", value: 5 },
  { label: "O+", value: 6 },
  { label: "O-", value: 7 }
];

export const documentTypeOptions = [
  { label: "Unspecified", value: 0 },
  { label: "Passport", value: 1 },
  { label: "Driver License", value: 2 },
  { label: "Birth Certificate", value: 3 },
  { label: "Marriage Certificate", value: 4 },
  { label: "Death Certificate", value: 5 },
  { label: "Insurance Policy", value: 12 },
  { label: "PAN", value: 15 },
  { label: "Other", value: 99 }
];

export const accountTypeOptions = [
  { label: "Unknown", value: 0 },
  { label: "Savings", value: 1 },
  { label: "Current", value: 2 },
  { label: "Salary", value: 3 },
  { label: "Fixed Deposit", value: 4 },
  { label: "Recurring Deposit", value: 5 }
];

export const financialCategoryOptions = [
  { label: "Bank Accounts", value: "bank-accounts" },
  { label: "Fixed Deposits", value: "fd" },
  { label: "Life Insurance", value: "life-insurance" },
  { label: "Mediclaim", value: "mediclaim" },
  { label: "Demat Accounts", value: "demat-accounts" },
  { label: "Mutual Funds", value: "mutual-funds" }
];

export const fixedDepositTypeOptions = [
  { label: "Unknown", value: 0 },
  { label: "Cumulative", value: 1 },
  { label: "Non Cumulative", value: 2 },
  { label: "Tax Saving", value: 3 }
];

export const lifeInsurancePolicyTypeOptions = [
  { label: "Unknown", value: 0 },
  { label: "Term", value: 1 },
  { label: "Endowment", value: 2 },
  { label: "ULIP", value: 3 },
  { label: "Whole Life", value: 4 }
];

export const premiumFrequencyOptions = [
  { label: "Unknown", value: 0 },
  { label: "Monthly", value: 1 },
  { label: "Quarterly", value: 2 },
  { label: "Half Yearly", value: 3 },
  { label: "Yearly", value: 4 },
  { label: "Single", value: 5 }
];

export const policyStatusOptions = [
  { label: "Unknown", value: 0 },
  { label: "Active", value: 1 },
  { label: "Matured", value: 2 },
  { label: "Lapsed", value: 3 },
  { label: "Closed", value: 4 }
];

export const mediclaimPolicyTypeOptions = [
  { label: "Unknown", value: 0 },
  { label: "Individual", value: 1 },
  { label: "Family Floater", value: 2 },
  { label: "Group", value: 3 },
  { label: "Top Up", value: 4 }
];

export const depositoryTypeOptions = [
  { label: "Unknown", value: 0 },
  { label: "NSDL", value: 1 },
  { label: "CDSL", value: 2 }
];

export const holdingPatternTypeOptions = [
  { label: "Unknown", value: 0 },
  { label: "Single", value: 1 },
  { label: "Joint", value: 2 },
  { label: "Either or Survivor", value: 3 }
];

export const mutualFundSchemeTypeOptions = [
  { label: "Unknown", value: 0 },
  { label: "Equity", value: 1 },
  { label: "Debt", value: 2 },
  { label: "Hybrid", value: 3 },
  { label: "Index", value: 4 },
  { label: "Other", value: 5 }
];

export const mutualFundPlanTypeOptions = [
  { label: "Unknown", value: 0 },
  { label: "Direct", value: 1 },
  { label: "Regular", value: 2 }
];

export const mutualFundOptionTypeOptions = [
  { label: "Unknown", value: 0 },
  { label: "Growth", value: 1 },
  { label: "IDCW", value: 2 }
];

export const investmentModeTypeOptions = [
  { label: "Unknown", value: 0 },
  { label: "Lump Sum", value: 1 },
  { label: "SIP", value: 2 },
  { label: "Both", value: 3 }
];

export function optionLabelByValue(options, value) {
  return options.find((option) => option.value === value)?.label ?? String(value);
}
