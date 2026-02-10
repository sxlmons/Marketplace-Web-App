import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import Header from "../components/Header";
import FeaturedCarousel from "../components/FeaturedCarousel";
import CategorySection from "../components/CategorySection";
import { PostsAPI } from "../services/api";

const styles = {
    main: {
        padding: "2rem",
    },
    input: {
        marginLeft: "0.5rem",
        padding: "0.25rem",
    },
};

export default function HomePage({ onLogout }) {
    const navigate = useNavigate();

    const [featuredPosts, setFeaturedPosts] = useState([]);
    const [electronicsPosts, setElectronicsPosts] = useState([]);
    const [vehiclePosts, setVehiclePosts] = useState([]);
    const [location] = useState("");

    useEffect(() => {
        async function loadPosts() {
            try {
                const featured = await PostsAPI.fetch({ location });
                const electronics = await PostsAPI.fetch({
                    location,
                    category: "Electronics",
                });
                const vehicles = await PostsAPI.fetch({
                    location,
                    category: "Vehicles",
                });

                setFeaturedPosts(featured);
                setElectronicsPosts(electronics);
                setVehiclePosts(vehicles);
            } catch (err) {
                console.error(err);
            }
        }

        loadPosts();
    }, [location]);

    return (
        <>
            <Header
                onAccountClick={() => navigate("/account")}
                onLogout={onLogout}
            />

            <main style={styles.main}>
                <FeaturedCarousel posts={featuredPosts} />
                <CategorySection title="Electronics" posts={electronicsPosts} />
                <CategorySection title="Vehicles" posts={vehiclePosts} />
            </main>
        </>
    );
}
