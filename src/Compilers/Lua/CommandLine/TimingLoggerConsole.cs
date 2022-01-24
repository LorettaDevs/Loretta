using System;
using System.CommandLine;
using System.CommandLine.IO;
using Tsu.Timing;

namespace Loretta.CLI
{
    internal class TimingLoggerConsole : IConsole
    {
        private readonly Writer _outWriter;
        private readonly Writer _errorWriter;

        public TimingLoggerConsole(TimingLogger logger)
        {
            _outWriter = new(logger, LogLevel.None);
            _errorWriter = new(logger, LogLevel.Error);
        }

        public IStandardStreamWriter Out => _outWriter;
        public bool IsOutputRedirected => false;
        public IStandardStreamWriter Error => _errorWriter;
        public bool IsErrorRedirected => false;
        public bool IsInputRedirected => false;

        private class Writer : IStandardStreamWriter
        {
            private readonly LogLevel _logLevel;
            private readonly Action<LogLevel, string> _processWrite;

            public Writer(TimingLogger logger, LogLevel logLevel)
            {
                _logLevel = logLevel;
                _processWrite = typeof(TimingLogger).GetMethod(
                    "ProcessWrite",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                    new Type[] { typeof(LogLevel), typeof(string) })!
                    .CreateDelegate<Action<LogLevel, string>>(logger);
            }

            public void Write(string value) => _processWrite(_logLevel, value);
        }
    }
}
