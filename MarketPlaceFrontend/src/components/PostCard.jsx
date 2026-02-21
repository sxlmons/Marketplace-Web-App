import React from "react";


const API_BASE = "http://localhost:5289/api";

export default function PostCard({ post, onClick }) {

    const imageUrl = post.PhotoCount > 0 ? `${API_BASE}/image/getsinglethumbnail?postId=${post.Id}` : null;

    return (
        <div style={styles.card} onClick={onClick} role="button" tabIndex={0}>
            {imageUrl && (
                <div style={styles.imageWrapper}>
                    <img src={imageUrl} alt={post.title} style={styles.image} />
                </div>
            )}

            <div style={styles.content}>
                <h3>{post.title}</h3>
                <p>{post.description}</p>
            </div>
        </div>
    );
}

const styles = {
    card: {
        width: "250px",
        border: "1px solid #ccc",
        borderRadius: "8px",
        overflow: "hidden",
        cursor: "pointer",
    },
    image: {
        width: "100%",
        height: "150px",
        objectFit: "cover",
    },
    content: {
        padding: "0.5rem",
    },
};