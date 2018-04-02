using System;
using System.Collections.Generic;
using System.Text;

namespace Loretta.CLI.Commands
{
    internal class CLICommandParser
    {
        public static IEnumerable<String> Parse ( String line )
        {
            var res = new List<String> ( );
            var buff = new StringBuilder ( );
            var inQuote = false;
            var quote = '\0';

            foreach ( var ch in line )
            {
                switch ( ch )
                {
                    // Handle quotes
                    case '\'':
                    case '"':
                        if ( inQuote && ch == quote )
                        {
                            inQuote = false;
                            quote = '\0';
                            yield return buff.ToString ( );
                            buff.Clear ( );
                        }
                        else if ( !inQuote )
                        {
                            inQuote = true;
                            quote = ch;
                        }
                        else
                            goto default;
                        break;

                    case ' ':
                        if ( inQuote )
                            goto default;
                        else if ( buff.Length > 0 )
                            yield return buff.ToString ( );
                        buff.Clear ( );
                        break;

                    default:
                        buff.Append ( ch );
                        break;
                }
            }

            if ( buff.Length > 0 )
                yield return buff.ToString ( );
        }
    }
}
