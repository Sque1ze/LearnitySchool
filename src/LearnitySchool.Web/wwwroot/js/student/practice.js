(() => {
    // ===== elements =====
    const tabs = Array.from(document.querySelectorAll(".tab"));
    const panes = Array.from(document.querySelectorAll(".tab-pane"));

    const htmlEl = document.getElementById("htmlEditor");
    const cssEl = document.getElementById("cssEditor");
    const jsEl = document.getElementById("jsEditor");

    const runBtn = document.getElementById("runBtn");
    const checkBtn = document.getElementById("checkBtn");
    const resetBtn = document.getElementById("resetBtn");

    const previewFrame = document.getElementById("previewFrame");
    const referenceFrame = document.getElementById("referenceFrame");

    const badge = document.getElementById("similarityBadge");
    const overlay = document.getElementById("overlay");

    const previewTabBtn = document.getElementById("previewTabBtn");
    const referenceTabBtn = document.getElementById("referenceTabBtn");

    // submit
    const submitBtn = document.getElementById("submitBtn");
    const submitHtml = document.getElementById("submitHtml");
    const submitCss = document.getElementById("submitCss");
    const submitJs = document.getElementById("submitJs");
    const submitSimilarity = document.getElementById("submitSimilarity");

    // draft
    const draftStatus = document.getElementById("draftStatus");
    const anti = document.querySelector('input[name="__RequestVerificationToken"]');
    const token = anti ? anti.value : null;
    const taskId = document.getElementById("taskMeta")?.dataset?.taskid || null;

    if (!htmlEl || !cssEl || !jsEl || !previewFrame) return;

    const threshold = submitBtn ? Number(submitBtn.dataset.threshold || "0") : 0;

    // ===== reference from hidden textareas (read once) =====
    const refHtml = document.getElementById("refHtml")?.value || "";
    const refCss = document.getElementById("refCss")?.value || "";
    const refJs = document.getElementById("refJs")?.value || "";

    const hasReference =
        (refHtml.trim().length > 0) ||
        (refCss.trim().length > 0) ||
        (refJs.trim().length > 0);

    // ===== starter original (for Reset) =====
    const starterHtml = document.getElementById("starterHtml")?.value ?? "";
    const starterCss = document.getElementById("starterCss")?.value ?? "";
    const starterJs = document.getElementById("starterJs")?.value ?? "";

    // ===== editor tabs =====
    function setTab(name) {
        tabs.forEach(t => t.classList.toggle("active", t.dataset.tab === name));
        panes.forEach(p => p.classList.toggle("active", p.dataset.pane === name));
    }
    tabs.forEach(t => t.addEventListener("click", () => setTab(t.dataset.tab)));
    setTab("html");

    // ===== iframe doc =====
    function buildDoc(html, css, js) {
        return `<!doctype html>
<html>
<head>
<meta charset="utf-8" />
<meta name="viewport" content="width=device-width, initial-scale=1" />
<style>${css || ""}</style>
</head>
<body>
${html || ""}
<script>
(function(){
  try {
    ${js || ""}
  } catch (e) {
    const pre = document.createElement('pre');
    pre.style.color = 'crimson';
    pre.style.whiteSpace = 'pre-wrap';
    pre.textContent = e && e.stack ? e.stack : String(e);
    document.body.appendChild(pre);
  }
})();
<\/script>
</body>
</html>`;
    }

    function renderMyPreview() {
        previewFrame.srcdoc = buildDoc(htmlEl.value, cssEl.value, jsEl.value);
    }

    function renderReferencePreview() {
        if (!hasReference || !referenceFrame) return;
        referenceFrame.srcdoc = buildDoc(refHtml, refCss, refJs);
    }

    function showMyPreview() {
        previewFrame.style.display = "block";
        if (referenceFrame) referenceFrame.style.display = "none";
    }

    function showReference() {
        if (!hasReference || !referenceFrame) return;
        renderReferencePreview();
        previewFrame.style.display = "none";
        referenceFrame.style.display = "block";
    }

    previewTabBtn?.addEventListener("click", showMyPreview);
    referenceTabBtn?.addEventListener("click", showReference);

    runBtn?.addEventListener("click", () => {
        renderMyPreview();
        showMyPreview();
    });

    // ===== similarity =====
    function normalize(s) {
        return (s || "")
            .replace(/\r\n/g, "\n")
            .replace(/[ \t]+/g, " ")
            .replace(/\n{3,}/g, "\n\n")
            .trim()
            .toLowerCase();
    }

    function tokenSet(s) {
        const m = normalize(s).match(/[a-zA-Z0-9_\-#\.]+/g);
        return new Set(m || []);
    }

    // if ref is empty => null => ignore part
    function similarityPercent(refText, curText) {
        const refSet = tokenSet(refText);
        if (refSet.size === 0) return null;

        const curSet = tokenSet(curText);
        let hit = 0;
        refSet.forEach(t => { if (curSet.has(t)) hit++; });

        return Math.round((hit * 100) / refSet.size);
    }

    function updateSubmitState(avg) {
        // payload for submit
        submitHtml && (submitHtml.value = htmlEl.value);
        submitCss && (submitCss.value = cssEl.value);
        submitJs && (submitJs.value = jsEl.value);
        submitSimilarity && (submitSimilarity.value = String(avg));

        if (submitBtn) {
            const ok = avg >= threshold;
            submitBtn.disabled = !ok;
            submitBtn.title = ok ? "Ready to submit" : `Need at least ${threshold}%`;
        }
    }

    function check() {
        overlay?.classList.remove("hidden");

        setTimeout(() => {
            const pHtml = similarityPercent(refHtml, htmlEl.value);
            const pCss = similarityPercent(refCss, cssEl.value);
            const pJs = similarityPercent(refJs, jsEl.value);

            const parts = [pHtml, pCss, pJs].filter(x => x !== null);
            const avg = parts.length
                ? Math.round(parts.reduce((a, b) => a + b, 0) / parts.length)
                : 0;

            if (badge) {
                badge.classList.remove("badge-muted");
                badge.classList.add("badge-info");
                badge.textContent = `Similarity: ${avg}% (need ${threshold}%)`;
            }

            updateSubmitState(avg);
            overlay?.classList.add("hidden");
        }, 200);
    }

    checkBtn?.addEventListener("click", check);

    // ===== draft autosave =====
    let saveTimer = null;
    let saving = false;

    function setDraftBadge(text, ok = true) {
        if (!draftStatus) return;
        draftStatus.textContent = text;
        draftStatus.classList.remove("badge-info", "badge-muted");
        draftStatus.classList.add(ok ? "badge-info" : "badge-muted");
    }

    async function saveDraft() {
        if (!taskId || !token) return;
        if (saving) return;

        saving = true;
        setDraftBadge("Draft: saving…", true);

        try {
            const res = await fetch("/Student/SavePracticeDraft", {
                method: "POST",
                credentials: "same-origin",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": token
                },
                body: JSON.stringify({
                    taskId,
                    html: htmlEl.value || "",
                    css: cssEl.value || "",
                    js: jsEl.value || ""
                })
            });

            if (!res.ok) throw new Error("Save failed");
            const data = await res.json();

            const t = data.updatedAt ? new Date(data.updatedAt) : new Date();
            const hh = String(t.getHours()).padStart(2, "0");
            const mm = String(t.getMinutes()).padStart(2, "0");
            const ss = String(t.getSeconds()).padStart(2, "0");

            setDraftBadge(`Draft: saved ${hh}:${mm}:${ss}`, true);
        } catch (e) {
            setDraftBadge("Draft: not saved", false);
            console.warn(e);
        } finally {
            saving = false;
        }
    }

    function scheduleSave() {
        if (saveTimer) clearTimeout(saveTimer);
        saveTimer = setTimeout(saveDraft, 900);
    }

    htmlEl.addEventListener("input", scheduleSave);
    cssEl.addEventListener("input", scheduleSave);
    jsEl.addEventListener("input", scheduleSave);

    // ===== reset =====
    function resetToStarter() {
        const ok = confirm("Reset code to starter version?");
        if (!ok) return;

        htmlEl.value = starterHtml;
        cssEl.value = starterCss;
        jsEl.value = starterJs;

        // reset badge + submit
        if (badge) {
            badge.classList.remove("badge-info");
            badge.classList.add("badge-muted");
            badge.textContent = `Similarity: — (need ${threshold}%)`;
        }
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.title = "Спочатку натисни Check і набери потрібний %";
        }

        renderMyPreview();
        showMyPreview();

        // save as draft after reset
        scheduleSave();
        setDraftBadge("Draft: saving…", true);
    }

    resetBtn?.addEventListener("click", resetToStarter);

    // ===== init =====
    renderMyPreview();
    showMyPreview();
    // reference iframe will render on click (or you can pre-render if you want):
    // if (hasReference) renderReferencePreview();
})();
