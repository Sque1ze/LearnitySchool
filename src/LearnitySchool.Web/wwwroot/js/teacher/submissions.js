(() => {
    const tabs = Array.from(document.querySelectorAll(".rev-tab"));
    const panes = {
        html: document.getElementById("reviewHtml"),
        css: document.getElementById("reviewCss"),
        js: document.getElementById("reviewJs"),
        refHtml: document.getElementById("referenceHtml"),
        refCss: document.getElementById("referenceCss"),
        refJs: document.getElementById("referenceJs")
    };

    function setTab(name) {
        tabs.forEach(t => t.classList.toggle("active", t.dataset.tab === name));
        Object.entries(panes).forEach(([key, el]) => el?.classList.toggle("active", key === name));
    }

    tabs.forEach(t => t.addEventListener("click", () => setTab(t.dataset.tab)));

    const studentFrame = document.getElementById("reviewPreview");
    const referenceFrame = document.getElementById("referencePreview");
    const runStudent = document.getElementById("runReviewPreview");
    const runReference = document.getElementById("runReferencePreview");

    const studentConsole = document.getElementById("studentConsole");
    const referenceConsole = document.getElementById("referenceConsole");
    const clearStudentConsole = document.getElementById("clearStudentConsole");
    const clearReferenceConsole = document.getElementById("clearReferenceConsole");

    function clearConsole(target, hint) {
        if (!target) return;
        target.innerHTML = `<div class="console-empty">${hint}</div>`;
    }

    function writeConsoleLine(target, type, args) {
        if (!target) return;
        target.querySelector(".console-empty")?.remove();
        const line = document.createElement("div");
        line.className = `console-line console-line--${type || "log"}`;
        const time = new Date().toLocaleTimeString([], { hour: "2-digit", minute: "2-digit", second: "2-digit" });
        line.textContent = `[${time}] ${Array.isArray(args) ? args.join(" ") : String(args || "")}`;
        target.appendChild(line);
        target.scrollTop = target.scrollHeight;
    }

    clearStudentConsole?.addEventListener("click", () => clearConsole(studentConsole, "Console output зʼявиться після Run student."));
    clearReferenceConsole?.addEventListener("click", () => clearConsole(referenceConsole, ""));

    window.addEventListener("message", (event) => {
        if (event?.data?.source !== "learnity-console") return;
        if (event.data.frame === "reference") return;
        writeConsoleLine(studentConsole, event.data.type, event.data.args);
    });

    function buildDoc(html, css, js, frameName, captureConsole = true) {
        const consoleBridge = captureConsole ? `
  var frameName = ${JSON.stringify(frameName || 'student')};
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
      frame: frameName,
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
    pre.textContent = e && e.stack ? e.stack : String(e);
    document.body.appendChild(pre);
  }
})();
<\/script>
</body>
</html>`;
    }

    function renderStudent() {
        if (!studentFrame) return;
        clearConsole(studentConsole, "Console output зʼявиться після Run student.");
        studentFrame.srcdoc = buildDoc(panes.html?.value || "", panes.css?.value || "", panes.js?.value || "", "student");
    }

    function renderReference() {
        if (!referenceFrame) return;
        clearConsole(referenceConsole, "");
        referenceFrame.srcdoc = buildDoc(panes.refHtml?.value || "", panes.refCss?.value || "", panes.refJs?.value || "", "reference", false);
    }

    runStudent?.addEventListener("click", renderStudent);
    runReference?.addEventListener("click", renderReference);
    renderStudent();
    renderReference();
})();
