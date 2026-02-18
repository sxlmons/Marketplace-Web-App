import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { PostsAPI } from "../services/api";

export default function EditPostPage() {
    const { postId } = useParams();
    const navigate = useNavigate();

    const [formData, setFormData] = useState({
        title: "",
        description: "",
        images: [],
    });
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState("");

    useEffect(() => {
        let isMounted = true;

        (async () => {
            try {
                const data = await PostsAPI.fetchById(postId);
                if (isMounted) {
                    setFormData({
                        title: data.title,
                        description: data.description,
                        images: data.images || [],
                    });
                }
            } catch (err) {
                if (isMounted) setError(err.message);
            } finally {
                if (isMounted) setLoading(false);
            }
        })();

        return () => {
            isMounted = false;
        };
    }, [postId]);

    function handleChange(e) {
        setFormData({
            ...formData,
            [e.target.name]: e.target.value,
        });
    }

    function handleImageUpload(e) {
        const files = Array.from(e.target.files);
        setFormData((prev) => ({
            ...prev,
            images: [...prev.images, ...files],
        }));
    }

    async function handleSubmit(e) {
        e.preventDefault();
        setSaving(true);
        setError("");

        try {
            const fd = new FormData();
            fd.append("Title", formData.title);
            fd.append("Description", formData.description);

            formData.images.forEach((img) => {
                if (img instanceof File) {
                    fd.append("Images", img);
                }
            });

            await PostsAPI.update(postId, fd);
            navigate(`/post/${postId}`);
        } catch (err) {
            setError(err.message);
        } finally {
            setSaving(false);
        }
    }

    async function handleDelete() {
        if (!window.confirm("Are you sure you want to delete this post?")) return;

        try {
            await PostsAPI.delete(postId);
            navigate("/home");
        } catch (err) {
            setError(err.message);
        }
    }

    if (loading) return <p className="center">Loading post...</p>;

    return (
        <main className="container">
            {error && <div className="error">{error}</div>}

            <h1>Edit Post</h1>

            <form onSubmit={handleSubmit} className="form">
                <label>Title</label>
                <input
                    name="title"
                    type="text"
                    value={formData.title}
                    onChange={handleChange}
                    className="input"
                />

                <label>Description</label>
                <textarea
                    name="description"
                    value={formData.description}
                    onChange={handleChange}
                    className="textarea"
                    rows={5}
                />

                <label>Images</label>
                <input
                    type="file"
                    multiple
                    onChange={handleImageUpload}
                    className="input"
                />

                <div style={{ display: "flex", gap: "0.5rem", marginTop: "1rem" }}>
                    <button type="submit" className="button" disabled={saving}>
                        {saving ? "Saving..." : "Save"}
                    </button>
                    <button
                        type="button"
                        onClick={handleDelete}
                        className="button"
                        style={{ backgroundColor: "#dc3545" }}
                    >
                        Delete
                    </button>
                </div>
            </form>
        </main>
    );
}
