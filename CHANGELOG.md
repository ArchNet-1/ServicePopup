# Changelog

## [2.0.0] - 2026-11-25

First version, numbered like the other ArchNet 2.0 packages.

### Added
- The Back key (ArchNet Input Module) closes this screen cleanly: the popup answers no / cancel.
- `PopupService`: `Alert`, `Confirm` and `Prompt` returning tasks, one popup at a time with a waiting line; open and waiting popups answer as cancelled when the service goes.
- `PopupView`: fills the popup screen; Enter in the text field confirms.
- Popups service for the ArchNet Hub (*Services > Popups*): creates `GamePopups.cs` and `Popups.prefab`, and puts the screen under the ArchNet Bootstrap.
- Popups window (*Open tool*): open the prefab, try each popup in Play Mode.
- Edit Mode tests.
