import React, { useState, useRef, useEffect } from "react";
import { useNavigate } from "react-router-dom";

export default function Header({ onAccountClick, onLogout, toggleTheme, currentTheme }) {
    const [open, setOpen] = useState(false);
    const menuRef = useRef(null);
    const navigate = useNavigate();

    useEffect(() => {
        function handleClickOutside(event) {
            if (menuRef.current && !menuRef.current.contains(event.target)) {
                setOpen(false);
            }
        }
        document.addEventListener("mousedown", handleClickOutside);
        return () => document.removeEventListener("mousedown", handleClickOutside);
    }, []);

    return (
        <header style={styles.header}>
            <h1
                style={{ ...styles.logo, cursor: "pointer" }}
                onClick={() => navigate("/home")}
            >
                MarketPlace
            </h1>

            <nav style={{ position: "relative" }} ref={menuRef}>
                <button
                    style={styles.linkButton}
                    onClick={() => navigate("/create")}
                >
                    Create Post
                </button>

                {}
                <button
                    onClick={toggleTheme}
                    style={styles.themeButton}
                    title="Toggle Dark/Light Mode"
                >
                    {currentTheme === "light" ? "🌙" : "🌞"}
                </button>

                <button
                    aria-label="Account"
                    onClick={() => setOpen((prev) => !prev)}
                    style={styles.accountButton}
                >
                    👤
                </button>

                {open && (
                    <div style={styles.dropdown}>
                        <button
                            style={styles.dropdownItem}
                            onClick={() => {
                                setOpen(false);
                                onAccountClick();
                            }}
                        >
                            Profile
                        </button>

                        <button
                            style={styles.dropdownItem}
                            onClick={() => {
                                setOpen(false);
                                onLogout?.();
                            }}
                        >
                            Logout
                        </button>
                    </div>
                )}
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
        borderBottom: "1px solid var(--border-color)",
    },
    logo: {
        margin: 0,
        fontSize: "1.5rem",
    },
    accountButton: {
        fontSize: "1.5rem",
        background: "none",
        border: "none",
        cursor: "pointer",
        marginLeft: "0.5rem",
    },
    themeButton: {
        fontSize: "1.5rem",
        background: "none",
        border: "none",
        cursor: "pointer",
        marginLeft: "0.5rem",
    },
    linkButton: {
        marginRight: "0.5rem",
        padding: "0.5rem 1rem",
        backgroundColor: "var(--primary-color)",
        border: "none",
        borderRadius: "4px",
        color: "#fff",
        cursor: "pointer",
    },
    dropdown: {
        position: "absolute",
        right: 0,
        marginTop: "0.5rem",
        background: "var(--card-bg)",
        border: "1px solid var(--border-color)",
        borderRadius: "4px",
        boxShadow: "0 4px 8px rgba(0,0,0,0.1)",
        minWidth: "140px",
        zIndex: 1000,
    },
    dropdownItem: {
        width: "100%",
        padding: "0.5rem 1rem",
        background: "none",
        border: "none",
        textAlign: "left",
        cursor: "pointer",
        color: "var(--text-color)",
    },
};
