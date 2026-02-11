import React from "react";
import { DeleteIcon, EditIcon } from "./Icons";
export default function CrudTable({ columns, rows, onEdit, onDelete, emptyMessage = "No records found." }) {
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
          {rows.length ? (
            rows.map((row) => (
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
            ))
          ) : (
            <tr>
              <td className="empty-row" colSpan={columns.length + 1}>
                {emptyMessage}
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}

