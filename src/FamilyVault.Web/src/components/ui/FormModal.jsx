import React, { useEffect, useState } from "react";
import { CancelIcon, SaveIcon } from "./Icons";

export default function FormModal({ title, isOpen, initialValues, fields, onClose, onSubmit, validate }) {
  const [values, setValues] = useState(initialValues);
  const [errors, setErrors] = useState({});

  useEffect(() => {
    setValues(initialValues);
    setErrors({});
  }, [initialValues]);

  if (!isOpen) {
    return null;
  }

  const handleChange = (event) => {
    const { name, value } = event.target;
    setValues((current) => ({
      ...current,
      [name]: value
    }));
    setErrors((current) => {
      if (!current[name]) {
        return current;
      }
      const next = { ...current };
      delete next[name];
      return next;
    });
  };

  const handleSubmit = (event) => {
    event.preventDefault();
    const nextErrors = validate ? validate(values) : {};
    if (Object.keys(nextErrors).length) {
      setErrors(nextErrors);
      return;
    }
    onSubmit(values);
  };

  return (
    <div className="modal-backdrop">
      <div className="modal">
        <h3>{title}</h3>
        <form onSubmit={handleSubmit} className="form-grid">
          {fields.map((field) => (
            <label key={field.name}>
              <span>{field.label}</span>
              {field.type === "select" ? (
                <select name={field.name} value={values[field.name] ?? ""} onChange={handleChange} required={field.required}>
                  <option value="">Select</option>
                  {field.options.map((option) => (
                    <option key={`${option.label}-${option.value}`} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
              ) : (
                <input
                  type={field.type ?? "text"}
                  name={field.name}
                  value={values[field.name] ?? ""}
                  onChange={handleChange}
                  required={field.required}
                />
              )}
              {errors[field.name] && <small className="field-error">{errors[field.name]}</small>}
            </label>
          ))}
          {errors._form && <p className="error-text">{errors._form}</p>}

          <div className="modal-actions">
            <button type="button" className="btn ghost" onClick={onClose}>
              <span className="btn-icon"><CancelIcon /></span>
              <span>Cancel</span>
            </button>
            <button type="submit" className="btn">
              <span className="btn-icon"><SaveIcon /></span>
              <span>Save</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

