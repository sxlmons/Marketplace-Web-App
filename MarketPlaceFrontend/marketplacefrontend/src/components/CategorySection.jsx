import React from "react";
import PostCard from "./PostCard";

export default function CategorySection({ title, posts }) {
  return (
    <section style={{ marginTop: "2rem" }}>
      <h2>{title}</h2>

      <div style={styles.grid}>
        {posts.map((post) => (
          <PostCard key={post.id} post={post} />
        ))}
      </div>
    </section>
  );
}

const styles = {
  grid: {
    display: "grid",
    gridTemplateColumns: "repeat(auto-fill, minmax(250px, 1fr))",
    gap: "1rem",
  },
};