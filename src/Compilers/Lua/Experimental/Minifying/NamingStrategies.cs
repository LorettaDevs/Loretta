using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
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

        private static IVariable? FindVariable(IScope scope, ReadOnlySpan<char> name)
        {
            IScope? currentScope = scope;
            while (currentScope is not null)
            {
                foreach (var variable in currentScope.DeclaredVariables)
                {
                    if (name.SequenceEqual(variable.Name.AsSpan()))
                        return variable;
                }
                currentScope = currentScope.ContainingScope;
            }

            return null;
        }

        private static string StringSequentialCore(IScope scope, int slot, char prefix, string alphabet, int minPrefixCount)
        {
            var len = getDigits(slot, alphabet.Length) + MaxPrefixCount;
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

            var prefixes = minPrefixCount;
            while (prefixes <= MaxPrefixCount)
            {
                var name = fullName[(MaxPrefixCount - prefixes)..];
                if (FindVariable(scope, name) == null)
                    return name.ToString();
                prefixes++;
            }
            throw new Exception($"Code has too many variables named {fullName[10..].ToString()} with '{prefix}'s at the start.");

            static int getDigits(int slot, int @base)
            {
                if (slot <= 1)
                    return 1;
                return (int) Math.Ceiling(Math.Log(slot, @base));
            }
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

            return (IScope scope, int slot) =>
            {
                var name = new StringBuilder();
                while (slot > 0)
                {
                    var num = slot % alphabet.Length;
                    slot /= alphabet.Length;
                    name.Insert(0, alphabet[num]);
                }

                var prefixes = 0;
                while (prefixes <= MaxPrefixCount)
                {
                    if (FindVariable(scope, name.ToString().AsSpan()) == null)
                        return name.ToString();
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
        /// <param name="scope"><inheritdoc cref="NamingStrategy" path="/param[name='scope']"/></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        public static string Alphabetical(IScope scope, int slot)
        {
            const char prefix = '_';
            const string alphabet = "abcdefghijklmnopqrstuvwxyz";
            return StringSequentialCore(scope, slot, prefix, alphabet, 0);
        }

        /// <summary>
        /// The alphabet naming strategy.
        /// This is similar to <see cref="Sequential(char, ImmutableArray{string})"/> with:
        /// <para><c>prefix</c> as <c>_</c></para>
        /// <para>and <c>alphabet</c> as <c>0123456789</c>.</para>
        /// It is more optimized than <see cref="Sequential(char, ImmutableArray{string})"/> so use this
        /// instead of it whenever possible.
        /// </summary>
        /// <param name="scope"><inheritdoc cref="NamingStrategy" path="/param[name='scope']"/></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        public static string Numerical(IScope scope, int slot)
        {
            const char prefix = '_';
            const string alphabet = "0123456789";
            return StringSequentialCore(scope, slot, prefix, alphabet, 1);
        }

        /// <summary>
        /// The non-printable characters naming strategy. ONLY WORKS WHEN TARGETTING LUAJIT.
        /// This works like <see cref="Sequential(char, ImmutableArray{string})"/>.
        /// With <c>prefix</c> as <c>_</c> and <c>alphabet</c> as zero width unicode characters.
        /// It is more optimized than <see cref="Sequential(char, ImmutableArray{string})"/> so use this
        /// instead of it whenever possible.
        /// 
        /// </summary>
        /// <param name="scope"><inheritdoc cref="NamingStrategy" path="/param[name='scope']"/></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        public static string ZeroWidth(IScope scope, int slot)
        {
            const char prefix = '\u200B'; // ZERO WIDTH SPACE
            const string alphabet = "\u200C" // ZERO WIDTH NON-JOINER
                + "\u200D" // ZERO WIDTH JOINER
                + "\uFEFF" // ZERO WIDTH NO-BREAK SPACE
                ;
            return StringSequentialCore(scope, slot, prefix, alphabet, 0);
        }
    }
}
