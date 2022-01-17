// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Loretta.Generators.SyntaxXml
{
    internal abstract class AbstractFileWriter
    {
        private readonly TextWriter _writer;
        private readonly Tree _tree;
        private readonly IDictionary<string, string> _parentMap;
        private readonly ILookup<string, string> _childMap;

        private readonly IDictionary<string, Node> _nodeMap;
        private readonly IDictionary<string, TreeType> _typeMap;
        private const int INDENT_SIZE = 4;
        private int _indentLevel;
        private bool _needIndent = true;

        protected AbstractFileWriter(TextWriter writer, Tree tree, CancellationToken cancellationToken)
        {
            _writer = writer;
            _tree = tree;
            _nodeMap = tree.Types.OfType<Node>().ToDictionary(getName);
            _typeMap = tree.Types.ToDictionary(getName);
            _parentMap = tree.Types.ToDictionary(getName, n => n.Base);
            _parentMap.Add(tree.Root, null);
            _childMap = tree.Types.ToLookup(n => n.Base, getName);

            CancellationToken = cancellationToken;

            static string getName(TreeType type)
            {
                var name = type.Name;
                if (string.IsNullOrWhiteSpace(name))
                    throw new Exception($"Tree type {{ Name = {type.Name}, Base = {type.Base}, TypeComment = {string.Join("\n", type.TypeComment.Body.Select(x => x.OuterXml))} }} has a null or whitespace name.");
                return name;
            }
        }

        protected IDictionary<string, string> ParentMap => _parentMap;
        protected ILookup<string, string> ChildMap => _childMap;
        protected Tree Tree => _tree;
        protected CancellationToken CancellationToken { get; }

        #region Output helpers

        protected void Indent() => _indentLevel++;

        protected void Unindent()
        {
            if (_indentLevel <= 0)
            {
                throw new InvalidOperationException("Cannot unindent from base level");
            }
            _indentLevel--;
        }

        protected void Write(string msg)
        {
            WriteIndentIfNeeded();
            _writer.Write(msg);
        }

        protected void WriteLine() => WriteLine("");

        protected void WriteLine(string msg)
        {
            CancellationToken.ThrowIfCancellationRequested();

            if (msg != "")
            {
                WriteIndentIfNeeded();
            }

            _writer.WriteLine(msg);
            _needIndent = true; //need an indent after each line break
        }

        protected void WriteLineWithoutIndent(string msg)
        {
            _writer.WriteLine(msg);
            _needIndent = true; //need an indent after each line break
        }

        private void WriteIndentIfNeeded()
        {
            if (_needIndent)
            {
                _writer.Write(new string(' ', _indentLevel * INDENT_SIZE));
                _needIndent = false;
            }
        }

        /// <summary>
        /// Joins all the values together in <paramref name="values"/> into one string with each
        /// value separated by a comma.  Values can be either <see cref="string"/>s or <see
        /// cref="IEnumerable{T}"/>s of <see cref="string"/>.  All of these are flattened into a
        /// single sequence that is joined. Empty strings are ignored.
        /// </summary>
        protected string CommaJoin(params object[] values)
            => Join(", ", values);

        protected string Join(string separator, params object[] values)
            => string.Join(separator, values.SelectMany(v => (v switch
            {
                string s => new[] { s },
                IEnumerable<string> ss => ss,
                _ => throw new InvalidOperationException("Join must be passed strings or collections of strings")
            }).Where(s => s != "")));

        protected void OpenBlock()
        {
            WriteLine("{");
            Indent();
        }

        protected void CloseBlock(string extra = "")
        {
            Unindent();
            WriteLine("}" + extra);
        }

        #endregion Output helpers

        #region Node helpers

        protected static string OverrideOrNewModifier(Field field) => IsOverride(field) ? "override " : IsNew(field) ? "new " : "";

        protected static bool CanBeField(Field field) => field.Type != "SyntaxToken" && !IsAnyList(field.Type) && !IsOverride(field) && !IsNew(field);

        protected static string GetFieldType(Field field, bool green)
        {
            // Fields in red trees are lazily initialized, with null as the uninitialized value
            return getNullableAwareType(field.Type, optionalOrLazy: IsOptional(field) || !green, green);

            static string getNullableAwareType(string fieldType, bool optionalOrLazy, bool green)
            {
                if (IsAnyList(fieldType))
                {
                    if (optionalOrLazy)
                        return green ? "GreenNode?" : "SyntaxNode?";
                    else
                        return green ? "GreenNode?" : "SyntaxNode";
                }

                switch (fieldType)
                {
                    case var _ when !optionalOrLazy:
                        return fieldType;

                    case "bool":
                    case "SyntaxToken" when !green:
                        return fieldType;

                    default:
                        return fieldType + "?";
                }
            }
        }

        protected bool IsDerivedOrListOfDerived(string baseType, string derivedType)
        {
            return IsDerivedType(baseType, derivedType)
                || ((IsNodeList(derivedType) || IsSeparatedNodeList(derivedType))
                    && IsDerivedType(baseType, GetElementType(derivedType)));
        }

        protected static bool IsSeparatedNodeList(string typeName) => typeName.StartsWith("SeparatedSyntaxList<", StringComparison.Ordinal);

        protected static bool IsNodeList(string typeName) => typeName.StartsWith("SyntaxList<", StringComparison.Ordinal);

        public static bool IsAnyNodeList(string typeName) => IsNodeList(typeName) || IsSeparatedNodeList(typeName);

        protected bool IsNodeOrNodeList(string typeName) => IsNode(typeName) || IsNodeList(typeName) || IsSeparatedNodeList(typeName) || typeName == "SyntaxNodeOrTokenList";

        protected static string GetElementType(string typeName)
        {
            if (!typeName.Contains("<"))
                return string.Empty;
            var iStart = typeName.IndexOf('<');
            var iEnd = typeName.IndexOf('>', iStart + 1);
            if (iEnd < iStart)
                return string.Empty;
            var sub = typeName.Substring(iStart + 1, iEnd - iStart - 1);
            return sub;
        }

        protected static bool IsAnyList(string typeName) => IsNodeList(typeName) || IsSeparatedNodeList(typeName) || typeName == "SyntaxNodeOrTokenList";

        protected bool IsDerivedType(string typeName, string derivedTypeName)
        {
            if (typeName == derivedTypeName)
                return true;
            if (derivedTypeName != null && _parentMap.TryGetValue(derivedTypeName, out var baseType))
            {
                return IsDerivedType(typeName, baseType);
            }
            return false;
        }

        protected static bool IsRoot(Node n) => n.Root != null && string.Compare(n.Root, "true", true) == 0;

        protected bool IsNode(string typeName) => _parentMap.ContainsKey(typeName);

        protected Node GetNode(string typeName)
            => _nodeMap.TryGetValue(typeName, out var node) ? node : null;

        protected TreeType GetTreeType(string typeName)
            => _typeMap.TryGetValue(typeName, out var node) ? node : null;

        protected static bool IsOptional(Field f)
            => f.Optional;

        protected static bool IsOverride(Field f)
            => f.Override;

        protected static bool IsNew(Field f)
            => f.New;

        protected static bool HasErrors(Node n) => n.Errors == null || string.Compare(n.Errors, "true", true) == 0;

        protected static string CamelCase(string name)
        {
            if (char.IsUpper(name[0]))
            {
                name = char.ToLowerInvariant(name[0]) + name.Substring(1);
            }
            return FixKeyword(name);
        }

        protected static string FixKeyword(string name)
        {
            if (IsKeyword(name))
            {
                return "@" + name;
            }
            return name;
        }

        protected static string StripPost(string name, string post)
        {
            return name.EndsWith(post, StringComparison.Ordinal)
                ? name.Substring(0, name.Length - post.Length)
                : name;
        }

        protected static bool IsKeyword(string name)
        {
            return name switch
            {
                "bool"
                or "byte"
                or "sbyte"
                or "short"
                or "ushort"
                or "int"
                or "uint"
                or "long"
                or "ulong"
                or "double"
                or "float"
                or "decimal"
                or "string"
                or "char"
                or "object"
                or "typeof"
                or "sizeof"
                or "null"
                or "true"
                or "false"
                or "if"
                or "else"
                or "while"
                or "for"
                or "foreach"
                or "do"
                or "switch"
                or "case"
                or "default"
                or "lock"
                or "try"
                or "throw"
                or "catch"
                or "finally"
                or "goto"
                or "break"
                or "continue"
                or "return"
                or "public"
                or "private"
                or "internal"
                or "protected"
                or "static"
                or "readonly"
                or "sealed"
                or "const"
                or "new"
                or "override"
                or "abstract"
                or "virtual"
                or "partial"
                or "ref"
                or "out"
                or "in"
                or "where"
                or "params"
                or "this"
                or "base"
                or "namespace"
                or "using"
                or "class"
                or "struct"
                or "interface"
                or "delegate"
                or "checked"
                or "get"
                or "set"
                or "add"
                or "remove"
                or "operator"
                or "implicit"
                or "explicit"
                or "fixed"
                or "extern"
                or "event"
                or "enum"
                or "unsafe" => true,
                _ => false,
            };
        }

        #endregion Node helpers
    }
}
