import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";

import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import HomePage from "./pages/HomePage";
import AccountProfilePage from "./pages/ProfilePage";
import CreatePostPage from "./pages/CreatePostPage";
import PostDetailsPage from "./pages/PostDetailsPage";
import EditPostPage from "./pages/EditPostPage";

import ProtectedRoute from "./components/ProtectedRoute";
import AppLayout from "./components/AppLayout";

function CatchAllRedirect() {
    return (
        <ProtectedRoute>
            <Navigate to="/home" replace />
        </ProtectedRoute>
    );
}

function AppRoutes({ toggleTheme, currentTheme }) {
    return (
        <Routes>
            {/* Public Routes */}
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />

            {/* Protected Routes */}
            <Route element={<ProtectedRoute />}>
                <Route element={<AppLayout toggleTheme={toggleTheme} currentTheme={currentTheme} />}>
                    <Route path="/home" element={<HomePage />} />
                    <Route path="/account" element={<AccountProfilePage />} />
                    <Route path="/create" element={<CreatePostPage />} />
                    <Route path="/post/:postId" element={<PostDetailsPage />} />
                    <Route path="/post/:postId/edit" element={<EditPostPage />} />
                </Route>
            </Route>

            {/* Fallback */}
            <Route path="*" element={<CatchAllRedirect />} />
        </Routes>
    );
}

export default function App() {
    return (
        <BrowserRouter>
            <AppRoutes />
        </BrowserRouter>
    );
}
