const API_BASE = "http://localhost:5289/api";

export const AuthAPI = {
  login: () => `${API_BASE}/auth/login`,
  register: () => `${API_BASE}/auth/register`,
  logout: () => `${API_BASE}/auth/logout`,
  me: () => `${API_BASE}/auth/me`,
};

export const UsersAPI = {
  me: () => `${API_BASE}/users/me`,
};

export const PostsAPI = {
  all: () => `${API_BASE}/posts`,
  byId: (id) => `${API_BASE}/posts/${id}`,
};

export const CommentsAPI = {
  forPost: (postId) => `${API_BASE}/posts/${postId}/comments`,
};

export const fetchPosts = async ({ location, category } = {}) => {
    const params = new URLSearchParams();

    if (location) params.append("location", location);
    if (category) params.append("category", category);

    const url = `${PostsAPI.all()}?${params.toString()}`;

    const res = await fetch(url);

    if (!res.ok) {
        throw new Error("Failed to fetch posts");
    }

    return res.json();
};

