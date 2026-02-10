(() => {
    const tabs = Array.from(document.querySelectorAll(".tab"));
    const panes = Array.from(document.querySelectorAll(".tab-pane"));

    function setTab(name) {
        tabs.forEach(t => t.classList.toggle("active", t.dataset.tab === name));
        panes.forEach(p => p.classList.toggle("active", p.dataset.pane === name));
    }

    tabs.forEach(t => t.addEventListener("click", () => setTab(t.dataset.tab)));

    function qs(sel, root = document) { return root.querySelector(sel); }
    function qsa(sel, root = document) { return Array.from(root.querySelectorAll(sel)); }

    const lessonBtns = qsa("#lessonList .lesson-item");
    const progressRows = qsa("#studentsProgress .progress-row");

    function showLesson(lessonId) {
        lessonBtns.forEach(b => b.classList.toggle("active", b.dataset.lesson === lessonId));
        progressRows.forEach(r => r.style.display = (r.dataset.lesson === lessonId) ? "" : "none");
    }

    if (lessonBtns.length && progressRows.length) {
        const active = lessonBtns.find(x => x.classList.contains("active")) || lessonBtns[0];
        if (active) showLesson(active.dataset.lesson);
        lessonBtns.forEach(b => b.addEventListener("click", () => showLesson(b.dataset.lesson)));
    }

    const courseRoot = document.getElementById("courseRoot");
    const courseId = courseRoot ? courseRoot.dataset.courseid : null;
    const studentsTbody = qs("#studentsTable tbody");

    async function loadStudentPanel(courseId, studentId, host) {
        host.innerHTML = `<div class="muted">Loading...</div>`;
        const url = `/TeacherGroups/StudentTasksPanel?courseId=${encodeURIComponent(courseId)}&studentUserId=${encodeURIComponent(studentId)}`;
        const resp = await fetch(url);

        if (!resp.ok) {
            host.innerHTML = `<div class="muted">Не вдалося завантажити.</div>`;
            return;
        }

        host.innerHTML = await resp.text();
        host.dataset.loaded = "1";
    }

    if (courseId && studentsTbody) {
        studentsTbody.addEventListener("click", async (e) => {
            const row = e.target.closest("tr.student-row");
            if (!row) return;

            const studentId = row.dataset.student;
            if (!studentId) return;

            const expandRow = qs(`.student-expand-row[data-expand="${studentId}"]`);
            const host = document.getElementById(`panel-${studentId}`);

            if (!expandRow || !host) return;

            const isOpen = expandRow.style.display === "table-row";
            if (isOpen) {
                expandRow.style.display = "none";
                return;
            }

            expandRow.style.display = "table-row";

            if (host.dataset.loaded === "1") return;
            await loadStudentPanel(courseId, studentId, host);
        });
    }

    const attBody = qs("#attendanceTable tbody");

    function nextStatus(cur) {
        if (cur === 0) return 1;
        if (cur === 1) return 2;
        return 0;
    }

    function applySquareUi(btn, status, lessonTitle) {
        btn.dataset.status = String(status);

        btn.classList.remove("att-none", "att-present", "att-absent");
        if (status === 1) btn.classList.add("att-present");
        else if (status === 2) btn.classList.add("att-absent");
        else btn.classList.add("att-none");

        const statusText = status === 1 ? "Присутній" : status === 2 ? "Відсутній" : "Не виставлено";
        const title = lessonTitle || (btn.title ? btn.title.split(" — ")[0] : "Урок");
        btn.title = `${title} — ${statusText}`;
    }

    async function postAttendance(courseId, lessonId, studentId, status) {
        const tokenEl = qs('#antiForm input[name="__RequestVerificationToken"]');
        const token = tokenEl ? tokenEl.value : "";

        const body = new URLSearchParams();
        body.append("courseId", courseId);
        body.append("lessonId", lessonId);
        body.append("studentUserId", studentId);
        body.append("status", String(status));

        const resp = await fetch("/TeacherGroups/SetAttendanceAjax", {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8",
                "RequestVerificationToken": token
            },
            body: body.toString()
        });

        if (!resp.ok) return null;
        return await resp.json();
    }

    if (attBody) {
        attBody.addEventListener("click", async (e) => {
            const btn = e.target.closest(".att-square");
            if (!btn) return;

            const courseId = btn.dataset.course;
            const lessonId = btn.dataset.lesson;
            const studentId = btn.dataset.student;

            const cur = parseInt(btn.dataset.status || "0", 10);
            const next = nextStatus(cur);

            const lessonTitle = (btn.title || "").split(" — ")[0];

            applySquareUi(btn, next, lessonTitle);
            btn.disabled = true;

            const result = await postAttendance(courseId, lessonId, studentId, next);

            btn.disabled = false;

            if (!result || !result.ok) {
                applySquareUi(btn, cur, lessonTitle);
                alert("Не вдалося зберегти присутність.");
                return;
            }

            applySquareUi(btn, result.status, lessonTitle);
        });
    }
})();
