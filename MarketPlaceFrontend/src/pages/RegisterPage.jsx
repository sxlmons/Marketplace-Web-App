import React, { useState } from "react";

export default function RegisterPage() {
    const [formData, setFormData] = useState({
        email: "",
        password: "",
        confirmPassword: "",
    });

    const [errors, setErrors] = useState({
        email: "",
        password: "",
        confirmPassword: "",
        general: "",
    });

    const [submitted, setSubmitted] = useState(false);
    const [loading, setLoading] = useState(false);

    function validateEmail(email) {
        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
    }

    function validatePassword(password) {
        return {
            length: password.length >= 8,
            uppercase: /[A-Z]/.test(password),
            lowercase: /[a-z]/.test(password),
            number: /[0-9]/.test(password),
            special: /[!@#$%^&*-]/.test(password),
        };
    }

    function handleChange(e) {
        const { name, value } = e.target;

        setFormData((prev) => ({
            ...prev,
            [name]: value,
        }));

        // After first submit attempt, clear field error as user edits
        if (submitted) {
            setErrors((prev) => ({
                ...prev,
                [name]: "",
            }));
        }
    }

    async function handleSubmit(e) {
        e.preventDefault();
        setSubmitted(true);
        setErrors((prev) => ({ ...prev, general: "" }));

        const passwordChecks = validatePassword(formData.password);
        const isPasswordValid = Object.values(passwordChecks).every(Boolean);
        const isEmailValid = validateEmail(formData.email);
        const doPasswordsMatch =
            formData.password === formData.confirmPassword;

        if (!isEmailValid || !isPasswordValid || !doPasswordsMatch) {
            setErrors({
                email: !isEmailValid ? "Invalid email format" : "",
                password: !isPasswordValid
                    ? "Password does not meet requirements"
                    : "",
                confirmPassword: !doPasswordsMatch
                    ? "Passwords do not match"
                    : "",
                general: "Please fix the errors below",
            });
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
            setErrors((prev) => ({
                ...prev,
                general: err.message,
            }));
        } finally {
            setLoading(false);
        }
    }

    const passwordChecks = validatePassword(formData.password);

    return (
        <main style={styles.container}>
            <form onSubmit={handleSubmit} style={styles.form} noValidate>
                <h1>Create Account</h1>
                <p>Join the marketplace</p>

                {errors.general && (
                    <div style={styles.error}>{errors.general}</div>
                )}

                <label style={styles.label}>
                    Email
                    <input
                        name="email"
                        type="email"
                        value={formData.email}
                        onChange={handleChange}
                        style={styles.input}
                    />
                    {submitted && errors.email && (
                        <span style={styles.fieldError}>
                            {errors.email}
                        </span>
                    )}
                </label>

                <label style={styles.label}>
                    Password
                    <input
                        name="password"
                        type="password"
                        value={formData.password}
                        onChange={handleChange}
                        style={styles.input}
                    />

                    {/* Show checklist only after failed submit */}
                    {submitted && errors.password && (
                        <ul style={styles.passwordList}>
                            <li style={{ color: passwordChecks.length ? "green" : "red" }}>
                                At least 8 characters
                            </li>
                            <li style={{ color: passwordChecks.uppercase ? "green" : "red" }}>
                                One uppercase letter
                            </li>
                            <li style={{ color: passwordChecks.lowercase ? "green" : "red" }}>
                                One lowercase letter
                            </li>
                            <li style={{ color: passwordChecks.number ? "green" : "red" }}>
                                One number
                            </li>
                            <li style={{ color: passwordChecks.special ? "green" : "red" }}>
                                One special character (!@#$%^&*-)
                            </li>
                        </ul>
                    )}

                    {submitted && errors.password && (
                        <span style={styles.fieldError}>
                            {errors.password}
                        </span>
                    )}
                </label>

                <label style={styles.label}>
                    Confirm Password
                    <input
                        name="confirmPassword"
                        type="password"
                        value={formData.confirmPassword}
                        onChange={handleChange}
                        style={styles.input}
                    />
                    {submitted && errors.confirmPassword && (
                        <span style={styles.fieldError}>
                            {errors.confirmPassword}
                        </span>
                    )}
                </label>

                <button type="submit" disabled={loading} style={styles.button}>
                    {loading ? "Creating account..." : "Create Account"}
                </button>
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
    error: {
        marginBottom: "1rem",
        color: "#b00020",
        backgroundColor: "#fdecea",
        padding: "0.5rem",
        borderRadius: "4px",
    },
    fieldError: {
        display: "block",
        marginTop: "0.25rem",
        fontSize: "0.85rem",
        color: "#b00020",
    },
    passwordList: {
        marginTop: "0.5rem",
        paddingLeft: "1rem",
        fontSize: "0.85rem",
    },
};
