## Generating Tests

Run the following to re-generate the tests:
```
dotnet build src/Tools/Loretta.Generators.SyntaxXml/ && dotnet src/Tools/Loretta.Generators.SyntaxXml/bin/Debug/net6.0/Loretta.Generators.SyntaxXml.dll src/Compilers/Lua/Portable/Syntax/Syntax.xml src/Compilers/Lua/Test/Portable/Generated/Syntax.Test.xml.Generated.cs /test
```