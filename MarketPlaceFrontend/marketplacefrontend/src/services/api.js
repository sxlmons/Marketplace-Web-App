const API_BASE = "http://localhost:5289/api";

// Helper function for handling responses
async function handleResponse(res) {
    if (!res.ok) {
        const error = await res.json().catch(() => ({ message: res.statusText }));
        throw new Error(error?.message || "An error occurred");
    }

    if (res.status === 204) return null;
    return res.json();
}


export const AuthAPI = {
    register: async ({ email, password }) => {
        const res = await fetch(`${API_BASE}/auth/register`, {
            method: "POST",
            credentials: "include",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ email, password }),
        });
        return handleResponse(res);
    },

    login: async ({ email, password }) => {
        const res = await fetch(`${API_BASE}/auth/login`, {
            method: "POST",
            credentials: "include",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ email, password }),
        });
        return handleResponse(res);
    },

    logout: async () => {
        const res = await fetch(`${API_BASE}/auth/logout`, {
            method: "POST",
            credentials: "include",
        });
        return handleResponse(res);
    },

    me: async () => {
        const res = await fetch(`${API_BASE}/auth/me`, {
            method: "GET",
            credentials: "include",
        });
        return handleResponse(res);
    },

    updateEmail: async (newEmail) => {
        const res = await fetch(`${API_BASE}/auth/updateemail`, {
            method: "PATCH",
            credentials: "include",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ newEmail }),
        });
        return handleResponse(res);
    },

    updatePassword: async (currentPassword, newPassword) => {
        const res = await fetch(`${API_BASE}/auth/updatepassword`, {
            method: "PATCH",
            credentials: "include",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ currentPassword, newPassword }),
        });
        return handleResponse(res);
    },
};

export const PostsAPI = {
    // Fetch posts for carousel / homepage
    // TODO: Modify after backend has a way to fetch user location - will default to using posts for that location
    fetch: async ({ limit = 5, location, category } = {}) => {
        let url = `${API_BASE}/posts?limit=${limit}`;
        if (location) url += `&location=${encodeURIComponent(location)}`;
        if (category) url += `&category=${encodeURIComponent(category)}`;
        const res = await fetch(url, { credentials: "include" });
        return handleResponse(res);
    },

    // Fetch single post by ID
    fetchById: async (postId) => {
        const res = await fetch(`${API_BASE}/posts/${postId}`, { credentials: "include" });
        return handleResponse(res);
    },

    // Create new post
    create: async (postData) => {
        const res = await fetch(`${API_BASE}/posts`, {
            method: "POST",
            credentials: "include",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(postData),
        });
        return handleResponse(res);
    },

    // Update existing post
    update: async (postId, postData) => {
        const res = await fetch(`${API_BASE}/posts/${postId}`, {
            method: "PUT",
            credentials: "include",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(postData),
        });
        return handleResponse(res);
    },

    // Delete post
    delete: async (postId) => {
        const res = await fetch(`${API_BASE}/posts/${postId}`, {
            method: "DELETE",
            credentials: "include",
        });
        return handleResponse(res);
    },
};

export const CommentsAPI = {
    fetchByPost: async (postId) => {
        const res = await fetch(`${API_BASE}/posts/${postId}/comments`, { credentials: "include" });
        return handleResponse(res);
    },

    create: async (postId, content) => {
        const res = await fetch(`${API_BASE}/posts/${postId}/comments`, {
            method: "POST",
            credentials: "include",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ content }),
        });
        return handleResponse(res);
    },

    update: async (commentId, content) => {
        const res = await fetch(`${API_BASE}/comments/${commentId}`, {
            method: "PUT",
            credentials: "include",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ content }),
        });
        return handleResponse(res);
    },

    delete: async (commentId) => {
        const res = await fetch(`${API_BASE}/comments/${commentId}`, {
            method: "DELETE",
            credentials: "include",
        });
        return handleResponse(res);
    },
};
