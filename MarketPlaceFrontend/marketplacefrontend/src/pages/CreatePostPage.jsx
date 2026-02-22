import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { PostsAPI } from "../services/api";

export default function CreatePostPage() {
    const navigate = useNavigate();

    const [title, setTitle] = useState("");
    const [description, setDescription] = useState("");
    const [images, setImages] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");
    const [success, setSuccess] = useState("");

    function handleImageUpload(e) {
        const files = Array.from(e.target.files);

        if (files.length + images.length > 5) {
            setError("Maximum 5 images allowed");
            return;
        }

        setImages((prev) => [...prev, ...files]);
        setError("");
    }

    async function handleSubmit(e) {
        e.preventDefault();
        setError("");
        setSuccess("");
        setLoading(true);

        try {
            const formData = new FormData();
            formData.append("Title", title);
            formData.append("Description", description);
            images.forEach((file) => formData.append("Images", file));

            const res = await PostsAPI.create(formData);

            setSuccess("Post created successfully");
            setTitle("");
            setDescription("");
            setImages([]);

            navigate(`/post/${res.postId}`);
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    }

    return (
        <main className="form-page-container">
            <h1>Create New Post</h1>

            {error && <div className="error">{error}</div>}
            {success && <div className="success">{success}</div>}

            <form onSubmit={handleSubmit} className="form-container">
                <div className="form-field">
                    <label>Title</label>
                    <input
                        type="text"
                        value={title}
                        onChange={(e) => {
                            const value = e.target.value;
                            if (value.length <= 100) {
                                setTitle(value);
                                setError("");
                            } else {
                                setError("Title cannot exceed 100 characters");
                            }
                        }}
                        maxLength={100}
                        required
                    />
                    <small>{title.length}/100 characters</small>
                </div>

                <div className="form-field">
                    <label>Description</label>
                    <textarea
                        value={description}
                        onChange={(e) => setDescription(e.target.value)}
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

                    {images.length > 0 && (
                        <div className="image-previews-container">
                            {images.map((file, i) => (
                                <div key={i} className="image-preview-wrapper">
                                    <img
                                        src={URL.createObjectURL(file)}
                                        alt={`Preview ${i + 1}`}
                                    />
                                    <button
                                        type="button"
                                        className="remove-image-button"
                                        onClick={() =>
                                            setImages((prev) =>
                                                prev.filter((_, index) => index !== i)
                                            )
                                        }
                                    >
                                        &times;
                                    </button>
                                </div>
                            ))}
                        </div>
                    )}
                </div>

                <div className="form-field">
                    <button type="submit" className="icon-button btn-primary" disabled={loading}>
                        {loading ? "Creating..." : "Create Post"}
                    </button>
                </div>
            </form>
        </main>
    );
}