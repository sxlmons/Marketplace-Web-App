import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { PostsAPI, CommentsAPI, AuthAPI, ImagesAPI } from "../services/api";
import Lightbox from "../components/LightBox";


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
    const [selectedImage, setSelectedImage] = useState(null);
    const [currentImageIndex, setCurrentImageIndex] = useState(0);

    useEffect(() => {
        let isMounted = true;

        async function loadAll() {
            try {
                const postData = await PostsAPI.fetchById(postId);
                if (!isMounted) return;

                const images = [];

                for (let i = 1; i <= postData.photoCount; i++) {
                    try {
                        const blob = await ImagesAPI.getPhotoForPost(postData.id, i);
                        const url = URL.createObjectURL(blob);
                        images.push(url);
                    } catch (err){
                        setError(err.message);                       
                    }
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

    // Handle ESC key to cancel editing
    useEffect(() => {
        function handleKeyDown(e) {
            if (e.key === "Escape" && editingCommentId !== null) {
                setEditingCommentId(null);
                setEditingText("");
            }
        }

        window.addEventListener("keydown", handleKeyDown);
        return () => window.removeEventListener("keydown", handleKeyDown);
    }, [editingCommentId]);

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

    useEffect(() => {
        return () => {
            if (post?.images) {
                post.images.forEach(url => URL.revokeObjectURL(url));
            }
        };
    }, [post]);

    if (!post) return <p>Loading post...</p>;

    const isOwner = currentUserId === post.userId;

    return (
        <main className="post-details-container">
            {error && <div className="error">{error}</div>}

            <section className="post-images-section">
                {post.images[0] && (
                    <img
                        src={post.images[0]}
                        alt="Hero"
                        className="post-hero-image-large clickable"
                        onClick={() => {
                            setSelectedImage(post.images[0]);
                            setCurrentImageIndex(0);
                        }}
                    />
                )}

                {post.images.length > 1 && (
                    <div className="post-remaining-images">
                        {post.images.slice(1).map((img, i) => (
                            <img
                                key={i}
                                src={img}
                                alt={`Post image ${i + 2}`}
                                className="post-remaining-image clickable"
                                onClick={() => {
                                    setSelectedImage(img);
                                    setCurrentImageIndex(i + 1);
                                }}
                            />
                        ))}
                    </div>
                )}
            </section>

            <div className="post-content-wrapper">
                <section className="post-details-text">
                    <div className="post-title-row">
                        <h1 className="post-title">{post.title}</h1>

                        {isOwner && (
                            <div className="post-title-buttons">
                                <button className="btn-primary" onClick={() => navigate(`/post/${post.id}/edit`)}>
                                    Edit
                                </button>
                                <button className="btn-delete" onClick={handleDeletePost}>
                                    Delete
                                </button>
                            </div>
                        )}
                    </div>

                    <p className="post-description">{post.description}</p>
                </section>

                <section className="comments-section">
                    <h2>Comments</h2>
                    <form onSubmit={handleAddComment} className="comment-form">
                        <textarea
                            value={newComment}
                            onChange={(e) => setNewComment(e.target.value)}
                            placeholder="Leave a comment..."
                            rows={1}
                        />
                        <button className="btn-primary">Post Comment</button>
                    </form>

                    {comments.map((comment) => {
                        const isCommentOwner = comment.userId === currentUserId;
                        return (
                            <div key={comment.id} className="comment">
                                {editingCommentId === comment.id && (
                                    <div className="comment-actions" style={{ marginBottom: "0.5rem" }}>
                                        <button
                                            onClick={() => handleEditComment(comment.id)}
                                            className="icon-button btn-primary"
                                        >
                                            Save
                                        </button>
                                        <button
                                            onClick={() => { setEditingCommentId(null); setEditingText(""); }}
                                            className="icon-button btn-secondary"
                                        >
                                            Cancel
                                        </button>
                                    </div>
                                )}

                                {isCommentOwner && editingCommentId !== comment.id && (
                                    <div className="comment-top-right">
                                        <button
                                            onClick={() => { setEditingCommentId(comment.id); setEditingText(comment.content); }}
                                            className="icon-button btn-primary"
                                            title="Edit Comment"
                                        >
                                            Edit
                                        </button>
                                        <button
                                            onClick={() => handleDeleteComment(comment.id)}
                                            className="icon-button btn-delete"
                                            title="Delete Comment"
                                        >
                                            Delete
                                        </button>
                                    </div>
                                )}

                                {editingCommentId === comment.id ? (
                                    <textarea
                                        value={editingText}
                                        onChange={(e) => setEditingText(e.target.value)}
                                    />
                                ) : (
                                    <p>{comment.content}</p>
                                )}
                            </div>
                        );
                    })}
                </section>
            </div>

            {selectedImage && (
                <Lightbox
                    images={post.images}
                    currentIndex={currentImageIndex}
                    onClose={() => setSelectedImage(null)}
                    onNavigate={(newIndex) => {
                        setCurrentImageIndex(newIndex);
                        setSelectedImage(post.images[newIndex]);
                    }}
                    navClassName="image-modal-nav"
                    overlayClassName="image-modal-overlay"
                    closeClassName="image-modal-close"
                />
            )}
        </main>
    );
}