# SkyFocus

SkyFocus is a Windows-native focus timer built with WinUI 3 and the Windows App SDK. It is designed as an original aviation-themed productivity app with focus sessions, ambient sound, local stats, soft-blocking, and a lightweight flight-progress experience.

## Current Status

This repository already contains a working v1 scaffold:
- WinUI 3 packaged desktop app
- Home / Focus / History / Sounds / Settings pages
- Local session persistence with SQLite
- Session lifecycle: create, start, pause, resume, complete, abandon
- Soft blocker state and reminder plumbing
- Ambient sound playback with placeholder audio assets
- Unit tests for focus session, statistics, and settings flows

Validation completed locally:
- `dotnet build SkyFocus.csproj -c Debug -p:Platform=x64`
- `dotnet test SkyFocus.Tests\\SkyFocus.Tests.csproj -c Debug -p:Platform=x64`
- App startup verified from built executable

## Stack

- C#
- .NET 8
- WinUI 3
- Windows App SDK
- CommunityToolkit.Mvvm
- Microsoft.Data.Sqlite
- MSTest

## Repository Layout

- `SkyFocus.csproj`: main WinUI app project
- `SkyFocus.Tests/`: test project
- `Models/`: domain records and state models
- `Services/`: focus engine, persistence, audio, reminders, settings, navigation, statistics
- `ViewModels/`: page and shell state
- `Views/`: XAML pages
- `Resources/`, `Strings/`, `Assets/`: theming, localization, packaged assets

## Build And Test

From the repository root:

```powershell
dotnet build SkyFocus.csproj -c Debug -p:Platform=x64
dotnet test SkyFocus.Tests\SkyFocus.Tests.csproj -c Debug -p:Platform=x64
```

To run the built app directly:

```powershell
.\bin\x64\Debug\net8.0-windows10.0.26100.0\win-x64\SkyFocus.exe
```

## What Is Still Next

Recommended next milestones:
- polish the visual system and motion
- replace placeholder audio with product-quality assets
- improve history insights and achievement presentation
- verify MSIX packaging and clean-machine install flow
- prepare store-facing copy, icons, and screenshots

## Notes

- The repository root is the app project root by design.
- The nested `SkyFocus.Tests/` folder is explicitly excluded from app compilation in `SkyFocus.csproj`.
- The app currently uses generated placeholder sound files in `Assets/Audio/`.
