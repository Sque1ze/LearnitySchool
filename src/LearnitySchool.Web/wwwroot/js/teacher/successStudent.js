(() => {
    // tabs
    const tabs = Array.from(document.querySelectorAll(".tab"));
    const panes = Array.from(document.querySelectorAll(".tab-pane"));
    function setTab(name) {
        tabs.forEach(t => t.classList.toggle("active", t.dataset.tab === name));
        panes.forEach(p => p.classList.toggle("active", p.dataset.pane === name));
    }
    tabs.forEach(t => t.addEventListener("click", () => setTab(t.dataset.tab)));

    // student search
    const input = document.getElementById("studentSearch");
    const rows = Array.from(document.querySelectorAll("#studentsTable tbody .student-row"));
    if (input && rows.length) {
        input.addEventListener("input", () => {
            const q = (input.value || "").trim().toLowerCase();
            rows.forEach(r => {
                const text = r.textContent.toLowerCase();
                r.style.display = text.includes(q) ? "" : "none";
            });
        });
    }

    // success (UI only): filter right side by selected lesson id
    const lessonBtns = Array.from(document.querySelectorAll("#lessonList .lesson-item"));
    const progressRows = Array.from(document.querySelectorAll("#studentsProgress .progress-row"));
    function showLesson(lessonId) {
        lessonBtns.forEach(b => b.classList.toggle("active", b.dataset.lesson === lessonId));
        progressRows.forEach(r => r.style.display = (r.dataset.lesson === lessonId) ? "" : "none");
    }
    if (lessonBtns.length && progressRows.length) {
        const active = lessonBtns.find(x => x.classList.contains("active")) || lessonBtns[0];
        if (active) showLesson(active.dataset.lesson);
        lessonBtns.forEach(b => b.addEventListener("click", () => showLesson(b.dataset.lesson)));
    }
})();