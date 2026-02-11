import React from "react";

function IconBase({ children }) {
  return (
    <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true" focusable="false">
      {children}
    </svg>
  );
}

export function EditIcon() {
  return (
    <IconBase>
      <path
        fill="currentColor"
        d="M3 17.25V21h3.75L17.8 9.94l-3.75-3.75L3 17.25Zm2.92 2.33H5v-.92l9.05-9.05.92.92-9.05 9.05ZM20.71 7.04a1 1 0 0 0 0-1.41L18.37 3.3a1 1 0 0 0-1.41 0l-1.55 1.55 3.75 3.75 1.55-1.56Z"
      />
    </IconBase>
  );
}

export function DeleteIcon() {
  return (
    <IconBase>
      <path
        fill="currentColor"
        d="M9 3h6l1 2h4v2H4V5h4l1-2Zm1 6h2v9h-2V9Zm4 0h2v9h-2V9ZM6 9h2v9H6V9Z"
      />
    </IconBase>
  );
}

export function PlusIcon() {
  return (
    <IconBase>
      <path fill="currentColor" d="M11 5h2v6h6v2h-6v6h-2v-6H5v-2h6V5Z" />
    </IconBase>
  );
}

export function ArrowRightIcon() {
  return (
    <IconBase>
      <path fill="currentColor" d="M12 5.5 10.6 6.9l3.6 3.6H5v2h9.2l-3.6 3.6 1.4 1.4L18 12l-6-6.5Z" />
    </IconBase>
  );
}

export function LogoutIcon() {
  return (
    <IconBase>
      <path
        fill="currentColor"
        d="M10 3H5a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h5v-2H5V5h5V3Zm6.59 4.59L15.17 9l2 2H8v2h9.17l-2 2 1.42 1.41L21 12l-4.41-4.41Z"
      />
    </IconBase>
  );
}

export function SaveIcon() {
  return (
    <IconBase>
      <path fill="currentColor" d="m9.2 16.6-4.1-4.1L3.7 14l5.5 5.5L20.3 8.4 18.9 7l-9.7 9.6Z" />
    </IconBase>
  );
}

export function CancelIcon() {
  return (
    <IconBase>
      <path fill="currentColor" d="m18.3 5.7-1.4-1.4L12 9.2 7.1 4.3 5.7 5.7 10.6 10.6 5.7 15.5l1.4 1.4 4.9-4.9 4.9 4.9 1.4-1.4-4.9-4.9 4.9-4.9Z" />
    </IconBase>
  );
}
