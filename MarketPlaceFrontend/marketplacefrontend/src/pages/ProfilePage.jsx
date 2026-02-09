import React, { useEffect, useState } from "react";

export default function AccountProfilePage() {
  const [user, setUser] = useState(null);
  const [formData, setFormData] = useState({
    username: "",
    email: "",
    location: "",
  });

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  useEffect(() => {
    async function fetchAccount() {
      try {
        const response = await fetch(
          "http://localhost:5000/api/users/me",
          {
            credentials: "include",
          }
        );

        if (!response.ok) {
          throw new Error("Failed to load account information");
        }

        const data = await response.json();
        setUser(data);
        setFormData({
          username: data.username,
          email: data.email,
          location: data.location,
        });
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    }

    fetchAccount();
  }, []);

  function handleChange(e) {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  }

  async function handleSave(e) {
    e.preventDefault();
    setSaving(true);
    setError("");
    setMessage("");

    try {
      const response = await fetch(
        "http://localhost:5000/api/users/me",
        {
          method: "PUT",
          credentials: "include",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(formData),
        }
      );

      if (!response.ok) {
        const data = await response.json();
        throw new Error(data.message || "Failed to update account");
      }

      setMessage("Account information updated successfully");
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  }

  if (loading) {
    return <p style={styles.center}>Loading account...</p>;
  }

  if (!user) {
    return <p style={styles.center}>Unable to load account</p>;
  }

  return (
    <main style={styles.container}>
      <section style={styles.card}>
        <h1>Account Profile</h1>
        <p>View and update your account information</p>

        {error && <div style={styles.error}>{error}</div>}
        {message && <div style={styles.success}>{message}</div>}

        <form onSubmit={handleSave}>
          <label style={styles.label}>
            Username
            <input
              name="username"
              value={formData.username}
              onChange={handleChange}
              style={styles.input}
              required
            />
          </label>

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
              required
            />
          </label>

          <button
            type="submit"
            disabled={saving}
            style={styles.button}
          >
            {saving ? "Saving..." : "Save Changes"}
          </button>
        </form>

        <hr style={styles.divider} />

        <button
          style={styles.logout}
          onClick={() => (window.location.href = "/logout")}
        >
          Log Out
        </button>
      </section>
    </main>
  );
}

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
  logout: {
    marginTop: "1rem",
    width: "100%",
    padding: "0.5rem",
    backgroundColor: "#dc3545",
    color: "#fff",
    border: "none",
    borderRadius: "4px",
    cursor: "pointer",
  },
  divider: {
    margin: "2rem 0 1rem",
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