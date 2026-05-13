// Animación suave al hacer scroll
document.addEventListener("DOMContentLoaded", function () {
    const elements = document.querySelectorAll(".card, table, form");

    const observer = new IntersectionObserver(entries => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add("fade-in");
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.2 });

    elements.forEach(el => {
        el.classList.add("hidden");
        observer.observe(el);
    });
});

// Clase CSS para animación
const style = document.createElement("style");
style.innerHTML = `
.hidden { opacity: 0; transform: translateY(20px); transition: all 0.6s ease-out; }
.fade-in { opacity: 1; transform: translateY(0); }
`;
document.head.appendChild(style);
