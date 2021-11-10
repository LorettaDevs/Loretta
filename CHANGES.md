# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## \[Unreleased\]
### Fixed
- Fix `SimpleFunctionName` handling to:
  - Not always add it as a write when it's part of a composite name.
  - Not assign the simple name variable to the entire name when it's a composite one.
- Fix formatting of `:` (`a:b` was being turned into `a : b`).

## v0.2.5
### Added
- Add parameterless ctor of `Script` and `Script.Empty` for easier creation of empty scripts.
- Add support for `SimpleFunctionName` in variable handling (local vars didn't have a write added to them and a global wasn't created if a local with the same name didn't exist).
- Add `SyntaxFacts.IsOperatorToken`, `SyntaxFacts.IsUnaryOperatorToken` and `SyntaxFacts.IsBinaryOperatorToken`.
- Add `Script.FindScope`.

### Fixed
- Fix jumps to non-existent labels (labels that don't have a `::label::`) not creating a new label.
- Fix jumps to a goto label not being added to the `IGotoLabel`.

### Changed
- Configure `IFileScope`, `IFunctionScope`, `IGotoLabel`, `IScope` and `IVariable` to only allow internal implementations.
- Make `IGotoLabel.LabelSyntax` nullable to account for the case where the label has no declarations.

## v0.2.4
### Added
- Add nuget params to `Loretta.CodeAnalysis`, `Loretta.CodeAnalysis.Lua` and `Loretta.CodeAnalysis.Lua.Experimental`;
- Add CD flow to publish packages to a public **myget.org** (not nuget.org) feed.
- Implement `ConstantFolder`;

### Changed
- [Breaking] Changed what is accepted as whitespace to not be unicode-aware (as lua isn't);

### Fixed
- Fix `SyntaxNormalizer` adding a space after `.`s;

## v0.2.3:
### Fixed
- Updated `Syntax.xml` to include `BangToken` in `UnaryExpressionSyntax` and `BangEqualsToken` in `BinaryExpressionSyntax`;
- Fixed infinite loop in `LuaSyntaxWalker` with empty `StatementList`s;

### Added
- Implemented `\z` whitespace voider escape for short strings;
- Implemented `SourceTextReader` as an attempt to reduce allocations when lexing;

### Changed
- Removed GParse dependency (side effect of `SourceTextReader`).

## v0.2.2
### Changed
- [Breaking] Only store `IScope`s that directly reference a `IVariable` in the `IVariable.ReferencingScopes`;
- [Breaking] Move `CapturedVariables` to `IFunctionScope` as it is not applicable to non-function scopes;
- Updated the README with instructions and documentation on v0.2.

### Added
- Add `ReferencedVariables` to `IScope` with all `IVariable`s referenced by the scope and its children;

## v0.2.1
### Added
- [Breaking] Implement 5.3 bitwise operators along with `LuaSyntaxOptions` setting for erroring on them;

  Breaking change: the precedences for each operator were altered.
- Implement a warning about `\n\r` being used as it is recognized as a single line break by Lua;

### Changed
- Amortize allocations made by string parsing.
