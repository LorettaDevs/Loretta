using Loretta.CodeAnalysis.Collections;
using Loretta.CodeAnalysis.Lua.StatisticsCollector.Mathematics;

namespace Loretta.CodeAnalysis.Lua.StatisticsCollector
{
    internal record TokenStatistics(
        long TokenCount,
        Statistics TokenLengthStatistics)
    {
        public class Builder
        {
            private long _tokenCount;
            private readonly SegmentedList<double> _tokenSizes;

            public Builder()
            {
                _tokenSizes = Pools.BigDoubleListPool.Allocate();
            }

            public void AddToken(in SyntaxToken token)
            {
                _tokenCount++;
                _tokenSizes.Add(token.Width);
            }

            public void AddToken(int tokenWidth)
            {
                _tokenCount++;
                _tokenSizes.Add(tokenWidth);
            }

            public TokenStatistics Summarize()
            {
                return new TokenStatistics(
                    _tokenCount,
                    new Statistics(_tokenSizes));
            }

            public TokenStatistics SummarizeAndFree()
            {
                try
                {
                    return new TokenStatistics(
                        _tokenCount,
                        new Statistics(_tokenSizes));
                }
                finally
                {
                    _tokenSizes.Clear();
                    Pools.BigDoubleListPool.Free(_tokenSizes);
                }
            }
        }
    }
}
