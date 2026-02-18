import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { PostsAPI, CommentsAPI, AuthAPI } from "../services/api";

const API_BASE = "http://localhost:5289/api";

export default function PostDetailsPage() {
    const { postId } = useParams();
    const navigate = useNavigate();

    const [post, setPost] = useState(null);
    const [comments, setComments] = useState([]);
    const [currentUserId, setCurrentUserId] = useState(null);
    const [newComment, setNewComment] = useState("");
    const [editingCommentId, setEditingCommentId] = useState(null);
    const [editingText, setEditingText] = useState("");
    const [error, setError] = useState("");

    useEffect(() => {
        let isMounted = true;

        async function loadAll() {
            try {
                const postData = await PostsAPI.fetchById(postId);
                if (!isMounted) return;

                const images = [];
                for (let i = 1; i <= postData.photoCount; i++) {
                    images.push(`${API_BASE}/image/GetPhotoForPost?postId=${postData.id}&imageId=${i}`);
                }

                setPost({ ...postData, images });

                try {
                    const me = await AuthAPI.me();
                    if (isMounted) setCurrentUserId(me.id);
                } catch { null; }

                const commentsData = await CommentsAPI.fetchByPost(postId);
                if (isMounted) setComments(commentsData);
            } catch (err) {
                if (isMounted) setError(err.message);
            }
        }

        loadAll();

        return () => { isMounted = false; };
    }, [postId]);

    async function handleDeletePost() {
        if (!window.confirm("Are you sure you want to delete this post?")) return;
        try {
            await PostsAPI.delete(postId);
            navigate("/home");
        } catch (err) {
            setError(err.message);
        }
    }

    async function handleAddComment(e) {
        e.preventDefault();
        if (!newComment.trim()) return;
        try {
            await CommentsAPI.create(postId, newComment);
            setNewComment("");
            const commentsData = await CommentsAPI.fetchByPost(postId);
            setComments(commentsData);
        } catch (err) {
            setError(err.message);
        }
    }

    async function handleEditComment(commentId) {
        if (!editingText.trim()) return;
        try {
            await CommentsAPI.update(commentId, editingText);
            setEditingCommentId(null);
            setEditingText("");
            const commentsData = await CommentsAPI.fetchByPost(postId);
            setComments(commentsData);
        } catch (err) {
            setError(err.message);
        }
    }

    async function handleDeleteComment(commentId) {
        if (!window.confirm("Delete this comment?")) return;
        try {
            await CommentsAPI.delete(commentId);
            const commentsData = await CommentsAPI.fetchByPost(postId);
            setComments(commentsData);
        } catch (err) {
            setError(err.message);
        }
    }

    if (!post) return <p>Loading post...</p>;

    const isOwner = currentUserId === post.userId;

    return (
        <main className="container">
            {error && <div className="error">{error}</div>}

            <section className="card">
                <div className="header-row">
                    <h1>{post.title}</h1>
                    {isOwner && (
                        <div className="header-buttons">
                            <button onClick={() => navigate(`/post/${postId}/edit`)} className="icon-button" title="Edit Post">
                                ✏️
                            </button>
                            <button onClick={handleDeletePost} className="icon-button delete" title="Delete Post">
                                ❌
                            </button>
                        </div>
                    )}
                </div>

                <p>{post.description}</p>

                <div className="image-grid">
                    {post.images?.map((img, i) => (
                        <img key={i} src={img} alt={`Post image ${i + 1}`} className="post-image" />
                    ))}
                </div>
            </section>

            <section className="card">
                <h2>Comments</h2>

                <form onSubmit={handleAddComment} className="comment-form">
                    <textarea
                        value={newComment}
                        onChange={(e) => setNewComment(e.target.value)}
                        placeholder="Leave a comment..."
                        rows={3}
                        className="textarea"
                    />
                    <button className="button">Post Comment</button>
                </form>

                {comments.map((comment) => {
                    const isCommentOwner = comment.userId === currentUserId;
                    return (
                        <div key={comment.id} className="comment">
                            {isCommentOwner && editingCommentId !== comment.id && (
                                <div className="comment-top-right">
                                    <button
                                        onClick={() => { setEditingCommentId(comment.id); setEditingText(comment.content); }}
                                        className="icon-button"
                                        title="Edit Comment"
                                    >✏️</button>
                                    <button
                                        onClick={() => handleDeleteComment(comment.id)}
                                        className="icon-button delete"
                                        title="Delete Comment"
                                    >❌</button>
                                </div>
                            )}

                            {editingCommentId === comment.id ? (
                                <>
                                    <textarea
                                        value={editingText}
                                        onChange={(e) => setEditingText(e.target.value)}
                                        className="textarea"
                                    />
                                    <div className="comment-actions">
                                        <button onClick={() => handleEditComment(comment.id)} className="icon-button">💾</button>
                                        <button onClick={() => { setEditingCommentId(null); setEditingText(""); }} className="icon-button">❌</button>
                                    </div>
                                </>
                            ) : (
                                <p>{comment.content}</p>
                            )}
                        </div>
                    );
                })}
            </section>
        </main>
    );
}
