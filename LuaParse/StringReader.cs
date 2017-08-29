using System;
using System.Text;

namespace LuaParse
{
    public class StringReader
    {
        private readonly String Value;
        private Int32 Position;

        public StringReader ( String Value )
        {
            this.Value = Value;
            this.Position = -1;
        }

        #region Basic Checking

        /// <summary>
        /// Checks wether we can move <paramref name="Delta" /> characters
        /// </summary>
        /// <param name="Delta">Amount of characters to move</param>
        /// <returns></returns>
        public Boolean CanMove ( Int32 Delta )
        {
            var newPos = Position + Delta;
            return -1 < newPos && newPos < Value.Length;
        }

        /// <summary>
        /// Checks whether the next character after the
        /// <paramref name="dist" /> th character is <paramref name="ch" />
        /// </summary>
        /// <param name="ch">The character to find</param>
        /// <param name="dist">
        /// The distance at when to start looking for
        /// </param>
        /// <returns></returns>
        public Boolean IsNext ( Char ch, Int32 dist = 1 )
        {
            return Next ( dist ) == ch;
        }

        /// <summary>
        /// Checks whether <paramref name="a" /> is before <paramref name="b" />
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public Boolean AIsBeforeB ( Char a, Char b )
        {
            var ia = IndexOf ( a );
            var ib = IndexOf ( b );
            return ( ia == -1 && ib == -1 ) || ia == -1 ? false : true;
        }

        #endregion Basic Checking

        #region Basic Movement

        /// <summary>
        /// Consumes the <paramref name="dist" /> th next character
        /// </summary>
        /// <param name="dist"></param>
        /// <returns></returns>
        public Char Read ( Int32 dist = 1 )
        {
            if ( !CanMove ( dist ) )
                return '\0';
            return Value[Position += dist];
        }

        /// <summary>
        /// Returns the next <paramref name="dist" /> th character
        /// </summary>
        /// <param name="dist"></param>
        /// <returns></returns>
        public Char Next ( Int32 dist = 1 )
        {
            if ( !CanMove ( dist ) )
                return '\0';
            return Value[Position + dist];
        }

        /// <summary>
        /// Unconsume the last <paramref name="dist" /> characters
        /// </summary>
        /// <param name="dist"></param>
        /// <returns></returns>
        public Char Unread ( Int32 dist = 1 ) => Read ( -dist );

        /// <summary>
        /// Returns the <paramref name="dist" /> th last caracter
        /// </summary>
        /// <param name="dist"></param>
        /// <returns></returns>
        public Char Last ( Int32 dist = 1 ) => Next ( -dist );

        /// <summary>
        /// Returns the distance from <see cref="Position" /> +
        /// <paramref name="start" /> to <paramref name="ch" />
        /// </summary>
        /// <param name="ch">Character to search for</param>
        /// <param name="start">
        /// Distance from current position where to start
        /// searching for
        /// </param>
        /// <returns></returns>
        public Int32 IndexOf ( Char ch, Int32 start = 0 )
        {
            var idx = this.Value.IndexOf ( ch, Position + start );
            return idx == -1 ? -1 : idx - Position;
        }

        /// <summary>
        /// Returns the distance from <see cref="Position" /> +
        /// <paramref name="start" /> to <paramref name="str" />
        /// </summary>
        /// <param name="str">String to search for</param>
        /// <param name="start">
        /// Distance from current position where to start
        /// searching for
        /// </param>
        /// <returns></returns>
        public Int32 IndexOf ( String str, Int32 start = 0 ) => Value.IndexOf ( str, Position + start );

        /// <summary>
        /// Returns the index of a character that passes the <paramref name="Filter" />
        /// </summary>
        /// <param name="Filter">The filter function</param>
        /// <returns></returns>
        public Int32 IndexOf ( Func<Char, Boolean> Filter )
        {
            for ( int i = Position ; i < Value.Length ; i++ )
                if ( Filter?.Invoke ( Value[i] ) ?? default ( Boolean ) )
                    return i - Position;
            return -1;
        }

        #endregion Basic Movement

        #region Advanced Movement

        /// <summary>
        /// Reads until <paramref name="ch" /> is found
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public String ReadUntil ( Char ch )
        {
            var idx = this.Value.IndexOf ( ch, Position );
            return idx == -1 ? "" : this.ReadString ( idx - Position );
        }

        /// <summary>
        /// Reads until <paramref name="Filter" /> is false
        /// </summary>
        /// <param name="Filter"></param>
        /// <returns></returns>
        public String ReadUntil ( Func<Char, Boolean> Filter )
        {
            var len = 0;
            while ( !Filter?.Invoke ( Next ( 1 ) ) ?? default ( Boolean ) )
                len++;
            return this.ReadString ( len );
        }

        /// <summary>
        /// Reads until <paramref name="Filter" /> is false
        /// </summary>
        /// <param name="Filter"></param>
        /// <returns></returns>
        public String ReadUntil ( Func<Char, Char, Boolean> Filter )
        {
            var len = 0;
            while ( !Filter?.Invoke ( Next ( len + 1 ), Next ( len + 2 ) ) ?? default ( Boolean ) )
                len++;
            return ReadString ( len );
        }

        /// <summary>
        /// Reads up to <paramref name="str" />
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public String ReadUntil ( String str )
        {
            var i = IndexOf ( str, Position );
            return i == -1 ? "" : ReadString ( i - Position );
        }

        /// <summary>
        /// Reads until <paramref name="Filter" /> is false
        /// </summary>
        /// <param name="Filter"></param>
        /// <returns></returns>
        public String ReadUntilNot ( Func<Char, Boolean> Filter )
        {
            var len = 0;
            while ( Filter?.Invoke ( Next ( 1 ) ) ?? default ( Boolean ) )
                len++;
            return this.ReadString ( len );
        }

        /// <summary>
        /// Reads until <paramref name="Filter" /> is false
        /// </summary>
        /// <param name="Filter"></param>
        /// <returns></returns>
        public String ReadUntilNot ( Func<Char, Char, Boolean> Filter )
        {
            var len = 0;
            while ( Filter?.Invoke ( Next ( 1 ), Next ( 2 ) ) ?? default ( Boolean ) )
                len++;
            return this.ReadString ( len );
        }

        /// <summary>
        /// Reads and returns a <see cref="String" /> of <paramref name="length" />
        /// </summary>
        /// <param name="length">Length of the string to read</param>
        /// <returns></returns>
        public String ReadString ( Int32 length )
        {
            if ( length == 0 )
                return String.Empty;
            try
            {
                return this.Value.Substring ( this.Position, length );
            }
            finally
            {
                this.Position += length;
            }
        }

        #endregion Advanced Movement
    }
}
