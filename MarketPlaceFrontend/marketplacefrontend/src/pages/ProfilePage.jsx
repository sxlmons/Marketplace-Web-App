import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { AuthAPI } from "../services/api";
import Header from "../components/Header";

const styles = {
    container: {
        minHeight: "100vh",
        display: "flex",
        justifyContent: "center",
        alignItems: "flex-start",
        paddingTop: "3rem",
        backgroundColor: "#f7f7f7",
    },
    card: {
        width: "100%",
        maxWidth: "500px",
        backgroundColor: "#fff",
        padding: "2rem",
        borderRadius: "8px",
        boxShadow: "0 4px 10px rgba(0,0,0,0.1)",
        marginBottom: "2rem",
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
    error: {
        marginBottom: "1rem",
        color: "#b00020",
        backgroundColor: "#fdecea",
        padding: "0.5rem",
        borderRadius: "4px",
    },
    success: {
        marginBottom: "1rem",
        color: "#155724",
        backgroundColor: "#d4edda",
        padding: "0.5rem",
        borderRadius: "4px",
    },
    center: {
        textAlign: "center",
        marginTop: "3rem",
    },
};

export default function AccountProfilePage({ onLogout }) {
    const navigate = useNavigate();

    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    // Form state: email/location + password fields
    const [formData, setFormData] = useState({
        email: "",
        location: "",
        currentPassword: "",
        newPassword: "",
        confirmPassword: "",
    });

    const [saving, setSaving] = useState(false);
    const [message, setMessage] = useState("");
    const [error, setError] = useState("");

    // Fetch user info
    useEffect(() => {
        async function fetchAccount() {
            try {
                const data = await AuthAPI.me();
                setUser(data);
                setFormData(prev => ({
                    ...prev,
                    email: data.email || "",
                    location: data.location || "",
                }));
            } catch {
                setError("Failed to load account information");
            } finally {
                setLoading(false);
            }
        }

        fetchAccount();
    }, []);

    function handleChange(e) {
        setFormData(prev => ({
            ...prev,
            [e.target.name]: e.target.value,
        }));
    }

    async function handleSave(e) {
        e.preventDefault();
        setSaving(true);
        setError("");
        setMessage("");

        let didUpdate = false;

        try {
            // Update email if changed
            if (formData.email !== user.email) {
                await AuthAPI.updateEmail(formData.email);
                didUpdate = true;
            }

            // Update password if all fields filled
            const { currentPassword, newPassword, confirmPassword } = formData;
            if (currentPassword || newPassword || confirmPassword) {
                if (!currentPassword || !newPassword || !confirmPassword) {
                    throw new Error("Please fill all password fields to change password");
                }
                if (newPassword !== confirmPassword) {
                    throw new Error("New password and confirmation do not match");
                }

                await AuthAPI.updatePassword(currentPassword, newPassword);
                didUpdate = true;
                // Clear password fields after update
                setFormData(prev => ({
                    ...prev,
                    currentPassword: "",
                    newPassword: "",
                    confirmPassword: "",
                }));
            }

            if (didUpdate) {
                setMessage("Account updated successfully");
                // Update user email locally
                setUser(prev => ({ ...prev, email: formData.email }));
            } else {
                setMessage("No changes detected");
            }
        } catch (err) {
            setError(err.message || "Failed to update account");
        } finally {
            setSaving(false);
        }
    }

    if (loading) return <p style={styles.center}>Loading account...</p>;
    if (!user) return <p style={styles.center}>Unable to load account</p>;

    return (
        <>
            <Header onAccountClick={() => navigate("/account")} onLogout={onLogout} />

            <main style={styles.container}>
                <section style={styles.card}>
                    <h1>Account Profile</h1>

                    {error && <div style={styles.error}>{error}</div>}
                    {message && <div style={styles.success}>{message}</div>}

                    <form onSubmit={handleSave}>
                        {/* Email / location */}
                        <label style={styles.label}>
                            Email
                            <input
                                name="email"
                                type="email"
                                value={formData.email}
                                onChange={handleChange}
                                style={styles.input}
                                required
                            />
                        </label>

                        <label style={styles.label}>
                            Location
                            <input
                                name="location"
                                value={formData.location}
                                onChange={handleChange}
                                style={styles.input}
                            />
                        </label>

                        <hr style={{ margin: "1.5rem 0" }} />

                        {/* Password fields */}
                        <label style={styles.label}>
                            Current Password
                            <input
                                name="currentPassword"
                                type="password"
                                value={formData.currentPassword}
                                onChange={handleChange}
                                style={styles.input}
                            />
                        </label>

                        <label style={styles.label}>
                            New Password
                            <input
                                name="newPassword"
                                type="password"
                                value={formData.newPassword}
                                onChange={handleChange}
                                style={styles.input}
                            />
                        </label>

                        <label style={styles.label}>
                            Confirm New Password
                            <input
                                name="confirmPassword"
                                type="password"
                                value={formData.confirmPassword}
                                onChange={handleChange}
                                style={styles.input}
                            />
                        </label>

                        <button type="submit" disabled={saving} style={styles.button}>
                            {saving ? "Saving..." : "Save Changes"}
                        </button>
                    </form>
                </section>
            </main>
        </>
    );
}
