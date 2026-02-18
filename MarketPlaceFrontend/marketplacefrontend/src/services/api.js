const API_BASE = "http://localhost:5289/api";

async function handleResponse(res) {
    if (!res.ok) {
        const error = await res.json().catch(() => ({ message: res.statusText }));
        throw new Error(error?.message || "An error occurred");
    }

    if (res.status === 204 || res.headers.get("Content-Length") === "0") return null;

    try {
        return await res.json();
    } catch {
        return null;
    }
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
    async getLatestPosts(limit) {
        const res = await fetch(
            `${API_BASE}/post/getlatestpostswithlimit?limit=${limit}`
        );

        if (!res.ok) {
            throw new Error("Failed to fetch posts");
        }

        return handleResponse(res);
    },

    fetchById: async (postId) => {
        const res = await fetch(
            `${API_BASE}/post/getsinglepostinfo?postId=${postId}`,
            {
                method: "GET",
                credentials: "include"
            }
        );
        return handleResponse(res);
    },

    create: async (postData) => {
        const res = await fetch(`${API_BASE}/post/createnewpost`, {
            method: "POST",
            credentials: "include",
            body: postData
        });
        return handleResponse(res);
    },

    update: async (postId, postData) => {
        const res = await fetch(`${API_BASE}/post/updatepost?postId=${postId}`, {
            method: "PUT",
            credentials: "include",
            body: postData
        });
        return handleResponse(res);
    },

    delete: async (postId) => {
        const res = await fetch(`${API_BASE}/post/deletepost?postId=${postId}`, {
            method: "DELETE",
            credentials: "include",
        });
        return handleResponse(res);
    },
};

export const CommentsAPI = {
    fetchByPost: async (postId) => {
        const res = await fetch(
            `${API_BASE}/comment/getpostscomments?postId=${postId}`,
            {
                credentials: "include"
            }
        );
        return handleResponse(res);
    },

    create: async (postId, content) => {
        const res = await fetch(
            `${API_BASE}/comment/createnewcomment`,
            {
                method: "POST",
                credentials: "include",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    postId: postId,
                    content: content
                }),
            }
        );
        return handleResponse(res);
    },

    update: async (commentId, content) => {
        const res = await fetch(
            `${API_BASE}/comment/updatecomment?commentId=${commentId}`,
            {
                method: "PUT",
                credentials: "include",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    content: content
                }),
            }
        );
        return handleResponse(res);
    },

    delete: async (commentId) => {
        const res = await fetch(
            `${API_BASE}/comment/deletecomment?commentId=${commentId}`,
            {
                method: "DELETE",
                credentials: "include",
            }
        );
        return handleResponse(res);
    },
};
