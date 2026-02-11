import React from "react";

export default function HelpTip({ text, label = "Help" }) {
  return (
    <span className="help-tip-wrap">
      <button type="button" className="help-tip-trigger" aria-label={label}>
        ?
      </button>
      <span role="tooltip" className="help-tip-bubble">
        {text}
      </span>
    </span>
  );
}
