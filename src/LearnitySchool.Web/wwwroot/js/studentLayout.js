(function () {
    function closeAll() {
        document.querySelectorAll(".dropdown.open").forEach(d => d.classList.remove("open"));
    }

    document.addEventListener("click", function (e) {
        const btn = e.target.closest("[data-dropdown]");
        const dropdown = e.target.closest(".dropdown");

        // клік по кнопці — toggle
        if (btn) {
            const id = btn.getAttribute("data-dropdown");
            const el = document.getElementById(id);
            if (!el) return;

            const isOpen = el.classList.contains("open");
            closeAll();
            if (!isOpen) el.classList.add("open");
            return;
        }

        // клік поза дропдауном — закрити
        if (!dropdown) closeAll();
    });

    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape") closeAll();
    });
})();
