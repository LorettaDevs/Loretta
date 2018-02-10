using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Loretta.Common
{
    public static class CInterop
    {
        [DllImport ( "ucrtbase.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "atof", CharSet = CharSet.Ansi )]
        [return: MarshalAs ( UnmanagedType.R8 )]
        public static extern Double atof ( [MarshalAs ( UnmanagedType.LPStr )] String number );

        public static String ConvertStringToASCII ( String value )
        {
            using ( var mem = new MemoryStream ( ) )
            using ( var writer = new StreamWriter ( mem ) )
            {
                writer.Write ( value );
                writer.Flush ( );
                mem.Seek ( 0, SeekOrigin.Begin );

                return ReadAsBytes ( mem );
            }
        }

        public static String ReadAsBytes ( Stream source )
        {
            if ( !source.CanRead )
                throw new Exception ( "Stream cannot be read." );

            Byte[] data = new Byte[source.Length];
            var build = new StringBuilder ( );

            source.Read ( data, 0, data.Length );
            foreach ( var @byte in data )
                build.Append ( ( Char ) @byte );

            return build.ToString ( );
        }
    }
}
