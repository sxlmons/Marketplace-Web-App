import React, { useEffect } from "react";

export default function Lightbox({ images, currentIndex, onClose, onNavigate }) {
    useEffect(() => {
        function handleKeyDown(e) {
            if (!images || images.length === 0) return;

            switch (e.key) {
                case "ArrowLeft":
                    onNavigate((currentIndex - 1 + images.length) % images.length);
                    break;
                case "ArrowRight":
                    onNavigate((currentIndex + 1) % images.length);
                    break;
                case "Escape":
                    onClose();
                    break;
                default:
                    break;
            }
        }

        window.addEventListener("keydown", handleKeyDown);
        return () => window.removeEventListener("keydown", handleKeyDown);
    }, [images, currentIndex, onClose, onNavigate]);

    if (!images || !images[currentIndex]) return null;

    return (
        <div className="image-modal-overlay" onClick={onClose}>
            <div className="image-modal-content" onClick={(e) => e.stopPropagation()}>
                <button className="image-modal-close" onClick={onClose}>X</button>

                <button
                    className="image-modal-nav left"
                    onClick={() => onNavigate((currentIndex - 1 + images.length) % images.length)}
                >
                    &lt;
                </button>
                <button
                    className="image-modal-nav right"
                    onClick={() => onNavigate((currentIndex + 1) % images.length)}
                >
                    &gt;
                </button>

                <img src={images[currentIndex]} alt={`Image ${currentIndex + 1}`} />
            </div>
        </div>
    );
}