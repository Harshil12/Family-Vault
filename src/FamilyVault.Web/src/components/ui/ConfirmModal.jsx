import React from "react";
import { CancelIcon, DeleteIcon } from "./Icons";

export default function ConfirmModal({ isOpen, title, message, confirmText = "Delete", cancelText = "Cancel", onConfirm, onCancel }) {
  if (!isOpen) {
    return null;
  }

  return (
    <div className="modal-backdrop">
      <div className="modal confirm-modal">
        <h3>{title}</h3>
        <p>{message}</p>
        <div className="modal-actions">
          <button type="button" className="btn ghost" onClick={onCancel}>
            <span className="btn-icon"><CancelIcon /></span>
            <span>{cancelText}</span>
          </button>
          <button type="button" className="btn danger" onClick={onConfirm}>
            <span className="btn-icon"><DeleteIcon /></span>
            <span>{confirmText}</span>
          </button>
        </div>
      </div>
    </div>
  );
}
