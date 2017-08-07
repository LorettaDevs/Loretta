using System;
using System.Text;

namespace LuaParse.Tokens.Abstract
{
    public interface IToken
    {
        StringBuilder WhitespaceAfter { get; set; }

        StringBuilder WhitespaceBefore { get; set; }

        String Raw { get; set; }

        void SetWhitespaceAfter ( String wp );

        void AddWhitespaceAfter ( String wp );

        void SetWhitespaceBefore ( String wp );

        void AddWhitespaceBefore ( String wp );
    }
}
