## Generating Tests

Run the following to re-generate the tests:

```console
$ dotnet run --project src/Tools/Generators/SyntaxXml/Loretta.Generators.SyntaxXml.csproj --framework net8.0 -- src/Compilers/Lua/Portable/Syntax/Syntax.xml src/Compilers/Lua/Test/Portable/Generated/Syntax.Test.xml.Generated.cs /test
...
$ dotnet run --project src/Tools/Generators/SyntaxXml/Loretta.Generators.SyntaxXml.csproj --framework net8.0 -- src/Compilers/Lua/Portab
le/Syntax/Syntax.xml src/Compilers/Lua/Portable/Generated/ /grammar
...
```