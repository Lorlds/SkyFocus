Continue the SkyFocus Windows app in `E:\Keaitus\Project\SkyFocus`.

Project summary:
- WinUI 3 + Windows App SDK desktop focus app
- .NET target: `net8.0-windows10.0.26100.0`
- Repo root is the app project root
- Test project is `SkyFocus.Tests/`
- Build and tests currently pass

Verified commands:
- `dotnet build SkyFocus.csproj -c Debug -p:Platform=x64`
- `dotnet test SkyFocus.Tests\SkyFocus.Tests.csproj -c Debug -p:Platform=x64`

Current implemented areas:
- app shell and navigation
- focus session state machine
- SQLite persistence
- reminders and soft blocker plumbing
- sounds page with placeholder ambient audio
- history/stats foundations
- unit tests for focus session, settings, and statistics

Please start by reading:
- `README.md`
- `README.zh-CN.md`
- `AGENTS.md`

Recommended next priorities:
1. polish UI/visual design and motion
2. improve history insights and achievement presentation
3. verify MSIX packaging and release readiness
4. replace placeholder audio and finalize product assets

Important notes:
- `SkyFocus.csproj` excludes `SkyFocus.Tests/**` from app compilation on purpose.
- Do not revert the current repo layout unless there is a strong reason.
- Keep the app original; do not copy FocusFlight branding or exact assets.
