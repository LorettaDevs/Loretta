// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Loretta.CodeAnalysis.Lua.Test.Utilities;
using Loretta.Test.Utilities;
using Xunit;
using Xunit.Sdk;

namespace Loretta.CodeAnalysis.Lua.UnitTests
{
    // This class uses the InterpolatedInlineData attribute defined at the end of it.
    // What it does is basically replace {[(EXPR)]}, {[(TYPE)]} and {[(;)]} by the predefined values.
    // - {[(EXPR)]} has 5 predefined strings it gets replaced with (check the end of the class for the exact ones);
    // - {[(TYPE)]} has 7 predefined strings it gets replaced with (check the end of the class for the exact ones);
    // - {[(;)]} is by a semicolon and an empty string.
    // The first argument is the input and the second is the expected output from running SyntaxNormalizer.
    // An example of this would be:
    //
    //     local x: {[(TYPE)]} = {[(EXPR)]}{[(;)]}
    //
    // Which results in 5 * 7 * 2 = 70 test cases (as they are replaced in a combinatory method).
    public sealed class SyntaxNormalizerTests : LuaTestBase
    {
        private static readonly LuaSyntaxOptions s_luaParseOptions = LuaSyntaxOptions.All;

        [Theory]
        #region Anonymous Functions
        [InlineData("""
                    function
                    (

                    )

                    end

                    """,
                    """
                    function()
                    end
                    """)]
        [InlineData("""
                    function
                    <
                            T1
                    , T2


                                ,T3>
                    (

                        arg1: T1
                    ,
                            arg2: T2

                    )

                    :           T3

                    end

                    """,
                    """
                    function<T1, T2, T3>(arg1: T1, arg2: T2): T3
                    end
                    """)]
        #endregion Anonymous Functions
        #region Binary Expressions
        [InterpolatedInlineData(" {[(EXPR)]}   &&   {[(EXPR)]} ",
                                "{[(EXPR)]} && {[(EXPR)]}")]
        [InterpolatedInlineData(" {[(EXPR)]}   &   {[(EXPR)]} ",
                                "{[(EXPR)]} & {[(EXPR)]}")]
        [InterpolatedInlineData(" {[(EXPR)]}   and   {[(EXPR)]} ",
                                "{[(EXPR)]} and {[(EXPR)]}")]
        [InterpolatedInlineData(" {[(EXPR)]}   !=   {[(EXPR)]} ",
                                "{[(EXPR)]} != {[(EXPR)]}")]
        [InterpolatedInlineData(" {[(EXPR)]}   ..   {[(EXPR)]} ",
                                "{[(EXPR)]} .. {[(EXPR)]}")]
        [InterpolatedInlineData(" {[(EXPR)]}   ==   {[(EXPR)]} ",
                                "{[(EXPR)]} == {[(EXPR)]}")]
        [InterpolatedInlineData(" {[(EXPR)]}   >=   {[(EXPR)]} ",
                                "{[(EXPR)]} >= {[(EXPR)]}")]
        [InterpolatedInlineData(" {[(EXPR)]}   >>   {[(EXPR)]} ",
                                "{[(EXPR)]} >> {[(EXPR)]}")]
        [InterpolatedInlineData(" {[(EXPR)]}   >   {[(EXPR)]} ",
                                "{[(EXPR)]} > {[(EXPR)]}")]
        [InterpolatedInlineData(" {[(EXPR)]}   ^   {[(EXPR)]} ",
                                "{[(EXPR)]} ^ {[(EXPR)]}")]
        [InterpolatedInlineData(" {[(EXPR)]}   <=   {[(EXPR)]} ",
                                "{[(EXPR)]} <= {[(EXPR)]}")]
        [InterpolatedInlineData(" {[(EXPR)]}   <<   {[(EXPR)]} ",
                                "{[(EXPR)]} << {[(EXPR)]}")]
        [InterpolatedInlineData(" {[(EXPR)]}   <   {[(EXPR)]} ",
                                "{[(EXPR)]} < {[(EXPR)]}")]
        [InterpolatedInlineData(" {[(EXPR)]}   -   {[(EXPR)]} ",
                                "{[(EXPR)]} - {[(EXPR)]}")]
        [InterpolatedInlineData(" {[(EXPR)]}   or   {[(EXPR)]} ",
                                "{[(EXPR)]} or {[(EXPR)]}")]
        [InterpolatedInlineData(" {[(EXPR)]}   %   {[(EXPR)]} ",
                                "{[(EXPR)]} % {[(EXPR)]}")]
        [InterpolatedInlineData(" {[(EXPR)]}   ||   {[(EXPR)]} ",
                                "{[(EXPR)]} || {[(EXPR)]}")]
        [InterpolatedInlineData(" {[(EXPR)]}   |   {[(EXPR)]} ",
                                "{[(EXPR)]} | {[(EXPR)]}")]
        [InterpolatedInlineData(" {[(EXPR)]}   +   {[(EXPR)]} ",
                                "{[(EXPR)]} + {[(EXPR)]}")]
        [InterpolatedInlineData(" {[(EXPR)]}   /   {[(EXPR)]} ",
                                "{[(EXPR)]} / {[(EXPR)]}")]
        [InterpolatedInlineData(" {[(EXPR)]}   *   {[(EXPR)]} ",
                                "{[(EXPR)]} * {[(EXPR)]}")]
        [InterpolatedInlineData(" {[(EXPR)]}  ~=   {[(EXPR)]} ",
                                "{[(EXPR)]} ~= {[(EXPR)]}")]
        [InterpolatedInlineData(" {[(EXPR)]}   ~   {[(EXPR)]} ",
                                "{[(EXPR)]} ~ {[(EXPR)]}")]
        #endregion Binary Expressions
        #region If Expressions
        [InterpolatedInlineData("""
                                if   {[(EXPR)]}
                                then
                                {[(EXPR)]}
                                else
                                {[(EXPR)]}

                                """,
                                "if {[(EXPR)]} then {[(EXPR)]} else {[(EXPR)]}")]
        [InterpolatedInlineData("""
                                if   {[(EXPR)]}
                                then
                                {[(EXPR)]}
                                elseif
                                {[(EXPR)]}   then   {[(EXPR)]}
                                else
                                {[(EXPR)]}

                                """,
                                "if {[(EXPR)]} then {[(EXPR)]} elseif {[(EXPR)]} then {[(EXPR)]} else {[(EXPR)]}")]
        #endregion If Expressions
        #region VarArg/Literal Expressions
        [InterpolatedInlineData(" {[(EXPR)]} ",
                                "{[(EXPR)]}")]
        #endregion VarArg/Literal Expressions
        #region Prefix Expressions
        #region Prefix Expressions - Function Calls
        [InlineData("a  (  )", "a()")]
        [InlineData("a . b  (  )", "a.b()")]
        [InlineData("a : b  (  )", "a:b()")]
        [InlineData("a  '(  )'", "a '(  )'")]
        [InlineData("a . b  '(  )'", "a.b '(  )'")]
        [InlineData("a : b  '(  )'", "a:b '(  )'")]
        [InlineData("a  {   }", "a {}")]
        [InlineData("a . b  {   }", "a.b {}")]
        [InlineData("a : b  {   }", "a:b {}")]
        #endregion Prefix Expressions - Function Calls
        #region Prefix Expressions - Parenthesized
        [InterpolatedInlineData("""
                                (
                                {[(EXPR)]}
                                )
                                """,
                                "({[(EXPR)]})")]
        #endregion Prefix Expressions - Parenthesized
        #region Prefix Expressions - Variable Expression
        [InlineData("""
                    a
                         .
                              a
                    """,
                    "a.a")]
        [InterpolatedInlineData("""
                                a



                                [



                                    {[(EXPR)]}



                                ]

                                """,
                                "a[{[(EXPR)]}]")]
        #endregion Prefix Expressions - Variable Expression
        #endregion Prefix Expressions
        #region Table Constructors
        [InlineData("{ }", "{}")]
        [InterpolatedInlineData("""
                                {

                                    a              =           2;


                                    [{[(EXPR)]}]
                                    =              3,
                                     4 }
                                """,
                                "{ a = 2; [{[(EXPR)]}] = 3, 4 }")]
        [InlineData("   {   a= function() end   }   ",
                    """
                    {
                        a = function()
                        end
                    }
                    """)]
        [InlineData("   {   [function()end]= function() end   }   ",
                    """
                    {
                        [function()
                        end] = function()
                        end
                    }
                    """)]
        #endregion Table Constructors
        #region Type Casts
        [InterpolatedInlineData("{[(EXPR)]}  ::  {[(TYPE)]}",
                                "{[(EXPR)]} :: {[(TYPE)]}")]
        #endregion Type Casts
        #region Unary Rewrites
        [InterpolatedInlineData("!  {[(EXPR)]}",
                                "!{[(EXPR)]}")]
        [InterpolatedInlineData("#  {[(EXPR)]}",
                                "#{[(EXPR)]}")]
        [InterpolatedInlineData("-  {[(EXPR)]}",
                                "-{[(EXPR)]}")]
        [InterpolatedInlineData("not  {[(EXPR)]}",
                                "not {[(EXPR)]}")]
        [InterpolatedInlineData("~  {[(EXPR)]}",
                                "~{[(EXPR)]}")]
        #endregion Unary Rewrites
        public void SyntaxNormalizer_CorrectlyRewritesExpressions(string input, string expected)
        {
            var root = ParseAndValidateExpression(input, s_luaParseOptions);

            AssertNormalizeCore(root, expected);
        }

        [Theory]
        #region Assignment Statement
        [InterpolatedInlineData("""
                                a
                                ,
                                b
                                ,
                                c
                                ,
                                d
                                =
                                {[(EXPR)]}
                                ,
                                {[(EXPR)]}
                                ,
                                {[(EXPR)]}
                                ,
                                {[(EXPR)]}
                                {[(;)]}
                                """,
                                """
                                a, b, c, d = {[(EXPR)]}, {[(EXPR)]}, {[(EXPR)]}, {[(EXPR)]}{[(;)]}
                                """)]
        #endregion Assignment Statement
        #region Break Statement
        [InterpolatedInlineData("   break   {[(;)]}", "break{[(;)]}")]
        #endregion Break Statement
        #region Compound Assignment Statement
        [InterpolatedInlineData("""
                                a
                                                          +=
                                {[(EXPR)]}
                                {[(;)]}
                                """,
                                """
                                a += {[(EXPR)]}{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                a
                                                          -=
                                {[(EXPR)]}
                                {[(;)]}
                                """,
                                """
                                a -= {[(EXPR)]}{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                a
                                                          *=
                                {[(EXPR)]}
                                {[(;)]}
                                """,
                                """
                                a *= {[(EXPR)]}{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                a
                                                          /=
                                {[(EXPR)]}
                                {[(;)]}
                                """,
                                """
                                a /= {[(EXPR)]}{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                a
                                                          %=
                                {[(EXPR)]}
                                {[(;)]}
                                """,
                                """
                                a %= {[(EXPR)]}{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                a
                                                          ..=
                                {[(EXPR)]}
                                {[(;)]}
                                """,
                                """
                                a ..= {[(EXPR)]}{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                a
                                                          ^=
                                {[(EXPR)]}
                                {[(;)]}
                                """,
                                """
                                a ^= {[(EXPR)]}{[(;)]}
                                """)]
        #endregion Compound Assignment Statement
        #region Continue Statement
        [InterpolatedInlineData("   continue   {[(;)]}", "continue{[(;)]}")]
        #endregion Continue Statement
        #region Do Statement
        [InterpolatedInlineData("""
                                                             do
                                                                                                                               end
                                {[(;)]}
                                """,
                                """
                                do
                                end{[(;)]}
                                """)]
        #endregion Do Statement
        #region Empty Statement
        [InlineData("     ;", ";")]
        #endregion Empty Statement
        #region Expression Statement
        [InterpolatedInlineData("""
                                a()
                                {[(;)]}
                                """,
                                "a(){[(;)]}")]
        #endregion Expression Statement
        #region Function Declaration Statement
        [InterpolatedInlineData("""
                                   function
                                   name
                                   (
                                   )
                                   end
                                   {[(;)]}
                                """,
                                """
                                function name()
                                end{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                   function
                                   name
                                   .    inner
                                   (
                                   )
                                   end
                                   {[(;)]}
                                """,
                                """
                                function name.inner()
                                end{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                   function
                                   name
                                   :    inner
                                   (
                                   )
                                   end
                                   {[(;)]}
                                """,
                                """
                                function name:inner()
                                end{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                   function
                                   name
                                   (
                                      arg
                                   )
                                   end
                                   {[(;)]}
                                """,
                                """
                                function name(arg)
                                end{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                   function
                                   name
                                   .    inner
                                   (
                                    arg
                                   )
                                   end
                                   {[(;)]}
                                """,
                                """
                                function name.inner(arg)
                                end{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                   function
                                   name
                                   :    inner
                                   (
                                    arg
                                   )
                                   end
                                   {[(;)]}
                                """,
                                """
                                function name:inner(arg)
                                end{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                   function
                                   name
                                   <

                                   T1
                                    ,

                                   T2

                                   >
                                   (
                                      arg                               :
                                   T1                                    ) :
                                   T2
                                   end
                                   {[(;)]}
                                """,
                                """
                                function name<T1, T2>(arg: T1): T2
                                end{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                   function
                                   name
                                   .    inner
                                   <

                                   T1
                                    ,

                                   T2
                                   >
                                   (
                                      arg                               :
                                   T1                                    ) :
                                   T2
                                   end
                                   {[(;)]}
                                """,
                                """
                                function name.inner<T1, T2>(arg: T1): T2
                                end{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                   function
                                   name
                                   :    inner
                                   <

                                   T1
                                    ,

                                   T2
                                   >
                                   (
                                      arg                               :
                                   T1                                    ) :
                                   T2
                                   end
                                   {[(;)]}
                                """,
                                """
                                function name:inner<T1, T2>(arg: T1): T2
                                end{[(;)]}
                                """)]
        #endregion Function Declaration Statement
        #region Generic For Statement
        [InterpolatedInlineData("""
                                    for
                                x
                                     ,
                                y
                                ,
                                z in
                                {[(EXPR)]}
                                                   ,
                                 {[(EXPR)]}
                                ,
                                               {[(EXPR)]}
                                do local x=1 end
                                {[(;)]}
                                """,
                                """
                                for x, y, z in {[(EXPR)]}, {[(EXPR)]}, {[(EXPR)]} do
                                    local x = 1
                                end{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                    for
                                x
                                :   T,
                                y :
                                T,
                                z : T in
                                {[(EXPR)]}
                                                   ,
                                 {[(EXPR)]}
                                ,
                                               {[(EXPR)]}
                                do local x=1 end
                                {[(;)]}
                                """,
                                """
                                for x: T, y: T, z: T in {[(EXPR)]}, {[(EXPR)]}, {[(EXPR)]} do
                                    local x = 1
                                end{[(;)]}
                                """)]
        #endregion Generic For Statement
        #region Goto Label Statement
        [InterpolatedInlineData("""
                                ::

                                        LABEL

                                ::

                                {[(;)]}
                                """,
                                "::LABEL::{[(;)]}")]
        #endregion Goto Label Statement
        #region Goto Statement
        [InterpolatedInlineData("""
                                goto


                                                        LABEL

                                                        {[(;)]}
                                """,
                                "goto LABEL{[(;)]}")]
        #endregion Goto Statement
        #region If Statement
        [InterpolatedInlineData("""
                                    if
                                    {[(EXPR)]}                                                            then local
                                 x end
                                 {[(;)]}
                                """,
                                """
                                if {[(EXPR)]} then
                                    local x
                                end{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                    if
                                    {[(EXPR)]}                                                            then
                                 local x else local x end
                                 {[(;)]}
                                """,
                                """
                                if {[(EXPR)]} then
                                    local x
                                else
                                    local x
                                end{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                    if
                                    {[(EXPR)]}                                                            then
                                 local x elseif
                                                          {[(EXPR)]} then local                           x
                                 else local x end
                                 {[(;)]}
                                """,
                                """
                                if {[(EXPR)]} then
                                    local x
                                elseif {[(EXPR)]} then
                                    local x
                                else
                                    local x
                                end{[(;)]}
                                """)]
        #endregion If Statement
        #region Local Function Declaration Statement
        [InterpolatedInlineData("""
                                   local      function
                                   name
                                   (
                                   )
                                   end
                                   {[(;)]}
                                """,
                                """
                                local function name()
                                end{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                   local      function
                                   name
                                   (
                                arg
                                   )
                                   end
                                   {[(;)]}
                                """,
                                """
                                local function name(arg)
                                end{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                   local      function
                                   name
                                   <T1               ,
                                   T2                >
                                   (
                                arg
                                   :T1):T2
                                   end
                                   {[(;)]}
                                """,
                                """
                                local function name<T1, T2>(arg: T1): T2
                                end{[(;)]}
                                """)]
        #endregion Local Function Declaration Statement
        #region Local Variable Declaration Statement
        [InterpolatedInlineData("""
                                local
                                   x
                                <     const
                                          >
                                          {[(;)]}
                                """,
                                """
                                local x <const>{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                local
                                   x
                                <     const
                                          >,
                                y       <
                                const       >,
                                z<const>
                                          {[(;)]}
                                """,
                                """
                                local x <const>, y <const>, z <const>{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                local
                                   x
                                :
                                          {[(TYPE)]}
                                          {[(;)]}
                                """,
                                """
                                local x: {[(TYPE)]}{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                local
                                   x
                                :
                                          {[(TYPE)]}
                                , y :
                                {[(TYPE)]}         ,
                                z
                                :
                                {[(TYPE)]}
                                          {[(;)]}
                                """,
                                """
                                local x: {[(TYPE)]}, y: {[(TYPE)]}, z: {[(TYPE)]}{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                local
                                   x
                                <     const
                                          >
                                =
                                            {[(EXPR)]}
                                          {[(;)]}
                                """,
                                """
                                local x <const> = {[(EXPR)]}{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                local
                                   x
                                <     const
                                          >,
                                y       <
                                const       >,
                                z<const>
                                =
                                {[(EXPR)]}
                                , {[(EXPR)]}
                                , {[(EXPR)]}
                                          {[(;)]}
                                """,
                                """
                                local x <const>, y <const>, z <const> = {[(EXPR)]}, {[(EXPR)]}, {[(EXPR)]}{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                local
                                   x
                                :
                                          {[(TYPE)]}

                                =

                                {[(EXPR)]}
                                          {[(;)]}
                                """,
                                """
                                local x: {[(TYPE)]} = {[(EXPR)]}{[(;)]}
                                """)]
        // IMPORTANT: We explicitly don't use {[(TYPE)]} here because it makes the tests take over
        //            2 minutes to run due to the amount of combinations (it resulted in 85750
        //            combinations with {[(TYPE)]} instead of T as it had 3 {[(EXPR)]}, 3 {[(TYPE)]}
        //            and 1 {[(;)]} which resulted in 5 * 5 * 5 * 7 * 7 * 7 * 2 test cases).
        [InterpolatedInlineData("""
                                local
                                   x
                                :
                                          T
                                , y :
                                T         ,
                                z
                                :
                                T
                                =
                                {[(EXPR)]}
                                , {[(EXPR)]}
                                , {[(EXPR)]}
                                          {[(;)]}
                                """,
                                """
                                local x: T, y: T, z: T = {[(EXPR)]}, {[(EXPR)]}, {[(EXPR)]}{[(;)]}
                                """)]
        #endregion Local Variable Declaration Statement
        #region Numeric For Statement
        [InterpolatedInlineData("""
                                    for
                                x
                                   =
                                {[(EXPR)]}
                                                   ,
                                 {[(EXPR)]}
                                do local x=1 end
                                {[(;)]}
                                """,
                                """
                                for x = {[(EXPR)]}, {[(EXPR)]} do
                                    local x = 1
                                end{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                    for
                                x
                                   =
                                {[(EXPR)]}
                                                   ,
                                 {[(EXPR)]}
                                ,
                                               {[(EXPR)]}
                                do local x=1 end
                                {[(;)]}
                                """,
                                """
                                for x = {[(EXPR)]}, {[(EXPR)]}, {[(EXPR)]} do
                                    local x = 1
                                end{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                    for
                                x
                                :   {[(TYPE)]}=
                                {[(EXPR)]}
                                                   ,
                                 {[(EXPR)]}
                                do local x=1 end
                                {[(;)]}
                                """,
                                """
                                for x: {[(TYPE)]} = {[(EXPR)]}, {[(EXPR)]} do
                                    local x = 1
                                end{[(;)]}
                                """)]
        [InterpolatedInlineData("""
                                    for
                                x
                                :   {[(TYPE)]}=
                                {[(EXPR)]}
                                                   ,
                                 {[(EXPR)]}
                                ,
                                               {[(EXPR)]}
                                do local x=1 end
                                {[(;)]}
                                """,
                                """
                                for x: {[(TYPE)]} = {[(EXPR)]}, {[(EXPR)]}, {[(EXPR)]} do
                                    local x = 1
                                end{[(;)]}
                                """)]
        #endregion Numeric For Statement
        #region Repeat Until Statement
        [InterpolatedInlineData("""
                                    repeat
                                 local
                                 x
                                 =
                                 1 until
                                 {[(EXPR)]}
                                 {[(;)]}
                                """,
                                """
                                repeat
                                    local x = 1
                                until {[(EXPR)]}{[(;)]}
                                """)]
        #endregion Repeat Until Statement
        #region Return Statement
        [InterpolatedInlineData("""
                                   return
                                {[(EXPR)]}
                                {[(;)]}
                                """,
                                "return {[(EXPR)]}{[(;)]}")]
        [InterpolatedInlineData("""
                                   return
                                {[(EXPR)]}
                                   ,
                                {[(EXPR)]}
                                {[(;)]}
                                """,
                                "return {[(EXPR)]}, {[(EXPR)]}{[(;)]}")]
        #endregion Return Statement
        #region Type Declaration Statement
        [InterpolatedInlineData("""
                                  export
                                  type
                                T
                                  <
                                  T1

                                  =

                                  {[(TYPE)]}
                                  ,
                                  T2

                                  =

                                  {[(TYPE)]}
                                  >
                                  =

                                     {[(TYPE)]}

                                {[(;)]}
                                """,
                                "export type T<T1 = {[(TYPE)]}, T2 = {[(TYPE)]}> = {[(TYPE)]}{[(;)]}")]
        [InterpolatedInlineData("""

                                   type
                                 T
                                   <
                                   T1

                                   =
                                   {[(TYPE)]}

                                   ,
                                   T2

                                   =

                                   {[(TYPE)]}
                                   >


                                   =

                                      {[(TYPE)]}
                                {[(;)]}
                                """,
                                "type T<T1 = {[(TYPE)]}, T2 = {[(TYPE)]}> = {[(TYPE)]}{[(;)]}")]
        #endregion Type Declaration Statement
        #region While Statement
        [InterpolatedInlineData("""
                                   while
                                      {[(EXPR)]}
                                   do
                                local x = 1
                                   end
                                   {[(;)]}
                                """,
                                """
                                while {[(EXPR)]} do
                                    local x = 1
                                end{[(;)]}
                                """)]
        #endregion While Statement
        public void SyntaxNormalizer_CorrectlyRewritesStatements(string input, string expected)
        {
            var tree = ParseAndValidate(input, s_luaParseOptions);
            var root = tree.GetRoot();

            AssertNormalizeCore(root, expected);
        }

        [Theory]
        #region Function Type
        [InlineData("""
                    (
                           T
                              )
                                  ->
                                      T

                    """, "(T) -> T")]
        [InlineData("""
                    <

                    T1
                        =
                            T2,

                            T2

                    =

                            T1

                    >

                    (

                        T1

                        , T2
                    )

                    ->

                    (T1,

                    T2)

                    """,
                    "<T1 = T2, T2 = T1>(T1, T2) -> (T1, T2)")]
        #endregion Function Type
        #region Generic Type Pack
        [InlineData("""
                    T<T

                    ...>
                    """,
                    "T<T...>")]
        #endregion Generic Type Pack
        #region Intersection Type
        [InterpolatedInlineData("""
                                {[(TYPE)]}

                                        &

                                {[(TYPE)]}
                                """,
                                "{[(TYPE)]} & {[(TYPE)]}")]
        #endregion Intersection Type
        #region Literal Types
        [InlineData("   false   ", "false")]
        [InlineData("   nil   ", "nil")]
        [InlineData("   true   ", "true")]
        [InlineData("   'true'   ", "'true'")]
        #endregion Literal Types
        #region Nilable Types
        [InterpolatedInlineData("""
                                {[(TYPE)]}

                                ?

                                """,
                                "{[(TYPE)]}?")]
        #endregion Nilable Types
        #region Parenthesized Type
        [InterpolatedInlineData("""
                                (


                                    {[(TYPE)]}



                                )

                                """,
                                "({[(TYPE)]})")]
        #endregion Parenthesized Type
        #region Table Based Type
        [InterpolatedInlineData("""
                                {

                                    [
                                        {[(TYPE)]}
                                    ]

                                    :

                                    {[(TYPE)]}
                                }
                                """,
                                "{ [{[(TYPE)]}]: {[(TYPE)]} }")]
        [InterpolatedInlineData("""
                                {

                                    x

                                    :

                                    {[(TYPE)]}


                                    ,

                                    y

                                    :

                                    {[(TYPE)]}
                                }
                                """,
                                "{ x: {[(TYPE)]}, y: {[(TYPE)]} }")]
        [InterpolatedInlineData("""
                                {


                                    {[(TYPE)]}



                                }
                                """,
                                "{ {[(TYPE)]} }")]
        #endregion Table Based Type
        #region Type Name
        [InlineData("""


                    T

                    """,
                    "T")]
        [InterpolatedInlineData("""


                                T


                                <


                                {[(TYPE)]}


                                >

                                """,
                                "T<{[(TYPE)]}>")]
        [InlineData("""
                    T

                    .

                    Inner

                    """,
                    "T.Inner")]
        [InterpolatedInlineData("""
                                T

                                .

                                Inner


                                <


                                {[(TYPE)]}


                                >

                                """,
                                "T.Inner<{[(TYPE)]}>")]
        [InlineData("""
                    T

                    .

                    Inner

                    .

                    Inner

                    .

                    Inner

                    .

                    Inner

                    """,
                    "T.Inner.Inner.Inner.Inner")]
        [InterpolatedInlineData("""
                                T

                                .

                                Inner

                                .

                                Inner

                                .

                                Inner

                                .

                                Inner


                                <


                                {[(TYPE)]}


                                >

                                """,
                                "T.Inner.Inner.Inner.Inner<{[(TYPE)]}>")]
        #endregion Type Name
        #region Type Pack
        [InterpolatedInlineData("""
                                () -> (

                                    {[(TYPE)]},

                                    {[(TYPE)]},

                                    {[(TYPE)]}
                                )
                                """,
                                "() -> ({[(TYPE)]}, {[(TYPE)]}, {[(TYPE)]})")]
        #endregion Type Pack
        #region Typeof Type
        [InterpolatedInlineData("""

                                typeof

                                (


                                    {[(EXPR)]}


                                )

                                """,
                                "typeof({[(EXPR)]})")]
        #endregion Typeof Type
        #region Union Type
        [InterpolatedInlineData("""
                                {[(TYPE)]}

                                        |

                                {[(TYPE)]}
                                """,
                                "{[(TYPE)]} | {[(TYPE)]}")]
        #endregion Union Type
        public void SyntaxNormalizer_CorrectlyRewritesTypes(string input, string expected)
        {
            var type = ParseAndValidateType(input, s_luaParseOptions);

            AssertNormalizeCore(type, expected);
        }

        [Fact]
        [WorkItem(108, "https://github.com/LorettaDevs/Loretta/issues/108")]
        public void SyntaxNormalizer_CorrectlyInsertsExpressionSpaces()
        {
            var tree = ParseAndValidate("print(1,2)", s_luaParseOptions);
            var root = tree.GetRoot();

            AssertNormalizeCore(root, "print(1, 2)");
        }

        [Theory]
        [WorkItem(117, "https://github.com/LorettaDevs/Loretta/issues/117")]
        [InlineData("""
                    string_format(
                        "%s %s",
                        "test", -- comment here
                        "test2"
                    )
                    """,
                    """
                    string_format("%s %s", "test", -- comment here
                    "test2")
                    """)]
        [InlineData("""
                    string_format(
                        "test", -- comment here
                        "%s %s",
                        "test2"
                    )
                    """,
                    """
                    string_format("test", -- comment here
                    "%s %s", "test2")
                    """)]
        [InlineData("""
                    string_format(
                        "%s %s",
                        "test2",
                        "test" -- comment here
                    )
                    """,
                    """
                    string_format("%s %s", "test2", "test" -- comment here
                    )
                    """)]
        [InlineData("""
                    string_format(
                        "%s %s",
                        "test", --[[ comment here ]]
                        "test2"
                    )
                    """,
                    """
                    string_format("%s %s", "test", --[[ comment here ]] "test2")
                    """)]
        [InlineData("""
                    string_format(
                        "test", --[[ comment here ]]
                        "%s %s",
                        "test2"
                    )
                    """,
                    """
                    string_format("test", --[[ comment here ]] "%s %s", "test2")
                    """)]
        [InlineData("""
                    string_format(
                        "%s %s",
                        "test2",
                        "test" --[[ comment here ]]
                    )
                    """,
                    """
                    string_format("%s %s", "test2", "test" --[[ comment here ]])
                    """)]
        public void SyntaxNormalizer_CorrectlyAddsLineBreaksAfterSingleLineComments(string input, string expected)
        {
            var tree = ParseAndValidate(input, s_luaParseOptions);
            var root = tree.GetRoot();

            AssertNormalizeCore(root, expected);
        }

        #region Class Implementation Details

        private static void AssertNormalizeCore(SyntaxNode node, string expected)
        {
            node = node.NormalizeWhitespace(indentation: "    ", eol: Environment.NewLine);
            Assert.Equal(expected, node.ToFullString());
        }

        private sealed class InterpolatedInlineData : DataAttribute
        {
            private static readonly ImmutableArray<KeyValuePair<string, string>> s_expressions = ImmutableArray.CreateRange(new[]
                {
                    new KeyValuePair<string, string>("...", "..."),
                    new KeyValuePair<string, string>("`aaa`", "`aaa`"),
                    new KeyValuePair<string, string>("a", "a"),
                    new KeyValuePair<string, string>("1", "1"),
                    new KeyValuePair<string, string>("'hi'", "'hi'"),
                });

            private static readonly ImmutableArray<KeyValuePair<string, string>> s_types = ImmutableArray.CreateRange(new[]
            {
                new KeyValuePair<string, string>("Type", "Type"),
                new KeyValuePair<string, string>("Type   .   SubType", "Type.SubType"),
                new KeyValuePair<string, string>("(   T   )    ->    T", "(T) -> T"),
                new KeyValuePair<string, string>("{   }", "{}"),
                new KeyValuePair<string, string>("{[ T ]:T}", "{ [T]: T }"),
                new KeyValuePair<string, string>("{x:T,y:T}", "{ x: T, y: T }"),
                new KeyValuePair<string, string>("typeof   (   'hi'   )", "typeof('hi')"),
            });

            private static readonly ImmutableArray<KeyValuePair<string, string>> s_semicolons = ImmutableArray.CreateRange(new[]
            {
                new KeyValuePair<string, string>("   ;   ", ";"),
                new KeyValuePair<string, string>("   ", ""),
            });

            private readonly string _inputTemplate, _expectedTemplate;

            public InterpolatedInlineData(string inputTemplate, string expectedTemplate)
            {
                _inputTemplate = inputTemplate;
                _expectedTemplate = expectedTemplate;
            }

            public override IEnumerable<object[]> GetData(MethodInfo testMethod)
            {
                using var inputEnumerator = CombineExprHoles(_inputTemplate, false);
                using var expectedEnumerator = CombineExprHoles(_expectedTemplate, true);

                while (inputEnumerator.MoveNext())
                {
                    if (!expectedEnumerator.MoveNext())
                        throw new InvalidOperationException($"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA: {_inputTemplate}  ---- {_expectedTemplate}");

                    yield return new object[] { inputEnumerator.Current, expectedEnumerator.Current };
                }

                if (expectedEnumerator.MoveNext())
                    throw new InvalidOperationException($"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA: {_inputTemplate}  ---- {_expectedTemplate}");
            }

            private static IEnumerator<string> CombineExprHoles(string input, bool isExpected)
            {
                var matches = Regex.Matches(input, @"\{\[\((?:EXPR|TYPE|;)\)\]\}");
                var holes = new byte[matches.Count];
                var builder = new StringBuilder();

                do
                {
                    builder.Clear().Append(input);
                    for (var idx = holes.Length - 1; idx >= 0; idx--)
                    {
                        var match = matches[idx];
                        var pair = arrayForHole(match.Value)[holes[idx]];
                        builder.Replace(match.Value, isExpected ? pair.Value : pair.Key, match.Index, match.Length);
                    }
                    yield return builder.ToString();
                }
                while (advance());

                bool advance()
                {
                    bool carry;
                    var idx = holes!.Length - 1;
                    do
                    {
                        if (idx < 0)
                            return false;

                        carry = false;
                        ref var val = ref holes[idx];
                        val += 1;
                        if (val >= arrayForHole(matches[idx].Value).Length)
                        {
                            val = 0;
                            carry = true;
                        }

                        idx--;
                    }
                    while (carry);

                    return true;
                }

                ImmutableArray<KeyValuePair<string, string>> arrayForHole(string value)
                {
                    return value switch
                    {
                        "{[(EXPR)]}" => s_expressions,
                        "{[(TYPE)]}" => s_types,
                        "{[(;)]}" => s_semicolons,
                        _ => throw new InvalidOperationException($"{value} is not a valid placeholder.")
                    };
                }
            }
        }

        #endregion Class Implementation Details
    }
}
