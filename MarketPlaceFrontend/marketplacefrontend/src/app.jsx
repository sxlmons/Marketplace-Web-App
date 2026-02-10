import { BrowserRouter, Routes, Route, useNavigate } from "react-router-dom";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import HomePage from "./pages/HomePage";
import AccountProfilePage from "./pages/ProfilePage";
import PostDetailsPage from "./pages/PostDetailsPage";
import EditPostPage from "./pages/EditPostPage";
import ProtectedRoute from "./components/ProtectedRoute";
import { AuthAPI } from "./services/api";

function AppRoutes() {
    const navigate = useNavigate();

    const handleLogout = async () => {
        try {
            await AuthAPI.logout();
        } catch (err) {
            console.error("Logout failed:", err);
        } finally {
            navigate("/login", { replace: true });
        }
    };

    return (
        <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />

            <Route element={<ProtectedRoute />}>
                <Route
                    path="/home"
                    element={<HomePage onLogout={handleLogout} />}
                />
                <Route
                    path="/account"
                    element={<AccountProfilePage onLogout={handleLogout} />}
                />
                <Route path="/posts/:postId" element={<PostDetailsPage />} />
                <Route path="/posts/:postId/edit" element={<EditPostPage />} />
            </Route>

            <Route path="*" element={<LoginPage />} />
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
