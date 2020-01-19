using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace Loretta.LanguageServer
{
    internal class TextDocumentHandler : ITextDocumentSyncHandler
    {
        private readonly ILogger<TextDocumentHandler> logger;

        public TextDocumentChangeRegistrationOptions GetRegistrationOptions ( ) => throw new NotImplementedException ( );

        public TextDocumentAttributes GetTextDocumentAttributes ( Uri uri ) => throw new NotImplementedException ( );

        public Task<Unit> Handle ( DidChangeTextDocumentParams request, CancellationToken cancellationToken ) => throw new NotImplementedException ( );

        public Task<Unit> Handle ( DidOpenTextDocumentParams request, CancellationToken cancellationToken ) => throw new NotImplementedException ( );

        public Task<Unit> Handle ( DidCloseTextDocumentParams request, CancellationToken cancellationToken ) => throw new NotImplementedException ( );

        public Task<Unit> Handle ( DidSaveTextDocumentParams request, CancellationToken cancellationToken ) => throw new NotImplementedException ( );

        public void SetCapability ( SynchronizationCapability capability ) => throw new NotImplementedException ( );

        TextDocumentRegistrationOptions IRegistration<TextDocumentRegistrationOptions>.GetRegistrationOptions ( ) => throw new NotImplementedException ( );

        TextDocumentSaveRegistrationOptions IRegistration<TextDocumentSaveRegistrationOptions>.GetRegistrationOptions ( ) => throw new NotImplementedException ( );
    }
}