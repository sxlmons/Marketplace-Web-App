import React, { useEffect, useState } from "react";
import Header from "../components/Header";
import FeaturedCarousel from "../components/FeaturedCarousel";
import CategorySection from "../components/CategorySection";
import { fetchPosts } from "../services/api";

export default function HomePage() {
  const [featuredPosts, setFeaturedPosts] = useState([]);
  const [electronicsPosts, setElectronicsPosts] = useState([]);
  const [vehiclePosts, setVehiclePosts] = useState([]);
  const [location, setLocation] = useState("");

  useEffect(() => {
    async function loadPosts() {
      try {
        const featured = await fetchPosts({ location });
        const electronics = await fetchPosts({
          location,
          category: "Electronics",
        });
        const vehicles = await fetchPosts({
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
      <Header onAccountClick={() => (window.location.href = "/account")} />

      <main style={styles.main}>
        {/* Location filter (stretch goal) */}
        <section style={styles.filterBar}>
          <label>
            Filter by location:
            <input
              type="text"
              placeholder="Enter city or zip"
              value={location}
              onChange={(e) => setLocation(e.target.value)}
              style={styles.input}
            />
          </label>
        </section>

        <FeaturedCarousel posts={featuredPosts} />

        <CategorySection
          title="Electronics"
          posts={electronicsPosts}
        />

        <CategorySection
          title="Vehicles"
          posts={vehiclePosts}
        />
      </main>
    </>
  );
}

const styles = {
  main: {
    padding: "2rem",
  },
  filterBar: {
    marginBottom: "1.5rem",
  },
  input: {
    marginLeft: "0.5rem",
    padding: "0.25rem",
  },
};