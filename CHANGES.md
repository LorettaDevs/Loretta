# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Unreleased
### Changed
- [Breaking] The following were changed as a result of the move to `EqualsValuesClauseSyntax`:
	- Replaced `EqualsToken` and `Values` in `AssignmentStatementSyntax` by `EqualsValues`;
	- Replaced `EqualsToken` and `Values` in `LocalVariableDeclarationStatementSyntax` by `EqualsValues`.

### Removed
- [Breaking] The following were removed as a result of the move to `EqualsValuesClauseSyntax`:
	- `AssignmentStatementSyntax.EqualsToken`;
	- `AssignmentStatementSyntax.Values`;
	- `AssignmentStatementSyntax.Update(SeparatedSyntaxList<PrefixExpressionSyntax> variables, SyntaxToken equalsToken, SeparatedSyntaxList<ExpressionSyntax> values, SyntaxToken semicolonToken)`;
	- `AssignmentStatementSyntax.AddValues(params ExpressionSyntax[] items)`;
	- `AssignmentStatementSyntax.WithEqualsToken(SyntaxToken equalsToken)`;
	- `AssignmentStatementSyntax.WithValues(SeparatedSyntaxList<ExpressionSyntax> values)`;
	- `LocalVariableDeclarationStatementSyntax.AddValues(params ExpressionSyntax[] items)`;
	- `LocalVariableDeclarationStatementSyntax.EqualsToken`;
	- `LocalVariableDeclarationStatementSyntax.Update(SyntaxToken localKeyword, SeparatedSyntaxList<IdentifierNameSyntax> names, SyntaxToken equalsToken, SeparatedSyntaxList<ExpressionSyntax> values, SyntaxToken semicolonToken)`;
	- `LocalVariableDeclarationStatementSyntax.Values`;
	- `LocalVariableDeclarationStatementSyntax.WithEqualsToken(SyntaxToken equalsToken)`;
	- `LocalVariableDeclarationStatementSyntax.WithValues(SeparatedSyntaxList<ExpressionSyntax> values)`;
	- `SyntaxFactory.AssignmentStatement()`;
	- `SyntaxFactory.AssignmentStatement(SeparatedSyntaxList<PrefixExpressionSyntax> variables, SyntaxToken equalsToken, SeparatedSyntaxList<ExpressionSyntax> values, SyntaxToken semicolonToken)`;
	- `SyntaxFactory.LocalVariableDeclarationStatement()`;
	- `SyntaxFactory.LocalVariableDeclarationStatement(SeparatedSyntaxList<IdentifierNameSyntax> names, SeparatedSyntaxList<ExpressionSyntax> values)`;
	- `SyntaxFactory.LocalVariableDeclarationStatement(SyntaxToken localKeyword, SeparatedSyntaxList<IdentifierNameSyntax> names, SyntaxToken equalsToken, SeparatedSyntaxList<ExpressionSyntax> values, SyntaxToken semicolonToken)`.

### Added
- The following were added as a result of the move to `EqualsValuesClauseSyntax`:
	- `SyntaxKind.EqualsValuesClause`;
	- `EqualsValuesClauseSyntax`;
	- `EqualsValuesClauseSyntax.EqualsToken`;
	- `EqualsValuesClauseSyntax.Values`;
	- `EqualsValuesClauseSyntax.Update(SyntaxToken equalsToken, SeparatedSyntaxList<ExpressionSyntax> values)`;
	- `EqualsValuesClauseSyntax.WithEqualsToken(SyntaxToken equalsToken)`;
	- `EqualsValuesClauseSyntax.AddValues(params ExpressionSyntax[] items)`;
	- `EqualsValuesClauseSyntax.WithValues(SeparatedSyntaxList<ExpressionSyntax> values)`;
	- `EqualsValuesClauseSyntax.Accept(LuaSyntaxVisitor visitor)`;
	- `EqualsValuesClauseSyntax.Accept<TResult>(LuaSyntaxVisitor<TResult> visitor)`;
	- `AssignmentStatementSyntax.EqualsValues`;
	- `AssignmentStatementSyntax.Update(SeparatedSyntaxList<PrefixExpressionSyntax> variables, EqualsValuesClauseSyntax equalsValues, SyntaxToken semicolonToken)`;
	- `AssignmentStatementSyntax.AddEqualsValuesValues(params ExpressionSyntax[] items)`;
	- `AssignmentStatementSyntax.WithEqualsValues(EqualsValuesClauseSyntax equalsValues)`;
	- `LocalVariableDeclarationStatementSyntax.EqualsValues`;
	- `LocalVariableDeclarationStatementSyntax.Update(SyntaxToken localKeyword, SeparatedSyntaxList<IdentifierNameSyntax> names, EqualsValuesClauseSyntax? equalsValues, SyntaxToken semicolonToken)`;
	- `LocalVariableDeclarationStatementSyntax.AddEqualsValuesValues(params ExpressionSyntax[] items)`;
	- `LocalVariableDeclarationStatementSyntax.WithEqualsValues(Loretta.CodeAnalysis.Lua.Syntax.EqualsValuesClauseSyntax? equalsValues)`;
	- `SyntaxFactory.AssignmentStatement(SeparatedSyntaxList<PrefixExpressionSyntax> variables, EqualsValuesClauseSyntax equalsValues, SyntaxToken semicolonToken)`;
	- `SyntaxFactory.AssignmentStatement(SeparatedSyntaxList<PrefixExpressionSyntax> variables, EqualsValuesClauseSyntax equalsValues)`;
	- `SyntaxFactory.EqualsValuesClause(SyntaxToken equalsToken, SeparatedSyntaxList<ExpressionSyntax> values)`;
	- `SyntaxFactory.LocalVariableDeclarationStatement(SeparatedSyntaxList<IdentifierNameSyntax> names, EqualsValuesClauseSyntax? equalsValues)`;
	- `SyntaxFactory.LocalVariableDeclarationStatement(SyntaxToken localKeyword, SeparatedSyntaxList<IdentifierNameSyntax> names, EqualsValuesClauseSyntax? equalsValues, SyntaxToken semicolonToken)`;
	- `LuaSyntaxVisitor.VisitEqualsValuesClause(EqualsValuesClauseSyntax node)`;
	- `LuaSyntaxVisitor<TResult>.VisitEqualsValuesClause(EqualsValuesClauseSyntax node)`;
	- `LuaSyntaxWalker.VisitEqualsValuesClause(EqualsValuesClauseSyntax node)`;
	- `LuaSyntaxRewriter.VisitEqualsValuesClause(EqualsValuesClauseSyntax node)`.

## v0.2.7-beta.11
### Fixed
- Fixed infinite loop on variable rename conflict.

## v0.2.7-beta.10
### Added
- Added `Loretta.CodeAnalysis.Lua.SymbolDisplay.ObjectDisplay.NilLiteral`.
- Added `==` and `!=` operators to `Loretta.CodeAnalysis.FileLinePositionSpan`.

### Deprecated
- Deprecated `Loretta.CodeAnalysis.Lua.SymbolDisplay.ObjectDisplay.NullLiteral` in favor of `NilLiteral`.

### Removed
- Removed the following unused things:
	- `Loretta.CodeAnalysis.CaseInsensitiveComparison`;
	- `Loretta.CodeAnalysis.GeneratedKind`;
	- `Loretta.CodeAnalysis.Optional<T>`;
	- `Loretta.CodeAnalysis.SourceReferenceResolver`;
	- `Loretta.CodeAnalysis.SyntaxTreeOptionsProvider`;
	- `Loretta.CodeAnalysis.ReportDiagnostic`;
	- `Loretta.CodeAnalysis.LocationKind.XmlFile`;
	- `Loretta.CodeAnalysis.SuppressionDescriptor`;
	- `Loretta.CodeAnalysis.Diagnostics.SuppressionInfo`.
- Removed the following as they did not make sense in our project:
	- `Loretta.CodeAnalysis.SourceLocation.GetMappedLineSpan`;
	- `Loretta.CodeAnalysis.WellKnownDiagnosticTags.EditAndContinue`;
	- `Loretta.CodeAnalysis.WellKnownDiagnosticTags.Telemetry`;
	- `Loretta.CodeAnalysis.WellKnownDiagnosticTags.AnalyzerException`;
	- `Loretta.CodeAnalysis.WellKnownDiagnosticTags.CustomObsolete`;
	- `Loretta.CodeAnalysis.WellKnownDiagnosticTags.CompilationEnd`;
	- `Loretta.CodeAnalysis.LineVisibility`;
	- `Loretta.CodeAnalysis.SyntaxTree.GetMappedLineSpan`;
	- `Loretta.CodeAnalysis.SyntaxTree.GetLineVisibility`;
	- `Loretta.CodeAnalysis.SyntaxTree.HasHiddenRegions`;
	- `Loretta.CodeAnalysis.Location.GetMappedLineSpan`;
	- `IsHiddenPosition` `SyntaxTree` extension method;
	- `Loretta.CodeAnalysis.Lua.LuaSyntaxTree.GetMappedLineSpan`;
	- `Loretta.CodeAnalysis.Lua.LuaSyntaxTree.GetLineVisibility`;
	- `Loretta.CodeAnalysis.Lua.LuaSyntaxTree.HasHiddenRegions`.

## v0.2.7-beta.9
### Added
- Added `IScope.ContainedScopes` which lets you check scopes contained within another scope.
- Added `IVariable.CanBeAccessedIn(IScope)` which lets you check if the variable can be accessed within a given scope.
- Added `IScope.FindVariable(string)` which lets you try to find a variable in a scope or any of its parents.
- Added `Script.RenameVariable(IVariable variable, string newName)` which lets you rename a variable in a new script instance.

### Changed
- Renamed `IScope.Parent` to `IScope.ContainingScope` to be more consistent with `ContainedScopes`.

## v0.2.7-beta.8
### Added
- Added a minifier to the experimental package.
- Added support for the Lua 5.1 and Luau lexing bug that makes them silently accept invalid
  single-char escapes (e.g.: `"\A\B\C\D" will be read as "ABCD" silently without any errors or warnings).

### Fixed
- `ObjectDisplay.FormatLiteral` was not emitting unicode escapes correctly.

## v0.2.7-beta.7
### Added
- Added `Loretta.CodeAnalysis.Lua.Experimental.SyntaxExtensions.ConstantFold`.

### Changed
- DEPRECATED: `Loretta.CodeAnalysis.Lua.Experimental.SyntaxExtensions.FoldConstants` in favor of `ConstantFold`.

### Fixed
- `ObjectDisplay.FormatLiteral` was not adding the leading quote properly.
   This also affected `SyntaxFactory.Literal`.
- Constant folder was not folding `"a" .. true` to `"atrue"`.
- Constant folder was not folding `nil == nil` to `true`.
- Constant folder was folding `not x` to `x` instead of the result of the negation.

## v0.2.7-beta.6
### Added
- Added support for FiveM's hash strings (thanks to @TheGreatSageEqualToHeaven).
  NOTE: There is currently no support for creating hash string literal nodes from `SyntaxFactory` because the API for
  that still hasn't been decided on. If you need to create a node for it use `SyntaxFactory.ParseExpression`.

## v0.2.7-beta.5
### Added
- Added support for Luau's if expression.

### Fixed
- Fixed errors not being reported for missing expressions.
- Fixed an error not being reported when a compound assignment is parsed and `AcceptCompoundAssignment` is false.
- Fixed typo in `SyntaxKind.StarEqualsToken` (was `StartEqualsToken`).
- `SyntaxNormalizer` fixes by @bmcq-0 in #37:
	- Fixed chained unary operators spacing;
	- Fixed spaces before semicolons;
	- Fixed missing space after assignment operator;
	- Fixed spaces being inserted inside parenthesis;
	- Fixed spacing inside table constructors;
	- Fixed spacing after `ìf`, `elseif`, `until` and `while`;
	- Fixed two line breaks being inserted after certain statements;
	- Fixed line breaks being inserted before semicolons;
	- Fixed line breaks being inserted twice between statements.

### Changed
- Changed the generated expression in the case of a missing expression (now it is a `IdentifierNameSyntax`
  with `IsMissing` set to `true`).
- Changed the generated statement in the case of a missing expression (now it is an `ExpressionStatementSyntax`
  with `IsMissing` set to `true` and the inner expression is the one in the bullet point above).
- Changed the generated error message in the case of a missing/invalid statement.

## v0.2.7-beta.4
### Changed
- Fixed a typo in `EmptyStatementSyntax` and its helper methods.

## v0.2.7-beta.3
### Removed
- Removed unused classes we got when vendoring Roslyn, of which, the following were public:
	- `Loretta.CodeAnalysis.SourceFileResolver`;
	- `Loretta.CodeAnalysis.AnalyzerConfig`;
	- `Loretta.CodeAnalysis.AnalyzerConfigOptionsResult`;
	- `Loretta.CodeAnalysis.AnalyzerConfigSet`;
	- `Loretta.CodeAnalysis.ErrorLogOptions`;
	- `Loretta.CodeAnalysis.SarifVersion`;
	- `Loretta.CodeAnalysis.SarifVersionFacts`.
- The following references were removed:
	- `Microsoft.CodeAnalysis.Analyzers`;
	- `System.Reflection.Metadata`.
- The following references were removed for the .NET Core 3.1 build:
	- `System.Collections.Immutable`;
	- `System.Memory`;
	- `System.Runtime.CompilerServices.Unsafe`;
	- `System.Threading.Tasks.Extensions`.

## v0.2.7-beta.2
### Added
- Add support for implicit `self` parameter in methods.

### Fixed
- Fix parameters not having an `IVariable` associated with them.

### Changed
- Fix file-level implicit `arg` parameter being named `args`.

## v0.2.7-beta.1
### Added
- Add note about non-`CompilationUnitSyntax` rooted trees in `Script`.

### Changed
- Add validation for `default` immutable arrays of syntax trees in `Script`.
- `FileScope`s are now only generated for `CompilationUnitSyntax`es.


### Fixed
- Fix `FindScope` not returning anything for root statements.
- Fix `CompilationUnitSyntax`es not having any associated scopes.

## v0.2.6
### Added
- Add better formatting for tables when they're multiline.

### Fixed
#### Scoping
- Fix `SimpleFunctionName` handling to:
  - Not always add it as a write when it's part of a composite name.
  - Not assign the simple name variable to the entire name when it's a composite one.

#### Formatting
- Fix formatting of `:` (`a:b` was being turned into `a : b`).
- Fix multiple line breaks being inserted when formatting.
- Fix line breaks not being added after single line comments.
- Fix formatting of `[` and `]` (`a[a]` was being turned into `a [ a ]`).
- Fix empty table formatting (`{}` instead of `{ }`).
- Fix excessive spacing around parenthesis.

#### Syntax
- Fix missing `SyntaxKind`s for `UnaryExpressionSyntax` and `BinaryExpressionSyntax` in `Syntax.xml`.

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
