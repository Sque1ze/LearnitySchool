(() => {
  const htmlHost = document.getElementById('htmlEditor');
  const cssHost = document.getElementById('cssEditor');
  const jsHost = document.getElementById('jsEditor');
  const preview = document.getElementById('previewFrame');
  const runBtn = document.getElementById('runBtn');
  const tabs = Array.from(document.querySelectorAll('.tab'));
  const panes = Array.from(document.querySelectorAll('.tab-pane'));
  const form = document.getElementById('projectForm');
  const projectId = document.getElementById('projectId');
  const title = document.getElementById('projectTitle');
  const desc = document.getElementById('projectDescription');
  const submitHtml = document.getElementById('submitHtml');
  const submitCss = document.getElementById('submitCss');
  const submitJs = document.getElementById('submitJs');
  const status = document.getElementById('saveStatus');
  const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
  if (!htmlHost || !cssHost || !jsHost || !preview) return;

  let htmlEditor, cssEditor, jsEditor, timer, saving = false;

  function loadScript(src) {
    return new Promise((resolve, reject) => {
      const s = document.createElement('script');
      s.src = src; s.onload = resolve; s.onerror = reject; document.head.appendChild(s);
    });
  }
  async function loadMonaco() {
    if (window.monaco?.editor) return;
    if (!window.require) await loadScript('https://cdn.jsdelivr.net/npm/monaco-editor@latest/min/vs/loader.js');
    await new Promise(resolve => {
      window.require.config({ paths: { vs: 'https://cdn.jsdelivr.net/npm/monaco-editor@latest/min/vs' }});
      window.require(['vs/editor/editor.main'], resolve);
    });
  }
  function createEditor(host, language, value) {
    return monaco.editor.create(host, {
      value: value || '', language, theme: 'vs-dark', automaticLayout: true, fixedOverflowWidgets: true,
      minimap: { enabled: false }, fontSize: 14, lineHeight: 22, wordWrap: 'on',
      tabSize: 2, insertSpaces: true, scrollBeyondLastLine: false,
      quickSuggestions: true, suggestOnTriggerCharacters: true, snippetSuggestions: 'top', suggest: { showIcons: true, preview: true, insertMode: 'replace', snippetsPreventQuickSuggestions: false },
      formatOnPaste: true, formatOnType: true,
      autoClosingBrackets: 'always', autoClosingQuotes: 'always', bracketPairColorization: { enabled: true }
    });
  }
  function installHtmlAutoClose(editor) {
    const voidTags = new Set(['area','base','br','col','embed','hr','img','input','link','meta','param','source','track','wbr']);
    editor.onDidType((text) => {
      if (text !== '>') return;
      const model = editor.getModel();
      const position = editor.getPosition();
      if (!model || !position) return;
      const lineUntilCursor = model.getLineContent(position.lineNumber).slice(0, position.column - 1);
      const match = lineUntilCursor.match(/<([a-zA-Z][\w:-]*)(?:\s[^<>]*)?>$/);
      if (!match) return;
      const fullTag = match[0];
      const tagName = match[1].toLowerCase();
      if (fullTag.startsWith('</') || fullTag.endsWith('/>') || voidTags.has(tagName)) return;
      const after = model.getValueInRange({ startLineNumber: position.lineNumber, startColumn: position.column, endLineNumber: position.lineNumber, endColumn: Math.min(model.getLineMaxColumn(position.lineNumber), position.column + tagName.length + 4) });
      if (after.startsWith(`</${tagName}>`)) return;
      editor.executeEdits('auto-close-html-tag', [{ range: new monaco.Range(position.lineNumber, position.column, position.lineNumber, position.column), text: `</${tagName}>`, forceMoveMarkers: true }]);
      editor.setPosition(position);
    });
  }

  const consoleOutput = document.getElementById('projectConsole');
  const clearConsoleBtn = document.getElementById('clearProjectConsoleBtn');
  function writeConsoleLine(type, args) {
    if (!consoleOutput) return;
    consoleOutput.querySelector('.console-empty')?.remove();
    const line = document.createElement('div');
    line.className = `console-line console-line--${type || 'log'}`;
    const time = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' });
    line.textContent = `[${time}] ${Array.isArray(args) ? args.join(' ') : String(args || '')}`;
    consoleOutput.appendChild(line);
    consoleOutput.scrollTop = consoleOutput.scrollHeight;
  }
  function clearConsole() {
    if (!consoleOutput) return;
    consoleOutput.innerHTML = '<div class="console-empty">Console output зʼявиться тут після Run.</div>';
  }
  clearConsoleBtn?.addEventListener('click', clearConsole);
  window.addEventListener('message', (event) => {
    if (event?.data?.source !== 'learnity-console') return;
    writeConsoleLine(event.data.type, event.data.args);
  });

  function getValues() {
    return { html: htmlEditor?.getValue() || '', css: cssEditor?.getValue() || '', js: jsEditor?.getValue() || '' };
  }
  function fillForm() {
    const v = getValues(); submitHtml.value = v.html; submitCss.value = v.css; submitJs.value = v.js;
  }
  function buildDoc({html, css, js}) {
    return `<!doctype html><html><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><style>${css||''}</style></head><body>${html||''}<script>(function(){function safeString(value){try{if(typeof value==='string')return value;if(value instanceof Error)return value.stack||value.message;if(typeof value==='object')return JSON.stringify(value);return String(value)}catch(_){return String(value)}}function send(type,args){window.parent&&window.parent.postMessage({source:'learnity-console',type:type,args:Array.prototype.slice.call(args||[]).map(safeString)},'*')}['log','info','warn','error'].forEach(function(type){var original=console[type];console[type]=function(){send(type,arguments);if(original)original.apply(console,arguments)}});window.addEventListener('error',function(e){send('error',[e.message+' at line '+e.lineno])});window.addEventListener('unhandledrejection',function(e){send('error',[e.reason])});try{${js||''}}catch(e){console.error(e&&e.stack?e.stack:e);const pre=document.createElement('pre');pre.style.color='crimson';pre.style.whiteSpace='pre-wrap';pre.textContent=e&&e.stack?e.stack:String(e);document.body.appendChild(pre)}})();<\/script></body></html>`;
  }
  function render() { clearConsole(); preview.srcdoc = buildDoc(getValues()); }
  function setTab(name) {
    tabs.forEach(t => t.classList.toggle('active', t.dataset.tab === name));
    panes.forEach(p => p.classList.toggle('active', p.dataset.pane === name));
    setTimeout(() => { htmlEditor?.layout(); cssEditor?.layout(); jsEditor?.layout(); }, 0);
  }
  async function saveAjax() {
    if (!token || saving) return;
    saving = true; status.textContent = 'Autosave: saving…';
    const v = getValues();
    try {
      const res = await fetch('/StudentProjects/SaveAjax', {
        method: 'POST', credentials: 'same-origin', headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
        body: JSON.stringify({ id: projectId.value || '00000000-0000-0000-0000-000000000000', title: title.value, description: desc.value, html: v.html, css: v.css, js: v.js })
      });
      if (!res.ok) throw new Error('save failed');
      const data = await res.json();
      if (data.id && (!projectId.value || projectId.value === '00000000-0000-0000-0000-000000000000')) projectId.value = data.id;
      const d = data.updatedAt ? new Date(data.updatedAt) : new Date();
      status.textContent = `Autosave: ${String(d.getHours()).padStart(2,'0')}:${String(d.getMinutes()).padStart(2,'0')}:${String(d.getSeconds()).padStart(2,'0')}`;
    } catch(e) { console.warn(e); status.textContent = 'Autosave: not saved'; }
    finally { saving = false; }
  }
  function scheduleSave() { clearTimeout(timer); timer = setTimeout(saveAjax, 900); }
  async function init() {
    await loadMonaco();
    htmlEditor = createEditor(htmlHost, 'html', document.getElementById('editorHtmlValue')?.value || '');
    cssEditor = createEditor(cssHost, 'css', document.getElementById('editorCssValue')?.value || '');
    jsEditor = createEditor(jsHost, 'javascript', document.getElementById('editorJsValue')?.value || '');
    installHtmlAutoClose(htmlEditor);
    [htmlEditor, cssEditor, jsEditor].forEach(e => e.onDidChangeModelContent(() => { fillForm(); scheduleSave(); }));
    title?.addEventListener('input', scheduleSave); desc?.addEventListener('input', scheduleSave);
    tabs.forEach(t => t.addEventListener('click', () => setTab(t.dataset.tab)));
    runBtn?.addEventListener('click', render);
    form?.addEventListener('submit', fillForm);
    fillForm(); render(); setTab('html');
  }
  init().catch(e => { console.error(e); status.textContent = 'Editor failed to load'; });
})();
