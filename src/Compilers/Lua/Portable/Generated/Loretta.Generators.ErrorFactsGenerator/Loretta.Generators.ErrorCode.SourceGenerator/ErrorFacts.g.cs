namespace Loretta.CodeAnalysis.Lua
{
    internal static partial class ErrorFacts
    {
        public static partial bool IsWarning(ErrorCode code)
        {
            switch(code)
            {
                case ErrorCode.WRN_LineBreakMayAffectErrorReporting:
                    return true;
                default:
                    return false;
            }
        }
        
        public static partial bool IsFatal(ErrorCode code)
        {
            switch(code)
            {
                default:
                    return false;
            }
        }
        
        public static partial bool IsInfo(ErrorCode code)
        {
            switch(code)
            {
                default:
                    return false;
            }
        }
        
        public static partial bool IsHidden(ErrorCode code)
        {
            switch(code)
            {
                default:
                    return false;
            }
        }
    }
}
