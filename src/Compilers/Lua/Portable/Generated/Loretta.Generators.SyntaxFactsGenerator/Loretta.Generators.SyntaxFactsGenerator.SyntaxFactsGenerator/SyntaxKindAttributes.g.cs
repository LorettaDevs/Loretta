
using System;

#nullable enable

namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// The extra categories attribute
    /// Can be checked by the Is{CategoryName} methods in SyntaxFacts.
    /// All members of a category can also be retrieved by the Get{CategoryName} methods in SyntaxFacts.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class ExtraCategoriesAttribute : Attribute
    {
        public ExtraCategoriesAttribute(params string[] extraCategories)
        {
            Categories = extraCategories;
        }

        public string[] Categories { get; }
    }

    /// <summary>
    /// Properties associated with the enum value.
    /// Can be retrieved from the Get{Key} methods in SyntaxFacts.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    internal sealed class PropertyAttribute : Attribute
    {
        public PropertyAttribute(string key, object? value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }
        public object? Value { get; }
    }

    /// <summary>
    /// The trivia indicator attribute.
    /// Indicates to the SyntaxFacts Source Generator that this <see cref="SyntaxKind"/> is a trivia's.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class TriviaAttribute : Attribute
    {
    }

    /// <summary>
    /// The token indicator attribute.
    /// Indicates to the SyntaxFacts Source Generator that this <see cref="SyntaxKind"/> is a token's.
    /// May optionally indicate a fixed text for the token.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class TokenAttribute : Attribute
    {
        /// <summary>
        /// The <see cref="SyntaxToken"/>'s fixed text.
        /// </summary>
        public string? Text { get; set; }
    }

    /// <summary>
    /// The keyword indicator attribute.
    /// Indicates to the SyntaxFacts Source Generator that this <see cref="SyntaxKind"/> is a keywords's
    /// and the keyword fixed text.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class KeywordAttribute : Attribute
    {
        public KeywordAttribute(string text)
        {
            Text = text;
        }

        /// <summary>
        /// The keyword's text.
        /// </summary>
        public String Text { get; }
    }

    /// <summary>
    /// The unary operator indicator attribute.
    /// Indicates to the SyntaxFacts Source Generator that this
    /// <see cref="SyntaxKind"/> is an unary operator's with the
    /// provided precedence.
    /// THIS DOES NOT IMPLY THE <see cref="TokenAttribute"/>
    /// ATTRIBUTE.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class UnaryOperatorAttribute : Attribute
    {
        public UnaryOperatorAttribute(int precedence, SyntaxKind expressionKind)
        {
            Precedence = precedence;
        }

        /// <summary>
        /// The unary operator's precedence.
        /// </summary>
        public int Precedence { get; }
    }

    /// <summary>
    /// The binary operator indicator attribute.
    /// Indicates to the SyntaxFacts Source Generator that
    /// this <see cref="SyntaxKind"/> is a binary operator's
    /// with the provided precedence.
    /// THIS DOES NOT IMPLY THE <see cref="TokenAttribute"/>
    /// ATTRIBUTE.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class BinaryOperatorAttribute : Attribute
    {
        public BinaryOperatorAttribute(int precedence, SyntaxKind expressionKind)
        {
            Precedence = precedence;
        }

        /// <summary>
        /// The binary operator's precedence.
        /// </summary>
        public int Precedence { get; }
    }
}