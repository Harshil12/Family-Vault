import React from "react";
import { DeleteIcon, EditIcon } from "./Icons";
export default function CrudTable({ columns, rows, onEdit, onDelete, emptyMessage = "No records found." }) {
  if (!rows.length) {
    return <p className="empty-state">{emptyMessage}</p>;
  }

  return (
    <div className="table-wrap">
      <table>
        <thead>
          <tr>
            {columns.map((column) => (
              <th key={column.key}>{column.header}</th>
            ))}
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr key={row.id}>
              {columns.map((column) => (
                <td key={`${row.id}-${column.key}`}>{column.render ? column.render(row) : row[column.key]}</td>
              ))}
              <td className="actions-cell">
                <button type="button" className="btn tiny" onClick={() => onEdit(row)}>
                  <span className="btn-icon"><EditIcon /></span>
                  <span>Edit</span>
                </button>
                <button type="button" className="btn tiny danger" onClick={() => onDelete(row)}>
                  <span className="btn-icon"><DeleteIcon /></span>
                  <span>Delete</span>
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

