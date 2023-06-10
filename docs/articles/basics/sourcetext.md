# Source Text
The [SourceText](xref:Loretta.CodeAnalysis.Text.SourceText) is probably the first piece of Loretta anyone who starts using the library has to deal with.

The primary duty of a SourceText instance is to hold a given source code file's contents in memory and provide access to them.
It also provides the following features:
- Splits the file's contents into lines;
- Enables a checksum/hash to be created from the contents of the text;
- Detecting changes between 2 SourceText instances;
- Replacing parts of the text;
- Applying patches ([`TextChange`](xref:Loretta.CodeAnalysis.Text.TextChange)s) to create a new SourceText;
- Writing the contents of the SourceText to an output TextWriter.

## Creating a SourceText
To create a SourceText instance, one of the [`SourceText.From`](xref:Loretta.CodeAnalysis.Text.SourceText.From*) overloads can be used.
These will load the contents into memory and allow the source text information such as lines and length to be read.

Example:

```cs
using Loretta.CodeAnaylsis.Text;

SourceText text;
using (var stream = File.OpenRead("file.lua"))
    text = SourceText.From(stream);

Console.WriteLine(text.Lines.Count); // Number of lines in the file.
```

## Text Navigation
After loading the source code into memory, SourceText allows you to enumerate the lines the file contains.

By using the [`SourceText.Lines`](xref:Loretta.CodeAnalysis.Text.SourceText.Lines) property, a [`TextLineCollection`](xref:Loretta.CodeAnalysis.Text.TextLineCollection) can be obtained which can be used to enumerate the lines that the file contains as well as find the line a position belongs to using [`TextLineCollection.GetLineFromPosition`](xref:Loretta.CodeAnalysis.Text.TextLineCollection.GetLineFromPosition*).

## Checksum/Hashing
SourceText allows one to calculate the checksum/hash for a given instance by providing the proper [`SourceHashAlgorithm`](xref:Loretta.CodeAnalysis.Text.SourceHashAlgorithm) to the `SourceText.From` overload and then calling the [`SourceText.GetChecksum`](xref:Loretta.CodeAnalysis.Text.SourceText.GetChecksum) method.

The method will return an `ImmutableArray<byte>` which contains the hashed bytes of the file's contents. This can be turned into a hexadecimal string to show the user or outputted to some place for identification and validation afterwards.

## Change Detection
If you wish to take advantage of incremental parsing or want to find out which parts of a file have been changed, SourceText can help with that by allowing you to find out which sections of a file have changed.

To get these changes, the [`SourceText.GetTextChanges(SourceText)`](xref:Loretta.CodeAnalysis.Text.SourceText.GetTextChanges(Loretta.CodeAnalysis.Text.SourceText)) method can be used which will then return a list of [`TextChange`s](xref:Loretta.CodeAnalysis.Text.TextChange).

A `TextChange` is basically composed of two values:
1. `Span`: The original location the text was in, represented by a [`TextSpan`](xref:Loretta.CodeAnalysis.Text.TextSpan);
2. `NewText`: The text that the "old" text has gotten replaced with.