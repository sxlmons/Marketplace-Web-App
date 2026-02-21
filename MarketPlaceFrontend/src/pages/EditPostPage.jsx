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

        return () => (isMounted = false);
    }, [postId]);

    function handleChange(e) {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    }

    function handleImageUpload(e) {
        const files = Array.from(e.target.files);

        if (files.length + formData.images.length > 5) {
            setError("Maximum 5 images allowed");
            return;
        }

        setFormData((prev) => ({
            ...prev,
            images: [...prev.images, ...files],
        }));
        setError("");
    }

    function removeImage(index) {
        setFormData((prev) => ({
            ...prev,
            images: prev.images.filter((_, i) => i !== index),
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
                if (img instanceof File) fd.append("Images", img);
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
        <main className="form-page-container">
            <h1>Edit Post</h1>

            {error && <div className="error">{error}</div>}

            <form onSubmit={handleSubmit} className="form-container">
                <div className="form-field">
                    <label>Title</label>
                    <input
                        name="title"
                        type="text"
                        value={formData.title}
                        onChange={handleChange}
                        required
                    />
                </div>

                <div className="form-field">
                    <label>Description</label>
                    <textarea
                        name="description"
                        value={formData.description}
                        onChange={handleChange}
                        rows={4}
                        required
                    />
                </div>

                <div className="form-field">
                    <label>Images (max 5)</label>
                    <input
                        type="file"
                        multiple
                        accept="image/*"
                        onChange={handleImageUpload}
                    />

                    {formData.images.length > 0 && (
                        <div className="image-previews-container">
                            {formData.images.map((img, i) => (
                                <div key={i} className="image-preview-wrapper">
                                    <img
                                        src={
                                            img instanceof File
                                                ? URL.createObjectURL(img)
                                                : img
                                        }
                                        alt={`Preview ${i + 1}`}
                                    />
                                    <button
                                        type="button"
                                        className="remove-image-button"
                                        onClick={() => removeImage(i)}
                                    >
                                        &times;
                                    </button>
                                </div>
                            ))}
                        </div>
                    )}
                </div>

                <div className="form-field" style={{ display: "flex", gap: "0.5rem" }}>
                    <button
                        type="submit"
                        className="button"
                        disabled={saving}
                        style={{ flex: 1 }}
                    >
                        {saving ? "Saving..." : "Save"}
                    </button>
                    <button
                        type="button"
                        className="button"
                        onClick={handleDelete}
                        style={{ flex: 1, backgroundColor: "var(--danger-color)" }}
                    >
                        Delete
                    </button>
                </div>
            </form>
        </main>
    );
}