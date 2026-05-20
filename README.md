# LearnitySchool

**LearnitySchool** — це навчальна вебплатформа для онлайн-школи, створена на **ASP.NET Core MVC (.NET 8)** з використанням **Entity Framework Core**, **SQL Server** та **ASP.NET Core Identity**. Проєкт має три основні ролі: **Manager**, **Teacher** і **Student**. Кожна роль має окремий інтерфейс, власний layout, функціонал і доступи.

Платформа дозволяє менеджерам створювати користувачів, курси, уроки та завдання; викладачам керувати доступом до уроків і перевіряти роботи; учням проходити уроки, виконувати квізи, практичні HTML/CSS/JS-завдання, оплачувати уроки та створювати власні проєкти.

---

## Зміст

- [Основні можливості](#основні-можливості)
- [Ролі користувачів](#ролі-користувачів)
- [Технології](#технології)
- [Архітектура проєкту](#архітектура-проєкту)
- [Структура папок](#структура-папок)
- [Функціонал менеджера](#функціонал-менеджера)
- [Функціонал викладача](#функціонал-викладача)
- [Функціонал учня](#функціонал-учня)
- [Редактор коду](#редактор-коду)
- [Квізи та практичні завдання](#квізи-та-практичні-завдання)
- [Оплати уроків](#оплати-уроків)
- [Сповіщення](#сповіщення)
- [Перемикання мови](#перемикання-мови)
- [Дизайн](#дизайн)
- [Запуск проєкту](#запуск-проєкту)
- [База даних і міграції](#база-даних-і-міграції)
- [Демо-користувачі](#демо-користувачі)
- [Корисні маршрути](#корисні-маршрути)
- [Типові проблеми та рішення](#типові-проблеми-та-рішення)
- [Можливі покращення в майбутньому](#можливі-покращення-в-майбутньому)

---

## Основні можливості

- Авторизація та реєстрація користувачів через **ASP.NET Core Identity**.
- Ролі: **Manager**, **Teacher**, **Student**.
- Окремі інтерфейси для кожної ролі.
- Світлий сучасний дизайн для всієї платформи.
- Менеджерський dashboard зі статистикою.
- CRUD для користувачів, курсів, уроків і завдань.
- Прив’язка викладачів і учнів до курсів.
- Пошук викладачів та учнів під час прив’язки до курсу.
- Копіювання готового курсу для інших викладачів або учнів.
- Відкриття та закриття уроків викладачем.
- Оплата уроків за принципом: **один урок = одна оплата**.
- Блокування платних уроків до підтвердження оплати.
- Квізи різних типів: стандартний тест, відповідність, пропуски.
- Практичні завдання з HTML/CSS/JS.
- Monaco Editor для написання коду.
- Live preview для коду учня.
- Автозбереження чернеток практичних завдань.
- Консоль для `console.log()`, `console.warn()`, `console.error()`.
- Розділ **Мої проєкти** для учнів.
- Перевірка робіт викладачем.
- Перегляд reference-рішення викладачем під час перевірки.
- Внутрішні сповіщення.
- Профіль користувача та зміна пароля.
- Перемикання мови інтерфейсу **UA / EN**.

---

## Ролі користувачів

### Manager

Менеджер відповідає за адміністративну частину платформи:

- керує користувачами;
- створює курси;
- додає уроки;
- створює завдання;
- налаштовує квізи;
- створює практичні завдання;
- прив’язує викладачів і учнів до курсів;
- копіює готові курси;
- переглядає dashboard зі статистикою.

### Teacher

Викладач працює з навчальним процесом:

- бачить свої курси та групи;
- відкриває або закриває доступ до уроків;
- переглядає учнів курсу;
- бачить прогрес учнів;
- перевіряє здані практичні роботи;
- бачить код учня, preview, console output і reference;
- переглядає інформацію про оплати;
- отримує сповіщення.

### Student

Учень проходить навчання:

- бачить доступні курси;
- відкриває уроки;
- виконує квізи;
- виконує практичні HTML/CSS/JS-завдання;
- здає роботи;
- бачить правильність виконання через кольорові статуси;
- може переробляти завдання, якщо результат нижче прохідного;
- оплачує платні уроки;
- створює власні проєкти в розділі **Мої проєкти**;
- редагує профіль і змінює пароль.

---

## Технології

### Backend

- **C#**
- **.NET 8**
- **ASP.NET Core MVC**
- **ASP.NET Core Identity**
- **Entity Framework Core 8**
- **SQL Server / LocalDB**
- **LINQ**
- **Razor Views**

### Frontend

- **HTML5**
- **CSS3**
- **JavaScript**
- **Razor syntax**
- **Monaco Editor**
- **iframe preview** для HTML/CSS/JS-завдань
- **localStorage** для збереження мови інтерфейсу

### Інструменти

- **Visual Studio** або **VS Code**
- **.NET SDK 8**
- **SQL Server / SQL Server Express / LocalDB**
- **Entity Framework Core Tools**

---

## Архітектура проєкту

Проєкт організований у стилі наближеної **Clean Architecture**:

```text
LearnitySchool
├── src
│   ├── LearnitySchool.Domain
│   ├── LearnitySchool.Application
│   ├── LearnitySchool.Infrastructure
│   └── LearnitySchool.Web
└── LearnitySchool.sln
```

### LearnitySchool.Domain

Містить основні сутності та enum-и системи:

- `Course`
- `Lesson`
- `LessonTask`
- `PracticeTask`
- `QuizQuestion`
- `QuizOption`
- `CourseTeacher`
- `CourseStudent`
- `CourseSchedule`
- `CourseLessonGate`
- `LessonPayment`
- `StudentProject`
- `StudentPracticeDraft`
- `StudentTaskSubmission`
- `StudentTaskProgress`
- `StudentQuizAttempt`
- `Notification`

Також тут знаходяться enum-и:

- `LessonTaskType`
- `QuizType`
- `PaymentStatus`
- `StudentSubmissionStatus`
- `NotificationType`
- `TaskAssessmentMode`
- `UserRole`

### LearnitySchool.Application

Містить application-шар:

- спільні константи ролей;
- DTO;
- абстракції сервісів;
- dependency injection для application-рівня.

### LearnitySchool.Infrastructure

Містить інфраструктурну частину:

- `AppDbContext`;
- Identity-користувача `ApplicationUser`;
- Identity-роль `ApplicationRole`;
- EF Core migrations;
- реалізації сервісів;
- підключення SQL Server та Identity.

### LearnitySchool.Web

MVC-рівень проєкту:

- controllers;
- Razor views;
- ViewModels;
- CSS;
- JavaScript;
- layouts для ролей;
- сторінки авторизації, профілю, dashboard-ів, курсів, уроків, завдань.

---

## Структура папок

```text
src/LearnitySchool.Web
├── Controllers
│   ├── AccountController.cs
│   ├── ManagerController.cs
│   ├── ManagerCoursesController.cs
│   ├── ManagerLessonsController.cs
│   ├── ManagerTasksController.cs
│   ├── ManagerQuizController.cs
│   ├── ManagerPracticeController.cs
│   ├── ManagerUsersController.cs
│   ├── TeacherController.cs
│   ├── TeacherGroupsController.cs
│   ├── TeacherSubmissionsController.cs
│   ├── TeacherPaymentsController.cs
│   ├── StudentController.cs
│   ├── StudentProjectsController.cs
│   ├── ProfileController.cs
│   └── NotificationsController.cs
│
├── ViewModels
│   ├── Account
│   ├── Manager
│   ├── Teacher
│   ├── Student
│   ├── Practice
│   ├── Profile
│   └── Notifications
│
├── Views
│   ├── Account
│   ├── Manager
│   ├── ManagerCourses
│   ├── ManagerLessons
│   ├── ManagerTasks
│   ├── ManagerQuiz
│   ├── ManagerPractice
│   ├── ManagerUsers
│   ├── Teacher
│   ├── TeacherGroups
│   ├── TeacherSubmissions
│   ├── TeacherPayments
│   ├── Student
│   ├── StudentProjects
│   ├── Profile
│   ├── Notifications
│   └── Shared
│
└── wwwroot
    ├── css
    └── js
```

---

## Функціонал менеджера

### Dashboard

Сторінка менеджера містить dashboard зі статистикою:

- кількість учнів;
- кількість викладачів;
- кількість курсів;
- кількість уроків;
- кількість завдань;
- кількість робіт на перевірці;
- pending payments;
- швидкі переходи до основних розділів.

Маршрут:

```text
/Manager/Index
```

### Користувачі

Менеджер може:

- переглядати список користувачів;
- створювати користувачів;
- редагувати дані користувачів;
- видаляти користувачів;
- призначати роль: Manager, Teacher або Student.

Маршрут:

```text
/ManagerUsers/Index
```

### Курси

Менеджер може:

- створювати курси;
- редагувати курси;
- видаляти курси;
- публікувати або приховувати курси;
- прив’язувати викладача;
- прив’язувати учнів;
- використовувати пошук по викладачах і учнях;
- копіювати готовий курс.

Маршрут:

```text
/ManagerCourses/Index
```

### Копіювання курсу

Функція копіювання дозволяє швидко створити новий курс на основі вже готового.

Під час копіювання переносяться:

- уроки;
- завдання;
- quiz questions;
- quiz options;
- practice task statement;
- starter code;
- reference code;
- розклад;
- ціни уроків.

Не копіюються:

- прогрес учнів;
- оплати;
- здані роботи;
- чернетки;
- quiz attempts;
- notifications.

Це потрібно для ситуацій, коли один і той самий курс треба використати для іншого викладача або іншої групи учнів.

### Уроки

Менеджер може:

- створювати уроки в межах курсу;
- задавати назву, опис і контент уроку;
- задавати порядок уроків;
- задавати ціну уроку;
- публікувати або приховувати уроки.

Маршрут:

```text
/ManagerLessons/Index?courseId=...
```

### Завдання

Менеджер може створювати два основні типи завдань:

- `Quiz`
- `Practice`

Маршрут:

```text
/ManagerTasks/Index?lessonId=...
```

### Quiz editor

Менеджер може:

- створювати питання;
- редагувати питання;
- видаляти питання;
- створювати варіанти відповідей;
- задавати правильні відповіді;
- налаштовувати порядок питань і відповідей.

У проєкті виправлена валідація `Order`: якщо номер уже зайнятий, сайт показує нормальне повідомлення, а не падає з помилкою.

### Practice editor

Менеджер може створювати практичні HTML/CSS/JS-завдання.

Поля starter code та reference code можуть бути порожніми. Це дозволяє створити завдання, де учень пише код повністю самостійно.

---

## Функціонал викладача

### Dashboard

Викладач бачить:

- свої курси;
- кількість учнів;
- роботи на перевірці;
- останні здачі;
- статуси завдань;
- інформацію про оплати.

Маршрут:

```text
/Teacher/Dashboard
```

### Групи та курси

Викладач може переглядати свої курси та учнів, які до них прив’язані.

Маршрут:

```text
/TeacherGroups/Index
```

### Відкриття та закриття уроків

Викладач може керувати доступом до уроків:

- відкрити урок для учнів;
- закрити урок;
- бачити, які уроки доступні;
- контролювати навчальний процес поетапно.

Якщо урок закритий, учень бачить його, але не може перейти до завдань.

### Прогрес учнів

Успішність учнів показується через кольори:

- зелений — завдання пройдено;
- червоний — завдання виконано неправильно або результат нижче 95%;
- сірий — завдання ще не виконувалось.

Для квізів статус також відображається у викладача. Якщо учень виконав квіз неправильно, викладач бачить червоний статус, а не сірий.

### Перевірка практичних робіт

Викладач може переглядати:

- HTML/CSS/JS код учня;
- preview роботи учня;
- console output учня;
- reference HTML/CSS/JS;
- reference preview;
- відсоток автоматичної схожості;
- статус здачі.

Reference preview не підключений до console output, тому консоль показує тільки результат коду учня.

---

## Функціонал учня

### Курси

Учень бачить курси, до яких він прив’язаний.

Маршрут:

```text
/Student/Index
```

### Уроки

Учень бачить уроки курсу. Урок може бути:

- доступний;
- закритий викладачем;
- заблокований через неоплату.

Якщо урок платний і ще не оплачений, учень не може перейти до завдань.

### Завдання

Учень може виконувати:

- quiz-завдання;
- practice-завдання.

Статуси відображаються кольорами:

- зелений — правильно або пройдено;
- червоний — неправильно або нижче прохідного результату;
- сірий — ще не виконано.

### Переробка завдань

Якщо результат нижче 95%, завдання не вважається пройденим, але учень може здати його і потім переробити.

Це зроблено для того, щоб дитина бачила помилку, але мала можливість повторити спробу без блокування.

---

## Редактор коду

У практичних завданнях і розділі **Мої проєкти** використовується редактор коду для HTML/CSS/JS.

Можливості редактора:

- вкладки HTML, CSS, JS;
- підсвічування синтаксису;
- підказки коду;
- автозакриття HTML-тегів;
- live preview через iframe;
- кнопка Run;
- кнопка Check для практичних завдань;
- кнопка Submit для здачі роботи;
- console output;
- кнопка Clear для очищення консолі;
- автозбереження чернетки.

### Консоль

Консоль підтримує:

```javascript
console.log('Hello');
console.warn('Warning');
console.error('Error');
```

Вивід показується в окремому блоці під preview.

У викладача консоль показує тільки результат коду учня. Reference preview не записує дані у консоль.

---

## Квізи та практичні завдання

### Типи квізів

Проєкт підтримує кілька типів квізів:

| Тип | Опис |
|---|---|
| `Standard` | Звичайне питання з одним або кількома правильними варіантами |
| `Matching` | Завдання на встановлення відповідності |
| `FillBlank` | Завдання, де потрібно вписати відповідь у пропуск |

### Перевірка квізів

- Прохідний результат: **95% і вище**.
- Якщо результат нижче 95%, квіз світиться червоним.
- Правильні відповіді не показуються, щоб учень міг спробувати ще раз.
- Учень може повторно пройти квіз.
- Викладач бачить статус квізу в успішності учня.

### Практичні завдання

Практичне завдання може містити:

- умову;
- starter HTML/CSS/JS;
- reference HTML/CSS/JS;
- поріг схожості;
- автоматичну перевірку;
- ручну перевірку викладачем.

Starter і reference можуть бути порожніми.

### Прохідний відсоток

Для практичних завдань використовується логіка:

- **95–100%** — завдання проходить;
- **менше 95%** — завдання можна здати, але воно буде червоним і доступним для переробки.

---

## Оплати уроків

У системі реалізована demo-логіка оплати уроків.

Основна ідея:

```text
1 урок = 1 оплата
```

Менеджер може задати ціну уроку. Якщо ціна `0`, урок безкоштовний.

Учень бачить сторінку оплат і може підтвердити demo-оплату. Після цього урок стає доступним.

Статуси оплати зберігаються в таблиці `LessonPayments`.

Маршрути:

```text
/Student/Payments
/TeacherPayments/Index
```

---

## Сповіщення

У проєкті є внутрішні сповіщення.

Сповіщення можуть використовуватись для:

- нових зданих робіт;
- змін статусу перевірки;
- інформації про оплату;
- системних повідомлень.

Маршрут:

```text
/Notifications/Index
```

---

## Перемикання мови

У проєкті реалізовано просте перемикання мови **UA / EN**.

Особливості:

- перемикач знаходиться в інтерфейсі;
- вибір мови зберігається в `localStorage`;
- переклад виконується через JavaScript;
- не використовується складна `.resx`-локалізація.

Файл:

```text
src/LearnitySchool.Web/wwwroot/js/language.js
```

---

## Дизайн

Проєкт переведений зі старого темно-синього стилю на світлу сучасну палітру.

Основна ідея дизайну:

- чистий світлий фон;
- м’які картки;
- акуратні тіні;
- синьо-блакитні акценти;
- зелені success-стани;
- червоні error-стани;
- зрозумілі dashboard-и;
- однаковий стиль для Manager, Teacher і Student.

CSS-файли розділені по ролях і сторінках:

```text
wwwroot/css/login.css
wwwroot/css/profile.css
wwwroot/css/studentLayout.css
wwwroot/css/student/...
wwwroot/css/teacher/...
wwwroot/css/userManager/...
```

---

## Запуск проєкту

### 1. Вимоги

Перед запуском потрібно мати встановлені:

- .NET SDK 8;
- SQL Server або SQL Server Express / LocalDB;
- Visual Studio або VS Code.

Перевірити .NET можна командою:

```bash
dotnet --version
```

### 2. Відкрити проєкт

Відкрий кореневу папку проєкту, де знаходиться файл:

```text
LearnitySchool.sln
```

### 3. Відновити залежності

У корені проєкту виконай:

```bash
dotnet restore
```

### 4. Перевірити збірку

```bash
dotnet build
```

### 5. Запустити Web-проєкт

Варіант 1 — з кореня:

```bash
dotnet run --project src/LearnitySchool.Web
```

Варіант 2 — перейти в Web-проєкт:

```bash
cd src/LearnitySchool.Web
dotnet run
```

Після запуску в консолі буде адреса сайту, наприклад:

```text
https://localhost:5001
http://localhost:5000
```

---

## База даних і міграції

### Connection string

Connection string знаходиться у файлах:

```text
src/LearnitySchool.Web/appsettings.json
src/LearnitySchool.Web/appsettings.Development.json
```

Приклад:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=DESKTOP-R6S1APS;Database=LearnitySchoolDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

Якщо на твоєму комп’ютері інша назва SQL Server, треба змінити `Server=...`.

Для LocalDB можна використати:

```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LearnitySchoolDb;Trusted_Connection=True;MultipleActiveResultSets=true"
```

### Автоматичне застосування міграцій

У `Program.cs` під час старту виконується:

```csharp
await db.Database.MigrateAsync();
```

Тому база даних оновлюється автоматично при запуску сайту.

### Додати нову міграцію вручну

Якщо потрібно створити нову міграцію:

```bash
dotnet ef migrations add НазваМіграції \
  --project src/LearnitySchool.Infrastructure \
  --startup-project src/LearnitySchool.Web \
  --context AppDbContext \
  --output-dir Persistence/Migrations
```

Оновити базу вручну:

```bash
dotnet ef database update \
  --project src/LearnitySchool.Infrastructure \
  --startup-project src/LearnitySchool.Web \
  --context AppDbContext
```

Якщо команда `dotnet ef` не працює, встанови інструмент:

```bash
dotnet tool install --global dotnet-ef
```

або онови його:

```bash
dotnet tool update --global dotnet-ef
```

---

## Демо-користувачі

При першому запуску автоматично створюються ролі та demo-користувачі.

| Роль | Email | Пароль |
|---|---|---|
| Manager | `manager@learnity.local` | `Manager123!` |
| Teacher | `teacher@learnity.local` | `Teacher123!` |
| Student | `student@learnity.local` | `Student123!` |

Логіка створення знаходиться у `Program.cs` у методі `SeedIdentityAsync`.

---

## Корисні маршрути

### Account

```text
/Account/Login
/Account/Register
/Account/AccessDenied
```

### Profile

```text
/Profile/Index
/Profile/Edit
/Profile/ChangePassword
```

### Manager

```text
/Manager/Index
/ManagerUsers/Index
/ManagerCourses/Index
/ManagerLessons/Index?courseId=...
/ManagerTasks/Index?lessonId=...
/ManagerQuiz/Quiz?taskId=...
/ManagerPractice/Edit?taskId=...
```

### Teacher

```text
/Teacher/Dashboard
/TeacherGroups/Index
/TeacherGroups/Details/{id}
/TeacherGroups/Success/{id}
/TeacherSubmissions/Index
/TeacherPayments/Index
```

### Student

```text
/Student/Index
/Student/Lessons?courseId=...
/Student/Tasks?lessonId=...
/Student/Quiz?taskId=...
/Student/Practice?taskId=...
/Student/Payments
/StudentProjects/Index
```

### Notifications

```text
/Notifications/Index
```

---

## Типові проблеми та рішення

### 1. `dotnet build` показує warnings CS8601 або CS8602

Це nullable warnings. Вони не блокують запуск проєкту.

Приклад:

```text
warning CS8601: Possible null reference assignment
warning CS8602: Dereference of a possibly null reference
```

Якщо build завершився без `error`, сайт можна запускати.

### 2. Не підключається база даних

Перевір `DefaultConnection` у:

```text
appsettings.json
appsettings.Development.json
```

Найчастіше треба змінити назву сервера:

```json
"Server=DESKTOP-R6S1APS;..."
```

на свою, наприклад:

```json
"Server=(localdb)\\mssqllocaldb;..."
```

або:

```json
"Server=localhost\\SQLEXPRESS;..."
```

### 3. `Add-Migration is not recognized`

Це означає, що команда запускається не в Package Manager Console або не встановлений EF tool.

Для VS Code краще використовувати:

```bash
dotnet ef migrations add НазваМіграції \
  --project src/LearnitySchool.Infrastructure \
  --startup-project src/LearnitySchool.Web \
  --context AppDbContext \
  --output-dir Persistence/Migrations
```

### 4. Сайт відкривається, але стилі старі

Можливий кеш браузера.

Рішення:

- натиснути `Ctrl + F5`;
- очистити кеш;
- перевірити, що в layout підключені CSS з `asp-append-version="true"`.

### 5. Помилка доступу після логіну

Перевір, чи користувач має роль:

- Manager;
- Teacher;
- Student.

Ролі створюються автоматично при старті проєкту.

---

## Можливі покращення в майбутньому

У майбутньому проєкт можна розширити такими функціями:

- реальна платіжна система замість demo-оплати;
- повна локалізація через `.resx`;
- email-сповіщення;
- календар занять;
- домашні завдання з дедлайнами;
- система оцінок;
- коментарі викладача до кожної роботи;
- чат між учнем і викладачем;
- завантаження файлів до завдань;
- аналітика успішності учня;
- експорт звітів у PDF;
- адмін-панель для налаштувань школи.

---

## Короткий опис для захисту

**LearnitySchool** — це вебплатформа для онлайн-школи, яка дозволяє організувати навчальний процес між менеджером, викладачем та учнем. Менеджер створює курси, уроки, завдання та користувачів. Викладач керує доступом до уроків і перевіряє роботи. Учень проходить уроки, виконує квізи та практичні завдання з HTML/CSS/JS, бачить свій прогрес і може створювати власні проєкти.

Проєкт реалізований на **ASP.NET Core MVC**, використовує **Entity Framework Core**, **SQL Server** і **ASP.NET Core Identity**. У системі є ролі, авторизація, dashboard-и, оплати уроків, сповіщення, Monaco Editor, live preview, консоль JavaScript і світлий адаптований UI.

---

## Автор

Проєкт: **LearnitySchool**  
Тип: навчальна онлайн-платформа  
Технології: **ASP.NET Core MVC / C# / .NET 8 / EF Core / SQL Server**
