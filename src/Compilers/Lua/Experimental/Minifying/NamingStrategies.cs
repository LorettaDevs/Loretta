using System.Text;

namespace Loretta.CodeAnalysis.Lua.Experimental.Minifying
{
    /// <summary>
    /// The default naming strategies.
    /// </summary>
    public static class NamingStrategies
    {
        // We'll add up to 5 prefixes otherwise we'll call quits.
        private const int MaxPrefixCount = 5;

        private static string StringSequentialCore(
            int slot,
            IEnumerable<IScope> scopes,
            char prefix,
            string alphabet,
            int minPrefixCount)
        {
            var len = getMaxDigits(slot, alphabet.Length) + MaxPrefixCount;
            Span<char> fullName = stackalloc char[len];
            fullName.Fill(prefix);
            var pos = len - 1;
            do
            {
                var num = slot % alphabet.Length;
                slot /= alphabet.Length;
                fullName[pos] = alphabet[num];
                pos--;
            }
            while (slot > 0);

            var firstNameChar = pos + 1;
            var prefixes = firstNameChar - minPrefixCount;
            var unavailableNames = MinifyingUtils.GetUnavailableNames(scopes);
            while (prefixes > 0)
            {
                var name = fullName[prefixes..].ToString();
                if (SyntaxFacts.GetKeywordKind(name) == SyntaxKind.IdentifierToken && !unavailableNames.Contains(name))
                    return name;
                prefixes--;
            }
            throw new Exception($"Code has too many variables named {fullName[firstNameChar..].ToString()} with '{prefix}'s at the start.");

            static int getMaxDigits(int slot, int @base) => slot <= 1 ? 1 : (int) Math.Ceiling(Math.Log(slot, @base) + 1);
        }

        /// <summary>
        /// The sequential naming strategy factory method.
        /// It'll create a <see cref="NamingStrategy"/> delegate using
        /// the provided <paramref name="prefix"/> and <paramref name="alphabet"/>.
        /// </summary>
        /// <param name="prefix">
        /// The prefix to use when a variable of the same name is found.
        /// </param>
        /// <param name="alphabet">
        /// The alphabet to use. Each character will be a single item.
        /// </param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// Thrown when too many variables with the same name and prefix are found.
        /// </exception>
        /// <remarks>
        /// To provide an example of how this strategy works, let's assume the following:
        /// <list type="number">
        ///   <item><description><paramref name="prefix"/> is <c>'_'</c></description></item>
        ///   <item><description><paramref name="alphabet"/> is <c>"a", "b" and "c"</c></description></item>
        ///   <item><description>The provided <see cref="IScope"/> has two variables in scope named 'a' and 'b'.</description></item>
        /// </list>
        /// And these are some of its returns:
        /// <list type="bullet">
        ///   <item><description>Slot 0: <c>_a</c> (as <c>a</c> is already in scope)</description></item>
        ///   <item><description>Slot 1: <c>_b</c> (as <c>b</c> is already in scope)</description></item>
        ///   <item><description>Slot 2: <c>c</c> (as there is no <c>c</c> already in scope)</description></item>
        ///   <item><description>Slot 3: <c>aa</c></description></item>
        ///   <item><description>Slot 13: <c>aaa</c></description></item>
        ///   <item><description>Slot 40: <c>aaaa</c></description></item>
        /// </list>
        /// </remarks>
        public static NamingStrategy Sequential(char prefix, ImmutableArray<string> alphabet)
        {
            if (alphabet.IsDefault)
                throw new ArgumentException("Alphabet must not be default.", nameof(alphabet));
            if (alphabet.Length < 2)
                throw new ArgumentException("Alphabet must have at least 2 elements.", nameof(alphabet));

            return (int slot, IEnumerable<IScope> scopes) =>
            {
                var name = new StringBuilder();
                while (slot > 0)
                {
                    var num = slot % alphabet.Length;
                    slot /= alphabet.Length;
                    name.Insert(0, alphabet[num]);
                }

                var prefixes = 0;
                var unavailableNames = MinifyingUtils.GetUnavailableNames(scopes);
                while (prefixes <= MaxPrefixCount)
                {
                    var strName = name.ToString();
                    if (SyntaxFacts.GetKeywordKind(strName) == SyntaxKind.IdentifierToken && !unavailableNames.Contains(strName))
                        return strName;
                    name.Insert(0, prefix);
                    prefixes++;
                }
                throw new Exception($"Code has too many variables named {name.Remove(0, prefixes)} with '{prefix}'s at the start.");
            };
        }

        /// <summary>
        /// The alphabet naming strategy.
        /// This is similar to <see cref="Sequential(char, ImmutableArray{string})"/> with:
        /// <para><c>prefix</c> as <c>_</c></para>
        /// <para>and <c>alphabet</c> as <c>abcdefghijklmnopqrstuvwxyz</c>.</para>
        /// It is more optimized than <see cref="Sequential(char, ImmutableArray{string})"/> so use this
        /// instead of it whenever possible.
        /// </summary>
        public static string Alphabetical(int slot, IEnumerable<IScope> scopes)
        {
            const char prefix = '_';
            const string alphabet = "abcdefghijklmnopqrstuvwxyz";
            return StringSequentialCore(slot, scopes, prefix, alphabet, 0);
        }

        /// <summary>
        /// The alphabet naming strategy.
        /// This is similar to <see cref="Sequential(char, ImmutableArray{string})"/> with:
        /// <para><c>prefix</c> as <c>_</c></para>
        /// <para>and <c>alphabet</c> as <c>0123456789</c>.</para>
        /// It is more optimized than <see cref="Sequential(char, ImmutableArray{string})"/> so use this
        /// instead of it whenever possible.
        /// </summary>
        public static string Numerical(int slot, IEnumerable<IScope> scopes)
        {
            const char prefix = '_';
            const string alphabet = "0123456789";
            return StringSequentialCore(slot, scopes, prefix, alphabet, 1);
        }

        /// <summary>
        /// The non-printable characters naming strategy. ONLY WORKS WHEN TARGETTING LUAJIT.
        /// This works like <see cref="Sequential(char, ImmutableArray{string})"/>.
        /// With <c>prefix</c> as <c>_</c> and <c>alphabet</c> as zero width unicode characters.
        /// It is more optimized than <see cref="Sequential(char, ImmutableArray{string})"/> so use this
        /// instead of it whenever possible.
        /// </summary>
        public static string ZeroWidth(int slot, IEnumerable<IScope> scopes)
        {
            const char prefix = '\u200B'; // ZERO WIDTH SPACE
            const string alphabet = "\u200C" // ZERO WIDTH NON-JOINER
                + "\u200D" // ZERO WIDTH JOINER
                + "\uFEFF" // ZERO WIDTH NO-BREAK SPACE
                ;
            return StringSequentialCore(slot, scopes, prefix, alphabet, 0);
        }
    }
}
