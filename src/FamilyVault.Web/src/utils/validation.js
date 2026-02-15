export function isEmail(value) {
  if (!value) {
    return false;
  }
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
}

export function validateSignup(values) {
  const errors = {};
  if (!values.firstName?.trim()) errors.firstName = "First name is required.";
  if (values.firstName && values.firstName.length > 50) errors.firstName = "First name cannot exceed 50 characters.";
  if (values.lastName && values.lastName.length > 50) errors.lastName = "Last name cannot exceed 50 characters.";
  if (!values.email?.trim()) errors.email = "Email is required.";
  else if (!isEmail(values.email)) errors.email = "Invalid email format.";
  if (!values.username?.trim()) errors.username = "Username is required.";
  if (!values.password) errors.password = "Password is required.";
  else {
    if (values.password.length < 8) errors.password = "Password must be at least 8 characters long.";
    else if (!/[A-Z]/.test(values.password)) errors.password = "Password must contain at least one uppercase letter.";
    else if (!/[a-z]/.test(values.password)) errors.password = "Password must contain at least one lowercase letter.";
    else if (!/[0-9]/.test(values.password)) errors.password = "Password must contain at least one number.";
    else if (!/[^a-zA-Z0-9]/.test(values.password)) errors.password = "Password must contain at least one special character.";
  }
  if (values.countryCode && values.countryCode.length > 5) errors.countryCode = "Country code cannot exceed 5 characters.";
  if (values.mobile && values.mobile.length > 10) errors.mobile = "Mobile number cannot exceed 10 characters.";
  if (values.mobile && !values.countryCode) errors.countryCode = "Country code is required when mobile number is provided.";
  if (values.countryCode && !values.mobile) errors.mobile = "Mobile number is required when country code is provided.";
  if (values.countryCode && !values.countryCode.startsWith("+")) errors.countryCode = "Country code must start with '+'.";
  return errors;
}

export function validateFamily(values) {
  const errors = {};
  if (!values.familyName?.trim()) errors.familyName = "Family name is required.";
  if (values.familyName && values.familyName.length > 100) errors.familyName = "Family name cannot exceed 100 characters.";
  return errors;
}

export function validateFamilyMember(values) {
  const errors = {};
  if (!values.firstName?.trim()) errors.firstName = "First name is required.";
  if (values.firstName && values.firstName.length > 50) errors.firstName = "First name cannot exceed 50 characters.";
  if (values.lastName && values.lastName.length > 50) errors.lastName = "Last name cannot exceed 50 characters.";
  if (!values.relationshipType && values.relationshipType !== 0) errors.relationshipType = "Relationship is required.";
  if (Number(values.relationshipType) === 0) errors.relationshipType = "Relationship type cannot be 'Self' for family members.";
  if (values.email && !isEmail(values.email)) errors.email = "Invalid email format.";
  if (values.countryCode && values.countryCode.length > 5) errors.countryCode = "Country code cannot exceed 5 characters.";
  if (values.mobile && values.mobile.length > 10) errors.mobile = "Mobile number cannot exceed 10 characters.";
  if (values.pan && values.pan.length > 10) errors.pan = "PAN cannot exceed 10 characters.";
  if (values.mobile && !values.countryCode) errors.countryCode = "Country code is required when mobile number is provided.";
  if (values.countryCode && !values.mobile) errors.mobile = "Mobile number is required when country code is provided.";
  if (values.countryCode && !values.countryCode.startsWith("+")) errors.countryCode = "Country code must start with '+'.";
  if (values.pan && !/^[A-Z]{5}[0-9]{4}[A-Z]$/.test(values.pan)) errors.pan = "PAN must be in the corect format";
  if (values.aadhar && !/^\d{12}$/.test(values.aadhar)) errors.aadhar = "Aadhaar number must be exactly 12 digits.";
  if (values.dateOfBirth && new Date(values.dateOfBirth) >= new Date()) errors.dateOfBirth = "Date of birth must be in the past.";
  return errors;
}

export function validateDocument(values, { requireFile = false } = {}) {
  const errors = {};
  const allowedFileExtensions = [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp"];
  const maxFileSizeBytes = 10 * 1024 * 1024;
  if (!values.documentNumber?.trim()) errors.documentNumber = "Document number is required.";
  if (values.documentType === "" || values.documentType === null || values.documentType === undefined) errors.documentType = "Type is required.";
  if (values.issueDate && values.expiryDate && new Date(values.expiryDate) <= new Date(values.issueDate)) {
    errors.expiryDate = "Expiry date must be greater than issue date.";
  }
  if (values.expiryDate && new Date(values.expiryDate) <= new Date()) {
    errors.expiryDate = "Expiry date must be in the future.";
  }
  if (Number(values.documentType) === 15 && values.documentNumber && !/^[A-Z]{5}[0-9]{4}[A-Z]$/.test(values.documentNumber)) {
    errors.documentNumber = "PAN must be in the corect format";
  }
  if (requireFile && !values.file) {
    errors.file = "File is required.";
  }
  if (values.file?.name) {
    const lower = values.file.name.toLowerCase();
    const hasAllowedExt = allowedFileExtensions.some((ext) => lower.endsWith(ext));
    if (!hasAllowedExt) {
      errors.file = "Only PDF, Word, Excel and image files are allowed.";
    }
    if (values.file.size > maxFileSizeBytes) {
      errors.file = "File size cannot exceed 10 MB.";
    }
  }
  return errors;
}

export function validateBankAccount(values) {
  const errors = {};
  if (!values.bankName?.trim()) errors.bankName = "Bank name is required.";
  if (values.bankName && values.bankName.length > 150) errors.bankName = "Bank name must be at most 150 characters.";
  if (!values.accountNumber?.trim()) errors.accountNumber = "Account number is required.";
  else if (!/^\d{6,20}$/.test(values.accountNumber)) errors.accountNumber = "Account number must contain 6 to 20 digits.";
  if (values.accountType === "" || Number(values.accountType) === 0) errors.accountType = "Account type is required.";
  if (values.accountHolderName && values.accountHolderName.length > 150) errors.accountHolderName = "Account holder name must be at most 150 characters.";
  if (values.ifsc && !/^[A-Z]{4}0[A-Z0-9]{6}$/.test(values.ifsc)) errors.ifsc = "IFSC must be in valid format.";
  if (values.branch && values.branch.length > 150) errors.branch = "Branch must be at most 150 characters.";
  if (values.nomineeName && values.nomineeName.length > 150) errors.nomineeName = "Nominee name must be at most 150 characters.";
  return errors;
}

export function validateFixedDeposit(values) {
  const errors = {};
  if (!values.institutionName?.trim()) errors.institutionName = "Institution name is required.";
  if (!values.depositNumber?.trim()) errors.depositNumber = "Deposit number is required.";
  if (values.depositType === "" || Number(values.depositType) === 0) errors.depositType = "Deposit type is required.";
  if (!values.principalAmount && values.principalAmount !== 0) errors.principalAmount = "Principal amount is required.";
  if (Number(values.principalAmount) <= 0) errors.principalAmount = "Principal amount must be greater than 0.";
  if (!values.interestRate && values.interestRate !== 0) errors.interestRate = "Interest rate is required.";
  if (Number(values.interestRate) < 0) errors.interestRate = "Interest rate cannot be negative.";
  if (!values.startDate) errors.startDate = "Start date is required.";
  if (!values.maturityDate) errors.maturityDate = "Maturity date is required.";
  if (values.startDate && values.maturityDate && new Date(values.maturityDate) < new Date(values.startDate)) {
    errors.maturityDate = "Maturity date must be after start date.";
  }
  if (values.nomineeName && values.nomineeName.length > 150) errors.nomineeName = "Nominee name must be at most 150 characters.";
  return errors;
}

export function validateLifeInsurance(values) {
  const errors = {};
  if (!values.insurerName?.trim()) errors.insurerName = "Insurer name is required.";
  if (!values.policyNumber?.trim()) errors.policyNumber = "Policy number is required.";
  if (values.policyType === "" || Number(values.policyType) === 0) errors.policyType = "Policy type is required.";
  if (!values.coverAmount && values.coverAmount !== 0) errors.coverAmount = "Cover amount is required.";
  if (Number(values.coverAmount) <= 0) errors.coverAmount = "Cover amount must be greater than 0.";
  if (!values.premiumAmount && values.premiumAmount !== 0) errors.premiumAmount = "Premium amount is required.";
  if (Number(values.premiumAmount) <= 0) errors.premiumAmount = "Premium amount must be greater than 0.";
  if (values.premiumFrequency === "" || Number(values.premiumFrequency) === 0) errors.premiumFrequency = "Premium frequency is required.";
  if (values.status === "" || Number(values.status) === 0) errors.status = "Status is required.";
  if (!values.policyStartDate) errors.policyStartDate = "Policy start date is required.";
  return errors;
}

export function validateMediclaim(values) {
  const errors = {};
  if (!values.insurerName?.trim()) errors.insurerName = "Insurer name is required.";
  if (!values.policyNumber?.trim()) errors.policyNumber = "Policy number is required.";
  if (values.policyType === "" || Number(values.policyType) === 0) errors.policyType = "Policy type is required.";
  if (!values.sumInsured && values.sumInsured !== 0) errors.sumInsured = "Sum insured is required.";
  if (Number(values.sumInsured) <= 0) errors.sumInsured = "Sum insured must be greater than 0.";
  if (!values.premiumAmount && values.premiumAmount !== 0) errors.premiumAmount = "Premium amount is required.";
  if (Number(values.premiumAmount) <= 0) errors.premiumAmount = "Premium amount must be greater than 0.";
  if (values.status === "" || Number(values.status) === 0) errors.status = "Status is required.";
  if (!values.policyStartDate) errors.policyStartDate = "Policy start date is required.";
  if (!values.policyEndDate) errors.policyEndDate = "Policy end date is required.";
  if (values.policyStartDate && values.policyEndDate && new Date(values.policyEndDate) < new Date(values.policyStartDate)) {
    errors.policyEndDate = "Policy end date must be after start date.";
  }
  return errors;
}

export function validateDematAccount(values) {
  const errors = {};
  if (!values.brokerName?.trim()) errors.brokerName = "Broker name is required.";
  if (values.depository === "" || Number(values.depository) === 0) errors.depository = "Depository is required.";
  if (!values.dpId?.trim()) errors.dpId = "DP ID is required.";
  if (!values.clientId?.trim()) errors.clientId = "Client ID is required.";
  if (values.holdingPattern === "" || Number(values.holdingPattern) === 0) errors.holdingPattern = "Holding pattern is required.";
  return errors;
}

export function validateMutualFund(values) {
  const errors = {};
  if (!values.amcName?.trim()) errors.amcName = "AMC name is required.";
  if (!values.folioNumber?.trim()) errors.folioNumber = "Folio number is required.";
  if (!values.schemeName?.trim()) errors.schemeName = "Scheme name is required.";
  if (values.schemeType === "" || Number(values.schemeType) === 0) errors.schemeType = "Scheme type is required.";
  if (values.planType === "" || Number(values.planType) === 0) errors.planType = "Plan type is required.";
  if (values.optionType === "" || Number(values.optionType) === 0) errors.optionType = "Option type is required.";
  if (values.investmentMode === "" || Number(values.investmentMode) === 0) errors.investmentMode = "Investment mode is required.";
  return errors;
}
