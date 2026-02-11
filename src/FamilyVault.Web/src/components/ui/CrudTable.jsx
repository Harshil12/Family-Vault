import React from "react";
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
                  Edit
                </button>
                <button type="button" className="btn tiny danger" onClick={() => onDelete(row)}>
                  Delete
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

