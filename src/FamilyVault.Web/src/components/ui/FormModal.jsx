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
    const { name, value, files, type } = event.target;
    const nextValue = type === "file" ? (files && files[0] ? files[0] : null) : value;
    setValues((current) => ({
      ...current,
      [name]: nextValue
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

  const handleSubmit = async (event) => {
    event.preventDefault();
    const nextErrors = validate ? validate(values) : {};
    if (Object.keys(nextErrors).length) {
      setErrors(nextErrors);
      return;
    }
    try {
      await onSubmit(values);
    } catch (submitError) {
      if (submitError?.fieldErrors && typeof submitError.fieldErrors === "object") {
        setErrors(submitError.fieldErrors);
      } else {
        setErrors({ _form: submitError?.message ?? "Unable to submit form." });
      }
    }
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
              ) : field.type === "file" ? (
                <input
                  type="file"
                  name={field.name}
                  onChange={handleChange}
                  required={field.required}
                  accept={field.accept}
                />
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

