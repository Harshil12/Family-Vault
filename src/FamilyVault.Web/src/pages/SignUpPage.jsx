import React, { useState } from "react";
import { Link } from "react-router-dom";
import { signUpRequest } from "../services/authService";

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
  const [form, setForm] = useState(initialForm);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const updateField = (event) => {
    const { name, value } = event.target;
    setForm((current) => ({ ...current, [name]: value }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError("");
    setMessage("");
    setLoading(true);

    try {
      await signUpRequest(form);
      setMessage("User created successfully. You can now login.");
      setForm(initialForm);
    } catch (requestError) {
      setError(requestError.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-shell">
      <section className="auth-card">
        <h1>Create user</h1>
        <p>Register the primary FamilyVault owner profile.</p>

        <form onSubmit={handleSubmit} className="form-grid">
          <label>
            <span>Username</span>
            <input name="username" value={form.username} onChange={updateField} required />
          </label>

          <label>
            <span>First name</span>
            <input name="firstName" value={form.firstName} onChange={updateField} required />
          </label>

          <label>
            <span>Last name</span>
            <input name="lastName" value={form.lastName} onChange={updateField} />
          </label>

          <label>
            <span>Email</span>
            <input type="email" name="email" value={form.email} onChange={updateField} required />
          </label>

          <label>
            <span>Password</span>
            <input type="password" name="password" value={form.password} onChange={updateField} required />
          </label>

          <label>
            <span>Country code</span>
            <input name="countryCode" value={form.countryCode} onChange={updateField} />
          </label>

          <label>
            <span>Mobile</span>
            <input name="mobile" value={form.mobile} onChange={updateField} />
          </label>

          {error && <p className="error-text">{error}</p>}
          {message && <p className="success-text">{message}</p>}

          <button type="submit" className="btn" disabled={loading}>
            {loading ? "Creating..." : "Create User"}
          </button>
        </form>

        <p className="subtle">
          Already have access? <Link to="/login">Back to login</Link>
        </p>
      </section>
    </div>
  );
}

