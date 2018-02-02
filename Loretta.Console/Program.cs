namespace Loretta.Console
{
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Text;

    internal class Program
    {
        private static void Main ( String[] args )
        {
            var fn = args.Length < 1 ? Console.ReadLine ( ) : args[0];
            var f = Path.GetFileNameWithoutExtension(fn);

            var ofi = new FileInfo ( $"{f}.json" );

            if ( ofi.Exists ) ofi.Delete ( );

            Console.WriteLine ( $"Writing to {ofi.FullName}" );
            try
            {
                using ( var fs = ofi.OpenWrite ( ) )
                using ( var sw = new StreamWriter ( fs ) )
                using ( var jw = new JsonTextWriter ( sw ) )
                {
                    jw.Formatting = Formatting.Indented;
                    jw.IndentChar = '\t';
                    jw.Indentation = 1;
                    var js = new JsonSerializer ( );
                    js.Converters.Add ( new MyEnumConverter ( ) );
                    js.Converters.Add ( new MyStringBuilderConverter ( ) );
                    //js.Serialize ( jw, Tokenizer.Tokenize ( File.ReadAllText ( fn ) ) );
                }
            }
            catch ( Exception e )
            { Console.WriteLine ( e ); }
        }
    }

    public class MyEnumConverter : JsonConverter
    {
        public override Boolean CanConvert ( Type ot )
        {
            return ot == typeof ( Tokens.TokenType );
        }

        public override Object ReadJson ( JsonReader reader, Type objectType, Object existingValue, JsonSerializer serializer )
        {
            return Enum.Parse ( typeof ( Tokens.TokenType ), reader.ReadAsString ( ) );
        }

        public override void WriteJson ( JsonWriter writer, Object value, JsonSerializer serializer )
        {
            writer.WriteComment ( ( ( Tokens.TokenType ) value ).ToString ( ) );
        }
    }

    public class MyStringBuilderConverter : JsonConverter
    {
        public override Boolean CanConvert ( Type ot )
        {
            return ot == typeof ( StringBuilder );
        }

        public override Object ReadJson ( JsonReader reader, Type objectType, Object existingValue, JsonSerializer serializer )
        {
            throw new NotImplementedException ( );
        }

        public override void WriteJson ( JsonWriter writer, Object value, JsonSerializer serializer )
        {
            writer.WriteValue ( ( value as StringBuilder )?.ToString ( ) );
        }
    }
}
