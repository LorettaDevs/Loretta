using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Loretta.Tests
{
    public class Utilities
    {
        private static IEnumerable<T> Prepend<T> ( T value, IEnumerable<T> enumerable )
        {
            yield return value;
            foreach ( T v in enumerable ) yield return v;
        }

        private static IEnumerable<T> SkipIndex<T> ( IEnumerable<T> enumerable, Int32 index )
        {
            var currentIndex = 0;
            foreach ( T val in enumerable )
            {
                if ( currentIndex != index )
                    yield return val;
                currentIndex++;
            }
        }

        public static IEnumerable<T[]> Permute<T> ( params T[] args )
        {
            return impl ( args ).Select ( e => e.ToArray ( ) );

            static IEnumerable<IEnumerable<T>> impl ( IEnumerable<T> args )
            {
                if ( args.Count ( ) == 1 )
                    return new[] { args };

                return args.SelectMany ( ( arg, idx ) =>
                {
                    IEnumerable<T> argsWithoutCurrent = SkipIndex ( args, idx );
                    return impl ( argsWithoutCurrent ).Select ( e => Prepend ( arg, e ) );
                } );
            }
        }

        public static IEnumerable<String> Toggle ( params String[] parts )
        {
            if ( parts.Length > 64 )
                throw new ArgumentException ( "Too many strings.", nameof ( parts ) );

            var builder = new StringBuilder ( parts.Sum ( part => part.Length ) );
            var permutations = 2 << ( parts.Length - 1 );
            for ( var set = 1; set < permutations; set++ )
            {
                builder.Clear ( );
                for ( var partIdx = 0; partIdx < parts.Length; partIdx++ )
                {
                    if ( ( set & ( 1 << partIdx ) ) != 0 )
                        builder.Append ( parts[partIdx] );
                }
                yield return builder.ToString ( );
            }
        }
    }
}
