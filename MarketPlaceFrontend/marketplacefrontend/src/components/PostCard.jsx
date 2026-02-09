import React from "react";

export default function PostCard({ post, onClick }) {
  return (
    <div style={styles.card} onClick={onClick} role="button" tabIndex={0}>
      {post.imageUrl && (
        <img
          src={post.imageUrl}
          alt={post.title}
          style={styles.image}
        />
      )}

      <div style={styles.content}>
        <h3>{post.title}</h3>
        <p>{post.description}</p>
        <small>{post.location}</small>
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