// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.

// These arise from modifications to existing API. The analysis seems too strict as the first
// required parameter type is different for all overloads and so there is no ambiguity. 
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0026:Do not add multiple public overloads with optional parameters", Justification = "<Pending>", Scope = "member", Target = "~M:Loretta.CodeAnalysis.Text.SourceText.From(System.IO.TextReader,System.Int32,System.Text.Encoding,Loretta.CodeAnalysis.Text.SourceHashAlgorithm)~Loretta.CodeAnalysis.Text.SourceText")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0027:Public API with optional parameter(s) should have the most parameters amongst its public overloads.", Justification = "<Pending>", Scope = "member", Target = "~M:Loretta.CodeAnalysis.Text.SourceText.From(System.String,System.Text.Encoding,Loretta.CodeAnalysis.Text.SourceHashAlgorithm)~Loretta.CodeAnalysis.Text.SourceText")]
