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
  return errors;
}
