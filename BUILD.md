# Build & Distribution Runbook

## Hotel Management System (WinForms Desktop Edition)

**Target:** Windows 10/11 x64 desktop executable
**Framework:** .NET 9 (`net9.0-windows`)
**Audience:** Developer building the binary, plus the instructor running it.

---

## 1. Prerequisites

### 1.1 On the build machine (must be Windows for the WinForms target)

- **Windows 10 or 11 x64** — the WinForms `net9.0-windows` target cannot be
  built or run on macOS / Linux.
- **.NET 9 SDK** — either:
  - Download from <https://dot.net/download> (pick the SDK, not the runtime), or
  - `winget install Microsoft.DotNet.SDK.9`
- **Git** — to clone the repository.
- **(Optional) SQL Server LocalDB or SQL Server Express** — to smoke-test
  the SQL Server flow before distribution.

Verify the SDK with:

```cmd
dotnet --version
```

It should print `9.0.x`.

### 1.2 On the target machine (where the .EXE will run)

| Build flavour | Target machine needs |
|---|---|
| Framework-dependent (~5 MB) | .NET 9 **Desktop** Runtime installed |
| Self-contained (~80 MB) | Nothing; the runtime is bundled |

Plus SQL Server (LocalDB / Express / full) if `Persistence:Mode` is set to
`SqlServer` in `appsettings.json`.

---

## 2. Building the .EXE

### 2.1 Step-by-step

```cmd
:: 1. Clone the repository
git clone <your-repo-url>
cd Windows

:: 2. Restore packages (one-time per branch)
dotnet restore HotelManagement.WinForms.csproj

:: 3. Pick a publish flavour (see below) and run it
```

### 2.2 Publish flavour A — Framework-dependent single-file

Smallest output (~5 MB). Requires .NET 9 Desktop Runtime on the target.

Pasted as a single line (works in **both Command Prompt and
PowerShell**, no fragile line-continuation characters):

```
dotnet publish HotelManagement.WinForms.csproj -c Release -f net9.0-windows -r win-x64 --no-self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish\framework-dependent
```

> **Note.** Newer .NET SDKs (8+) prefer `--no-self-contained` over the
> older `--self-contained=false`. Both still work on .NET 9 but the
> switch form is friendlier to PowerShell and to SDK 10's parser.

### 2.3 Publish flavour B — Self-contained single-file *(recommended for the instructor demo)*

Bigger output (~80 MB). Runs on any Windows 10/11 x64 with **no
prerequisites**.

```
dotnet publish HotelManagement.WinForms.csproj -c Release -f net9.0-windows -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish\self-contained
```

### 2.4 Publish flavour C — Folder (for debugging)

```
dotnet publish HotelManagement.WinForms.csproj -c Release -f net9.0-windows -o publish\folder
```

### 2.5 If your line-continuation character matters

The single-line form above is the safest. If you prefer multi-line for
readability, the right separator depends on the shell:

| Shell | Continuation | Example |
|---|---|---|
| `cmd.exe` | `^` (caret) at end of line | `--self-contained ^` |
| PowerShell | `` ` `` (backtick) at end of line | `` --self-contained ` `` |
| bash / zsh (WSL) | `\` (backslash) at end of line | `--self-contained \` |

A `^` in PowerShell is *not* a line-continuation — it would get
parsed as bitwise XOR and the next line would be treated as a fresh
command. That's the classic cause of *"the value after the `--` is
wrong"* errors when copy-pasting a `cmd`-style block into PowerShell.

### 2.5 Output checklist

Look inside `publish\<flavour>\` and confirm the following files are present:

- `HotelManagement.WinForms.exe` — double-click to launch.
- `appsettings.json` — edit before distribution (see §3).
- `db\schema_sqlserver.sql` — first-launch bootstrap script.
- `db\seed_sqlserver.sql` — seeds rooms / users / menu items.
- `Assets\menu_placeholder.jpg` — menu-image fallback.
- (Self-contained only) bundled .NET 9 runtime files.

---

## 3. SQL Server Setup

### 3.1 Decide your mode

Open `appsettings.json`:

```json
{
  "Persistence": { "Mode": "SqlServer" },
  "ConnectionStrings": {
    "HotelManagement": "Server=.;Database=HotelManagement;Trusted_Connection=True;TrustServerCertificate=True;",
    "_master":          "Server=.;Database=master;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Settings:

| Field | Meaning |
|---|---|
| `Persistence.Mode` | `InMemory` (no SQL Server, transient seed) or `SqlServer` (full persistence). |
| `HotelManagement` | The connection string used for normal operations. |
| `_master` | The connection string used **only** during first-launch bootstrap to `CREATE DATABASE`. |

### 3.2 Pick a connection string

| Your SQL Server is… | Use |
|---|---|
| `(localdb)\MSSQLLocalDB` | `Server=(localdb)\\MSSQLLocalDB;…` (LocalDB) |
| `MACHINE\SQLEXPRESS`     | `Server=.\\SQLEXPRESS;…` (Express) |
| Default instance (`MACHINE`) | `Server=.;…` |

(The `\\` is a JSON-encoded single backslash.)

Verify the instance name with `sqllocaldb info` (LocalDB) or by connecting in
SSMS.

### 3.3 Two paths to a populated database

**Path A — Let the app bootstrap.** Just launch the .EXE. On first run:

1. `SqlBootstrap` connects to `master` and creates the `HotelManagement` DB.
2. If `dbo.rooms` doesn't exist, it runs `db\schema_sqlserver.sql`.
3. If `dbo.rooms` is empty, it runs `db\seed_sqlserver.sql`.

**Path B — Run scripts in SSMS first.** Better for an instructor demo because
the SQL is visible:

1. Open `db\schema_sqlserver.sql` in SSMS → **Execute**.
2. Open `db\seed_sqlserver.sql` → switch database dropdown to
   `HotelManagement` → **Execute**.
3. Launch the .EXE — it will load straight from the seeded data.

### 3.4 Verify the connection

In SSMS run:

```sql
USE HotelManagement;
SELECT TOP 5 number, type, rate, is_occupied, [condition] FROM dbo.rooms;
```

Should return rooms 101, 102, 201, 202, 301.

---

## 4. First-Launch Credentials

| Username | Password | Role |
|---|---|---|
| `superadmin` | `superadmin123` | SuperAdmin (full access) |
| `staff` | `staff123` | Staff (front-desk operations) |

Passwords are stored as **BCrypt hashes**. If the SQL seed stored a
plaintext value, the app **auto-upgrades** it to BCrypt on first load; you
won't notice unless you inspect the `users.password_hash` column.

**Change these before any real deployment.**

---

## 5. Distribution Checklist

Before zipping the publish folder for the instructor:

- [ ] `appsettings.json` edited for her environment (or left on LocalDB if
      she has it).
- [ ] If using Path B (SSMS) — instructor knows to run the two SQL scripts
      before launching.
- [ ] The `db\` folder is present in the publish output (`PreserveNewest`
      copy is configured in the csproj — verify after publish).
- [ ] Self-contained build was chosen if you're not sure the instructor has
      the .NET 9 desktop runtime.
- [ ] Test the .EXE on a clean Windows VM (or a different account) before
      shipping — catches missing dependencies.

---

## 6. Common Gotchas

| Symptom | Cause | Fix |
|---|---|---|
| "The application failed to start" with no detail | Missing .NET 9 Desktop Runtime | Use the self-contained build OR install the runtime |
| "Could not connect to SQL Server" warning at startup | Wrong connection string / SQL Server not running | Check `appsettings.json`; verify with SSMS |
| "Login failed for user…" | Authentication mode mismatch | Use `Trusted_Connection=True` for Windows auth; or supply `User Id=…;Password=…` |
| Login UI rejects valid credentials after switching to SqlServer | Schema seed used the placeholder hash format | Replace plaintext / placeholder values in `users.password_hash`; the app auto-migrates on next launch |
| PDF reports show truncated text at the right edge | Locale font fall-back | Reports already render landscape A4 with bundled Lato; if still cropped, install Lato system-wide |
| First launch hangs ~10 s | QuestPDF initialising | One-time cost; subsequent launches are instant |
| App reports it cannot find `db\schema_sqlserver.sql` | Files not copied during publish | Re-run publish; verify the `<None Include="db\…"><CopyToOutputDirectory>` items in `HotelManagement.WinForms.csproj` |
| `dotnet publish` complains *"the value after the `--` is wrong"* on `--self-contained=false ^` | Running a `cmd`-style multi-line command in PowerShell where `^` isn't a continuation | Paste the publish command as a **single line**, or replace `^` with backticks `` ` `` for PowerShell. See §2.5. |
| `error NETSDK1045: The current .NET SDK does not support targeting .NET 9.0` | SDK 10 installed without the .NET 9 targeting pack | Either install .NET 9 SDK side-by-side (`winget install Microsoft.DotNet.SDK.9`) **or** retarget the csproj to `net10.0-windows`/`net10.0` |

---

## 7. Running From Source (no publish)

For development on Windows you can skip publish entirely:

```cmd
dotnet run --project HotelManagement.WinForms.csproj -c Debug
```

This is the fastest iteration loop. The compiled binary lives in
`bin\Debug\net9.0-windows\` with `appsettings.json` copied alongside.

---

## 8. Running Tests

The test project targets cross-platform `net9.0`, so tests run on **macOS,
Linux, or Windows**:

```cmd
dotnet test HotelManagement.Tests/HotelManagement.Tests.csproj -c Release
```

Expected output: **149 passed**. Total wall-clock time on a recent machine
should be under 200 ms.

To run a specific use case's acceptance tests only:

```cmd
dotnet test HotelManagement.Tests/HotelManagement.Tests.csproj ^
  --filter FullyQualifiedName~UC2_ReservationTests
```

---

## 9. Reverting / Re-seeding the Database

If a demo gets messy and you want to start over from the seeded baseline:

```sql
USE master;
DROP DATABASE HotelManagement;
```

…then relaunch the .EXE. `SqlBootstrap` will recreate the database, run the
schema script, and apply the seed.

---

*End of build runbook.*
