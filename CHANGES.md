## [Unreleased]
- Add support for `SimpleFunctionName` in variable handling (local vars didn't have a write added to them and a global wasn't created if a local with the same name didn't exist).

## v0.2.4
- [Breaking] Changed what is accepted as whitespace to not be unicode-aware (as lua isn't);
- Fix `SyntaxNormalizer` adding a space after `.`s;
- Implement the `ConstantFolder`;
- Add nuget params to `Loretta.CodeAnalysis`, `Loretta.CodeAnalysis.Lua` and `Loretta.CodeAnalysis.Lua.Experimental`;
- Add CD flow to publish packages to a public **myget.org** (not nuget.org) feed.

## v0.2.3:
- Updated `Syntax.xml` to include `BangToken` in `UnaryExpressionSyntax` and `BangEqualsToken` in `BinaryExpressionSyntax`;
- Implemented `\z` whitespace voider escape for short strings;
- Fixed infinite loop in `LuaSyntaxWalker` with empty `StatementList`s;
- Implemented `SourceTextReader` as an attempt to reduce allocations when lexing;
- Removed GParse dependency (side effect of `SourceTextReader`).

## v0.2.2
- [Breaking] Only store `IScope`s that directly reference a `IVariable` in the `IVariable.ReferencingScopes`;
- [Breaking] Move `CapturedVariables` to `IFunctionScope` as it is not applicable to non-function scopes;
- Add `ReferencedVariables` to `IScope` with all `IVariable`s referenced by the scope and its children;
- Updated the README with instructions and documentation on v0.2.

## v0.2.1
- [Breaking] Implement 5.3 bitwise operators along with `LuaSyntaxOptions` setting for erroring on them;

  Breaking change: the precedences for each operator were altered.
- Implement a warning about `\n\r` being used as it is recognized as a single line break by Lua;
- Amortize allocations made by string parsing.
