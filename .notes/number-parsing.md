# Number Lexing Module Notes
## Table of Characters

Table of the relevant characters, their representation in binary, and the values they should be converted to (both in decimal and binary):

| Character | Binary  |    Result     |
|:---------:|:-------:|:-------------:|
|     0     | 0110000 |  0₁₀ ≡ 00000₂ |
|     1     | 0110001 |  1₁₀ ≡ 00001₂ |
|     2     | 0110010 |  2₁₀ ≡ 00010₂ |
|     3     | 0110011 |  3₁₀ ≡ 00011₂ |
|     4     | 0110100 |  4₁₀ ≡ 00100₂ |
|     5     | 0110101 |  5₁₀ ≡ 00101₂ |
|     6     | 0110110 |  6₁₀ ≡ 00110₂ |
|     7     | 0110111 |  7₁₀ ≡ 00111₂ |
|     8     | 0111000 |  8₁₀ ≡ 01000₂ |
|     9     | 0111001 |  9₁₀ ≡ 01001₂ |
|     a     | 1100001 | 10₁₀ ≡ 01010₂ |
|     b     | 1100010 | 11₁₀ ≡ 01011₂ |
|     c     | 1100011 | 12₁₀ ≡ 01100₂ |
|     d     | 1100100 | 13₁₀ ≡ 01101₂ |
|     e     | 1100101 | 14₁₀ ≡ 01110₂ |
|     f     | 1100110 | 15₁₀ ≡ 01111₂ |
|     A     | 1000001 | 10₁₀ ≡ 01010₂ |
|     B     | 1000010 | 11₁₀ ≡ 01011₂ |
|     C     | 1000011 | 12₁₀ ≡ 01100₂ |
|     D     | 1000100 | 13₁₀ ≡ 01101₂ |
|     E     | 1000101 | 14₁₀ ≡ 01110₂ |
|     F     | 1000110 | 15₁₀ ≡ 01111₂ |

## Conversion of characters to integers

Looking at the above table, a pattern can be seen from the digits `0` to `9`: their 4 least significant bits are the value we want, therefore we just need to isolate them.

The implementation uses a bitwise and (`&`) of the character and `1111₂`.

Now, the upper and lower case letters from `a` to `f` only differ in their 6th least significant bit, and their 5 least significant bits already give us the 1st digit (although off by 1), so upper and lower case letters can be handled in indifferently by isolating the 5 least significant bits and adding `9₁₀` to the isolated bits.

This is not being used because hexadecimal number parsing is left up to HexFloat from FParsec.
