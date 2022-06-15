import React from "react";
import "./app.css";

export const Header = (props) => {
  const { clientSettings } = props;
  return (
    <header>
      <a href={clientSettings?.vendorLink} target="_blank" rel="noreferrer">
        <img
          className="vendor-logo"
          src="/vendor.png"
          alt="Vendor Logo"
          onError={(e) => {
            e.target.style.display = "none";
          }}
        />
      </a>
    </header>
  );
};

export default Header;
