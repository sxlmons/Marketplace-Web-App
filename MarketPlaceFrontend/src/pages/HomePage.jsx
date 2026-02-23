import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { PostsAPI, ImagesAPI } from "../services/api";

const PAGE_SIZE = 10;

export default function HomePage() {
    const navigate = useNavigate();

    const [posts, setPosts] = useState([]);
    const [limit, setLimit] = useState(PAGE_SIZE);
    const [loading, setLoading] = useState(false);
    const [hasMore, setHasMore] = useState(true);

    // Fetch posts when limit changes
    useEffect(() => {
        let cancelled = false;

        async function fetchPosts() {
            if (!hasMore) return;

            setLoading(true);

            try {
                const data = await PostsAPI.getLatestPosts(limit);

                if (!cancelled) {
                    const postsWithImages = await Promise.all(
                        data.map(async (post) => {
                            try {
                                const blob = await ImagesAPI.getThumbnail(post.id);
                                const imageUrl = URL.createObjectURL(blob);
                                return { ...post, thumbnail: imageUrl };
                            } catch {
                                return { ...post, thumbnail: null };
                            }
                        })
                    );

                    setPosts(prevPosts => {
                        const newPosts = postsWithImages.filter(
                            p => !prevPosts.some(existing => existing.id === p.id)
                        );

                        if (newPosts.length < PAGE_SIZE) setHasMore(false);

                        return [...prevPosts, ...newPosts];
                    });
                }
            } catch (err) {
                console.error(err);
            } finally {
                if (!cancelled) setLoading(false);
            }
        }

        fetchPosts();

        return () => { cancelled = true; };
    }, [limit, hasMore]);

    // Infinite scroll listener
    useEffect(() => {
        function handleScroll() {
            if (
                window.innerHeight + document.documentElement.scrollTop >=
                document.documentElement.scrollHeight - 100 &&
                !loading &&
                hasMore
            ) {
                setLimit(prev => prev + PAGE_SIZE);
            }
        }

        window.addEventListener("scroll", handleScroll);
        return () => window.removeEventListener("scroll", handleScroll);
    }, [loading, hasMore]);

    useEffect(() => {
        return () => {
            posts.forEach((post) => {
                if (post.thumbnail) {
                    URL.revokeObjectURL(post.thumbnail);
                }
            });
        };
    }, [posts]);

    return (
        <main className="container">
            {posts.map(post => (
                <div
                    key={post.id}
                    className="post-card"
                    onClick={() => navigate(`/post/${post.id}`)}
                >
                    {post.thumbnail && (
                        <img
                            src={post.thumbnail}
                            alt={`Post ${post.id} thumbnail`}
                            loading="lazy"
                            className="post-hero-image"
                            onError={(e) => {
                                e.target.style.display = "none";
                            }}
                        />
                    )}

                    <div className="post-title">{post.title}</div>
                    <div className="post-description">{post.description}</div>
                </div>
            ))}

            {loading && <p>Loading...</p>}
            {!hasMore && <p>No more posts</p>}
        </main>
    );
}