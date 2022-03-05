### Added
- Added `LuaSyntaxOptions.AcceptLuaJITNumberSuffixes`.
- Added `SyntaxFactory.Literal(ulong value)`.
- Added `SyntaxFactory.Literal(string text, ulong value)`.
- Added support for `ULL` and `LL` suffixes from LuaJIT.

### Fixed
- Fixed `ObjectDisplay.FormatLiteral(long value, ObjectDisplayOptions options, CultureInfo? cultureInfo = null)` returning numbers in hex for longs.

### Changed
- Changed `LuaSyntaxOptions.ToString` to return `AcceptLuaJITNumberSuffixes`