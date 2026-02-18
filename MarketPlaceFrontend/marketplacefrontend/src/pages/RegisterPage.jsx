import React, { useState } from "react";

export default function RegisterPage() {
    const [formData, setFormData] = useState({
        username: "",
        email: "",
        password: "",
        confirmPassword: "",
        location: "",
    });

    const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);

    function handleChange(e) {
        setFormData({
            ...formData,
            [e.target.name]: e.target.value,
        });
    }

    async function handleSubmit(e) {
        e.preventDefault();
        setError("");

        if (formData.password !== formData.confirmPassword) {
            setError("Passwords do not match");
            return;
        }

        setLoading(true);

        try {
            const response = await fetch(
                "http://localhost:5289/api/auth/register",
                {
                    method: "POST",
                    credentials: "include",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        email: formData.email,
                        password: formData.password,
                    }),
                }
            );

            if (!response.ok) {
                const data = await response.json();
                throw new Error(data.message || "Registration failed");
            }

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
                <h1>Create Account</h1>
                <p>Join the marketplace</p>

                {error && <div style={styles.error}>{error}</div>}

                <label style={styles.label}>
                    Email
                    <input
                        name="email"
                        type="email"
                        required
                        value={formData.email}
                        onChange={handleChange}
                        style={styles.input}
                        autoComplete="email"
                    />
                </label>

                <label style={styles.label}>
                    Password
                    <input
                        name="password"
                        type="password"
                        required
                        value={formData.password}
                        onChange={handleChange}
                        style={styles.input}
                        autoComplete="new-password"
                    />
                </label>

                <label style={styles.label}>
                    Confirm Password
                    <input
                        name="confirmPassword"
                        type="password"
                        required
                        value={formData.confirmPassword}
                        onChange={handleChange}
                        style={styles.input}
                        autoComplete="new-password"
                    />
                </label>

                <button
                    type="submit"
                    disabled={loading}
                    style={styles.button}
                >
                    {loading ? "Creating account..." : "Create Account"}
                </button>

                <div style={styles.footer}>
                    <span>Already have an account?</span>
                    <a href="/login" style={styles.link}>
                        Log in
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
        maxWidth: "420px",
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
        backgroundColor: "#28a745",
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