using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Loretta.LanguageServer.Utils
{
    internal static class DocumentSelectorFactory
    {
        public static DocumentSelector Create ( ) =>
            DocumentSelector.ForLanguage ( LanguageServerConstants.LanguageId );
    }
}
