using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Loretta.LanguageServer.Utils;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Loretta.LanguageServer.Handlers
{
    public class LuaDocumentHighlightHandler : DocumentHighlightHandlerBase
    {
        public LuaDocumentHighlightHandler ( )
        {
        }

        protected override DocumentHighlightRegistrationOptions CreateRegistrationOptions (
            DocumentHighlightCapability capability,
            ClientCapabilities clientCapabilities )
        {
            return new DocumentHighlightRegistrationOptions
            {
                DocumentSelector = DocumentSelectorFactory.Create ( )
            };
        }

        public override Task<DocumentHighlightContainer?> Handle ( DocumentHighlightParams request, CancellationToken cancellationToken )
        {
        }
    }
}
