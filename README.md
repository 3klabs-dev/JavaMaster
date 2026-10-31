# Java Master

Java Master is a gamified website for learning Java. Learners follow a path of
lessons, answer multiple-choice, fill-in-the-blank, arrange-the-code,
fill-the-slots, and find-the-bug questions, earn XP and coins, keep a daily
streak, and climb through eight ranks from **Starter** to **Legend**.

Every answer is checked on the server. The browser is only ever given the
question — never the answer key.

This README explains what the app is, how to run it, and how the main pieces fit
together. 

> If you only want to run the app, follow **Steps 1–4** and stop there. The rest
> explains how it works.

---

## A quick glossary

| Word | What it means here |
|---|---|
| **Backend / server** | The part that stores player data, checks answers, and pays rewards. |
| **Frontend / pages** | The part you see in the browser. It is plain HTML, CSS, and JavaScript. |
| **API** | The set of URLs the pages use to talk to the server. |
| **Endpoint** | One address in the API, such as `GET /api/me`. |
| **Database** | Where player progress and lesson content live. This app uses SQL Server. |
| **Migration** | A saved change to the database structure or data. The app applies new ones when it starts. |

The project has two halves:

- **Backend** — `Controllers/`, `Services/`, `Data/`, `Entities/`, `Migrations/`
- **Frontend** — `wwwroot/` (the pages, styles, scripts, images, and sounds)

They talk to each other through the API.

---

# Running the app

You need two things:

1. **.NET 10 SDK** — check with `dotnet --version`. It should say `10.0.100` or newer.
2. **SQL Server** — Express is fine, and so is another edition or LocalDB.

There is no `npm install` and no frontend build. The pages in `wwwroot` are plain
files.

---

## Step 1 — Install SQL Server

Skip this if you already have SQL Server.

The easiest option is **SQL Server Express**, which installs an instance named
`SQLEXPRESS`:

```powershell
winget install --id Microsoft.SQLServer.2025.Express --silent --accept-package-agreements --accept-source-agreements
```

It takes a few minutes and installs a Windows service, so Windows will ask for
administrator approval.

If `winget` is not available, install SQL Server Express from Microsoft's
website instead.

---

## Step 2 — Get the code

```bash
git clone https://github.com/3klabs-dev/JavaMaster.git
cd JavaMaster
```

---

## Step 3 — Tell the app where your database is

A **connection string** is just the address of your database.

The app comes with a default connection string in `appsettings.json`:

```text
Server=localhost;Database=JavaMaster;Trusted_Connection=True;TrustServerCertificate=True;
```

That works for a normal SQL Server instance on the same computer. If your SQL
Server has a different name, override it in your terminal.

### SQL Server Express (most common)

```powershell
$env:ConnectionStrings__Default = "Server=localhost\SQLEXPRESS;Database=JavaMaster;Trusted_Connection=True;TrustServerCertificate=True;"
```

### Normal SQL Server instance

```powershell
$env:ConnectionStrings__Default = "Server=localhost;Database=JavaMaster;Trusted_Connection=True;TrustServerCertificate=True;"
```

### LocalDB

```powershell
$env:ConnectionStrings__Default = "Server=(localdb)\MSSQLLocalDB;Database=JavaMaster;Trusted_Connection=True;TrustServerCertificate=True;"
```

The `$env:` form only lasts for the terminal window you are using. That is fine
if you start the app in the same window.

If you want it to last, set it once for your Windows user:

```powershell
[Environment]::SetEnvironmentVariable("ConnectionStrings__Default",
  "Server=localhost\SQLEXPRESS;Database=JavaMaster;Trusted_Connection=True;TrustServerCertificate=True;",
  "User")
```

That writes the value to your user settings. It does **not** change terminals
that are already open — open a new window before running the app.

> Do not put a real password in a file that gets committed to git. For this
> project, Windows authentication is enough.

---

## Step 4 — Start the app

From the `JavaMaster` folder:

```bash
dotnet run --project JavaMaster.Api.csproj --launch-profile http
```

The first run takes a little longer while it builds. When you see:

```text
Now listening on: http://localhost:5176
```

open this in your browser:

**<http://localhost:5176>**

That is the whole app. The first run also:

- creates the database,
- applies database migrations,
- adds the starting lesson content.

There is no SQL script to run by hand.

To stop the app, press `Ctrl+C` in the terminal.

---

## Checking your work

### Build the app

```bash
dotnet build
```

A good build ends with `0 Warning(s)` and `0 Error(s)`.

### Run the tests

```bash
dotnet test tests/JavaMaster.Api.Tests/JavaMaster.Api.Tests.csproj
```

The tests do not need SQL Server. They check the answer rules, rank boundaries,
streak rules, and the shape of validation errors.

A good run ends with:

```text
Passed! - Failed: 0, Passed: 36, Skipped: 0, Total: 36
```

### Automatic checks on GitHub

GitHub Actions runs the build and the tests on every push to `master` or `main`,
and on every pull request. The workflow is in `.github/workflows/ci.yml`.

Open the **Actions** tab on GitHub to see the result.

### Database changes

If you add a migration, give it a short, clear name:

```bash
dotnet ef migrations add AddSomething
```

If `dotnet ef` is not found, install it once:

```bash
dotnet tool install --global dotnet-ef
```

The app applies new migrations the next time it starts. To apply them right away:

```bash
dotnet ef database update
```

---

# Using the app

## Signing in (temporary)

There is no real login yet. Every request is made as **Test Student**, so you can
click straight into the app and everything works.

The **Log in** and **Sign up** pages are placeholders. The Log in page accepts
three hardcoded accounts (`admin` / `password123`, `user1` / `mypassword`,
`testuser` / `test123`), but they only write to the browser's `localStorage`.
They do not talk to the server and do not change which player the API treats you
as.

When real authentication is added, it replaces `Services/CurrentUserService.cs`.
The rest of the API should not need to change.

> Because there is no real login, do not use real student information in the app
> yet.

---

## Where the data comes from

The server is the only place player progress lives. The pages do not keep a
copy in the browser.

| Page | What it reads | From |
|---|---|---|
| Learn | which lessons are open | `GET /api/learn/path` |
| A lesson | the questions, and the grading | `GET /api/learn/nodes/{id}`, `POST /api/learn/questions/{id}/check` |
| Progress | XP, coins, streak, rank | `GET /api/me` |
| Profile | name, school, grade, avatar, XP, streak, rank, badges, joined date, best streak | `GET /api/me` |
| Home, Shop | coins, inventory | `GET /api/me`, `GET /api/shop` |
| Onboarding | writes the name, school, and grade | `PATCH /api/me/profile` |

Finishing a lesson posts to `POST /api/learn/nodes/{id}/complete`. The server
pays the XP and coins, unlocks the next lesson, and records the streak. None of
that is written to `localStorage`.

The browser still keeps a few things of its own because the server has no
endpoint for them yet:

- the "repair notified today" flag,
- the sound settings (background music and sound effects).

---

## The profile belongs to the account

The name, school, grade, and chosen avatar live on the user row. The profile page
reads them from `GET /api/me` and saves them with `PATCH /api/me/profile`.

A field that is left out of the request stays as it is. An empty string clears
the school, grade, or avatar. The name cannot be empty. That means the avatar can
be changed on its own:

```json
{ "avatar": "/avatars/Avatar 5.png" }
```

The server only accepts an avatar the app ships with, so a bad path can never end
up in the navbar of every page.

The onboarding page saves the same three fields the same way.

---

# Lesson content

## Who owns the questions

The lesson questions live in the database.

`Data/LessonContent.cs` is only the **starting point**. It is what the app puts
in the database the first time it runs.

Each seeded question has a stable `Code` such as `hello-world.q1` or
`variables.q3`. That code lets the app tell two situations apart:

- **The question is not in the database yet** → insert it, using the wording and
  answer key from the code.
- **The question is already in the database** → leave it alone. The text, answer
  key, hint, and explanation all stay exactly as they are.

So the app writes the content in once. After that, the **database is the source
of truth**, and the code never overwrites it again.

That means a professor can rewrite a question — the wording, the options, the
answer key, the praise message — and it stays that way across restarts. There is
no need to keep editing `LessonContent.cs` just to change what learners see.

---

## Changing content that is already in the database

Because the seeder only inserts, editing `Data/LessonContent.cs` only changes
what a **fresh install** gets. A database that already has the question keeps the
wording and answer key it already has.

To carry a content change into databases that already exist, add a migration
that updates the rows by their code:

```csharp
migrationBuilder.Sql(
    "UPDATE Questions SET Explanation = {0} WHERE Code = {1}",
    "The new explanation.", "variables.q2");
```

The `FixCaseSensitiveAnswers` migration is a worked example. It changed how a
typed answer is compared and rewrote the corrections that go with it.

Only patch questions that have a known code. A question a professor added by hand
has no code and must not be touched.

The lesson text (`IntroText` and `OutroText` on a learn node) works the same way.
The code fills it in while it is empty. After that, the database owns it.

The daily challenge pool works the same way too. Its five questions have the codes
`daily.q1` to `daily.q5`, and they are only ever inserted once.

---

## Deleting a question

- **Deleting a question brings it back.** If a row with a known code is deleted,
  the next app start inserts it again from the code.
- **To retire a question instead**, set `IsActive = 0` on it. The API only serves
  active questions, so the question disappears from the lesson and the rest of
  the path carries on.
- **Questions added by hand** in the database have no code, so the seeder never
  touches them.

---

# How a lesson works

1. The lesson page asks the server for its node and questions
   (`GET /api/learn/nodes/{id}`). The response has the prompts and the options —
   never the answers.
2. Each answer is posted to `POST /api/learn/questions/{id}/check`. The server
   grades it and replies with the praise or the correction.
3. A wrong answer does not move on. James Gosling corrects the learner, and the
   same question stays on screen until it is answered right. Pressing **Try
   Again** puts the model back on its feet and clears the correction.
4. When all questions are answered correctly, the page asks the server to
   complete the node (`POST /api/learn/nodes/{id}/complete`). The server pays the
   XP and coins, unlocks the next lesson, and records the streak.
5. The **finish screen** says the lesson is done and what comes next. It shows
   the lesson's own closing text, which the database owns.
6. The **completed screen** is the plain one: the icon, the heading, and a
   **Continue** button.

The finish screen only happens the first time. The server says so in the
completion response (`firstTime`). A replay skips it and goes straight to the
completed screen.

A lesson can only be rewarded once. Replaying it still means answering every
question again, but it pays no XP and no coins, so nobody can farm the same
lesson for unlimited money.

---

## A lesson with no questions is locked

A node that has no questions yet — a placeholder — cannot be played.

- `GET /api/learn/path` reports it as **Locked**, the same as a node the learner
  has not reached yet. The Learn page draws the padlock and refuses to open it.
  It still does not block the nodes behind it.
- `GET /api/learn/nodes/{id}` answers **404 "This lesson has no questions yet."**
  instead of handing the browser an empty lesson.

Nothing about this is hardcoded. Both answers are worked out from the questions
in the database, so the moment a node is given questions it opens up on its own.

---

## The correction never gives the answer away

The feedback on a wrong answer points at the rule rather than naming the option.
The learner still has to work it out.

Example:

> Three of these follow every naming rule. The odd one out breaks the rule about
> where a name is allowed to begin.

---

## A typed answer cannot be pasted in

A question that asks the learner to type something usually has the answer sitting
in the question itself — for example, *"Code `Hello World!` between the quotation
marks."*

So the typed field closes the clipboard off. Paste, cut, drop, and the context
menu are blocked on it, and the question text cannot be selected.

This lives in the shared lesson engine (`wwwroot/js/lesson.js`), so every lesson
gets it — including lessons added later. Any new question that renders a text
field must go through the same engine, or it will not be protected.

---

## How an answer is compared

Java is case-sensitive, so a typed answer is compared **exactly**:

- `DOUBLE` is not `double`.
- `hello world!` is not `Hello World!`.

A learner who writes a Java keyword in the wrong case gets the answer wrong, and
the correction says so.

The rule lives in `Services/AnswerChecker.cs` and applies to every answer type
that carries text: a typed answer, a filled slot, and an arranged token.

| In the answer key | What happens |
|---|---|
| nothing, or `"caseSensitive": true` | compared exactly (the default) |
| `"caseSensitive": false` | compared without regard to case |

A missing flag means the strict default, so a question added without one still
gets Java's rule. A flag that is not a real boolean is ignored rather than read,
so a broken flag can never make a question impossible to answer.

Only one question opts out: the `long` suffix question, because Java accepts both
`L` and `l` there.

Whitespace around a typed answer is ignored, so `"  double  "` is the same as
`"double"`.

---

# The API at a glance

`GET` reads something. `POST` does something. `PATCH` updates part of something.

| Method | Route | What it does |
|---|---|---|
| GET | `/api/me` | the player: coins, XP, rank, streak, inventory, profile |
| PATCH | `/api/me/profile` | save the name, school, grade, or avatar |
| GET | `/api/learn/path` | every node with its state: Completed, Unlocked, or Locked |
| GET | `/api/learn/nodes/{id}` | one node and its questions |
| POST | `/api/learn/questions/{id}/check` | grade an answer |
| POST | `/api/learn/nodes/{id}/complete` | finish a node and collect the rewards |
| GET | `/api/daily` | today's daily challenge |
| POST | `/api/daily/answer` | answer the daily challenge |
| GET | `/api/shop` | shop items and how many are owned |
| POST | `/api/shop/buy` | buy an item |
| POST | `/api/streak/repair` | use a streak repair |
| GET | `/api/topics` | topics |

Creating a topic is not exposed yet. Without login, a public `POST` would let any
visitor add topics. It can come back when there is an admin account to protect it.

All timestamps in responses are UTC and end in `Z`, so the browser shows the
correct local date and time.

There is also a `JavaMaster.Api.http` file with ready-to-send example requests
for the main endpoints.

---

# Project layout

```text
Controllers/          the API endpoints
  DailyController.cs      daily challenge
  LearnController.cs      the learn path, questions, and rewards
  MeController.cs         the signed-in player
  ShopController.cs       buying items
  StreakController.cs     repairing a broken streak
  TopicsController.cs     topics

Converters/           small helpers for JSON and dates
Data/                 the database model, seeding, and starting lesson content
Entities/             one file per database table
Migrations/           saved database changes
Services/             shared rules
  AnswerChecker.cs        grades an answer against the stored key
  ApiValidation.cs        makes bad requests use the same error shape
  CurrentUserService.cs   who the caller is (temporary Test Student)
  GameRules.cs            shared constants
  RankCalculator.cs       XP to rank
  StartingKit.cs          the items a new player starts with
  StreakService.cs        daily streak bookkeeping
  UserRowLock.cs          stops two requests changing one player at once

wwwroot/              the whole frontend, no build step
  css/                    styles
  js/lesson.js            the lesson engine
  js/api.js               small fetch wrapper for the API
  lesson/                 one page per lesson
  pages/                  the rest of the screens

tests/                automated backend tests
.github/workflows/    GitHub Actions CI
Program.cs            app startup, database setup, error handling
```

---

# If something goes wrong

The app needs a working database before it will start. If it cannot reach one, it
tells you why instead of crashing:

```text
  Java Master could not reach SQL Server, so it cannot start.

  Connection string: Server=localhost;...
  Error: A network-related or instance-specific error occurred ...

  Things to check:
    1. SQL Server is installed and running (open services.msc).
    2. The connection string names the right instance:
         Server=localhost              default instance
         Server=localhost\SQLEXPRESS   SQL Server Express
         Server=(localdb)\MSSQLLocalDB LocalDB
    3. Your Windows account can sign in to it (Windows authentication).
```

| What you see | What it means |
|---|---|
| *Error Locating Server/Instance* | Wrong instance name. Express is `SQLEXPRESS`, not `localhost` on its own. |
| *Login failed for user* | Your Windows account has no permission on that instance. |
| *The instance has been stopped* | The SQL service is not running — check `services.msc`. |
| *The file is locked by JavaMaster.Api* | The app is already running. Stop it first, then run it again. |

**"The file is locked by JavaMaster.Api"** means a copy of the app is already
running from this folder, and it is holding its own `.exe` open, so the build
cannot overwrite it. Stop the running copy:

```powershell
Stop-Process -Name JavaMaster.Api -Force
```

If the port is already in use, something else is already on `5176`. Stop it, or
run on a different port:

```bash
dotnet run --project JavaMaster.Api.csproj --launch-profile http -- --urls http://localhost:5177
```

---

# Known gaps

- **No real login.** Every request is Test Student. The Log in and Sign up pages
  are placeholders.
- **Three lessons have no questions yet** — `Java Basics 1`, `Java Basics 2`, and
  `Test`. They are placeholders: they are reported as locked and do not block the
  lessons behind them.
- **Some screens are not built yet** and their links lead nowhere:
  `Study.html`, `Modules.html`, `My level.html`, `overview.html`,
  `Practice 1.html`, `Practice 2.html`, `Test 1.html`, `View course.html`,
  `Multiplayer.html`, `Overview.html`.
- **Some settings are still browser-only** — sound preferences and the
  "repair notified today" flag.
