import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";

const CATEGORIES = [
  "Electronics",
  "Vehicles",
  "Furniture",
  "Clothing",
  "Services",
  "Other",
];

export default function EditPostPage() {
  const { postId } = useParams();
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    title: "",
    category: "",
    description: "",
    images: [],
  });

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    loadPost();
  }, [postId]);

  async function loadPost() {
    try {
      const res = await fetch(
        `http://localhost:5000/api/posts/${postId}`,
        { credentials: "include" }
      );

      if (!res.ok) {
        throw new Error("Failed to load post");
      }

      const data = await res.json();
      setFormData({
        title: data.title,
        category: data.category,
        description: data.description,
        images: data.images || [],
      });
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  function handleChange(e) {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  }

  function handleImageUpload(e) {
    const files = Array.from(e.target.files);

    files.forEach((file) => {
      const reader = new FileReader();
      reader.onloadend = () => {
        setFormData((prev) => ({
          ...prev,
          images: [...prev.images, reader.result],
        }));
      };
      reader.readAsDataURL(file);
    });
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setSaving(true);
    setError("");

    try {
      const res = await fetch(
        `http://localhost:5000/api/posts/${postId}`,
        {
          method: "PUT",
          credentials: "include",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(formData),
        }
      );

      if (!res.ok) {
        const data = await res.json();
        throw new Error(data.message || "Failed to update post");
      }

      navigate(`/posts/${postId}`);
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete() {
    if (!window.confirm("Are you sure you want to delete this post?")) return;

    try {
      const res = await fetch(
        `http://localhost:5000/api/posts/${postId}`,
        {
          method: "DELETE",
          credentials: "include",
        }
      );

      if (!res.ok) {
        throw new Error("Failed to delete post");
      }

      navigate("/home");
    } catch (err) {
      setError(err.message);
    }
  }

  if (loading) {
    return <p style={styles.center}>Loading post...</p>;
  }

  return (
    <main style={styles.container}>
      <section style={styles.card}>
        <h1>Edit Post</h1>
        <p>Update your post details</p>

        {error && <div style={styles.error}>{error}</div>}

        <form onSubmit={handleSubmit}>
          <label style={styles.label}>
            Title
            <input
              name="title"
              value={formData.title}
              onChange={handleChange}
              required
              style={styles.input}
            />
          </label>

          <label style={styles.label}>
            Category
            <select
              name="category"
              value={formData.category}
              onChange={handleChange}
              required
              style={styles.input}
            >
              <option value="">Select a category</option>
              {CATEGORIES.map((cat) => (
                <option key={cat} value={cat}>
                  {cat}
                </option>
              ))}
            </select>
          </label>

          <label style={styles.label}>
            Description
            <textarea
              name="description"
              value={formData.description}
              onChange={handleChange}
              rows={4}
              required
              style={styles.textarea}
            />
          </label>

          <label style={styles.label}>
            Add Images (optional)
            <input
              type="file"
              multiple
              accept="image/*"
              onChange={handleImageUpload}
            />
          </label>

          {formData.images.length > 0 && (
            <div style={styles.previewGrid}>
              {formData.images.map((img, i) => (
                <img key={i} src={img} alt="Preview" style={styles.preview} />
              ))}
            </div>
          )}

          <button
            type="submit"
            disabled={saving}
            style={styles.button}
          >
            {saving ? "Saving..." : "Save Changes"}
          </button>
        </form>

        <hr style={styles.divider} />

        <button onClick={handleDelete} style={styles.deleteButton}>
          Delete Post
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
    maxWidth: "600px",
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
  textarea: {
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
  deleteButton: {
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
  previewGrid: {
    display: "flex",
    gap: "0.5rem",
    marginBottom: "1rem",
    flexWrap: "wrap",
  },
  preview: {
    width: "80px",
    height: "80px",
    objectFit: "cover",
    borderRadius: "4px",
    border: "1px solid #ccc",
  },
  error: {
    marginBottom: "1rem",
    color: "#b00020",
    backgroundColor: "#fdecea",
    padding: "0.5rem",
    borderRadius: "4px",
  },
  center: {
    textAlign: "center",
    marginTop: "3rem",
  },
};