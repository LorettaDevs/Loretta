# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Unreleased
### Fixed
- Fixed `LuaSyntaxOptions.AcceptInvalidEscapes` not suppressing errors in cases where `LuaSyntaxOptions.{AcceptWhitespaceEscape,AcceptHexEscapesInStrings,AcceptUnicodeEscape}` were `false` by @TheGreatSageEqualToHeaven in https://github.com/LorettaDevs/Loretta/pull/116.
- Fixed single line comments not getting a line break added after them in `NormalizeWhitespace` by @GGG-KILLER in https://github.com/LorettaDevs/Loretta/pull/118.
- Fixed warnings being generated for hex numbers on Lua 5.1, Lua 5.2 and other presets where `HexIntegerFormat` was `NotSupported` and `AcceptHexFloatLiterals` was `false` by @GGG-KILLER.
- Fixed the `SyntaxNormalizer` turning nested negation expressions into comments by @GGG-KILLER.

### Removed
- Removed `LanguageNames.{CSharp,FSharp,VisualBasic}` by @GGG-KILLER in https://github.com/LorettaDevs/Loretta/pull/115.

### Changed
- The `Loretta.CodeAnalysis.FileLinePositionSpan` has been made into `readonly struct`s by @GGG-KILLER in https://github.com/LorettaDevs/Loretta/pull/115.

## v0.2.11
### Fixed
- Fixed `ContainedScopes` not being populated.
- Fixed NormalizeWhitespace not inserting spaces between expression list arguments.

### Removed
- .NET Core 3.1 support has been removed as .NET Core 3.1 has officially hit EOL.

## v0.2.10
### Added
- Added the following new `SyntaxFactory` overloads to partially restore compatibility with pre-typed-lua era:
	- `SyntaxFactory.AnonymousFunctionExpression(ParameterListSyntax parameters, StatementListSyntax body)`;
	- `SyntaxFactory.FunctionDeclarationStatement(FunctionNameSyntax name, ParameterListSyntax parameters, StatementListSyntax body)`;
	- `SyntaxFactory.LocalDeclarationName(IdentifierNameSyntax identifierName, VariableAttributeSyntax? attribute)`;
	- `SyntaxFactory.LocalDeclarationName(string name, VariableAttributeSyntax? attribute)`;
	- `SyntaxFactory.LocalFunctionDeclarationStatement(SyntaxToken localKeyword, SyntaxToken functionKeyword, IdentifierNameSyntax name, ParameterListSyntax parameters, StatementListSyntax body, SyntaxToken endKeyword, SyntaxToken semicolonToken)`;
	- `SyntaxFactory.NumericForStatement(IdentifierNameSyntax identifier, ExpressionSyntax initialValue, ExpressionSyntax finalValue, ExpressionSyntax? stepValue, StatementListSyntax body)`;
	- `SyntaxFactory.NumericForStatement(SyntaxToken forKeyword, IdentifierNameSyntax identifier, SyntaxToken equalsToken, ExpressionSyntax initialValue, SyntaxToken finalValueCommaToken, ExpressionSyntax finalValue, SyntaxToken stepValueCommaToken, ExpressionSyntax? stepValue, SyntaxToken doKeyword, StatementListSyntax body, SyntaxToken endKeyword, SyntaxToken semicolonToken)`;
	- `SyntaxFactory.NumericForStatement(string identifier, ExpressionSyntax initialValue, ExpressionSyntax finalValue, ExpressionSyntax? stepValue, StatementListSyntax body)`;
	- `SyntaxFactory.NumericForStatement(string identifier, ExpressionSyntax initialValue, ExpressionSyntax finalValue)`.
- Added support for typed Lua in `.NormalizeWhitespace`;
- Added support for local variable attributes in `.NormalizeWhitespace`;
- Added the new `SyntaxFactory.MethodFunctionName(FunctionNameSyntax baseName, string name)` overload.
- Added support for Lua 5.1's long string nesting error.

### Changed
- **[Breaking]** The following SyntaxFactory overloads have been changed:
	- `SyntaxFactory.NumericForStatement(TypedIdentifierNameSyntax identifier, ExpressionSyntax initialValue, ExpressionSyntax finalValue)` has been changed to always require the loop's body (into `SyntaxFactory.NumericForStatement(TypedIdentifierNameSyntax identifier, ExpressionSyntax initialValue, ExpressionSyntax finalValue, StatementListSyntax body)`);
	- `SyntaxFactory.NumericForStatement(string identifier, ExpressionSyntax initialValue, ExpressionSyntax finalValue)` has been changed to always require the loop's body (into `SyntaxFactory.NumericForStatement(string identifier, ExpressionSyntax initialValue, ExpressionSyntax finalValue, StatementListSyntax body)`);
	- `SyntaxFactory.IfStatement(ExpressionSyntax condition)` has been changed to always require the clause's body (into `SyntaxFactory.IfStatement(ExpressionSyntax condition, StatementListSyntax body)`);
	- `SyntaxFactory.ElseClause(StatementListSyntax? elseBody = default)` has been changed for the body to always be required (into `SyntaxFactory.ElseClause(StatementListSyntax elseBody)`);
	- `SyntaxFactory.LocalFunctionDeclarationStatement(IdentifierNameSyntax name)` has been changed to always require the parameter list and function's body (into `SyntaxFactory.LocalFunctionDeclarationStatement(IdentifierNameSyntax name, ParameterListSyntax parameters, StatementListSyntax body)`);
	- `SyntaxFactory.LocalFunctionDeclarationStatement(string name)` has been changed to always require the parameter list and function's body (into `LocalFunctionDeclarationStatement(string name, ParameterListSyntax parameters, StatementListSyntax body)`);
	- `SyntaxFactory.FunctionDeclarationStatement(FunctionNameSyntax name)` has been changed to always require the parameter list and the function's body (into `SyntaxFactory.FunctionDeclarationStatement(FunctionNameSyntax name, ParameterListSyntax parameters, StatementListSyntax body)`);
	- `SyntaxFactory.DoStatement(StatementListSyntax? body = default)` has been changed to make the body always required (into `SyntaxFactory.DoStatement(StatementListSyntax body)`);
	- `SyntaxFactory.CompilationUnit(StatementListSyntax? statements = default)` has been changed to make the body always required (into `SyntaxFactory.CompilationUnit(StatementListSyntax statements)`).
- Changed the existing handling of certain constructs in `.NormalizeWhitespace`:
	- Tables are now kept in a single line unless one of its fields spans more than one line;
	- Spaces are not added to a table if it has no elements;
	- No unnecessary line breaks are added after statements anymore.
- **[Breaking]** `SyntaxFactory.IdentifierName` will now throw an exception if the provided token is not an identifier.
- **[Breaking]** The following have been changed as a result of the Lua 5.1 long string nesting error:
	- `LuaSyntaxOptions`'s constructor and `.With` method have been changed to accept the new `AcceptNestingOfLongStrings` option.
	- An error is now generated when long strings are nested and `LuaSyntaxOptions.AcceptNestingOfLongStrings` is `false`.

### Removed
- **[Breaking]** The following have been removed:
	- `SyntaxFactory.AnonymousFunctionExpression()` (as the parameter list and function's body should always be required);
	- `SyntaxFactory.GenericForStatement(SeparatedSyntaxList<TypedIdentifierNameSyntax> identifiers, SeparatedSyntaxList<ExpressionSyntax> expressions)` (as the loop's body should always be required);
	- `SyntaxFactory.WhileStatement(ExpressionSyntax condition)` (as the loop's body should always be required);
	- `SyntaxFactory.RepeatUntilStatement(ExpressionSyntax condition)` (as the loop's body should always be required);
	- `SyntaxFactory.ElseIfClause(ExpressionSyntax condition)` (as the `elseif` clause should always be required);
- **[Breaking]** The `SyntaxKind.StartEqualsToken` has been removed as it has been obsolete for a while.

### Fixed
- Fixed a bug where the leading new line was included for long strings;
- Fixed a bug where `ObjectDisplay.FormatLiteral(string value, ObjectDisplayOptions options)` would escape the space character;
- Fixed a bug where `ObjectDisplay.FormatLiteral(string value, ObjectDisplayOptions options)` would not generate correct verbatim/long strings;
- Fixed parsing of anonymous functions that had type parameters;
- Fixed union and intersection types having errors for bitwise operators generated in them.

## v0.2.9
### Added
- We've added support for LuaJIT imaginary numbers which also resulted in the following being added:
	- `SyntaxFactory.Literal(Complex value)`;
	- `SyntaxFactory.Literal(string text, Complex value)`;
	- `SyntaxFactory.Literal(SyntaxTriviaList leading, string text, Complex value, SyntaxTriviaList trailing)`;
	- `ObjectDisplay.FormatLiteral(Complex value, ObjectDisplayOptions options, CultureInfo? cultureInfo = null)`.

### Fixed
- [Breaking] We've reviewed the existing `LuaSyntaxOptions` presets and the following were fixed:
	- Changed the Lua 5.1 preset to accept shebangs;
	- Changed the Lua 5.1 preset to not accept `if` expressions;
	- Changed the Lua 5.2 preset to accept shebangs;
	- Changed the Lua 5.2 preset to not accept `if` expressions;
	- Changed the Lua 5.3 preset to accept shebangs;
	- Changed the Lua 5.3 preset to not accept `if` expressions;
	- Changed the Lua 5.3 preset to accept floor division;
	- Changed the Lua 5.4 preset to accept shebangs;
	- Changed the Lua 5.4 preset to not accept `if` expressions;
	- Changed the Lua 5.4 preset to accept floor division;
	- Changed the LuaJIT 2.0 preset to not accept empty statements;
	- Changed the LuaJIT 2.0 preset to accept shebangs;
	- Changed the LuaJIT 2.1 preset to not accept empty statements;
	- Changed the LuaJIT 2.1 preset to accept shebangs;
	- Changed the FiveM preset to accept shebangs;
	- Changed the FiveM preset to not accept `if` expressions;
	- Changed the FiveM preset to accept floor division;
	- Changed the Luau preset to not accept hex float literals;
	- Changed the Luau preset to accept shebangs;
	- Changed the Luau preset to not accept bitwise operators.
- Fixed `ObjectDisplay.FormatPrimitive(object? obj, ObjectDisplayOptions options)` returning `null` for `long`s and `ulong`s.
- Fixed LuaJIT suffixes not being handled in a case-insensitive manner.
- Fixed empty type argument lists not being accepted.

## v0.2.9-beta.5
### Added
- Added `LuaSyntaxOptions.AcceptLuaJITNumberSuffixes`.
- Added `SyntaxFactory.Literal(ulong value)`.
- Added `SyntaxFactory.Literal(string text, ulong value)`.
- Added support for `ULL` and `LL` suffixes from LuaJIT.

### Fixed
- Fixed `ObjectDisplay.FormatLiteral(long value, ObjectDisplayOptions options, CultureInfo? cultureInfo = null)` returning numbers in hex for longs.

### Changed
- Changed `LuaSyntaxOptions.ToString` to return `AcceptLuaJITNumberSuffixes`

## v0.2.9-beta.4
### Added
- Added `LuaSyntaxOptions.Luau`.

### Changed
- Changed `LuaSyntaxOptions.ToString` to return `"Luau"` for the luau preset.

### Fixed
- Fixed a bug with incremental parsing where a `System.InvalidCastException` was thrown from `Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax.Blender.Reader.CanReuse`.

## v0.2.9-beta.3
### Added
- Typed Lua Support.
- The following were added as a part of implementing typed-luau:
	- `Loretta.CodeAnalysis.Lua.Syntax.TypeSyntax`;
	- `Loretta.CodeAnalysis.Lua.Syntax.TypeBindingSyntax`;
	- `Loretta.CodeAnalysis.Lua.Syntax.NilableTypeSyntax`;
	- `Loretta.CodeAnalysis.Lua.Syntax.ParenthesizedTypeSyntax`;
	- `Loretta.CodeAnalysis.Lua.Syntax.TypePackSyntax`;
	- `Loretta.CodeAnalysis.Lua.Syntax.FunctionTypeSyntax`;
	- `Loretta.CodeAnalysis.Lua.Syntax.TableBasedTypeSyntax`;
	- `Loretta.CodeAnalysis.Lua.Syntax.TableTypeElementSyntax`;
	- `Loretta.CodeAnalysis.Lua.Syntax.TableTypePropertySyntax`;
	- `Loretta.CodeAnalysis.Lua.Syntax.TableTypeSyntax`;
	- `Loretta.CodeAnalysis.Lua.Syntax.TypeCastExpressionSyntax`;
	- `Loretta.CodeAnalysis.Lua.Syntax.UnionTypeSyntax`;
	- `Loretta.CodeAnalysis.Lua.Syntax.IntersectionTypeSyntax`;
	- `Loretta.CodeAnalysis.Lua.Syntax.EqualsTypeSyntax`;
	- `Loretta.CodeAnalysis.Lua.Syntax.TypeParameterSyntax`;
	- `Loretta.CodeAnalysis.Lua.Syntax.TypeParameterListSyntax`;
	- `Loretta.CodeAnalysis.Lua.Syntax.TypeArgumentListSyntax`;
	- `Loretta.CodeAnalysis.Lua.Syntax.TypeNameSyntax`;
	- `Loretta.CodeAnalysis.Lua.Syntax.CompositeTypeNameSyntax`;
	- `Loretta.CodeAnalysis.Lua.Syntax.LiteralTypeSyntax`;
	- `Loretta.CodeAnalysis.Lua.Syntax.TypeofTypeSyntax`;
	- `Loretta.CodeAnalysis.Lua.Syntax.VariadicTypePackSyntax`;
	- `Loretta.CodeAnalysis.Lua.Syntax.GenericTypePackSyntax`;
	- `Loretta.CodeAnalysis.Lua.Syntax.TypedIdentifierNameSyntax`;
	- `LuaSyntaxOptions.AcceptTypedLua`.

### Changed
- [Breaking] The following were changed as a result of implementing typed Lua syntax:
	- `LuaSyntaxOptions`'s constructor and `With` were changed to accept a bool for `AcceptTypedLua`;
	- The following were changed to accept a `TypeBindingSyntax`:
		- `AnonymousFunctionExpressionSyntax.Update`;
		- `FunctionDeclarationStatementSyntax.Update`;
		- `LocalDeclarationNameSyntax.Update`;
		- `LocalFunctionDeclarationStatementSyntax.Update`;
		- `NamedParameterSyntax.Update`;
		- `VarArgParameterSyntax.Update`;
		- `SyntaxFactory.AnonymousFunctionExpression`;
		- `SyntaxFactory.FunctionDeclarationStatement`;
		- `SyntaxFactory.LocalDeclarationName`;
		- `SyntaxFactory.LocalFunctionDeclarationStatement`;
		- `SyntaxFactory.VarArgParameter`.
	- The following were changed to accept a `TypeParameterListSyntax`:
		- `AnonymousFunctionExpressionSyntax.Update`;
		- `FunctionDeclarationStatementSyntax.Update`;
		- `LocalFunctionDeclarationStatementSyntax.Update`;
		- `SyntaxFactory.AnonymousFunctionExpression`;
		- `SyntaxFactory.FunctionDeclarationStatement`;
		- `SyntaxFactory.LocalFunctionDeclarationStatement`.
	- The following were changed to accept (or be) a `TypedIdentifierNameSyntax`:
		- `GenericForStatementSyntax.AddIdentifiers`;
		- `GenericForStatementSyntax.Identifiers`;
		- `GenericForStatementSyntax.Update`;
		- `GenericForStatementSyntax.WithIdentifiers`;
		- `NumericForStatementSyntax.Identifier`;
		- `NumericForStatementSyntax.Update`;
		- `NumericForStatementSyntax.WithIdentifier`;
		- `SyntaxFactory.GenericForStatement`;
		- `SyntaxFactory.NumericForStatement`.

## v0.2.9-beta.2
### Added
- Added `ConstantFoldingOptions` with the option to enable number extraction from strings.

### Changed
- [Breaking] Changed `ConstantFold` extension method to accept a `ConstantFoldingOptions`.

### Fixed
- Fixed `SyntaxFacts.GetConstantValue` not returning the correct value for tokens.
- Fixed `SyntaxFacts.GetConstantValue` not returning `None` for values outside the valid range.
- Fixed the constant folder turning `/` into integer division when both sides are integers.
- Fixed the constant folder storing the length as an integer token.
- Fixed `LuaSyntaxNode.GetStructure` throwing an exception because we *do* have structured trivia.

## v0.2.9-beta.1
### Changed
- Optimized the lexing process with a reduction of 80% in lexing time and 76% in parsing time.

## v0.2.8
### Added
- Added support for string length to the constant folder.
- The following were added as a part of implementing local variable attributes:
	- `Loretta.CodeAnalysis.Lua.Syntax.LocalDeclarationNameSyntax`;
	- `SyntaxKind.LocalDeclarationName`;
	- `SyntaxFactory.LocalDeclarationName`;
	- `LuaSyntaxVisitor.VisitLocalDeclarationName`;
	- `LuaSyntaxVisitor<TResult>.VisitLocalDeclarationName`;
	- `LuaSyntaxRewriter.VisitLocalDeclarationName`;
	- `Loretta.CodeAnalysis.Lua.Syntax.VariableAttributeSyntax`;
	- `SyntaxKind.VariableAttribute`;
	- `SyntaxFactory.VariableAttribute`;
	- `LuaSyntaxVisitor.VisitVariableAttribute`;
	- `LuaSyntaxVisitor<TResult>.VisitVariableAttribute`;
	- `LuaSyntaxRewriter.VisitVariableAttribute`;
	- `LuaSyntaxOptions.AcceptLocalVariableAttributes`;
	- `LuaSyntaxOptions.Lua54` preset.
- Added `SyntaxFactory.HashString` for creating FiveM hash string literal tokens.
- The following were added as part of implementing integers:
	- `IntegerFormats`;
	- `LuaSyntaxOptions.AllWithIntegers`;
	- `LuaSyntaxOptions.BinaryIntegerFormat`;
	- `LuaSyntaxOptions.OctalIntegerFormat`;
	- `LuaSyntaxOptions.DecimalIntegerFormat`;
	- `LuaSyntaxOptions.HexIntegerFormat`;
	- `ObjectDisplay.FormatLiteral(long value, ObjectDisplayOptions options, CultureInfo? cultureInfo = null)`;
	- **IMPORTANT:** `SyntaxFactory.Literal(long value)`;
	- **IMPORTANT:** `SyntaxFactory.Literal(string text, long value)`;
	- **IMPORTANT:** `SyntaxFactory.Literal(SyntaxTriviaList leading, string text, long value, SyntaxTriviaList trailing)`.

### Changed
- Constant folder now attempts to preserve trivia around nodes that were folded.
- [Breaking] The following were changed as a result of implementing local variable attributes:
	- `LocalVariableDeclarationStatementSyntax.Names` now returns a `SeparatedSyntaxList<LocalDeclarationNameSyntax>` instead of `SeparatedSyntaxList<IdentifierNameSyntax>`;
	- `LocalVariableDeclarationStatementSyntax.Update` now receives a `SeparatedSyntaxList<LocalDeclarationNameSyntax>` instead of `SeparatedSyntaxList<IdentifierNameSyntax>`;
	- `LocalVariableDeclarationStatementSyntax.AddNames` now receives a `params LocalDeclarationNameSyntax[]` instead of `params IdentifierNameSyntax[]`;
	- `LocalVariableDeclarationStatementSyntax.WithNames` now receives a `SeparatedSyntaxList<LocalDeclarationNameSyntax>` instead of `SeparatedSyntaxList<IdentifierNameSyntax>`;
	- `SyntaxFactory.LocalVariableDeclarationStatement` overloads now receive a `SeparatedSyntaxList<LocalDeclarationNameSyntax>` instead of `SeparatedSyntaxList<IdentifierNameSyntax>`;
	- `LuaSyntaxOptions` constructor and `With` method now accept an `acceptLocalVariableAttributes` parameter.
- [Breaking] The following were changed as a result of implementing integers:
	- The Lua 5.3 preset was changed to accept integers;
	- **IMPORTANT:** `SyntaxToken.Value` now can be a `long` when any of the `*IntegerFormat` settings are set to `IntegerFormats.Int64`;
	- `LuaSyntaxOptions` constructor and `With` method now accept 4 `IntegerFormats` for each number format (binary, octal, decimal and hexadecimal);
	- The constant folder was modified to support integers.

## v0.2.7-beta.13
### Fixed
- Fixed `Minify` double-freeing a slot.

## v0.2.7-beta.12
### Fixed
- Fixed `Minify` renaming variables that were being used incorrectly.
- Fixed `Minify` not adding a leading `_` for certain variable names.
- Fixed `Minify` adding prefixes for variable names that were already renamed.

### Changed
- [Breaking] The following were changed as a result of the move to `EqualsValuesClauseSyntax`:
	- Replaced `EqualsToken` and `Values` in `AssignmentStatementSyntax` by `EqualsValues`;
	- Replaced `EqualsToken` and `Values` in `LocalVariableDeclarationStatementSyntax` by `EqualsValues`.
- [Breaking] `SyntaxFactory` methods will now throw exceptions if the lists provided to them do not have the minimum amount of items required.
- [Breaking] `NamingStrategy` was changed to receive a list of `IScope`s instead of a single one.
  The received scopes are the scopes where the variable is accessed in.
- [Breaking] The following strategies were changed as a result of the above:
	- `NamingStrategies.Alphabetic`;
	- `NamingStrategies.Numeric`;
	- `NamingStrategies.ZeroWidth`.

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
- [Breaking] The following were removed as a result of the `MinCount` fix for `Syntax.xml`:
	- `SyntaxFactory.GenericForStatement(StatementListSyntax? body = null)`.

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
	- `AssignmentStatementSyntax.WithEqualsValues(EqualsValuesClauseSyntax equalsValues)`;
	- `LocalVariableDeclarationStatementSyntax.EqualsValues`;
	- `LocalVariableDeclarationStatementSyntax.Update(SyntaxToken localKeyword, SeparatedSyntaxList<IdentifierNameSyntax> names, EqualsValuesClauseSyntax? equalsValues, SyntaxToken semicolonToken)`;
	- `LocalVariableDeclarationStatementSyntax.WithEqualsValues(Loretta.CodeAnalysis.Lua.Syntax.EqualsValuesClauseSyntax? equalsValues)`;
	- `SyntaxFactory.AssignmentStatement(SeparatedSyntaxList<PrefixExpressionSyntax> variables, EqualsValuesClauseSyntax equalsValues, SyntaxToken semicolonToken)`;
	- `SyntaxFactory.AssignmentStatement(SeparatedSyntaxList<PrefixExpressionSyntax> variables, EqualsValuesClauseSyntax equalsValues)`;
	- `SyntaxFactory.EqualsValuesClause(SyntaxToken equalsToken, SeparatedSyntaxList<ExpressionSyntax> values)`;
	- `SyntaxFactory.EqualsValuesClause(SeparatedSyntaxList<ExpressionSyntax> values)`;
	- `SyntaxFactory.LocalVariableDeclarationStatement(SyntaxToken localKeyword, SeparatedSyntaxList<IdentifierNameSyntax> names, EqualsValuesClauseSyntax? equalsValues, SyntaxToken semicolonToken)`;
	- `SyntaxFactory.LocalVariableDeclarationStatement(SeparatedSyntaxList<IdentifierNameSyntax> names, EqualsValuesClauseSyntax? equalsValues)`;
	- `SyntaxFactory.LocalVariableDeclarationStatement(SeparatedSyntaxList<IdentifierNameSyntax> names)`;
	- `LuaSyntaxVisitor.VisitEqualsValuesClause(EqualsValuesClauseSyntax node)`;
	- `LuaSyntaxVisitor<TResult>.VisitEqualsValuesClause(EqualsValuesClauseSyntax node)`;
	- `LuaSyntaxWalker.VisitEqualsValuesClause(EqualsValuesClauseSyntax node)`;
	- `LuaSyntaxRewriter.VisitEqualsValuesClause(EqualsValuesClauseSyntax node)`.
- Added `SyntaxFacts.GetKeywordKind(ReadOnlySpan<char> span)`.
- The following were added as a result of the `MinCount` fix for `Syntax.xml`:
	- `SyntaxFactory.GenericForStatement(SeparatedSyntaxList<IdentifierNameSyntax> identifiers, SeparatedSyntaxList<ExpressionSyntax> expressions)`.
- Added `MinifyingUtils.CanRename` to check if a variable will be renamed by the minifier.
- Added `MinifyingUtils.GetUnavailableNames` to get the list of variable names that can't be used for a given scope or list of scopes.
  This is particularly useful for implementing your own `NamingStrategy`.

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
- `ObjectDisplay.FormatLiteral` was not emitting Unicode escapes correctly.

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
