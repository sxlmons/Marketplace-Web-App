import React, { useState } from "react";
import { AuthAPI } from "../services/api"

export default function LoginPage() {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);

    async function handleSubmit(e) {
        e.preventDefault();
        setError("");
        setLoading(true);

        try {
            await AuthAPI.login({ email, password });

            window.location.href = "/home";
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    }

    return (
        <main style={styles.container}>
            <form onSubmit={handleSubmit} style={styles.form} noValidate>
                <h1>Welcome Back</h1>
                <p>Log in to continue</p>

                {error && <div style={styles.error}>{error}</div>}

                <label style={styles.label}>
                    Email
                    <input
                        type="email"
                        value={email}
                        required
                        onChange={(e) => setEmail(e.target.value)}
                        style={styles.input}
                        autoComplete="email"
                    />
                </label>

                <label style={styles.label}>
                    Password
                    <input
                        type="password"
                        value={password}
                        required
                        onChange={(e) => setPassword(e.target.value)}
                        style={styles.input}
                        autoComplete="current-password"
                    />
                </label>

                <button
                    type="submit"
                    disabled={loading}
                    style={styles.button}
                >
                    {loading ? "Logging in..." : "Log In"}
                </button>

                <div style={styles.footer}>
                    <span>Don’t have an account?</span>
                    <a href="/register" style={styles.link}>
                        Create one
                    </a>
                </div>
            </form>
        </main>
    );
}

const styles = {
    container: {
        minHeight: "100vh",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        backgroundColor: "#f7f7f7",
    },
    form: {
        width: "100%",
        maxWidth: "400px",
        padding: "2rem",
        backgroundColor: "#fff",
        borderRadius: "8px",
        boxShadow: "0 4px 10px rgba(0,0,0,0.1)",
    },
    label: {
        display: "block",
        marginBottom: "1rem",
        fontWeight: "bold",
    },
    input: {
        width: "100%",
        padding: "0.5rem",
        marginTop: "0.25rem",
        borderRadius: "4px",
        border: "1px solid #ccc",
    },
    button: {
        width: "100%",
        padding: "0.75rem",
        marginTop: "1rem",
        backgroundColor: "#007bff",
        color: "#fff",
        border: "none",
        borderRadius: "4px",
        cursor: "pointer",
    },
    footer: {
        marginTop: "1rem",
        textAlign: "center",
    },
    link: {
        marginLeft: "0.25rem",
        color: "#007bff",
        textDecoration: "none",
        fontWeight: "bold",
    },
    error: {
        marginBottom: "1rem",
        color: "#b00020",
        backgroundColor: "#fdecea",
        padding: "0.5rem",
        borderRadius: "4px",
    },
};