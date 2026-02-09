import React from "react";
import PostCard from "./PostCard";

export default function FeaturedCarousel({ posts }) {
  return (
    <section>
      <h2>Featured Near You</h2>
      <div style={styles.carousel}>
        {posts.map((post) => (
          <PostCard key={post.id} post={post} />
        ))}
      </div>
    </section>
  );
}

const styles = {
  carousel: {
    display: "flex",
    gap: "1rem",
    overflowX: "auto",
    paddingBottom: "1rem",
  },
};