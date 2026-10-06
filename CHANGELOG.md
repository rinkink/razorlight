# Changelog

All notable changes to this fork are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/); versions follow [SemVer](https://semver.org/).

This project is a fork of [toddams/RazorLight](https://github.com/toddams/RazorLight), taken at
[v2.3.1 (0b39203)](https://github.com/toddams/RazorLight/commit/0b39203f547699345d24ccf955dd6fa66c0441c4).
Upstream history before that point is unchanged and is not repeated here.

## [Unreleased]

### Added
- Templates can use C# 12 syntax. Roslyn is now referenced explicitly at 4.8.0 instead of the 4.0.0 that Razor 6.0.36 pulls in. Fixes upstream [#555](https://github.com/toddams/RazorLight/issues/555).
- `IncludeRawAsync(key)` writes a project item verbatim, without Razor compilation and without appending `.cshtml`. Use it for CSS and other static files. Closes upstream [#359](https://github.com/toddams/RazorLight/issues/359) and [#536](https://github.com/toddams/RazorLight/issues/536).
- `RazorLightProject.GetRawItemAsync` (virtual) and `IRazorTemplateCompiler.Project` to support the above.

### Fixed
- `FileSystemRazorProject.GetItemAsync` throws `ArgumentNullException` on a null key instead of `NullReferenceException`.
- Three tests that called `Assert.ThrowsAsync` without awaiting it never asserted anything; they do now.

## [3.0.0] - 2026-10-05

First release of the fork. Published to NuGet as `Rinkink.RazorLight`.

### Changed
- **Breaking:** targets `net8.0` only. Dropped `netstandard2.0`, `netcoreapp3.1`, `net5.0` and `net6.0`.
- **Breaking:** Razor compiler packages (`Microsoft.AspNetCore.Mvc.Razor.Extensions`, `Microsoft.CodeAnalysis.Razor`) pinned to exactly `6.0.36`. That is the last version Microsoft published to NuGet; from .NET 7 the Razor compiler ships only inside the SDK, so this fork, and RazorLight itself, cannot move past it.
- `Microsoft.Extensions.*` dependencies moved to the 8.0.x line.
- `DefaultMetadataReferenceManager` tracks visited assemblies by `Assembly.FullName` instead of the obsolete `EscapedCodeBase`.
- `DefaultRazorEngine` no longer carries target-framework conditionals; the `netstandard2.0`-only `InstrumentationPass` is gone.
- Package version is now set in `Version.props` at the repository root (was `src/Directory.Build.props`).
- Package id is `Rinkink.RazorLight`; README is packed into the NuGet package.

### Removed
- **Breaking:** `RazorLightDependencyBuilder.UseNetFrameworkLegacyFix()` and `LegacyFixAssemblyPathFormatter`. Both relied on `Assembly.CodeBase`, which is obsolete on .NET 8.
- `RazorLight.Precompile` tool and its tests.
- `RazorLight.Sandbox` and `samples/` projects.
- `README.source.md` and the MarkdownSnippets generation step; `README.md` is now edited directly.

### Tests
- Test project targets `net8.0` only; test SDK and hosting packages moved to the 8.0.x line.
- Dropped `Pose` and `MarkdownSnippets.MsBuild` test dependencies.
- `Multiple_Simultaneous_Compilations_RaceCondition_Test` runs 10 iterations instead of 100 and is tagged `Category=Slow`.

[Unreleased]: https://github.com/rinkink/razorlight/compare/v3.0.0...HEAD
[3.0.0]: https://github.com/rinkink/razorlight/compare/v2.3.1...v3.0.0
