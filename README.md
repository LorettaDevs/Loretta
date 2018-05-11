# Loretta
A C# (G)Lua code processor with some extensions.

Honestly, **this is really broken and you shoulndn't use it**, but go ahead if you want. I'll rewrite it some day but depends on my motivation.

## Features:
- Code Analysis (store data on nodes before your folder runs or simply fix things that don't require replacing nodes)
- Code Folding (replace nodes with other nodes to optimize or beautify code)
- Scope-based variable keeping

## Dependencies:
- [VS 2017](https://www.visualstudio.com/vs/) or [VS Code](https://code.visualstudio.com/) and [.NET Core 2.0 SDK](https://www.microsoft.com/net/download/dotnet-core/sdk-2.0.3)
- [GParse](https://github.com/GGG-KILLER/GParse)

## License
[GPL v3.0](/LICENSE).
However most part of the [parser](/Loretta/Parsing/) was taken from a project by [@SwadicalRag](https://github.com/SwadicalRag) made in lua that is not available anymore.
