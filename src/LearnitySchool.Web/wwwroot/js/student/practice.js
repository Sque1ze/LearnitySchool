(() => {
    const tabs = Array.from(document.querySelectorAll(".tab"));
    const panes = Array.from(document.querySelectorAll(".tab-pane"));

    const htmlHost = document.getElementById("htmlEditor");
    const cssHost = document.getElementById("cssEditor");
    const jsHost = document.getElementById("jsEditor");

    const runBtn = document.getElementById("runBtn");
    const checkBtn = document.getElementById("checkBtn");
    const resetBtn = document.getElementById("resetBtn");

    const previewFrame = document.getElementById("previewFrame");
    const referenceFrame = document.getElementById("referenceFrame");

    const badge = document.getElementById("similarityBadge");
    const overlay = document.getElementById("overlay");

    const previewTabBtn = document.getElementById("previewTabBtn");
    const referenceTabBtn = document.getElementById("referenceTabBtn");

    const submitBtn = document.getElementById("submitBtn");
    const submitForm = document.getElementById("submitForm");
    const submitHtml = document.getElementById("submitHtml");
    const submitCss = document.getElementById("submitCss");
    const submitJs = document.getElementById("submitJs");
    const submitSimilarity = document.getElementById("submitSimilarity");

    const draftStatus = document.getElementById("draftStatus");
    const anti = document.querySelector('input[name="__RequestVerificationToken"]');
    const token = anti ? anti.value : null;

    const meta = document.getElementById("taskMeta");
    const taskId = meta?.dataset?.taskid || null;
    const threshold = submitBtn ? Number(submitBtn.dataset.threshold || "0") : 0;

    const initialEditorHtml = document.getElementById("editorHtmlValue")?.value ?? "";
    const initialEditorCss = document.getElementById("editorCssValue")?.value ?? "";
    const initialEditorJs = document.getElementById("editorJsValue")?.value ?? "";

    const starterHtml = document.getElementById("starterHtml")?.value ?? "";
    const starterCss = document.getElementById("starterCss")?.value ?? "";
    const starterJs = document.getElementById("starterJs")?.value ?? "";

    const refHtml = document.getElementById("refHtml")?.value || "";
    const refCss = document.getElementById("refCss")?.value || "";
    const refJs = document.getElementById("refJs")?.value || "";

    const hasReference =
        refHtml.trim().length > 0 ||
        refCss.trim().length > 0 ||
        refJs.trim().length > 0;

    if (!htmlHost || !cssHost || !jsHost || !previewFrame) return;

    let htmlEditor = null;
    let cssEditor = null;
    let jsEditor = null;

    // =========================
    // LOAD MONACO FROM CDN
    // =========================
    function loadScript(src) {
        return new Promise((resolve, reject) => {
            const s = document.createElement("script");
            s.src = src;
            s.onload = resolve;
            s.onerror = reject;
            document.head.appendChild(s);
        });
    }

    async function loadMonaco() {
        if (window.monaco && window.monaco.editor) return;

        if (!window.require) {
            await loadScript("https://cdn.jsdelivr.net/npm/monaco-editor@latest/min/vs/loader.js");
        }

        await new Promise((resolve) => {
            window.require.config({
                paths: {
                    vs: "https://cdn.jsdelivr.net/npm/monaco-editor@latest/min/vs"
                }
            });

            window.require(["vs/editor/editor.main"], function () {
                resolve();
            });
        });
    }

    // =========================
    // MONACO
    // =========================
    function createEditor(host, language, value) {
        return monaco.editor.create(host, {
            value: value || "",
            language,
            theme: "vs-dark",
            automaticLayout: true,
            minimap: { enabled: false },
            fontSize: 14,
            lineHeight: 22,
            roundedSelection: true,
            scrollBeyondLastLine: false,
            wordWrap: "on",
            renderWhitespace: "selection",
            tabSize: 2,
            insertSpaces: true,
            detectIndentation: false,
            quickSuggestions: {
                other: true,
                comments: false,
                strings: true
            },
            quickSuggestionsDelay: 80,
            suggestOnTriggerCharacters: true,
            acceptSuggestionOnEnter: "on",
            snippetSuggestions: "inline",
            formatOnPaste: true,
            formatOnType: true,
            autoClosingBrackets: "always",
            autoClosingQuotes: "always",
            autoClosingComments: "always",
            autoSurround: "languageDefined",
            autoIndent: "full",
            bracketPairColorization: {
                enabled: true
            },
            guides: {
                bracketPairs: true,
                indentation: true
            },
            padding: {
                top: 12,
                bottom: 12
            }
        });
    }

    function getHtmlValue() {
        return htmlEditor ? htmlEditor.getValue() : "";
    }

    function getCssValue() {
        return cssEditor ? cssEditor.getValue() : "";
    }

    function getJsValue() {
        return jsEditor ? jsEditor.getValue() : "";
    }

    function setSubmitValues(avg = 0) {
        if (submitHtml) submitHtml.value = getHtmlValue();
        if (submitCss) submitCss.value = getCssValue();
        if (submitJs) submitJs.value = getJsValue();
        if (submitSimilarity) submitSimilarity.value = String(avg);
    }

    // =========================
    // TABS
    // =========================
    function layoutEditors() {
        htmlEditor?.layout();
        cssEditor?.layout();
        jsEditor?.layout();
    }

    function setTab(name) {
        tabs.forEach(t => t.classList.toggle("active", t.dataset.tab === name));
        panes.forEach(p => p.classList.toggle("active", p.dataset.pane === name));

        setTimeout(() => {
            layoutEditors();

            if (name === "html") htmlEditor?.focus();
            if (name === "css") cssEditor?.focus();
            if (name === "js") jsEditor?.focus();
        }, 0);
    }

    tabs.forEach(t => {
        t.addEventListener("click", () => setTab(t.dataset.tab));
    });

    // =========================
    // PREVIEW
    // =========================
    function setPreviewMode(mode) {
        const isPreview = mode === "preview";
        const isRef = mode === "reference";

        previewFrame.style.display = isPreview ? "block" : "none";
        if (referenceFrame) {
            referenceFrame.style.display = isRef ? "block" : "none";
        }

        if (previewTabBtn) previewTabBtn.classList.toggle("active", isPreview);
        if (referenceTabBtn) referenceTabBtn.classList.toggle("active", isRef);
    }

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
    pre.style.padding = '12px';
    pre.textContent = e && e.stack ? e.stack : String(e);
    document.body.appendChild(pre);
  }
})();
<\/script>
</body>
</html>`;
    }

    function renderMyPreview() {
        previewFrame.srcdoc = buildDoc(getHtmlValue(), getCssValue(), getJsValue());
    }

    function renderReferencePreview() {
        if (!hasReference || !referenceFrame) return;
        referenceFrame.srcdoc = buildDoc(refHtml, refCss, refJs);
    }

    function showMyPreview() {
        setPreviewMode("preview");
    }

    function showReference() {
        if (!hasReference || !referenceFrame) return;
        renderReferencePreview();
        setPreviewMode("reference");
    }

    previewTabBtn?.addEventListener("click", showMyPreview);
    referenceTabBtn?.addEventListener("click", showReference);

    // =========================
    // SIMILARITY
    // =========================
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

    function similarityPercent(refText, curText) {
        const refSet = tokenSet(refText);
        if (refSet.size === 0) return null;

        const curSet = tokenSet(curText);
        let hit = 0;

        refSet.forEach(t => {
            if (curSet.has(t)) hit++;
        });

        return Math.round((hit * 100) / refSet.size);
    }

    function updateSubmitState(avg) {
        setSubmitValues(avg);

        if (!submitBtn) return;

        const ok = avg >= threshold;
        submitBtn.disabled = !ok;
        submitBtn.title = ok ? "Ready to submit" : `Need at least ${threshold}%`;
    }

    function check() {
        overlay?.classList.remove("hidden");

        setTimeout(() => {
            const pHtml = similarityPercent(refHtml, getHtmlValue());
            const pCss = similarityPercent(refCss, getCssValue());
            const pJs = similarityPercent(refJs, getJsValue());

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

    // =========================
    // DRAFT
    // =========================
    let saveTimer = null;
    let saving = false;

    function setDraftBadge(text, ok = true) {
        if (!draftStatus) return;

        draftStatus.textContent = text;
        draftStatus.classList.remove("badge-info", "badge-muted");
        draftStatus.classList.add(ok ? "badge-info" : "badge-muted");
    }

    async function saveDraft() {
        if (!taskId || !token || saving) return;

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
                    html: getHtmlValue(),
                    css: getCssValue(),
                    js: getJsValue()
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

    // =========================
    // RESET
    // =========================
    function resetToStarter() {
        const ok = confirm("Reset code to starter version?");
        if (!ok) return;

        htmlEditor?.setValue(starterHtml);
        cssEditor?.setValue(starterCss);
        jsEditor?.setValue(starterJs);

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

        scheduleSave();
        setDraftBadge("Draft: saving…", true);
    }

    // =========================
    // INIT
    // =========================
    async function init() {
        await loadMonaco();

        // HTML defaults + better completion behavior
        monaco.languages.html.htmlDefaults.setOptions({
            format: {
                tabSize: 2,
                insertSpaces: true,
                wrapLineLength: 120,
                unformatted: "wbr"
            },
            suggest: {
                html5: true
            }
        });

        monaco.languages.css.cssDefaults.setOptions({
            validate: true,
            lint: {
                unknownProperties: "warning",
                duplicateProperties: "warning"
            }
        });

        monaco.languages.typescript.javascriptDefaults.setDiagnosticsOptions({
            noSemanticValidation: false,
            noSyntaxValidation: false
        });

        monaco.languages.typescript.javascriptDefaults.setCompilerOptions({
            target: monaco.languages.typescript.ScriptTarget.ES2020,
            allowNonTsExtensions: true
        });

        htmlEditor = createEditor(htmlHost, "html", initialEditorHtml);
        cssEditor = createEditor(cssHost, "css", initialEditorCss);
        jsEditor = createEditor(jsHost, "javascript", initialEditorJs);

        htmlEditor.onDidChangeModelContent(() => {
            setSubmitValues(Number(submitSimilarity?.value || "0"));
            scheduleSave();
        });

        cssEditor.onDidChangeModelContent(() => {
            setSubmitValues(Number(submitSimilarity?.value || "0"));
            scheduleSave();
        });

        jsEditor.onDidChangeModelContent(() => {
            setSubmitValues(Number(submitSimilarity?.value || "0"));
            scheduleSave();
        });

        runBtn?.addEventListener("click", () => {
            renderMyPreview();
            showMyPreview();
        });

        checkBtn?.addEventListener("click", check);
        resetBtn?.addEventListener("click", resetToStarter);

        submitForm?.addEventListener("submit", () => {
            setSubmitValues(Number(submitSimilarity?.value || "0"));
        });

        setSubmitValues(0);
        setPreviewMode("preview");
        renderMyPreview();
        setTab("html");
        showMyPreview();

        window.addEventListener("resize", layoutEditors);
    }

    init().catch(err => {
        console.error("Monaco init failed:", err);

        if (badge) {
            badge.classList.remove("badge-info");
            badge.classList.add("badge-muted");
            badge.textContent = "Editor failed to load";
        }
    });
})();