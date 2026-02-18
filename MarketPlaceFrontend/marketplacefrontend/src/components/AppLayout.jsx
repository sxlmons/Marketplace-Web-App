import React, { useState } from "react";
import { Outlet, useNavigate } from "react-router-dom";
import Header from "./Header";

export default function AppLayout() {
    const [theme, setTheme] = useState(
        localStorage.getItem("darkMode") === "true" ? "dark" : "light"
    );

    const navigate = useNavigate();

    const toggleTheme = () => {
        const newTheme = theme === "light" ? "dark" : "light";
        setTheme(newTheme);
        localStorage.setItem("darkMode", newTheme === "dark");
        const link = document.getElementById("theme-link");
        if (link) link.href = newTheme === "dark" ? "/styles/dark.css" : "/styles/light.css";
    };

    const handleAccountClick = () => {
        navigate("/account");
    };

    return (
        <>
            <Header
                toggleTheme={toggleTheme}
                currentTheme={theme}
                onAccountClick={handleAccountClick}
            />
            <Outlet />
        </>
    );
}
