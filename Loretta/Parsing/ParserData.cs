using System;
using System.Collections.Generic;

namespace Loretta.Parsing
{
    public static class ParserData
    {
        public static readonly Dictionary<String, Double[]> OpPriorities = new Dictionary<String, Double[]>
        {
            { "+", new[] { 6d, 6 } },
            { "-", new[] { 6d, 6 } },
            { "<<", new[] { 5d, 5 } },
            { ">>", new[] { 5d, 5 } },
            { "|", new[] { 4.5, 4.5 } },
            { "&", new[] { 4.25, 4.25 } },
            { "~", new[] { 4d, 4 } },
            { "%", new[] { 7d, 7 } },
            { "/", new[] { 7d, 7 } },
            { "*", new[] { 7d, 7 } },
            { "^", new[] { 10d, 9 } },
            { "..", new[] { 5d, 4 } },
            { "==", new[] { 3d, 3 } },
            { "<", new[] { 3d, 3 } },
            { "<=", new[] { 3d, 3 } },
            { "~=", new[] { 3d, 3 } },
            { "!=", new[] { 3d, 3 } },
            { ">", new[] { 3d, 3 } },
            { ">=", new[] { 3d, 3 } },
            { "and", new[] { 2d, 2 } },
            { "or", new[] { 1d, 1 } },
            { "&&", new[] { 2d, 2 } },
            { "||", new[] { 1d, 1 } },
        };

        public const Int32 UnaryOpPriority = 8;

        public static readonly String[] UnaryOps = new[]
        {
            "!",
            "-",
            "not",
            "#",
            "~", // binary not
        };

        public static readonly String[] StatListCloseKeywords = new[]
        {
            "end",
            "else",
            "elseif",
            "until",
        };

        public static Boolean Contains<T> ( this T[] arr, T val )
        {
            for ( var i = 0; i < arr.Length; i++ )
                if ( arr[i].Equals ( val ) )
                    return true;
            return false;
        }
    }
}
