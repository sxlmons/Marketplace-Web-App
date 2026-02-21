import React, { useState, useEffect } from "react";
import { Outlet, useNavigate } from "react-router-dom";
import Header from "./Header";
import { AuthAPI } from "../services/api"

export default function AppLayout() {

    const navigate = useNavigate();

    const [theme, setTheme] = useState(() => {
        return localStorage.getItem("darkMode") === "true" ? "dark" : "light";
    });

    useEffect(() => {
        document.documentElement.classList.toggle("dark", theme === "dark");
        localStorage.setItem("darkMode", theme === "dark");
    }, [theme]);

    const toggleTheme = () => {
        setTheme(prev => (prev === "light" ? "dark" : "light"));
    };

    const handleAccountClick = () => {
        navigate("/account");
    };

    const handleLogout = async () => {
        try {
            localStorage.removeItem("darkMode");
            document.documentElement.classList.remove("dark");
            await AuthAPI.logout();
            navigate("/login", { replace: true });
        } catch (err) {
            console.error("Logout failed:", err.message);
        }
    };

    return (
        <>
            <Header
                toggleTheme={toggleTheme}
                currentTheme={theme}
                onAccountClick={handleAccountClick}
                onLogout={handleLogout}
            />
            <Outlet />
        </>
    );
}
