import React, { useEffect, useState } from "react";
import { AuthAPI } from "../services/api";

export default function AccountProfilePage() {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    const [formData, setFormData] = useState({
        email: "",
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
        setFormData(prev => ({ ...prev, [e.target.name]: e.target.value }));
    }

    async function handleSave(e) {
        e.preventDefault();
        setSaving(true);
        setError("");
        setMessage("");

        try {
            const { email, currentPassword, newPassword, confirmPassword } = formData;

            const emailChanged = email !== user.email;
            const isChangingPassword = newPassword || confirmPassword;

            // Nothing changed
            if (!emailChanged && !isChangingPassword) {
                setMessage("No changes detected");
                return;
            }

            // Require current password for ANY change
            if (!currentPassword) {
                throw new Error("Current password is required to update account information");
            }

            // Handle password change ONLY if new password fields are used
            if (isChangingPassword) {
                if (!newPassword || !confirmPassword) {
                    throw new Error("Please fill all new password fields");
                }
                if (newPassword !== confirmPassword) {
                    throw new Error("New password and confirmation do not match");
                }

                await AuthAPI.updatePassword(currentPassword, newPassword);
            }

            // Handle email change
            if (emailChanged) {
                await AuthAPI.updateEmail(email, currentPassword);
                setUser(prev => ({ ...prev, email }));
            }

            // Clear sensitive fields
            setFormData(prev => ({
                ...prev,
                currentPassword: "",
                newPassword: "",
                confirmPassword: "",
            }));

            setMessage("Account updated successfully");

        } catch (err) {
            setError(err.message || "Failed to update account");
        } finally {
            setSaving(false);
        }
    }


    if (loading) return <p className="center">Loading account...</p>;
    if (!user) return <p className="center">Unable to load account</p>;

    return (
        <main className="form-page-container">
            <h1>Account Profile</h1>

            {error && <div className="error">{error}</div>}
            {message && <div className="success">{message}</div>}

            <form onSubmit={handleSave} className="form-container">
                <div className="form-field">
                    <label>Email</label>
                    <input
                        name="email"
                        type="email"
                        value={formData.email}
                        onChange={handleChange}
                        required
                    />
                </div>

                <div className="form-separator" />

                <div className="form-field">
                    <label>Current Password</label>
                    <input
                        name="currentPassword"
                        type="password"
                        value={formData.currentPassword}
                        onChange={handleChange}
                    />
                </div>

                <div className="form-field">
                    <label>New Password</label>
                    <input
                        name="newPassword"
                        type="password"
                        value={formData.newPassword}
                        onChange={handleChange}
                    />
                </div>

                <div className="form-field">
                    <label>Confirm New Password</label>
                    <input
                        name="confirmPassword"
                        type="password"
                        value={formData.confirmPassword}
                        onChange={handleChange}
                    />
                </div>

                <div className="form-field">
                    <button type="submit" className="btn-primary" disabled={saving}>
                        {saving ? "Saving..." : "Save Changes"}
                    </button>
                </div>
            </form>
        </main>
    );
}