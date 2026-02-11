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

export function optionLabelByValue(options, value) {
  return options.find((option) => option.value === value)?.label ?? String(value);
}
