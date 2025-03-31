// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace Loretta.CodeAnalysis.EditAndContinue.UnitTests
{
    internal delegate TRet FuncInOutOut<in T1, T2, T3, out TRet>(T1 guid, out T2 errorCode, out T3 localizedMessage);
}
