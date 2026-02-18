import React, { useEffect, useState } from "react";
import { AuthAPI } from "../services/api";

export default function AccountProfilePage() {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

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
            if (formData.email !== user.email) {
                await AuthAPI.updateEmail(formData.email);
                didUpdate = true;
            }

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

                setFormData(prev => ({
                    ...prev,
                    currentPassword: "",
                    newPassword: "",
                    confirmPassword: "",
                }));
            }

            if (didUpdate) {
                setMessage("Account updated successfully");
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

    if (loading) return <p className="center">Loading account...</p>;
    if (!user) return <p className="center">Unable to load account</p>;

    return (
        <main className="container">
            <section className="card">
                <h1>Account Profile</h1>

                {error && <div className="error">{error}</div>}
                {message && <div className="success">{message}</div>}

                <form onSubmit={handleSave} className="form">
                    {/* Email */}
                    <label>
                        Email
                        <input
                            name="email"
                            type="email"
                            value={formData.email}
                            onChange={handleChange}
                            className="input"
                            required
                        />
                    </label>

                    <hr />

                    {/* Password fields */}
                    <label>
                        Current Password
                        <input
                            name="currentPassword"
                            type="password"
                            value={formData.currentPassword}
                            onChange={handleChange}
                            className="input"
                        />
                    </label>

                    <label>
                        New Password
                        <input
                            name="newPassword"
                            type="password"
                            value={formData.newPassword}
                            onChange={handleChange}
                            className="input"
                        />
                    </label>

                    <label>
                        Confirm New Password
                        <input
                            name="confirmPassword"
                            type="password"
                            value={formData.confirmPassword}
                            onChange={handleChange}
                            className="input"
                        />
                    </label>

                    <button type="submit" className="button" disabled={saving}>
                        {saving ? "Saving..." : "Save Changes"}
                    </button>
                </form>
            </section>
        </main>
    );
}
