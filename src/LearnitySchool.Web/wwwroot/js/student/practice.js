(() => {
    const tabs = Array.from(document.querySelectorAll('.tab'));
    const panes = Array.from(document.querySelectorAll('.tab-pane'));

    const htmlEl = document.getElementById('htmlEditor');
    const cssEl = document.getElementById('cssEditor');
    const jsEl = document.getElementById('jsEditor');

    const runBtn = document.getElementById('runBtn');
    const checkBtn = document.getElementById('checkBtn');

    const frame = document.getElementById('previewFrame');
    const badge = document.getElementById('similarityBadge');

    const overlay = document.getElementById('overlay');

    if (!htmlEl || !cssEl || !jsEl || !runBtn || !checkBtn || !frame) {
        console.warn("Practice UI elements not found. Check Practice.cshtml ids.");
        return;
    }

    // reference = starter (поки)
    const reference = {
        html: htmlEl.value || "",
        css: cssEl.value || "",
        js: jsEl.value || ""
    };

    function setTab(name) {
        tabs.forEach(t => t.classList.toggle('active', t.dataset.tab === name));
        panes.forEach(p => p.classList.toggle('active', p.dataset.pane === name));
    }

    tabs.forEach(t => t.addEventListener('click', () => setTab(t.dataset.tab)));

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

    function run() {
        const doc = buildDoc(htmlEl.value, cssEl.value, jsEl.value);

        // ✅ найпростіший і безпечний спосіб
        frame.srcdoc = doc;
    }


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

    function similarityPercent(ref, cur) {
        const refSet = tokenSet(ref);
        const curSet = tokenSet(cur);
        if (refSet.size === 0) return 0;

        let hit = 0;
        refSet.forEach(t => { if (curSet.has(t)) hit++; });

        return Math.round((hit * 100) / refSet.size);
    }

    function check() {
        if (overlay) overlay.classList.remove('hidden');

        setTimeout(() => {
            const sHtml = similarityPercent(reference.html, htmlEl.value);
            const sCss = similarityPercent(reference.css, cssEl.value);
            const sJs = similarityPercent(reference.js, jsEl.value);

            const avg = Math.round((sHtml + sCss + sJs) / 3);

            if (badge) {
                badge.classList.remove('badge-muted');
                badge.classList.add('badge-info');
                badge.textContent = `Similarity: ${avg}%`;
            }

            if (overlay) overlay.classList.add('hidden');
        }, 250);
    }

    runBtn.addEventListener('click', run);
    checkBtn.addEventListener('click', check);

    // авто-run
    run();
})();
