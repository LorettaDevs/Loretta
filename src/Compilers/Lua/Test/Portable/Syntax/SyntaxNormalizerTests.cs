// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;
using System.Text.RegularExpressions;
using Loretta.CodeAnalysis.Lua.Test.Utilities;
using Loretta.Test.Utilities;

namespace Loretta.CodeAnalysis.Lua.UnitTests;

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

    [Test]

    #region Anonymous Functions

    [Arguments(
        """
        function
        (

        )

        end

        """,
        """
        function()
        end
        """)]
    [Arguments(
        """
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

    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments = [" {[(EXPR)]}   &&   {[(EXPR)]} ", "{[(EXPR)]} && {[(EXPR)]}"])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments = [" {[(EXPR)]}   &   {[(EXPR)]} ", "{[(EXPR)]} & {[(EXPR)]}"])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments = [" {[(EXPR)]}   and   {[(EXPR)]} ", "{[(EXPR)]} and {[(EXPR)]}"])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments = [" {[(EXPR)]}   !=   {[(EXPR)]} ", "{[(EXPR)]} != {[(EXPR)]}"])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments = [" {[(EXPR)]}   ..   {[(EXPR)]} ", "{[(EXPR)]} .. {[(EXPR)]}"])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments = [" {[(EXPR)]}   ==   {[(EXPR)]} ", "{[(EXPR)]} == {[(EXPR)]}"])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments = [" {[(EXPR)]}   >=   {[(EXPR)]} ", "{[(EXPR)]} >= {[(EXPR)]}"])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments = [" {[(EXPR)]}   >>   {[(EXPR)]} ", "{[(EXPR)]} >> {[(EXPR)]}"])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments = [" {[(EXPR)]}   >   {[(EXPR)]} ", "{[(EXPR)]} > {[(EXPR)]}"])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments = [" {[(EXPR)]}   ^   {[(EXPR)]} ", "{[(EXPR)]} ^ {[(EXPR)]}"])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments = [" {[(EXPR)]}   <=   {[(EXPR)]} ", "{[(EXPR)]} <= {[(EXPR)]}"])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments = [" {[(EXPR)]}   <<   {[(EXPR)]} ", "{[(EXPR)]} << {[(EXPR)]}"])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments = [" {[(EXPR)]}   <   {[(EXPR)]} ", "{[(EXPR)]} < {[(EXPR)]}"])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments = [" {[(EXPR)]}   -   {[(EXPR)]} ", "{[(EXPR)]} - {[(EXPR)]}"])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments = [" {[(EXPR)]}   or   {[(EXPR)]} ", "{[(EXPR)]} or {[(EXPR)]}"])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments = [" {[(EXPR)]}   %   {[(EXPR)]} ", "{[(EXPR)]} % {[(EXPR)]}"])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments = [" {[(EXPR)]}   ||   {[(EXPR)]} ", "{[(EXPR)]} || {[(EXPR)]}"])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments = [" {[(EXPR)]}   |   {[(EXPR)]} ", "{[(EXPR)]} | {[(EXPR)]}"])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments = [" {[(EXPR)]}   +   {[(EXPR)]} ", "{[(EXPR)]} + {[(EXPR)]}"])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments = [" {[(EXPR)]}   /   {[(EXPR)]} ", "{[(EXPR)]} / {[(EXPR)]}"])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments = [" {[(EXPR)]}   *   {[(EXPR)]} ", "{[(EXPR)]} * {[(EXPR)]}"])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments = [" {[(EXPR)]}  ~=   {[(EXPR)]} ", "{[(EXPR)]} ~= {[(EXPR)]}"])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments = [" {[(EXPR)]}   ~   {[(EXPR)]} ", "{[(EXPR)]} ~ {[(EXPR)]}"])]

    #endregion Binary Expressions

    #region If Expressions

    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
            if   {[(EXPR)]}
            then
            {[(EXPR)]}
            else
            {[(EXPR)]}

            """,
            "if {[(EXPR)]} then {[(EXPR)]} else {[(EXPR)]}"
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
            if   {[(EXPR)]}
            then
            {[(EXPR)]}
            elseif
            {[(EXPR)]}   then   {[(EXPR)]}
            else
            {[(EXPR)]}

            """,
            "if {[(EXPR)]} then {[(EXPR)]} elseif {[(EXPR)]} then {[(EXPR)]} else {[(EXPR)]}"
        ])]

    #endregion If Expressions

    #region VarArg/Literal Expressions

    [MethodDataSource(nameof(GetInterpolatedPairs), Arguments = [" {[(EXPR)]} ", "{[(EXPR)]}"])]

    #endregion VarArg/Literal Expressions

    #region Prefix Expressions

    #region Prefix Expressions - Function Calls

    [Arguments("a  (  )", "a()")]
    [Arguments("a . b  (  )", "a.b()")]
    [Arguments("a : b  (  )", "a:b()")]
    [Arguments("a  '(  )'", "a '(  )'")]
    [Arguments("a . b  '(  )'", "a.b '(  )'")]
    [Arguments("a : b  '(  )'", "a:b '(  )'")]
    [Arguments("a  {   }", "a {}")]
    [Arguments("a . b  {   }", "a.b {}")]
    [Arguments("a : b  {   }", "a:b {}")]

    #endregion Prefix Expressions - Function Calls

    #region Prefix Expressions - Parenthesized

    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
            (
            {[(EXPR)]}
            )
            """,
            "({[(EXPR)]})"
        ])]

    #endregion Prefix Expressions - Parenthesized

    #region Prefix Expressions - Variable Expression

    [Arguments(
        """
        a
             .
                  a
        """,
        "a.a")]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
            a



            [
            
            
            
                {[(EXPR)]}



            ]

            """,
            "a[{[(EXPR)]}]"
        ])]

    #endregion Prefix Expressions - Variable Expression

    #endregion Prefix Expressions

    #region Table Constructors

    [Arguments("{ }", "{}")]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
            {
            
                a              =           2;
            
            
                [{[(EXPR)]}]
                =              3,
                 4 }
            """,
            "{ a = 2; [{[(EXPR)]}] = 3, 4 }"
        ])]
    [Arguments(
        "   {   a= function() end   }   ",
        """
        {
            a = function()
            end
        }
        """)]
    [Arguments(
        "   {   [function()end]= function() end   }   ",
        """
        {
            [function()
            end] = function()
            end
        }
        """)]

    #endregion Table Constructors

    #region Type Casts

    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments = ["{[(EXPR)]}  ::  {[(TYPE)]}", "{[(EXPR)]} :: {[(TYPE)]}"])]

    #endregion Type Casts

    #region Unary Rewrites

    [MethodDataSource(nameof(GetInterpolatedPairs), Arguments = ["!  {[(EXPR)]}", "!{[(EXPR)]}"])]
    [MethodDataSource(nameof(GetInterpolatedPairs), Arguments = ["#  {[(EXPR)]}", "#{[(EXPR)]}"])]
    [MethodDataSource(nameof(GetInterpolatedPairs), Arguments = ["-  {[(EXPR)]}", "-{[(EXPR)]}"])]
    [MethodDataSource(nameof(GetInterpolatedPairs), Arguments = ["not  {[(EXPR)]}", "not {[(EXPR)]}"])]
    [MethodDataSource(nameof(GetInterpolatedPairs), Arguments = ["~  {[(EXPR)]}", "~{[(EXPR)]}"])]

    #endregion Unary Rewrites

    public async Task SyntaxNormalizer_CorrectlyRewritesExpressions(string input, string expected)
    {
        var root = await ParseAndValidateExpressionAsync(input, s_luaParseOptions);
        await AssertNormalizeCoreAsync(root, expected);
    }

    [Test]

    #region Assignment Statement

    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]

    #endregion Assignment Statement

    #region Break Statement

    [MethodDataSource(nameof(GetInterpolatedPairs), Arguments = ["   break   {[(;)]}", "break{[(;)]}"])]

    #endregion Break Statement

    #region Compound Assignment Statement

    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
            a
                                      +=
            {[(EXPR)]}
            {[(;)]}
            """,
            """
            a += {[(EXPR)]}{[(;)]}
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
            a
                                      -=
            {[(EXPR)]}
            {[(;)]}
            """,
            """
            a -= {[(EXPR)]}{[(;)]}
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
            a
                                      *=
            {[(EXPR)]}
            {[(;)]}
            """,
            """
            a *= {[(EXPR)]}{[(;)]}
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
            a
                                      /=
            {[(EXPR)]}
            {[(;)]}
            """,
            """
            a /= {[(EXPR)]}{[(;)]}
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
            a
                                      %=
            {[(EXPR)]}
            {[(;)]}
            """,
            """
            a %= {[(EXPR)]}{[(;)]}
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
            a
                                      ..=
            {[(EXPR)]}
            {[(;)]}
            """,
            """
            a ..= {[(EXPR)]}{[(;)]}
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
            a
                                      ^=
            {[(EXPR)]}
            {[(;)]}
            """,
            """
            a ^= {[(EXPR)]}{[(;)]}
            """
        ])]

    #endregion Compound Assignment Statement

    #region Continue Statement

    [MethodDataSource(nameof(GetInterpolatedPairs), Arguments = ["   continue   {[(;)]}", "continue{[(;)]}"])]

    #endregion Continue Statement

    #region Do Statement

    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
                                         do
                                                                                                           end
            {[(;)]}
            """,
            """
            do
            end{[(;)]}
            """
        ])]

    #endregion Do Statement

    #region Empty Statement

    [Arguments("     ;", ";")]

    #endregion Empty Statement

    #region Expression Statement

    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
            a()
            {[(;)]}
            """,
            "a(){[(;)]}"
        ])]

    #endregion Expression Statement

    #region Function Declaration Statement

    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]

    #endregion Function Declaration Statement

    #region Generic For Statement

    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]

    #endregion Generic For Statement

    #region Goto Label Statement

    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
            ::
            
                    LABEL

            ::

            {[(;)]}
            """,
            "::LABEL::{[(;)]}"
        ])]

    #endregion Goto Label Statement

    #region Goto Statement

    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
            goto
            
            
                                    LABEL
            
                                    {[(;)]}
            """,
            "goto LABEL{[(;)]}"
        ])]

    #endregion Goto Statement

    #region If Statement

    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
                if
                {[(EXPR)]}                                                            then local
             x end
             {[(;)]}
            """,
            """
            if {[(EXPR)]} then
                local x
            end{[(;)]}
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]

    #endregion If Statement

    #region Local Function Declaration Statement

    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]

    #endregion Local Function Declaration Statement

    #region Local Variable Declaration Statement

    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
            local
               x
            <     const
                      >
                      {[(;)]}
            """,
            """
            local x <const>{[(;)]}
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
            local
               x
            :
                      {[(TYPE)]}
                      {[(;)]}
            """,
            """
            local x: {[(TYPE)]}{[(;)]}
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]
    // IMPORTANT: We explicitly don't use {[(TYPE)]} here because it makes the tests take over
    //            2 minutes to run due to the amount of combinations (it resulted in 85750
    //            combinations with {[(TYPE)]} instead of T as it had 3 {[(EXPR)]}, 3 {[(TYPE)]}
    //            and 1 {[(;)]} which resulted in 5 * 5 * 5 * 7 * 7 * 7 * 2 test cases).
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]

    #endregion Local Variable Declaration Statement

    #region Numeric For Statement

    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]

    #endregion Numeric For Statement

    #region Repeat Until Statement

    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]

    #endregion Repeat Until Statement

    #region Return Statement

    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
               return
            {[(EXPR)]}
            {[(;)]}
            """,
            "return {[(EXPR)]}{[(;)]}"
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
               return
            {[(EXPR)]}
               ,
            {[(EXPR)]}
            {[(;)]}
            """,
            "return {[(EXPR)]}, {[(EXPR)]}{[(;)]}"
        ])]

    #endregion Return Statement

    #region Type Declaration Statement

    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            "export type T<T1 = {[(TYPE)]}, T2 = {[(TYPE)]}> = {[(TYPE)]}{[(;)]}"
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
            
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
            "type T<T1 = {[(TYPE)]}, T2 = {[(TYPE)]}> = {[(TYPE)]}{[(;)]}"
        ])]

    #endregion Type Declaration Statement

    #region While Statement

    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            """
        ])]

    #endregion While Statement

    public async Task SyntaxNormalizer_CorrectlyRewritesStatements(string input, string expected)
    {
        var tree = await ParseAndValidateAsync(input, s_luaParseOptions);
        var root = await tree.GetRootAsync();

        await AssertNormalizeCoreAsync(root, expected);
    }

    [Test]

    #region Function Type

    [Arguments(
        """
        (
               T
                  )
                      ->
                          T

        """,
        "(T) -> T")]
    [Arguments(
        """
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

    [Arguments(
        """
        T<T

        ...>
        """,
        "T<T...>")]

    #endregion Generic Type Pack

    #region Intersection Type

    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
            {[(TYPE)]}
            
                    &

            {[(TYPE)]}
            """,
            "{[(TYPE)]} & {[(TYPE)]}"
        ])]

    #endregion Intersection Type

    #region Literal Types

    [Arguments("   false   ", "false")]
    [Arguments("   nil   ", "nil")]
    [Arguments("   true   ", "true")]
    [Arguments("   'true'   ", "'true'")]

    #endregion Literal Types

    #region Nilable Types

    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
            {[(TYPE)]}

            ?

            """,
            "{[(TYPE)]}?"
        ])]

    #endregion Nilable Types

    #region Parenthesized Type

    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
            (
            
            
                {[(TYPE)]}



            )

            """,
            "({[(TYPE)]})"
        ])]

    #endregion Parenthesized Type

    #region Table Based Type

    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
            {
            
                [
                    {[(TYPE)]}
                ]
            
                :
            
                {[(TYPE)]}
            }
            """,
            "{ [{[(TYPE)]}]: {[(TYPE)]} }"
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            "{ x: {[(TYPE)]}, y: {[(TYPE)]} }"
        ])]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
            {
            
            
                {[(TYPE)]}



            }
            """,
            "{ {[(TYPE)]} }"
        ])]

    #endregion Table Based Type

    #region Type Name

    [Arguments(
        """


        T

        """,
        "T")]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """


            T


            <


            {[(TYPE)]}


            >

            """,
            "T<{[(TYPE)]}>"
        ])]
    [Arguments(
        """
        T

        .

        Inner

        """,
        "T.Inner")]
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
            T

            .

            Inner


            <


            {[(TYPE)]}


            >

            """,
            "T.Inner<{[(TYPE)]}>"
        ])]
    [Arguments(
        """
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
    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
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
            "T.Inner.Inner.Inner.Inner<{[(TYPE)]}>"
        ])]

    #endregion Type Name

    #region Type Pack

    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
            () -> (
            
                {[(TYPE)]},
            
                {[(TYPE)]},
            
                {[(TYPE)]}
            )
            """,
            "() -> ({[(TYPE)]}, {[(TYPE)]}, {[(TYPE)]})"
        ])]

    #endregion Type Pack

    #region Typeof Type

    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """

            typeof

            (
            
            
                {[(EXPR)]}


            )

            """,
            "typeof({[(EXPR)]})"
        ])]

    #endregion Typeof Type

    #region Union Type

    [MethodDataSource(
        nameof(GetInterpolatedPairs),
        Arguments =
        [
            """
            {[(TYPE)]}
            
                    |

            {[(TYPE)]}
            """,
            "{[(TYPE)]} | {[(TYPE)]}"
        ])]

    #endregion Union Type

    public async Task SyntaxNormalizer_CorrectlyRewritesTypes(string input, string expected)
    {
        var type = await ParseAndValidateTypeAsync(input, s_luaParseOptions);
        await AssertNormalizeCoreAsync(type, expected);
    }

    [Test]
    [WorkItem(108, "https://github.com/LorettaDevs/Loretta/issues/108")]
    public async Task SyntaxNormalizer_CorrectlyInsertsExpressionSpaces()
    {
        var tree = await ParseAndValidateAsync("print(1,2)", s_luaParseOptions);
        var root = await tree.GetRootAsync();

        await AssertNormalizeCoreAsync(root, "print(1, 2)");
    }

    [Test]
    [WorkItem(117, "https://github.com/LorettaDevs/Loretta/issues/117")]
    [Arguments(
        """
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
    [Arguments(
        """
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
    [Arguments(
        """
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
    [Arguments(
        """
        string_format(
            "%s %s",
            "test", --[[ comment here ]]
            "test2"
        )
        """,
        """
        string_format("%s %s", "test", --[[ comment here ]] "test2")
        """)]
    [Arguments(
        """
        string_format(
            "test", --[[ comment here ]]
            "%s %s",
            "test2"
        )
        """,
        """
        string_format("test", --[[ comment here ]] "%s %s", "test2")
        """)]
    [Arguments(
        """
        string_format(
            "%s %s",
            "test2",
            "test" --[[ comment here ]]
        )
        """,
        """
        string_format("%s %s", "test2", "test" --[[ comment here ]])
        """)]
    public async Task SyntaxNormalizer_CorrectlyAddsLineBreaksAfterSingleLineComments(string input, string expected)
    {
        var tree = await ParseAndValidateAsync(input, s_luaParseOptions);
        var root = await tree.GetRootAsync();

        await AssertNormalizeCoreAsync(root, expected);
    }

    [Test]
    [WorkItem(122, "https://github.com/LorettaDevs/Loretta/issues/122")]
    [Arguments("print(  -      -      2)", "print(- -2)")]
    [Arguments("print(  -      -      -      2)", "print(- - -2)")]
    [Arguments("print(  -      -      -      -      2)", "print(- - - -2)")]
    [Arguments("print(  -      -      -      -      -      2)", "print(- - - - -2)")]
    [Arguments(
        "print(  -      -      -      -      -      -      -      -      -      -      -      -      -      2)",
        "print(- - - - - - - - - - - - -2)")]
    public async Task SyntaxNormalizer_CorrectlyAddsSpacesOnDoubleUnaryMinus(string input, string expected)
    {
        var tree = await ParseAndValidateAsync(input, s_luaParseOptions);
        var root = await tree.GetRootAsync();

        await AssertNormalizeCoreAsync(root, expected);
    }

    #region Class Implementation Details

    private static async Task AssertNormalizeCoreAsync(SyntaxNode node, string expected)
    {
        node = node.NormalizeWhitespace(indentation: "    ", eol: Environment.NewLine);
        await Assert.That(node.ToFullString()).IsEqualTo(expected);
    }

    private static readonly ImmutableArray<KeyValuePair<string, string>> s_expressions =
    [
        new("...", "..."), new("`aaa`", "`aaa`"), new("a", "a"), new("1", "1"), new("'hi'", "'hi'"),
    ];

    private static readonly ImmutableArray<KeyValuePair<string, string>> s_types =
    [
        new("Type", "Type"), new("Type   .   SubType", "Type.SubType"), new("(   T   )    ->    T", "(T) -> T"),
        new("{   }", "{}"), new("{[ T ]:T}", "{ [T]: T }"), new("{x:T,y:T}", "{ x: T, y: T }"),
        new("typeof   (   'hi'   )", "typeof('hi')"),
    ];

    private static readonly ImmutableArray<KeyValuePair<string, string>> s_semicolons =
    [
        new("   ;   ", ";"), new("   ", ""),
    ];

    public IEnumerable<(string input, string expected)> GetInterpolatedPairs(
        string inputTemplate,
        string expectedTemplate)
    {
        using var inputEnumerator    = CombineExprHoles(inputTemplate, false);
        using var expectedEnumerator = CombineExprHoles(expectedTemplate, true);

        while (inputEnumerator.MoveNext())
        {
            if (!expectedEnumerator.MoveNext())
                throw new InvalidOperationException($"Unbalanced templates: {inputTemplate} |=| {expectedTemplate}");

            yield return (inputEnumerator.Current, expectedEnumerator.Current);
        }

        if (expectedEnumerator.MoveNext())
            throw new InvalidOperationException($"Unbalanced templates: {inputTemplate} |=| {expectedTemplate}");
    }

    private static IEnumerator<string> CombineExprHoles(string input, bool isExpected)
    {
        var matches = Regex.Matches(input, @"\{\[\((?:EXPR|TYPE|;)\)\]\}");
        var holes   = new byte[matches.Count];
        var builder = new StringBuilder();

        do
        {
            builder.Clear().Append(input);
            for (var idx = holes.Length - 1; idx >= 0; idx--)
            {
                var match = matches[idx];
                var pair  = ArrayForHole(match.Value)[holes[idx]];
                builder.Replace(match.Value, isExpected ? pair.Value : pair.Key, match.Index, match.Length);
            }
            yield return builder.ToString();
        } while (Advance());
        yield break;

        bool Advance()
        {
            bool carry;
            var  idx = holes.Length - 1;
            do
            {
                if (idx < 0) return false;

                carry = false;
                ref var val = ref holes[idx];
                val += 1;
                if (val >= ArrayForHole(matches[idx].Value).Length)
                {
                    val   = 0;
                    carry = true;
                }

                idx--;
            } while (carry);

            return true;
        }

        static ImmutableArray<KeyValuePair<string, string>> ArrayForHole(string value)
        {
            return value switch
            {
                "{[(EXPR)]}" => s_expressions,
                "{[(TYPE)]}" => s_types,
                "{[(;)]}"    => s_semicolons,
                _            => throw new InvalidOperationException($"{value} is not a valid placeholder.")
            };
        }
    }

    #endregion Class Implementation Details
}
