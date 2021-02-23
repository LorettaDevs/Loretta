// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Loretta.CodeAnalysis.Text;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// Information decoded from early well-known custom attributes applied on an event.
    /// </summary>
    internal class CommonEventEarlyWellKnownAttributeData : EarlyWellKnownAttributeData
    {
        #region ObsoleteAttribute
        private ObsoleteAttributeData _obsoleteAttributeData = ObsoleteAttributeData.Uninitialized;
        public ObsoleteAttributeData ObsoleteAttributeData
        {
            get
            {
                VerifySealed(expected: true);
                return _obsoleteAttributeData.IsUninitialized ? null : _obsoleteAttributeData;
            }
            set
            {
                VerifySealed(expected: false);
                RoslynDebug.Assert(value != null);
                RoslynDebug.Assert(!value.IsUninitialized);

                _obsoleteAttributeData = value;
                SetDataStored();
            }
        }
        #endregion
    }
}
