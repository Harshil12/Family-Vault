import React, { useState } from "react";
import { Link } from "react-router-dom";
import { signUpRequest } from "../services/authService";
import useCountryCodes from "../hooks/useCountryCodes";
import { validateSignup } from "../utils/validation";

const initialForm = {
  username: "",
  firstName: "",
  lastName: "",
  email: "",
  password: "",
  countryCode: "+91",
  mobile: ""
};

export default function SignUpPage() {
  const countryCodeOptions = useCountryCodes();
  const [form, setForm] = useState(initialForm);
  const [message, setMessage] = useState("");
  const [fieldErrors, setFieldErrors] = useState({});
  const [apiError, setApiError] = useState("");
  const [loading, setLoading] = useState(false);

  const updateField = (event) => {
    const { name, value } = event.target;
    setForm((current) => ({ ...current, [name]: value }));
    setFieldErrors((current) => {
      if (!current[name]) {
        return current;
      }
      const next = { ...current };
      delete next[name];
      return next;
    });
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setFieldErrors({});
    setApiError("");
    setMessage("");

    const validationErrors = validateSignup(form);
    if (Object.keys(validationErrors).length) {
      setFieldErrors(validationErrors);
      return;
    }

    setLoading(true);

    try {
      await signUpRequest(form);
      setMessage("User created successfully. You can now login.");
      setForm(initialForm);
    } catch (requestError) {
      if (requestError?.fieldErrors && typeof requestError.fieldErrors === "object") {
        setFieldErrors(requestError.fieldErrors);
      } else {
        setApiError(requestError.message);
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-shell">
      <section className="auth-card">
        <h1>Create Account</h1>
        <p>Set up the primary owner account for your Family Vault.</p>

        <form onSubmit={handleSubmit} className="form-grid">
          <label>
            <span>Username</span>
            <input name="username" value={form.username} onChange={updateField} required />
            {fieldErrors.username && <small className="field-error">{fieldErrors.username}</small>}
          </label>

          <label>
            <span>First name</span>
            <input name="firstName" value={form.firstName} onChange={updateField} required />
            {fieldErrors.firstName && <small className="field-error">{fieldErrors.firstName}</small>}
          </label>

          <label>
            <span>Last name</span>
            <input name="lastName" value={form.lastName} onChange={updateField} />
            {fieldErrors.lastName && <small className="field-error">{fieldErrors.lastName}</small>}
          </label>

          <label>
            <span>Email</span>
            <input type="email" name="email" value={form.email} onChange={updateField} required />
            {fieldErrors.email && <small className="field-error">{fieldErrors.email}</small>}
          </label>

          <label>
            <span>Password</span>
            <input type="password" name="password" value={form.password} onChange={updateField} required />
            {fieldErrors.password && <small className="field-error">{fieldErrors.password}</small>}
          </label>

          <label>
            <span>Country code</span>
            <select name="countryCode" value={form.countryCode} onChange={updateField}>
              <option value="">Select country code</option>
              {countryCodeOptions.map((option) => (
                <option key={`${option.label}-${option.value}`} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
            {fieldErrors.countryCode && <small className="field-error">{fieldErrors.countryCode}</small>}
          </label>

          <label>
            <span>Mobile</span>
            <input name="mobile" value={form.mobile} onChange={updateField} />
            {fieldErrors.mobile && <small className="field-error">{fieldErrors.mobile}</small>}
          </label>

          {apiError && <p className="error-text">{apiError}</p>}
          {message && <p className="success-text">{message}</p>}

          <button type="submit" className="btn" disabled={loading}>
            {loading ? "Creating..." : "Create Account"}
          </button>
        </form>

        <p className="subtle">
          Already have access? <Link to="/login">Back to Sign In</Link>
        </p>
      </section>
    </div>
  );
}


