import React, { useEffect, useState } from "react";

export default function FormModal({ title, isOpen, initialValues, fields, onClose, onSubmit }) {
  const [values, setValues] = useState(initialValues);

  useEffect(() => {
    setValues(initialValues);
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
  };

  const handleSubmit = (event) => {
    event.preventDefault();
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
                    <option key={option.value} value={option.value}>
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
            </label>
          ))}

          <div className="modal-actions">
            <button type="button" className="btn ghost" onClick={onClose}>
              Cancel
            </button>
            <button type="submit" className="btn">
              Save
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

