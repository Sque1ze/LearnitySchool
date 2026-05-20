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

    const threshold = submitBtn
        ? Number(submitBtn.dataset.threshold || meta?.dataset?.threshold || "0")
        : Number(meta?.dataset?.threshold || "0");

    function isTrue(value) {
        return String(value || "").toLowerCase() === "true";
    }

    const teacherReviewMode =
        isTrue(submitBtn?.dataset?.isReview) ||
        isTrue(meta?.dataset?.isReview) ||
        submitBtn?.dataset?.mode === "1" ||
        meta?.dataset?.mode === "1";

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
    let lastSimilarity = 0;

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
    // MONACO EDITOR
    // =========================

    function createEditor(host, language, value) {
        return monaco.editor.create(host, {
            value: value || "",
            language,
            theme: "vs-dark",

            automaticLayout: true,
            fixedOverflowWidgets: true,
            minimap: {
                enabled: false
            },

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
            snippetSuggestions: "top",
            suggest: {
                showIcons: true,
                preview: true,
                insertMode: "replace",
                snippetsPreventQuickSuggestions: false
            },

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

    function installHtmlAutoClose(editor) {
        const voidTags = new Set(["area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "param", "source", "track", "wbr"]);

        editor.onDidType((text) => {
            if (text !== ">") return;

            const model = editor.getModel();
            const position = editor.getPosition();
            if (!model || !position) return;

            const lineUntilCursor = model.getLineContent(position.lineNumber).slice(0, position.column - 1);
            const match = lineUntilCursor.match(/<([a-zA-Z][\w:-]*)(?:\s[^<>]*)?>$/);
            if (!match) return;

            const fullTag = match[0];
            const tagName = match[1].toLowerCase();

            if (fullTag.startsWith("</") || fullTag.endsWith("/>") || voidTags.has(tagName)) return;

            const after = model.getValueInRange({
                startLineNumber: position.lineNumber,
                startColumn: position.column,
                endLineNumber: position.lineNumber,
                endColumn: Math.min(model.getLineMaxColumn(position.lineNumber), position.column + tagName.length + 4)
            });

            if (after.startsWith(`</${tagName}>`)) return;

            editor.executeEdits("auto-close-html-tag", [{
                range: new monaco.Range(position.lineNumber, position.column, position.lineNumber, position.column),
                text: `</${tagName}>`,
                forceMoveMarkers: true
            }]);

            editor.setPosition(position);
        });
    }

    function writeConsoleLine(target, type, args) {
        if (!target) return;
        const empty = target.querySelector(".console-empty");
        empty?.remove();

        const line = document.createElement("div");
        line.className = `console-line console-line--${type || "log"}`;
        const time = new Date().toLocaleTimeString([], { hour: "2-digit", minute: "2-digit", second: "2-digit" });
        line.textContent = `[${time}] ${Array.isArray(args) ? args.join(" ") : String(args || "")}`;
        target.appendChild(line);
        target.scrollTop = target.scrollHeight;
    }

    function clearConsole(target, hint = "Console output зʼявиться тут після Run.") {
        if (!target) return;
        target.innerHTML = hint ? `<div class="console-empty">${hint}</div>` : "";
    }

    const previewConsole = document.getElementById("previewConsole");
    const clearConsoleBtn = document.getElementById("clearConsoleBtn");
    clearConsoleBtn?.addEventListener("click", () => clearConsole(previewConsole));
    window.addEventListener("message", (event) => {
        if (event?.data?.source !== "learnity-console") return;
        writeConsoleLine(previewConsole, event.data.type, event.data.args);
    });

    function getHtmlValue() {
        return htmlEditor ? htmlEditor.getValue() : "";
    }

    function getCssValue() {
        return cssEditor ? cssEditor.getValue() : "";
    }

    function getJsValue() {
        return jsEditor ? jsEditor.getValue() : "";
    }

    function setEditorValues(html, css, js) {
        htmlEditor?.setValue(html || "");
        cssEditor?.setValue(css || "");
        jsEditor?.setValue(js || "");
    }

    function setSubmitValues(avg = lastSimilarity) {
        if (submitHtml) submitHtml.value = getHtmlValue();
        if (submitCss) submitCss.value = getCssValue();
        if (submitJs) submitJs.value = getJsValue();
        if (submitSimilarity) submitSimilarity.value = String(avg || 0);
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
        tabs.forEach(t => {
            t.classList.toggle("active", t.dataset.tab === name);
        });

        panes.forEach(p => {
            p.classList.toggle("active", p.dataset.pane === name);
        });

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

        previewTabBtn?.classList.toggle("active", isPreview);
        referenceTabBtn?.classList.toggle("active", isRef);
    }

    function buildDoc(html, css, js, captureConsole = true) {
        const consoleBridge = captureConsole ? `
  function safeString(value) {
    try {
      if (typeof value === 'string') return value;
      if (value instanceof Error) return value.stack || value.message;
      if (typeof value === 'object') return JSON.stringify(value);
      return String(value);
    } catch (_) {
      return String(value);
    }
  }
  function send(type, args) {
    window.parent && window.parent.postMessage({
      source: 'learnity-console',
      type: type,
      args: Array.prototype.slice.call(args || []).map(safeString)
    }, '*');
  }
  ['log','info','warn','error'].forEach(function(type){
    var original = console[type];
    console[type] = function(){
      send(type, arguments);
      if (original) original.apply(console, arguments);
    };
  });
  window.addEventListener('error', function(e){ send('error', [e.message + ' at line ' + e.lineno]); });
  window.addEventListener('unhandledrejection', function(e){ send('error', [e.reason]); });
` : "";

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
  ${consoleBridge}
  try {
    ${js || ""}
  } catch (e) {
    console.error(e && e.stack ? e.stack : e);
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
        clearConsole(previewConsole);
        previewFrame.srcdoc = buildDoc(
            getHtmlValue(),
            getCssValue(),
            getJsValue()
        );
    }

    function renderReferencePreview() {
        if (!hasReference || !referenceFrame) return;

        referenceFrame.srcdoc = buildDoc(
            refHtml,
            refCss,
            refJs,
            false
        );
    }

    function showMyPreview() {
        setPreviewMode("preview");
    }

    function showReference() {
        if (!hasReference || !referenceFrame) return;

        clearConsole(previewConsole, "");
        renderReferencePreview();
        setPreviewMode("reference");
    }

    previewTabBtn?.addEventListener("click", showMyPreview);
    referenceTabBtn?.addEventListener("click", showReference);

    // =========================
    // SIMILARITY CHECK
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

        if (refSet.size === 0) {
            return null;
        }

        const curSet = tokenSet(curText);
        let hit = 0;

        refSet.forEach(t => {
            if (curSet.has(t)) {
                hit++;
            }
        });

        return Math.round((hit * 100) / refSet.size);
    }

    function updateSubmitState(avg) {
        lastSimilarity = avg || 0;
        setSubmitValues(lastSimilarity);

        if (!submitBtn) return;

        if (teacherReviewMode) {
            submitBtn.disabled = false;
            submitBtn.title = "Здати роботу на перевірку вчителю";
            return;
        }

        const ok = lastSimilarity >= threshold;

        submitBtn.disabled = false;
        submitBtn.title = ok
            ? "Ready to submit"
            : `Можна здати, але буде не зараховано до ${threshold}%`;
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

                badge.textContent = teacherReviewMode
                    ? `Similarity: ${avg}% (optional)`
                    : `Similarity: ${avg}% (need ${threshold}%)`;
            }

            updateSubmitState(avg);

            overlay?.classList.add("hidden");
        }, 200);
    }

    // =========================
    // DRAFT AUTOSAVE
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

            if (!res.ok) {
                throw new Error("Save failed");
            }

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
        if (saveTimer) {
            clearTimeout(saveTimer);
        }

        saveTimer = setTimeout(saveDraft, 900);
    }

    function onEditorChanged() {
        setSubmitValues(lastSimilarity);

        if (teacherReviewMode && submitBtn) {
            submitBtn.disabled = false;
        }

        scheduleSave();
    }

    // =========================
    // RESET
    // =========================

    function resetToStarter() {
        const ok = confirm("Reset code to starter version?");

        if (!ok) return;

        setEditorValues(
            starterHtml,
            starterCss,
            starterJs
        );

        lastSimilarity = 0;

        if (badge) {
            badge.classList.remove("badge-info");
            badge.classList.add("badge-muted");

            badge.textContent = teacherReviewMode
                ? "Similarity: optional"
                : `Similarity: — (need ${threshold}%)`;
        }

        if (submitBtn) {
            if (teacherReviewMode) {
                submitBtn.disabled = false;
                submitBtn.title = "Здати роботу на перевірку вчителю";
            } else {
                submitBtn.disabled = false;
                submitBtn.title = `Можна здати, але для зарахування потрібно ${threshold}%`;
            }
        }

        setSubmitValues(0);

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

        installHtmlAutoClose(htmlEditor);

        htmlEditor.onDidChangeModelContent(onEditorChanged);
        cssEditor.onDidChangeModelContent(onEditorChanged);
        jsEditor.onDidChangeModelContent(onEditorChanged);

        runBtn?.addEventListener("click", () => {
            renderMyPreview();
            showMyPreview();
        });

        checkBtn?.addEventListener("click", check);
        resetBtn?.addEventListener("click", resetToStarter);

        submitForm?.addEventListener("submit", () => {
            setSubmitValues(lastSimilarity);
        });

        if (teacherReviewMode && submitBtn) {
            submitBtn.disabled = false;
            submitBtn.title = "Здати роботу на перевірку вчителю";
        }

        if (badge && teacherReviewMode) {
            badge.textContent = "Similarity: optional";
        }

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