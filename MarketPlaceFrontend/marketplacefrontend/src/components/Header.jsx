import React from "react";

export default function Header({ onAccountClick }) {
  return (
    <header style={styles.header}>
      <h1 style={styles.logo}>MarketPlace</h1>

      <nav>
        <button
          aria-label="Account"
          onClick={onAccountClick}
          style={styles.accountButton}
        >
          👤
        </button>
      </nav>
    </header>
  );
}

const styles = {
  header: {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
    padding: "1rem 2rem",
    borderBottom: "1px solid #ddd",
  },
  logo: {
    margin: 0,
  },
  accountButton: {
    fontSize: "1.5rem",
    background: "none",
    border: "none",
    cursor: "pointer",
  },
};