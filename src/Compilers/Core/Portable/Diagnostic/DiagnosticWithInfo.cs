// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// A diagnostic (such as a compiler error or a warning), along with the location where it occurred.
    /// </summary>
    [DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
    internal class DiagnosticWithInfo : Diagnostic
    {
        private readonly DiagnosticInfo _info;
        private readonly Location _location;
        private readonly bool _isSuppressed;

        internal DiagnosticWithInfo(DiagnosticInfo info, Location location, bool isSuppressed = false)
        {
            LorettaDebug.Assert(info != null);
            LorettaDebug.Assert(location != null);
            _info = info;
            _location = location;
            _isSuppressed = isSuppressed;
        }

        public override Location Location => _location;

        public override IReadOnlyList<Location> AdditionalLocations => Info.AdditionalLocations;

        internal override IReadOnlyList<string> CustomTags => Info.CustomTags;

        public override DiagnosticDescriptor Descriptor => Info.Descriptor;

        public override string Id => Info.MessageIdentifier;

        internal override string Category => Info.Category;

        internal sealed override int Code => Info.Code;

        public sealed override DiagnosticSeverity Severity => Info.Severity;

        public sealed override DiagnosticSeverity DefaultSeverity => Info.DefaultSeverity;

        internal sealed override bool IsEnabledByDefault => true;

        public override bool IsSuppressed => _isSuppressed;

        public sealed override int WarningLevel => Info.WarningLevel;

        public override string GetMessage(IFormatProvider? formatProvider = null) => Info.GetMessage(formatProvider);

        internal override IReadOnlyList<object?> Arguments => Info.Arguments;

        /// <summary>
        /// Get the information about the diagnostic: the code, severity, message, etc.
        /// </summary>
        public DiagnosticInfo Info
        {
            get
            {
                if (_info.Severity == InternalDiagnosticSeverity.Unknown)
                {
                    return _info.GetResolvedInfo();
                }

                return _info;
            }
        }

        /// <summary>
        /// True if the DiagnosticInfo for this diagnostic requires (or required - this property
        /// is immutable) resolution.
        /// </summary>
        internal bool HasLazyInfo
        {
            get
            {
                return _info.Severity is InternalDiagnosticSeverity.Unknown
                                      or InternalDiagnosticSeverity.Void;
            }
        }

        public override int GetHashCode() => Hash.Combine(Location.GetHashCode(), Info.GetHashCode());

        public override bool Equals(object? obj) => Equals(obj as Diagnostic);

        public override bool Equals(Diagnostic? obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is not DiagnosticWithInfo other || GetType() != other.GetType())
            {
                return false;
            }

            return
                Location.Equals(other._location) &&
                Info.Equals(other.Info) &&
                AdditionalLocations.SequenceEqual(other.AdditionalLocations);
        }

        private string GetDebuggerDisplay()
        {
            return _info.Severity switch
            {
                // If we called ToString before the diagnostic was resolved,
                // we would risk infinite recursion (e.g. if we were still computing
                // member lists).
                InternalDiagnosticSeverity.Unknown => "Unresolved diagnostic at " + Location,
                // If we called ToString on a void diagnostic, the MessageProvider
                // would complain about the code.
                InternalDiagnosticSeverity.Void => "Void diagnostic at " + Location,
                _ => ToString(),
            };
        }

        internal override Diagnostic WithLocation(Location location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            if (location != _location)
            {
                return new DiagnosticWithInfo(_info, location, _isSuppressed);
            }

            return this;
        }

        internal override Diagnostic WithSeverity(DiagnosticSeverity severity)
        {
            if (Severity != severity)
            {
                return new DiagnosticWithInfo(Info.GetInstanceWithSeverity(severity), _location, _isSuppressed);
            }

            return this;
        }

        internal override Diagnostic WithIsSuppressed(bool isSuppressed)
        {
            if (IsSuppressed != isSuppressed)
            {
                return new DiagnosticWithInfo(Info, _location, isSuppressed);
            }

            return this;
        }

        internal sealed override bool IsNotConfigurable() => Info.IsNotConfigurable();
    }
}
