import React, { useEffect, useState } from "react";
import { Navigate, Outlet } from "react-router-dom";
import { AuthAPI } from "../services/api";

export default function ProtectedRoute() {
    const [loading, setLoading] = useState(true);
    const [authenticated, setAuthenticated] = useState(false);

    useEffect(() => {
        async function checkAuth() {
            try {
                await AuthAPI.me();
                setAuthenticated(true);
            } catch {
                setAuthenticated(false);
            } finally {
                setLoading(false);
            }
        }

        checkAuth();
    }, []);

    if (loading) {
        return <p style={{ textAlign: "center" }}>Checking authentication...</p>;
    }

    if (!authenticated) {
        return <Navigate to="/login" replace />;
    }

    return <Outlet />;
}