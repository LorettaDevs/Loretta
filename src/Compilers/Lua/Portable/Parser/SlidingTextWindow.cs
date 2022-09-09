// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Diagnostics;
using System.Text;
using Loretta.CodeAnalysis.Lua.Utilities;
using Loretta.CodeAnalysis.PooledObjects;
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    /// <summary>
    /// Keeps a sliding buffer over the SourceText of a file for the lexer. Also
    /// provides the lexer with the ability to keep track of a current "lexeme"
    /// by leaving a marker and advancing ahead the offset. The lexer can then
    /// decide to "keep" the lexeme by erasing the marker, or abandon the current
    /// lexeme by moving the offset back to the marker.
    /// </summary>
    internal sealed class SlidingTextWindow : IDisposable
    {
        /// <summary>
        /// In many cases, e.g. PeekChar, we need the ability to indicate that there are
        /// no characters left and we have reached the end of the stream, or some other
        /// invalid or not present character was asked for. Due to perf concerns, things
        /// like nullable or out variables are not viable. Instead we need to choose a
        /// char value which can never be legal.
        /// 
        /// In .NET, all characters are represented in 16 bits using the UTF-16 encoding.
        /// Fortunately for us, there are a variety of different bit patterns which
        /// are *not* legal UTF-16 characters. 0xffff (char.MaxValue) is one of these
        /// characters -- a legal Unicode code point, but not a legal UTF-16 bit pattern.
        /// </summary>
        public const char InvalidCharacter = char.MaxValue;

        private const int DefaultWindowLength = 2048;

        private readonly SourceText _text;                 // Source of text to parse.
        private int _basis;                                // Offset of the window relative to the SourceText start.
        private int _offset;                               // Offset from the start of the window.
        private readonly int _textEnd;                     // Absolute end position
        private char[] _characterWindow;                   // Moveable window of chars from source text
        private int _characterWindowCount;                 // # of valid characters in chars buffer

        private int _lexemeStart;                          // Start of current lexeme relative to the window start.

        // Example for the above variables:
        // The text starts at 0.
        // The window onto the text starts at basis.
        // The current character is at (basis + offset), AKA the current "Position".
        // The current lexeme started at (basis + lexemeStart), which is <= (basis + offset)
        // The current lexeme is the characters between the lexemeStart and the offset.

        private readonly StringTable _strings;

        private static readonly ObjectPool<char[]> s_windowPool = new(() => new char[DefaultWindowLength]);

        public SlidingTextWindow(SourceText text)
        {
            _text = text;
            _basis = 0;
            _offset = 0;
            _textEnd = text.Length;
            _strings = StringTable.GetInstance();
            _characterWindow = s_windowPool.Allocate();
            _lexemeStart = 0;
        }

        public void Dispose()
        {
            if (_characterWindow != null)
            {
                s_windowPool.Free(_characterWindow);
                _characterWindow = null;
                _strings.Free();
            }
        }

        public SourceText Text => _text;

        /// <summary>
        /// The current absolute position in the text file.
        /// </summary>
        public int Position => _basis + _offset;

        /// <summary>
        /// The current offset inside the window (relative to the window start).
        /// </summary>
        public int Offset => _offset;

        /// <summary>
        /// The buffer backing the current window.
        /// </summary>
        public char[] CharacterWindow => _characterWindow;

        /// <summary>
        /// Returns the start of the current lexeme relative to the window start.
        /// </summary>
        public int LexemeRelativeStart => _lexemeStart;

        /// <summary>
        /// Number of characters in the character window.
        /// </summary>
        public int CharacterWindowCount => _characterWindowCount;

        /// <summary>
        /// The absolute position of the start of the current lexeme in the given
        /// SourceText.
        /// </summary>
        public int LexemeStartPosition => _basis + _lexemeStart;

        /// <summary>
        /// The number of characters in the current lexeme.
        /// </summary>
        public int Width => _offset - _lexemeStart;

        /// <summary>
        /// Start parsing a new lexeme.
        /// </summary>
        public void Start() => _lexemeStart = _offset;

        public void Reset(int position)
        {
            // if position is within already read character range then just use what we have
            var relative = position - _basis;
            if (relative >= 0 && relative <= _characterWindowCount)
            {
                _offset = relative;
            }
            else
            {
                // we need to reread text buffer
                var amountToRead = Math.Min(_text.Length, position + _characterWindow.Length) - position;
                amountToRead = Math.Max(amountToRead, 0);
                if (amountToRead > 0)
                {
                    _text.CopyTo(position, _characterWindow, 0, amountToRead);
                }

                _lexemeStart = 0;
                _offset = 0;
                _basis = position;
                _characterWindowCount = amountToRead;
            }
        }

        private bool MoreChars()
        {
            if (_offset >= _characterWindowCount)
            {
                if (Position >= _textEnd)
                {
                    return false;
                }

                // if lexeme scanning is sufficiently into the char buffer, 
                // then refocus the window onto the lexeme
                if (_lexemeStart > (_characterWindowCount / 4))
                {
                    Array.Copy(_characterWindow,
                        _lexemeStart,
                        _characterWindow,
                        0,
                        _characterWindowCount - _lexemeStart);
                    _characterWindowCount -= _lexemeStart;
                    _offset -= _lexemeStart;
                    _basis += _lexemeStart;
                    _lexemeStart = 0;
                }

                if (_characterWindowCount >= _characterWindow.Length)
                {
                    // grow char array, since we need more contiguous space
                    var oldWindow = _characterWindow;
                    var newWindow = new char[_characterWindow.Length * 2];
                    Array.Copy(oldWindow, 0, newWindow, 0, _characterWindowCount);
                    s_windowPool.ForgetTrackedObject(oldWindow, newWindow);
                    _characterWindow = newWindow;
                }

                var amountToRead = Math.Min(_textEnd - (_basis + _characterWindowCount),
                    _characterWindow.Length - _characterWindowCount);
                _text.CopyTo(_basis + _characterWindowCount,
                    _characterWindow,
                    _characterWindowCount,
                    amountToRead);
                _characterWindowCount += amountToRead;
                return amountToRead > 0;
            }

            return true;
        }

        /// <summary>
        /// After reading <see cref=" InvalidCharacter"/>, a consumer can determine
        /// if the InvalidCharacter was in the user's source or a sentinel.
        /// 
        /// Comments and string literals are allowed to contain any Unicode character.
        /// </summary>
        /// <returns></returns>
        internal bool IsReallyAtEnd() => _offset >= _characterWindowCount && Position >= _textEnd;

        /// <summary>
        /// Advance the current position by one. No guarantee that this
        /// position is valid.
        /// </summary>
        public void AdvanceChar() => _offset++;

        /// <summary>
        /// Advance the current position by n. No guarantee that this position
        /// is valid.
        /// </summary>
        public void AdvanceChar(int n) => _offset += n;

        /// <summary>
        /// Moves past the newline that the text window is currently pointing at.  The text window must be pointing at a
        /// newline.  If the newline is <c>\r\n</c> then that entire sequence will be skipped.  Otherwise, the text
        /// window will only advance past a single character.
        /// </summary>
        public void AdvancePastNewLine() => AdvanceChar(GetNewLineWidth());

        /// <summary>
        /// Gets the length of the newline the text window must be pointing at here.  For <c>\r\n</c> this is <c>2</c>,
        /// for everything else, this is <c>1</c>.
        /// </summary>
        public int GetNewLineWidth()
        {
            Debug.Assert(CharUtils.IsNewLine(PeekChar()));
            return GetNewLineWidth(PeekChar(), PeekChar(1));
        }

        public static int GetNewLineWidth(char currentChar, char nextChar)
        {
            Debug.Assert(CharUtils.IsNewLine(currentChar));
            return currentChar is '\r' or '\n' && nextChar is '\r' or '\n' && currentChar != nextChar ? 2 : 1;
        }

        /// <summary>
        /// Grab the next character and advance the position.
        /// </summary>
        /// <returns>
        /// The next character, <see cref="InvalidCharacter" /> if there were no characters 
        /// remaining.
        /// </returns>
        public char NextChar()
        {
            var c = PeekChar();
            if (c != InvalidCharacter)
            {
                AdvanceChar();
            }
            return c;
        }

        /// <summary>
        /// Gets the next character if there are any characters in the 
        /// SourceText. May advance the window if we are at the end.
        /// </summary>
        /// <returns>
        /// The next character if any are available. InvalidCharacter otherwise.
        /// </returns>
        public char PeekChar()
        {
            if (_offset >= _characterWindowCount && !MoreChars())
            {
                return InvalidCharacter;
            }

            // N.B. MoreChars may update the offset.
            return _characterWindow[_offset];
        }

        /// <summary>
        /// Gets the character at the given offset to the current position if
        /// the position is valid within the SourceText.
        /// </summary>
        /// <returns>
        /// The next character if any are available. InvalidCharacter otherwise.
        /// </returns>
        public char PeekChar(int delta)
        {
            var position = Position;
            AdvanceChar(delta);

            char ch;
            if (_offset >= _characterWindowCount
                && !MoreChars())
            {
                ch = InvalidCharacter;
            }
            else
            {
                // N.B. MoreChars may update the offset.
                ch = _characterWindow[_offset];
            }

            Reset(position);
            return ch;
        }

        /// <summary>
        /// If the next characters in the window match the given string,
        /// then advance past those characters.  Otherwise, do nothing.
        /// </summary>
        public bool AdvanceIfMatches(string desired, bool isCaseInsensitive = false)
        {
            var length = desired.Length;

            for (var i = 0; i < length; i++)
            {
                var character = PeekChar(i);

                if (isCaseInsensitive)
                {
                    if (CharUtils.AsciiLowerCase(character) != desired[i])
                    {
                        return false;
                    }
                }
                else if (character != desired[i])
                {
                    return false;
                }
            }

            AdvanceChar(length);
            return true;
        }

        public string Intern(StringBuilder text) => _strings.Add(text);

        public string Intern(char[] array, int start, int length) => _strings.Add(array, start, length);

        public string GetText(bool intern) => GetText(LexemeStartPosition, Width, intern);

        public string GetText(int position, int length, bool intern)
        {
            var offset = position - _basis;

            // PERF: Whether interning or not, there are some frequently occurring
            // easy cases we can pick off easily.
            switch (length)
            {
                case 0:
                    return string.Empty;

                #region Generated Code
                case 1:
                    switch (_characterWindow[offset])
                    {
                        #region Top 50 1 char texts
                        case ' ': return " ";
                        case '\t': return "\t";
                        case '\n': return "\n";
                        case '0': return "0";
                        case '1': return "1";
                        case 'v': return "v";
                        case '2': return "2";
                        case '\r': return "\r";
                        case 'y': return "y";
                        case '3': return "3";
                        case 'k': return "k";
                        case 'x': return "x";
                        case '5': return "5";
                        case '4': return "4";
                        case 'i': return "i";
                        case 'p': return "p";
                        case 'z': return "z";
                        case '8': return "8";
                        case 'r': return "r";
                        case '6': return "6";
                        case 'w': return "w";
                        case 'h': return "h";
                        case 'b': return "b";
                        case '7': return "7";
                        case 'a': return "a";
                        case 't': return "t";
                        case 'e': return "e";
                        case 's': return "s";
                        case '9': return "9";
                        case '_': return "_";
                        case 'B': return "B";
                        case 'c': return "c";
                        case 'g': return "g";
                        case 'm': return "m";
                        case 'W': return "W";
                        case 'A': return "A";
                        case 'L': return "L";
                        case 'R': return "R";
                        case 'V': return "V";
                        case 'j': return "j";
                        case 'f': return "f";
                        case 'l': return "l";
                        case 'n': return "n";
                        case 'H': return "H";
                        case 'd': return "d";
                        case 'I': return "I";
                        case 'Y': return "Y";
                        case 'X': return "X";
                        case 'S': return "S";
                        case 'C': return "C";
                            #endregion Top 50 1 char texts
                    }
                    break;

                case 2:
                    switch (_characterWindow[offset], _characterWindow[offset + 1])
                    {
                        #region Top 50 2 char texts
                        case ('\r', '\n'): return "\r\n";
                        case ('\t', '\t'): return "\t\t";
                        case (' ', ' '): return "  ";
                        case ('"', '"'): return "\"\"";
                        case ('v', 'm'): return "vm";
                        case ('1', '0'): return "10";
                        case ('t', 'r'): return "tr";
                        case (' ', '\t'): return " \t";
                        case ('9', '0'): return "90";
                        case ('2', '0'): return "20";
                        case ('U', 'p'): return "Up";
                        case ('5', '0'): return "50";
                        case ('3', '0'): return "30";
                        case ('p', 'l'): return "pl";
                        case ('1', '5'): return "15";
                        case ('8', '0'): return "80";
                        case ('\t', ' '): return "\t ";
                        case ('6', '0'): return "60";
                        case ('1', '6'): return "16";
                        case ('-', '-'): return "--";
                        case ('2', '5'): return "25";
                        case ('4', '0'): return "40";
                        case ('7', '0'): return "70";
                        case ('1', '2'): return "12";
                        case ('G', 'M'): return "GM";
                        case ('I', 'D'): return "ID";
                        case ('3', '2'): return "32";
                        case ('7', '5'): return "75";
                        case ('i', 'd'): return "id";
                        case ('3', '5'): return "35";
                        case ('4', '5'): return "45";
                        case ('1', '1'): return "11";
                        case ('1', '8'): return "18";
                        case ('1', '3'): return "13";
                        case ('1', '4'): return "14";
                        case ('f', 'x'): return "fx";
                        case ('6', '4'): return "64";
                        case ('m', 's'): return "ms";
                        case ('2', '4'): return "24";
                        case ('d', 't'): return "dt";
                        case ('5', '5'): return "55";
                        case ('2', '8'): return "28";
                        case ('1', '7'): return "17";
                        case ('6', '5'): return "65";
                        case ('2', '9'): return "29";
                        case ('2', '2'): return "22";
                        case ('C', 'T'): return "CT";
                        case ('1', '9'): return "19";
                        case ('3', '1'): return "31";
                        case ('9', '5'): return "95";
                            #endregion Top 50 2 char texts
                    }
                    break;

                case 3:
                    switch (_characterWindow[offset], _characterWindow[offset + 1], _characterWindow[offset + 2])
                    {
                        #region Top 50 3 char texts
                        case ('\t', '\t', '\t'): return "\t\t\t";
                        case ('E', 'N', 'T'): return "ENT";
                        case ('2', '5', '5'): return "255";
                        case ('"', 'S', '"'): return "\"S\"";
                        case ('"', 'N', '"'): return "\"N\"";
                        case ('p', 'o', 's'): return "pos";
                        case ('e', 'n', 't'): return "ent";
                        case ('a', 'n', 'g'): return "ang";
                        case ('p', 'l', 'y'): return "ply";
                        case ('A', 'd', 'd'): return "Add";
                        case (' ', '\t', '\t'): return " \t\t";
                        case (' ', ' ', ' '): return "   ";
                        case ('1', '0', '0'): return "100";
                        case ('"', '1', '"'): return "\"1\"";
                        case ('n', 'e', 't'): return "net";
                        case ('0', '.', '5'): return "0.5";
                        case ('P', 'o', 's'): return "Pos";
                        case ('"', '2', '"'): return "\"2\"";
                        case ('n', 'p', 'c'): return "npc";
                        case ('"', '3', '"'): return "\"3\"";
                        case ('"', '5', '"'): return "\"5\"";
                        case ('"', '4', '"'): return "\"4\"";
                        case ('1', '.', '0'): return "1.0";
                        case ('0', '.', '1'): return "0.1";
                        case ('"', 'x', '"'): return "\"x\"";
                        case ('2', '0', '0'): return "200";
                        case ('"', 'y', '"'): return "\"y\"";
                        case ('1', '8', '0'): return "180";
                        case ('"', 'C', '"'): return "\"C\"";
                        case ('"', 'P', '"'): return "\"P\"";
                        case ('t', 'a', 'b'): return "tab";
                        case ('A', 'n', 'g'): return "Ang";
                        case ('S', 'e', 't'): return "Set";
                        case ('"', '6', '"'): return "\"6\"";
                        case ('r', 'e', 'l'): return "rel";
                        case ('"', '8', '"'): return "\"8\"";
                        case ('"', '7', '"'): return "\"7\"";
                        case ('0', '.', '3'): return "0.3";
                        case ('0', '.', '2'): return "0.2";
                        case ('w', 'e', 'p'): return "wep";
                        case ('1', '5', '0'): return "150";
                        case ('N', 'P', 'C'): return "NPC";
                        case ('a', 't', 't'): return "att";
                        case ('1', '.', '5'): return "1.5";
                        case ('H', 'i', 't'): return "Hit";
                        case ('"', '0', '"'): return "\"0\"";
                        case ('5', '0', '0'): return "500";
                        case ('E', 'M', 'V'): return "EMV";
                        case ('3', '6', '0'): return "360";
                        case ('r', 'e', 's'): return "res";
                            #endregion Top 50 3 char texts
                    }
                    break;

                case 4:
                    switch (_characterWindow[offset], _characterWindow[offset + 1], _characterWindow[offset + 2], _characterWindow[offset + 3])
                    {
                        #region Top 50 4 char texts
                        case ('s', 'e', 'l', 'f'): return "self";
                        case ('S', 'W', 'E', 'P'): return "SWEP";
                        case ('\t', '\t', '\t', '\t'): return "\t\t\t\t";
                        case (' ', ' ', ' ', ' '): return "    ";
                        case ('m', 'a', 't', 'h'): return "math";
                        case ('n', 'a', 'm', 'e'): return "name";
                        case ('t', 'y', 'p', 'e'): return "type";
                        case ('u', 't', 'i', 'l'): return "util";
                        case ('s', 'i', 'z', 'e'): return "size";
                        case ('b', 'o', 'n', 'e'): return "bone";
                        case (' ', '\t', '\t', '\t'): return " \t\t\t";
                        case ('d', 'a', 't', 'a'): return "data";
                        case ('p', 'h', 'y', 's'): return "phys";
                        case ('R', 'a', 'n', 'd'): return "Rand";
                        case ('A', 'm', 'm', 'o'): return "Ammo";
                        case ('e', 'n', 't', 's'): return "ents";
                        case ('N', 'a', 'm', 'e'): return "Name";
                        case ('l', 'i', 's', 't'): return "list";
                        case ('s', 'k', 'i', 'n'): return "skin";
                        case ('B', 'a', 's', 'e'): return "Base";
                        case ('d', 'r', 'a', 'w'): return "draw";
                        case ('t', 'i', 'm', 'e'): return "time";
                        case ('h', 'o', 'o', 'k'): return "hook";
                        case ('v', 'g', 'u', 'i'): return "vgui";
                        case ('S', 'l', 'o', 't'): return "Slot";
                        case ('T', 'y', 'p', 'e'): return "Type";
                        case ('S', 'c', 'r', 'H'): return "ScrH";
                        case ('f', 'i', 'l', 'e'): return "file";
                        case ('a', 'r', 'g', 's'): return "args";
                        case ('S', 'c', 'r', 'W'): return "ScrW";
                        case ('T', 'O', 'O', 'L'): return "TOOL";
                        case ('t', 'e', 'x', 't'): return "text";
                        case ('x', 'P', 'o', 's'): return "xPos";
                        case ('y', 'P', 'o', 's'): return "yPos";
                        case ('F', 'i', 'r', 'e'): return "Fire";
                        case ('g', 'a', 'm', 'e'): return "game";
                        case ('z', 'P', 'o', 's'): return "zPos";
                        case ('C', 'o', 'n', 'e'): return "Cone";
                        case ('S', 'i', 'z', 'e'): return "Size";
                        case ('1', '0', '0', '0'): return "1000";
                        case ('N', 'U', 'L', 'L'): return "NULL";
                        case ('D', 'e', 'a', 'd'): return "Dead";
                        case ('0', '.', '0', '5'): return "0.05";
                        case ('f', 'i', 'n', 'd'): return "find";
                        case ('u', 'm', 's', 'g'): return "umsg";
                        case ('I', 'T', 'E', 'M'): return "ITEM";
                        case ('0', '.', '2', '5'): return "0.25";
                        case ('0', '.', '0', '1'): return "0.01";
                        case ('e', 'n', 't', '1'): return "ent1";
                        case ('S', 't', 'o', 'p'): return "Stop";
                            #endregion Top 50 4 char texts
                    }
                    break;

                case 5:
                    switch (_characterWindow[offset], _characterWindow[offset + 1], _characterWindow[offset + 2], _characterWindow[offset + 3], _characterWindow[offset + 4])
                    {
                        #region Top 50 5 char texts
                        case ('\t', '\t', '\t', '\t', '\t'): return "\t\t\t\t\t";
                        case ('O', 'w', 'n', 'e', 'r'): return "Owner";
                        case ('A', 'n', 'g', 'l', 'e'): return "Angle";
                        case ('m', 'o', 'd', 'e', 'l'): return "model";
                        case ('C', 'o', 'l', 'o', 'r'): return "Color";
                        case ('s', 'o', 'u', 'n', 'd'): return "sound";
                        case ('a', 'n', 'g', 'l', 'e'): return "angle";
                        case ('t', 'a', 'b', 'l', 'e'): return "table";
                        case ('S', 'o', 'u', 'n', 'd'): return "Sound";
                        case ('p', 'a', 'i', 'r', 's'): return "pairs";
                        case ('c', 'o', 'l', 'o', 'r'): return "color";
                        case ('t', 'i', 'm', 'e', 'r'): return "timer";
                        case ('t', 'r', 'a', 'c', 'e'): return "trace";
                        case ('R', 'i', 'g', 'h', 't'): return "Right";
                        case ('S', 'c', 'a', 'l', 'e'): return "Scale";
                        case ('M', 'o', 'd', 'e', 'l'): return "Model";
                        case ('s', 'c', 'a', 'l', 'e'): return "scale";
                        case ('S', 'p', 'a', 'w', 'n'): return "Spawn";
                        case ('D', 'e', 'l', 'a', 'y'): return "Delay";
                        case (' ', '\t', '\t', '\t', '\t'): return " \t\t\t\t";
                        case ('T', 'h', 'i', 'n', 'k'): return "Think";
                        case (' ', ' ', ' ', ' ', ' '): return "     ";
                        case ('C', 'l', 'a', 's', 's'): return "Class";
                        case ('s', 't', 'a', 'r', 't'): return "start";
                        case ('p', 'r', 'i', 'n', 't'): return "print";
                        case ('p', 'a', 'n', 'e', 'l'): return "panel";
                        case ('C', 'l', 'a', 'm', 'p'): return "Clamp";
                        case ('G', 'e', 't', 'U', 'p'): return "GetUp";
                        case ('C', 'l', 'i', 'p', '1'): return "Clip1";
                        case ('P', 'A', 'N', 'E', 'L'): return "PANEL";
                        case ('p', 'i', 't', 'c', 'h'): return "pitch";
                        case ('I', 's', 'N', 'P', 'C'): return "IsNPC";
                        case ('S', 't', 'a', 'r', 't'): return "Start";
                        case ('"', 'p', 'o', 's', '"'): return "\"pos\"";
                        case ('v', 'a', 'l', 'u', 'e'): return "value";
                        case ('F', 'o', 'r', 'c', 'e'): return "Force";
                        case ('o', 'w', 'n', 'e', 'r'): return "owner";
                        case ('0', '.', '0', '0', '9'): return "0.009";
                        case ('i', 'n', 'd', 'e', 'x'): return "index";
                        case ('"', 'a', 'r', '2', '"'): return "\"ar2\"";
                        case ('"', 'N', 'P', 'C', '"'): return "\"NPC\"";
                        case ('L', 'a', 'b', 'e', 'l'): return "Label";
                        case ('c', 'l', 'a', 's', 's'): return "class";
                        case ('l', 'e', 'v', 'e', 'l'): return "level";
                        case ('S', 'm', 'o', 'k', 'e'): return "Smoke";
                        case ('A', 'l', 'i', 'v', 'e'): return "Alive";
                        case ('R', 'o', 'u', 'n', 'd'): return "Round";
                        case ('P', 'a', 'n', 'e', 'l'): return "Panel";
                        case ('P', 'a', 'i', 'n', 't'): return "Paint";
                        case ('S', 'p', 'e', 'e', 'd'): return "Speed";
                            #endregion Top 50 5 char texts
                    }
                    break;
                    #endregion Generated Code
            }

            if (intern)
            {
                return Intern(_characterWindow, offset, length);
            }
            else
            {
                return new string(_characterWindow, offset, length);
            }
        }
    }
}
