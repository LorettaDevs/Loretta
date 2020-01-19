using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Server;
using Serilog;

namespace Loretta.LanguageServer
{
    internal class Program
    {
        private static async Task Main ( String[] args )
        {
            Log.Logger = new LoggerConfiguration ( )
              .Enrich.FromLogContext ( )
              .WriteTo.File ( "log.txt", rollingInterval: RollingInterval.Day )
              .CreateLogger ( );


            Log.Logger.Information ( "This only goes file..." );

        }
    }
}