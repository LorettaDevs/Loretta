### Added
- Added `LuaSyntaxOptions.Luau`.

### Changed
- Changed `LuaSyntaxOptions.ToString` to return `"Luau"` for the luau preset.

### Fixed
- Fixed a bug with incremental parsing where a `System.InvalidCastException` was thrown from `Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax.Blender.Reader.CanReuse`.