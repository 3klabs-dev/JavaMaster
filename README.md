# Java Master

A gamified web app for learning Java. Learners work through a path of lessons,
answer questions in three formats, earn XP and coins, keep a daily streak, and
climb through eight ranks from **Starter** to **Legend**.

Every answer is checked on the server. The browser is only ever given the
question — never the answer key.

---

# Running it

You need two things:

- **.NET 10 SDK** — check with `dotnet --version`. It should say `10.0.100` or newer.
- **SQL Server** — Express is fine, and so is any other edition or LocalDB.

There is no `npm install` and no frontend build. The pages in `wwwroot` are
plain HTML, CSS and JavaScript.

---

## Step 1 — Install SQL Server

Skip this if you already have it.

The easiest option is **SQL Server Express**, which installs a named instance
called `SQLEXPRESS`:

```powershell
winget install --id Microsoft.SQLServer.2025.Express --silent --accept-package-agreements --accept-source-agreements
```

It takes a few minutes and it installs a Windows service, so it will ask for
administrator approval.

---

## Step 2 — Get the code

```bash
git clone https://github.com/3klabs-dev/JavaMaster.git
cd JavaMaster
```

---

## Step 3 — Tell the app where your database is

The app needs to know which SQL Server to use. This is different on every
machine, so it is **not** stored in `appsettings.json` — that file is tracked by
git and you should not change it.

Copy one of these into your terminal. Use the one that matches your setup:

```powershell
# SQL Server Express (named instance) — the most common
$env:ConnectionStrings__Default = "Server=localhost\SQLEXPRESS;Database=JavaMaster;Trusted_Connection=True;TrustServerCertificate=True;"

# SQL Server with a default instance
$env:ConnectionStrings__Default = "Server=localhost;Database=JavaMaster;Trusted_Connection=True;TrustServerCertificate=True;"

# LocalDB
$env:ConnectionStrings__Default = "Server=(localdb)\MSSQLLocalDB;Database=JavaMaster;Trusted_Connection=True;TrustServerCertificate=True;"
```

> **Important:** the `$env:` form only lasts for this terminal window. That is
> fine if you run the app in the same window — which is what Step 4 does.
>
> If you would rather set it once and keep it, use this instead:
>
> ```powershell
> [Environment]::SetEnvironmentVariable("ConnectionStrings__Default",
>   "Server=localhost\SQLEXPRESS;Database=JavaMaster;Trusted_Connection=True;TrustServerCertificate=True;",
>   "User")
> ```
>
> That writes it to your user settings, so it survives closing the terminal.
> But it does **not** change any terminal you already have open — open a new
> window before running the app.

---

## Step 4 — Run it

In the same terminal, from the `JavaMaster` folder:

```bash
dotnet run --project JavaMaster.Api.csproj --launch-profile http
```

The first run takes a little longer while it builds. When it says
`Now listening on: http://localhost:5176`, open:

**<http://localhost:5176>**

That is the whole app. The database is created, migrated and seeded for you on
the first run — there is no SQL script to run and no data to import.

To stop it, press `Ctrl+C` in that terminal.

---

## If it does not start

The app needs a working database before it will start. If it cannot reach one,
it tells you why instead of crashing:

```
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
| *The file is locked by JavaMaster.Api* | The app is already running. Stop it first, then run it again — see below. |

**"The file is locked by JavaMaster.Api"** means a copy of the app is already
running from this folder, and it is holding its own `.exe` open, so the build
cannot overwrite it. Stop the running one and try again:

```powershell
Stop-Process -Name JavaMaster.Api -Force
```

If the port is already in use, something else is already on 5176.
Stop it, or run on a different port:

```bash
dotnet run --project JavaMaster.Api.csproj --launch-profile http -- --urls http://localhost:5177
```

---

## Signing in

There is no real login yet. Every request is made as **Test Student**, so you
can click straight into the app and everything works.

The Log in and Sign up pages are placeholders. The Log in page accepts three
hardcoded accounts (`admin` / `password123`, `user1` / `mypassword`,
`testuser` / `test123`), but they only write to the browser's `localStorage`.
They do not talk to the server and do not change which player the API treats
you as.

When real authentication lands it replaces `Services/CurrentUserService.cs`, and
nothing else needs to change.

---

## How a lesson works

1. The lesson page asks the server for its node and questions
   (`GET /api/learn/nodes/{id}`). The response has the prompts and the options
   — never the answers.
2. Each answer is posted to `POST /api/learn/questions/{id}/check`. The server
   grades it and replies with the praise or the correction.
3. A wrong answer does not move on. James Gosling corrects the learner and the
   same question stays on screen until it is answered right.
4. When all five questions are answered correctly the page asks the server to
   complete the node (`POST /api/learn/nodes/{id}/complete`). The server pays
   the XP and coins, unlocks the next lesson, and records the streak.

A lesson can only be rewarded once. Replaying it still means answering every
question again, but it pays no XP and no coins, so nobody can farm a lesson.

### The correction never gives the answer away

The feedback on a wrong answer points at the rule rather than naming the
option, so the learner still has to work it out:

> *Three of these follow every naming rule. The odd one out breaks the rule
> about where a name is allowed to begin.*

---

## The API

| Method | Route | |
|---|---|---|
| GET | `/api/me` | the player: coins, XP, rank, streak, inventory |
| GET | `/api/learn/path` | every node with its state: Completed, Unlocked or Locked |
| GET | `/api/learn/nodes/{id}` | one node and its questions |
| POST | `/api/learn/questions/{id}/check` | grade an answer |
| POST | `/api/learn/nodes/{id}/complete` | finish a node and collect the rewards |
| GET | `/api/daily` | today's daily challenge |
| POST | `/api/daily/answer` | answer the daily challenge |
| GET | `/api/shop` | shop items and how many are owned |
| POST | `/api/shop/buy` | buy an item |
| POST | `/api/streak/repair` | use a streak repair |
| GET/POST | `/api/topics` | topics |

---

## Project layout

```
Controllers/          the API
  DailyController.cs    daily challenge
  LearnController.cs    the learn path, questions and rewards
  MeController.cs       the signed-in player
  ShopController.cs     buying items
  StreakController.cs   repairing a broken streak
  TopicsController.cs
  WeatherForecastController.cs   leftover from the template, safe to delete

Data/
  AppDbContext.cs       the EF Core model
  DbSeeder.cs           ranks, shop items, daily questions
  LessonContent.cs      the lesson questions, the single source of truth
  Entities/             one file per table
  Migrations/           EF Core migrations

Services/
  AnswerChecker.cs      grades an answer against the stored key
  CurrentUserService.cs who the caller is (stubbed for now)
  GameRules.cs          shared constants
  RankCalculator.cs     XP to rank
  StartingKit.cs        the items a new player starts with
  StreakService.cs      daily streak bookkeeping

wwwroot/               the whole frontend, no build step
  css/lesson.css        the lesson screens
  js/lesson.js          the lesson engine
  js/api.js             small fetch wrapper for the API
  lesson/               one page per lesson
  pages/                the rest of the screens
```

---

## Known gaps

- **No real login.** Every request is Test Student.
- **Three lessons have no questions yet** — `Java Basics 1`, `Java Basics 2` and
  `Test`. They are placeholders and do not block the lessons behind them.
- **Some screens are not built yet** and their links lead nowhere:
  `Study.html`, `Modules.html`, `My level.html`, `overview.html`,
  `Practice 1.html`, `Practice 2.html`, `Test 1.html`, `View course.html`,
  `Multiplayer.html`, `Overview.html`.
