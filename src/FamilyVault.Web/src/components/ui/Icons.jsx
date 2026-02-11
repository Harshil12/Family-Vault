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

export function ViewIcon() {
  return (
    <IconBase>
      <path
        fill="currentColor"
        d="M12 5c-5.5 0-9.5 4.7-10.8 6.5a.9.9 0 0 0 0 1c1.3 1.8 5.3 6.5 10.8 6.5s9.5-4.7 10.8-6.5a.9.9 0 0 0 0-1C21.5 9.7 17.5 5 12 5Zm0 12c-3.9 0-7-3-8.7-5 1.7-2 4.8-5 8.7-5s7 3 8.7 5c-1.7 2-4.8 5-8.7 5Zm0-8a3 3 0 1 0 0 6 3 3 0 0 0 0-6Z"
      />
    </IconBase>
  );
}

export function MembersIcon() {
  return (
    <IconBase>
      <path
        fill="currentColor"
        d="M16 11a3 3 0 1 0-2.99-3A3 3 0 0 0 16 11Zm-8 0A2.5 2.5 0 1 0 8 6a2.5 2.5 0 0 0 0 5Zm0 2c-2.5 0-4.5 1.4-4.5 3v1h9v-1c0-1.6-2-3-4.5-3Zm8 0c-.63 0-1.22.08-1.77.23 1.01.75 1.77 1.78 1.77 2.77v1h6v-1c0-1.66-2.69-3-6-3Z"
      />
    </IconBase>
  );
}

export function DocumentIcon() {
  return (
    <IconBase>
      <path
        fill="currentColor"
        d="M7 3h7l5 5v13a1 1 0 0 1-1 1H7a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2Zm6 1.5V9h4.5L13 4.5ZM8 12h8v1.5H8V12Zm0 3.5h8V17H8v-1.5Z"
      />
    </IconBase>
  );
}

export function BankIcon() {
  return (
    <IconBase>
      <path
        fill="currentColor"
        d="M12 3 3 7.5V9h18V7.5L12 3Zm8 8H4v2h16v-2ZM5 14v4h2v-4H5Zm4 0v4h2v-4H9Zm4 0v4h2v-4h-2Zm4 0v4h2v-4h-2ZM3 20h18v2H3v-2Z"
      />
    </IconBase>
  );
}

export function BackIcon() {
  return (
    <IconBase>
      <path fill="currentColor" d="m11 6-6 6 6 6 1.4-1.4L8.8 13H20v-2H8.8l3.6-3.6L11 6Z" />
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
