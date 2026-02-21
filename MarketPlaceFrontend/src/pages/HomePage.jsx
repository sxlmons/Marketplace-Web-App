import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { PostsAPI } from "../services/api";

const API_BASE = "http://localhost:5289/api"

const PAGE_SIZE = 10;

export default function HomePage() {
    const navigate = useNavigate();

    const [posts, setPosts] = useState([]);
    const [limit, setLimit] = useState(PAGE_SIZE);
    const [loading, setLoading] = useState(false);
    const [hasMore, setHasMore] = useState(true);

    useEffect(() => {
        let cancelled = false;

        async function fetchPosts() {
            setLoading(true);

            try {
                const data = await PostsAPI.getLatestPosts(limit);

                if (!cancelled) {

                    data.forEach(post => {
                        const images = [];
                        for (let i = 1; i <= post.photoCount; i++) {
                            images.push(`${API_BASE}/Image/GetPhotoForPost?postId=${post.id}&imageId=${i}`);
                        }
                        post.images = images;
                    });

                    setPosts(data);

                    if (data.length < limit) {
                        setHasMore(false);
                    }
                }
            } catch (err) {
                console.error(err);
            }

            if (!cancelled) {
                setLoading(false);
            }
        }

        fetchPosts();

        return () => {
            cancelled = true;
        };
    }, [limit]);

    useEffect(() => {
        function handleScroll() {
            if (
                window.innerHeight + document.documentElement.scrollTop >=
                document.documentElement.scrollHeight - 100 &&
                !loading &&
                hasMore
            ) {
                setLimit((prev) => prev + PAGE_SIZE);
            }
        }

        window.addEventListener("scroll", handleScroll);
        return () => window.removeEventListener("scroll", handleScroll);
    }, [loading, hasMore]);

    return (
        <>
            <main className="container">
                {posts.map((post) => (
                    <div
                        key={post.id}
                        className="post-card"
                        onClick={() => navigate(`/post/${post.id}`)}
                    >
                        {post.images?.length > 0 && (
                            <img
                                src={post.images[0]}
                                alt={`Post ${post.id} hero`}
                                className="post-hero-image"
                            />
                        )}

                        <div className="post-title">{post.title}</div>
                        <div className="post-description">{post.description}</div>
                    </div>
                ))}

                {loading && <p>Loading...</p>}
                {!hasMore && <p>No more posts</p>}
            </main>

        </>
    );
}
