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
        <main className="container">
            <section className="card">
                <h1>Create New Post</h1>

                {error && <div className="error">{error}</div>}
                {success && <div className="success">{success}</div>}

                <form onSubmit={handleSubmit} className="form">
                    <label>
                        Title
                        <input
                            type="text"
                            value={title}
                            onChange={(e) => setTitle(e.target.value)}
                            className="input"
                            required
                        />
                    </label>

                    <label>
                        Description
                        <textarea
                            value={description}
                            onChange={(e) => setDescription(e.target.value)}
                            className="textarea"
                            rows={4}
                            required
                        />
                    </label>

                    <label>
                        Images (max 5)
                        <input
                            type="file"
                            multiple
                            accept="image/*"
                            onChange={handleImageUpload}
                            className="input"
                        />
                    </label>

                    {images.length > 0 && (
                        <div className="image-previews">
                            {images.map((file, i) => (
                                <img
                                    key={i}
                                    src={URL.createObjectURL(file)}
                                    alt={`Preview ${i + 1}`}
                                    className="image-preview"
                                />
                            ))}
                        </div>
                    )}

                    <button type="submit" className="button" disabled={loading}>
                        {loading ? "Creating..." : "Create Post"}
                    </button>
                </form>
            </section>
        </main>
    );
}
