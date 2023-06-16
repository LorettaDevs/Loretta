# Source Locations

Locations of things in source can be represented as 3 different types:
- [`TextSpan`](xref:Loretta.CodeAnalysis.Text.TextSpan);
- [`LinePosition`](xref:Loretta.CodeAnalysis.Text.LinePosition);
- and [`LinePositionSpan`](xref:Loretta.CodeAnalysis.Text.LinePositionSpan).

## TextSpan
A @Loretta.CodeAnalysis.Text.TextSpan basically represents a range of positions (offset from the start of the @Loretta.CodeAnalysis.Text.SourceText).

So in the following code:
```lua
print("Hello there")
```

The location of the word `there` could be represented with a @Loretta.CodeAnalysis.Text.TextSpan created with `new TextSpan(13, 5)` or `TextSpan.FromBounds(13, 18)`.

A @Loretta.CodeAnalysis.Text.TextSpan also comes with a few builtin methods that allow comparison between instances of it. A few examples are:
- @Loretta.CodeAnalysis.Text.TextSpan.OverlapsWith(Loretta.CodeAnalysis.Text.TextSpan): Checks if a @Loretta.CodeAnalysis.Text.TextSpan overlaps with another;
- @Loretta.CodeAnalysis.Text.TextSpan.IntersectsWith(Loretta.CodeAnalysis.Text.TextSpan): Checks if a @Loretta.CodeAnalysis.Text.TextSpan intersects with another.
- @Loretta.CodeAnalysis.Text.TextSpan.Contains(Loretta.CodeAnalysis.Text.TextSpan): Checks if a @Loretta.CodeAnalysis.Text.TextSpan contains another.

## LinePosition
A @Loretta.CodeAnalysis.Text.LinePosition is the type of representation of a position in a SourceText that is more suitable for humans to use, as it is a combination of line number and column number.

Given that is is simply a representation meant for human consumption, it doesn't have any methods other than comparison and equality.

Obtaining a `LinePosition` is a bit more of an involved process, requiring using @Loretta.CodeAnalysis.Text.TextLineCollection.GetLinePosition(System.Int32) on the @Loretta.CodeAnalysis.Text.SourceText.Lines of a @Loretta.CodeAnalysis.Text.SourceText.

## LinePositionSpan
A @Loretta.CodeAnalysis.Text.LinePositionSpan is the type of representation of a @Loretta.CodeAnalysis.Text.TextSpan that is meant to be used to 