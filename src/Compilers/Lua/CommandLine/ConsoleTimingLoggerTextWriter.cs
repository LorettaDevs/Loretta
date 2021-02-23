using System;
using System.IO;
using System.Text;
using Tsu.Timing;

namespace Loretta.CLI
{
    internal class ConsoleTimingLoggerTextWriter : TextWriter
    {
        private readonly ConsoleTimingLogger logger;

        public ConsoleTimingLoggerTextWriter ( ConsoleTimingLogger logger )
        {
            this.logger = logger;
        }

        public override Encoding Encoding => Console.OutputEncoding;

        public override void Write ( Char value ) => this.logger.Write ( value );
        public override void Write ( String? value ) => this.logger.Write ( value ?? "" );

        public override void WriteLine ( ) => this.logger.WriteLine ( "" );
        public override void WriteLine ( Char value ) => this.logger.WriteLine ( value );
        public override void WriteLine ( String? value ) => this.logger.WriteLine ( value ?? "" );
    }
}
