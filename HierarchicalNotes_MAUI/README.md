# Hierarchical Notes MAUI conversion

This solution contains:
- `HierarchicalNotes.Core`: portable note models, encryption, search, and HTML export helpers.
- `HierarchicalNotes.Maui`: the MAUI app shell and generic cross-platform UI.

Notes:
- The original WPF-only windowing, tree drag/drop handlers, and WebBrowser integration were replaced with MAUI-friendly controls and commands.
- Linux is not targeted because it is not an official .NET MAUI deployment platform.
- The solution targets .NET 10 and is intended for the current Visual Studio 2026 toolchain.
