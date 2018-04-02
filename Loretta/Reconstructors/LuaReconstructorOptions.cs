using System;

namespace Loretta.Reconstructors
{
    // WIP: still not used in the lua reconstructor
    public struct LuaReconstructorOptions
    {
        public struct ParenthesisParams
        {
            public Boolean OnAnonymousFunction;
            public Boolean OnNamedFunction;
            public Boolean OnFunctionCall;
            public Boolean OnFunctionWithArguments;
            public Boolean OnFunctionWithoutArguments;
        }

        public Boolean InsertSemicolonsAfterStatements;
        public Boolean UseOriginalWhitespace;
        public Boolean IncludeComments;

        /// <summary>
        /// Will be ignored if
        /// <see cref="UseOriginalWhitespace" /> is true
        /// </summary>
        public ParenthesisParams SpaceBeforeParenthesis;

        /// <summary>
        /// Will be ignored if
        /// <see cref="UseOriginalWhitespace" /> is true
        /// </summary>
        public ParenthesisParams SpaceAfterParenthesis;
    }
}
