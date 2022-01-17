using System;
using System.IO;
using System.Text;
using Tsu.Timing;

namespace Loretta.CLI
{
    internal class ConsoleTimingLoggerTextWriter : TextWriter
    {
        private readonly ConsoleTimingLogger _logger;

        public ConsoleTimingLoggerTextWriter(ConsoleTimingLogger logger)
        {
            _logger = logger;
        }

        public override Encoding Encoding => Console.OutputEncoding;

        public override void Write(char value) => _logger.Write(value);
        public override void Write(string? value) => _logger.Write(value ?? "");

        public override void WriteLine() => _logger.WriteLine("");
        public override void WriteLine(char value) => _logger.WriteLine(value);
        public override void WriteLine(string? value) => _logger.WriteLine(value ?? "");
    }
}
