import React, { useEffect, useRef, useState } from "react";
import {
  flexRender,
  getCoreRowModel,
  getSortedRowModel,
  useReactTable
} from "@tanstack/react-table";
import { DeleteIcon, EditIcon } from "./Icons";

function normalizeCellValue(value) {
  if (value === null || value === undefined || value === "") {
    return "-";
  }
  return value;
}

function displayText(value) {
  const normalized = normalizeCellValue(value);
  return typeof normalized === "string" ? normalized : String(normalized);
}

export default function DataGridTable({ columns, rows, onEdit, onDelete, emptyMessage = "No records found." }) {
  const wrapRef = useRef(null);
  const [hasHorizontalOverflow, setHasHorizontalOverflow] = useState(false);
  const [isNearRightEdge, setIsNearRightEdge] = useState(true);
  const [sorting, setSorting] = useState([]);
  const [columnOrder, setColumnOrder] = useState(() => columns.map((column) => column.key));
  const dragColumnIdRef = useRef(null);

  useEffect(() => {
    setColumnOrder((current) => {
      const nextIds = columns.map((column) => column.key);
      const kept = current.filter((id) => nextIds.includes(id));
      const missing = nextIds.filter((id) => !kept.includes(id));
      return [...kept, ...missing];
    });
  }, [columns]);

  const tanstackColumns = React.useMemo(
    () =>
      columns.map((column) => ({
        id: column.key,
        accessorFn: (row) => {
          if (typeof column.sortValue === "function") {
            return column.sortValue(row);
          }
          return row[column.key];
        },
        header: column.header,
        cell: ({ row, getValue }) => {
          if (column.render) {
            return column.render(row.original);
          }
          const text = displayText(getValue());
          return (
            <span className="grid-cell-text" title={text}>
              {text}
            </span>
          );
        },
        enableSorting: column.sortable ?? !column.render
      })),
    [columns]
  );

  const table = useReactTable({
    data: rows,
    columns: tanstackColumns,
    state: { sorting, columnOrder },
    onSortingChange: setSorting,
    onColumnOrderChange: setColumnOrder,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getRowId: (row) => String(row.id)
  });

  const reorderColumns = (sourceId, targetId) => {
    if (!sourceId || !targetId || sourceId === targetId) {
      return;
    }
    setColumnOrder((current) => {
      const sourceIndex = current.indexOf(sourceId);
      const targetIndex = current.indexOf(targetId);
      if (sourceIndex < 0 || targetIndex < 0) {
        return current;
      }
      const next = [...current];
      const [moved] = next.splice(sourceIndex, 1);
      next.splice(targetIndex, 0, moved);
      return next;
    });
  };

  useEffect(() => {
    const node = wrapRef.current;
    if (!node) {
      return undefined;
    }

    const measure = () => {
      const overflow = node.scrollWidth > node.clientWidth + 1;
      setHasHorizontalOverflow(overflow);
      if (!overflow) {
        setIsNearRightEdge(true);
        return;
      }
      const remaining = node.scrollWidth - node.clientWidth - node.scrollLeft;
      setIsNearRightEdge(remaining < 12);
    };

    measure();
    node.addEventListener("scroll", measure);
    window.addEventListener("resize", measure);

    return () => {
      node.removeEventListener("scroll", measure);
      window.removeEventListener("resize", measure);
    };
  }, [columns.length, rows.length]);

  return (
    <div
      ref={wrapRef}
      className={`table-wrap ${hasHorizontalOverflow ? "has-horizontal-overflow" : ""} ${isNearRightEdge ? "at-right-edge" : ""}`}
    >
      {hasHorizontalOverflow && !isNearRightEdge && (
        <div className="table-scroll-hint">Scroll right to see more columns</div>
      )}
      <table>
        <thead>
          {table.getHeaderGroups().map((headerGroup) => (
            <tr key={headerGroup.id}>
              {headerGroup.headers.map((header) => {
                const sortState = header.column.getIsSorted();
                return (
                  <th
                    key={header.id}
                    onClick={header.column.getCanSort() ? header.column.getToggleSortingHandler() : undefined}
                    style={{ cursor: "grab", userSelect: "none" }}
                    title={header.column.getCanSort() ? "Sort or drag to reorder" : "Drag to reorder"}
                    draggable
                    onDragStart={(event) => {
                      dragColumnIdRef.current = header.column.id;
                      event.dataTransfer.effectAllowed = "move";
                    }}
                    onDragOver={(event) => {
                      event.preventDefault();
                      event.dataTransfer.dropEffect = "move";
                    }}
                    onDrop={(event) => {
                      event.preventDefault();
                      reorderColumns(dragColumnIdRef.current, header.column.id);
                      dragColumnIdRef.current = null;
                    }}
                    onDragEnd={() => {
                      dragColumnIdRef.current = null;
                    }}
                  >
                    <span className="grid-head-cell">
                      <span className="grid-head-label">{flexRender(header.column.columnDef.header, header.getContext())}</span>
                      {header.column.getCanSort() && (
                        <span className="grid-sort-indicator" aria-hidden="true">
                          {sortState === "asc" ? "▲" : sortState === "desc" ? "▼" : ""}
                        </span>
                      )}
                    </span>
                  </th>
                );
              })}
              <th>Actions</th>
            </tr>
          ))}
        </thead>
        <tbody>
          {table.getRowModel().rows.length ? (
            table.getRowModel().rows.map((row) => (
              <tr key={row.id}>
                {row.getVisibleCells().map((cell) => (
                  <td key={cell.id}>{flexRender(cell.column.columnDef.cell, cell.getContext())}</td>
                ))}
                <td className="actions-cell" data-actions-cell="true">
                  <button type="button" className="btn tiny icon-only" onClick={() => onEdit(row.original)} aria-label="Edit record" title="Edit">
                    <span className="btn-icon"><EditIcon /></span>
                  </button>
                  <button type="button" className="btn tiny danger icon-only" onClick={() => onDelete(row.original)} aria-label="Delete record" title="Delete">
                    <span className="btn-icon"><DeleteIcon /></span>
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

