(function () {
    const storageKey = "learnity-lang";
    const dictionary = {
        "uk|Головна": "Dashboard",
        "uk|Користувачі": "Users",
        "uk|Курси": "Courses",
        "uk|Сповіщення": "Notifications",
        "uk|Мої курси": "My courses",
        "uk|Оплата": "Payments",
        "uk|Мої проєкти": "My projects",
        "uk|Групи": "Groups",
        "uk|Учні": "Students",
        "uk|Перевірка робіт": "Submissions",
        "uk|Оплати": "Payments",
        "uk|Розклад": "Schedule",
        "uk|Профіль": "Profile",
        "uk|Змінити пароль": "Change password",
        "uk|Вийти": "Log out",
        "uk|Увійти": "Log in",
        "uk|Вхід": "Log in",
        "uk|Реєстрація": "Registration",
        "uk|Увійди, щоб продовжити": "Log in to continue",
        "uk|Створи акаунт за хвилину": "Create an account in a minute",
        "uk|Перевір форму": "Check the form",
        "uk|Пароль": "Password",
        "uk|Підтвердження пароля": "Confirm password",
        "uk|Імʼя": "First name",
        "uk|Прізвище": "Last name",
        "uk|Запам’ятати мене": "Remember me",
        "uk|Немає акаунту?": "No account?",
        "uk|Зареєструватися": "Register",
        "uk|Вже є акаунт?": "Already have an account?",
        "uk|Створити акаунт": "Create account",
        "uk|Навчання, яке хочеться відкривати щодня.": "Learning students want to open every day.",
        "uk|Світла, зручна платформа для учнів, викладачів і менеджерів: курси, уроки, практичні завдання, квізи та перевірка робіт.": "A clean, friendly platform for students, teachers and managers: courses, lessons, practice tasks, quizzes and submissions.",
        "uk|Курси та уроки в одному місці": "Courses and lessons in one place",
        "uk|Практика з живим редактором коду": "Practice with a live code editor",
        "uk|Сповіщення та прогрес без хаосу": "Notifications and progress without chaos",
        "uk|Завдання": "Tasks",
        "uk|Завдання цього уроку": "Tasks for this lesson",
        "uk|Назва завдання": "Task title",
        "uk|Тип": "Type",
        "uk|Перевірка": "Review",
        "uk|Статус": "Status",
        "uk|Не виконано": "Not completed",
        "uk|Зараховано": "Completed",
        "uk|Не зараховано": "Not passed",
        "uk|Очікує перевірки": "Waiting for review",
        "uk|Повернуто": "Returned",
        "uk|Відкрити": "Open",
        "uk|Переробити": "Redo",
        "uk|Назад до завдань": "Back to tasks",
        "uk|Назад до уроків": "Back to lessons",
        "uk|Умова завдання": "Task requirements",
        "uk|Порада": "Tip",
        "uk|Здати роботу": "Submit work",
        "uk|Здати роботу 🟡": "Submit for review 🟡",
        "uk|Робота очікує перевірки.": "The work is waiting for review.",
        "uk|Роботу зараховано.": "The work was approved.",
        "uk|Роботу повернуто на доопрацювання.": "The work was returned for improvement.",
        "uk|Коментар вчителя:": "Teacher comment:",
        "uk|Можна переробити і здати ще раз.": "You can redo and submit again.",
        "uk|Правильні відповіді не показуються, можна повторити.": "Correct answers are hidden; you can try again.",
        "uk|Тести": "Quizzes",
        "uk|Правильно": "Correct",
        "uk|Неправильно": "Incorrect",
        "uk|Поки неправильно. Спробуй ще раз — правильну відповідь не показуємо.": "Not correct yet. Try again — the correct answer is hidden.",
        "uk|Відповідь правильна.": "The answer is correct.",
        "uk|Обрана пара правильна.": "The selected match is correct.",
        "uk|Оберіть пару...": "Choose a pair...",
        "uk|Введіть відповідь": "Enter an answer",
        "uk|Перевірка нечутлива до регістру.": "Checking is case-insensitive.",
        "uk|Submit quiz": "Submit quiz",
        "uk|Додати": "Add",
        "uk|Фільтрувати": "Filter",
        "uk|Скинути": "Reset",
        "uk|Усі ролі": "All roles",
        "uk|Немає користувачів": "No users",
        "uk|Додати користувача": "Add user",
        "uk|Редагувати користувача": "Edit user",
        "uk|Дані користувача": "User details",
        "uk|Видалити користувача": "Delete user",
        "uk|Назва": "Title",
        "uk|Опис": "Description",
        "uk|Зберегти": "Save",
        "uk|Скасувати": "Cancel",
        "uk|Назад": "Back",
        "uk|До проєктів": "To projects",
        "uk|Новий проєкт": "New project",
        "uk|Українська": "Ukrainian",
        "uk|Англійська": "English"
    };

    function normalize(text) {
        return (text || "").replace(/\s+/g, " ").trim();
    }

    function shouldSkip(node) {
        const parent = node.parentElement;
        if (!parent) return true;
        return !!parent.closest("script, style, textarea, code, pre, .monaco-editor, .console-output, .rev-console-output");
    }

    function translateTextNodes(lang) {
        const walker = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT);
        const nodes = [];
        let current;
        while ((current = walker.nextNode())) nodes.push(current);

        nodes.forEach(node => {
            if (shouldSkip(node)) return;
            if (!node.__learnityOriginal) node.__learnityOriginal = node.nodeValue;
            const original = node.__learnityOriginal;
            if (lang === "uk") {
                node.nodeValue = original;
                return;
            }
            const trimmed = normalize(original);
            if (!trimmed) return;
            const translated = dictionary[`uk|${trimmed}`];
            if (translated) node.nodeValue = original.replace(trimmed, translated);
        });
    }

    function translateAttributes(lang) {
        document.querySelectorAll("input[placeholder], textarea[placeholder], [title]").forEach(el => {
            for (const attr of ["placeholder", "title"]) {
                if (!el.hasAttribute(attr)) continue;
                const key = `learnityOriginal${attr}`;
                if (!el.dataset[key]) el.dataset[key] = el.getAttribute(attr) || "";
                const original = el.dataset[key];
                if (lang === "uk") {
                    el.setAttribute(attr, original);
                    continue;
                }
                const translated = dictionary[`uk|${normalize(original)}`];
                if (translated) el.setAttribute(attr, translated);
            }
        });
    }

    function updateButtons(lang) {
        document.querySelectorAll("[data-lang]").forEach(btn => {
            btn.classList.toggle("active", btn.dataset.lang === lang);
        });
        document.querySelectorAll("[data-lang-current]").forEach(el => {
            el.textContent = lang === "en" ? "EN" : "UA";
        });
        document.documentElement.lang = lang === "en" ? "en" : "uk";
    }

    function applyLanguage(lang) {
        const safeLang = lang === "en" ? "en" : "uk";
        localStorage.setItem(storageKey, safeLang);
        translateTextNodes(safeLang);
        translateAttributes(safeLang);
        updateButtons(safeLang);
    }

    document.addEventListener("click", (event) => {
        const btn = event.target.closest("[data-lang]");
        if (!btn) return;
        applyLanguage(btn.dataset.lang);
    });

    document.addEventListener("DOMContentLoaded", () => {
        applyLanguage(localStorage.getItem(storageKey) || "uk");
    });
})();
