﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// A DiagnosticInfo object has information about a diagnostic, but without any attached location information.
    /// </summary>
    /// <remarks>
    /// More specialized diagnostics with additional information (e.g., ambiguity errors) can derive from this class to
    /// provide access to additional information about the error, such as what symbols were involved in the ambiguity.
    /// </remarks>
    [DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
    internal class DiagnosticInfo : IFormattable, IObjectWritable
    {
        private readonly CommonMessageProvider _messageProvider;
        private readonly int _errorCode;
        private readonly DiagnosticSeverity _defaultSeverity;
        private readonly DiagnosticSeverity _effectiveSeverity;
        private readonly object[] _arguments;

        private static ImmutableDictionary<int, DiagnosticDescriptor> s_errorCodeToDescriptorMap = ImmutableDictionary<int, DiagnosticDescriptor>.Empty;

        // Mark compiler errors as non-configurable to ensure they can never be suppressed or filtered.
        private static readonly ImmutableArray<string> s_compilerErrorCustomTags = ImmutableArray.Create(WellKnownDiagnosticTags.Compiler, WellKnownDiagnosticTags.NotConfigurable);
        private static readonly ImmutableArray<string> s_compilerNonErrorCustomTags = ImmutableArray.Create(WellKnownDiagnosticTags.Compiler);

        static DiagnosticInfo()
        {
            ObjectBinder.RegisterTypeReader(typeof(DiagnosticInfo), r => new DiagnosticInfo(r));
        }

        // Only the compiler creates instances.
        internal DiagnosticInfo(CommonMessageProvider messageProvider, int errorCode)
        {
            _messageProvider = messageProvider;
            _errorCode = errorCode;
            _defaultSeverity = messageProvider.GetSeverity(errorCode);
            _effectiveSeverity = _defaultSeverity;
            _arguments = Array.Empty<object>();
        }

        // Only the compiler creates instances.
        internal DiagnosticInfo(CommonMessageProvider messageProvider, int errorCode, params object[] arguments)
            : this(messageProvider, errorCode)
        {
            AssertMessageSerializable(arguments);

            _arguments = arguments;
        }

        protected DiagnosticInfo(DiagnosticInfo original, DiagnosticSeverity overriddenSeverity)
        {
            _messageProvider = original.MessageProvider;
            _errorCode = original._errorCode;
            _defaultSeverity = original.DefaultSeverity;
            _arguments = original._arguments;

            _effectiveSeverity = overriddenSeverity;
        }

        internal static DiagnosticDescriptor GetDescriptor(int errorCode, CommonMessageProvider messageProvider)
        {
            var defaultSeverity = messageProvider.GetSeverity(errorCode);
            return GetOrCreateDescriptor(errorCode, defaultSeverity, messageProvider);
        }

        private static DiagnosticDescriptor GetOrCreateDescriptor(int errorCode, DiagnosticSeverity defaultSeverity, CommonMessageProvider messageProvider) => ImmutableInterlocked.GetOrAdd(ref s_errorCodeToDescriptorMap, errorCode, code => CreateDescriptor(code, defaultSeverity, messageProvider));

        private static DiagnosticDescriptor CreateDescriptor(int errorCode, DiagnosticSeverity defaultSeverity, CommonMessageProvider messageProvider)
        {
            var id = messageProvider.GetIdForErrorCode(errorCode);
            var title = messageProvider.GetTitle(errorCode);
            var description = messageProvider.GetDescription(errorCode);
            var messageFormat = messageProvider.GetMessageFormat(errorCode);
            var helpLink = messageProvider.GetHelpLink(errorCode);
            var category = messageProvider.GetCategory(errorCode);
            var customTags = GetCustomTags(defaultSeverity);
            return new DiagnosticDescriptor(id, title, messageFormat, category, defaultSeverity,
                isEnabledByDefault: true, description: description, helpLinkUri: helpLink, customTags: customTags);
        }

        [Conditional("DEBUG")]
        internal static void AssertMessageSerializable(object[] args)
        {
            foreach (var arg in args)
            {
                LorettaDebug.Assert(arg != null);

                if (arg is IFormattable)
                {
                    continue;
                }

                var type = arg.GetType();
                if (type == typeof(string))
                {
                    continue;
                }

                var info = type.GetTypeInfo();
                if (info.IsPrimitive)
                {
                    continue;
                }

                throw ExceptionUtilities.UnexpectedValue(type);
            }
        }

        // Only the compiler creates instances.
        internal DiagnosticInfo(CommonMessageProvider messageProvider, bool isWarningAsError, int errorCode, params object[] arguments)
            : this(messageProvider, errorCode, arguments)
        {
            LorettaDebug.Assert(!isWarningAsError || _defaultSeverity == DiagnosticSeverity.Warning);

            if (isWarningAsError)
            {
                _effectiveSeverity = DiagnosticSeverity.Error;
            }
        }

        // Create a copy of this instance with a explicit overridden severity
        internal virtual DiagnosticInfo GetInstanceWithSeverity(DiagnosticSeverity severity) => new(this, severity);

        #region Serialization

        bool IObjectWritable.ShouldReuseInSerialization => false;

        void IObjectWritable.WriteTo(ObjectWriter writer) => WriteTo(writer);

        protected virtual void WriteTo(ObjectWriter writer)
        {
            writer.WriteValue(_messageProvider);
            writer.WriteUInt32((uint) _errorCode);
            writer.WriteInt32((int) _effectiveSeverity);
            writer.WriteInt32((int) _defaultSeverity);

            var count = _arguments.Length;
            writer.WriteUInt32((uint) count);

            if (count > 0)
            {
                foreach (var arg in _arguments)
                {
                    writer.WriteString(arg.ToString());
                }
            }
        }

        protected DiagnosticInfo(ObjectReader reader)
        {
            _messageProvider = (CommonMessageProvider) reader.ReadValue();
            _errorCode = (int) reader.ReadUInt32();
            _effectiveSeverity = (DiagnosticSeverity) reader.ReadInt32();
            _defaultSeverity = (DiagnosticSeverity) reader.ReadInt32();

            var count = (int) reader.ReadUInt32();
            if (count > 0)
            {
                _arguments = new string[count];
                for (var i = 0; i < count; i++)
                {
                    _arguments[i] = reader.ReadString();
                }
            }
            else
            {
                _arguments = Array.Empty<object>();
            }
        }

        #endregion

        /// <summary>
        /// The error code, as an integer.
        /// </summary>
        public int Code => _errorCode;

        public virtual DiagnosticDescriptor Descriptor => GetOrCreateDescriptor(_errorCode, _defaultSeverity, _messageProvider);

        /// <summary>
        /// Returns the effective severity of the diagnostic: whether this diagnostic is informational, warning, or error.
        /// If IsWarningsAsError is true, then this returns <see cref="DiagnosticSeverity.Error"/>, while <see cref="DefaultSeverity"/> returns <see cref="DiagnosticSeverity.Warning"/>.
        /// </summary>
        public DiagnosticSeverity Severity => _effectiveSeverity;

        /// <summary>
        /// Returns whether this diagnostic is informational, warning, or error by default, based on the error code.
        /// To get diagnostic's effective severity, use <see cref="Severity"/>.
        /// </summary>
        public DiagnosticSeverity DefaultSeverity => _defaultSeverity;

        /// <summary>
        /// Gets the warning level. This is 0 for diagnostics with severity <see cref="DiagnosticSeverity.Error"/>,
        /// otherwise an integer greater than zero.
        /// </summary>
        public int WarningLevel
        {
            get
            {
                if (_effectiveSeverity != _defaultSeverity)
                {
                    return Diagnostic.GetDefaultWarningLevel(_effectiveSeverity);
                }

                return _messageProvider.GetWarningLevel(_errorCode);
            }
        }

        /// <summary>
        /// Returns true if this is a warning treated as an error.
        /// </summary>
        /// <remarks>
        /// True implies <see cref="Severity"/> = <see cref="DiagnosticSeverity.Error"/> and
        /// <see cref="DefaultSeverity"/> = <see cref="DiagnosticSeverity.Warning"/>.
        /// </remarks>
        public bool IsWarningAsError
        {
            get
            {
                return DefaultSeverity == DiagnosticSeverity.Warning &&
                    Severity == DiagnosticSeverity.Error;
            }
        }

        /// <summary>
        /// Get the diagnostic category for the given diagnostic code.
        /// Default category is <see cref="Diagnostic.CompilerDiagnosticCategory"/>.
        /// </summary>
        public string Category => _messageProvider.GetCategory(_errorCode);

        internal ImmutableArray<string> CustomTags => GetCustomTags(_defaultSeverity);

        private static ImmutableArray<string> GetCustomTags(DiagnosticSeverity defaultSeverity)
        {
            return defaultSeverity == DiagnosticSeverity.Error ?
                s_compilerErrorCustomTags :
                s_compilerNonErrorCustomTags;
        }

        internal bool IsNotConfigurable() =>
            // Only compiler errors are non-configurable.
            _defaultSeverity == DiagnosticSeverity.Error;

        /// <summary>
        /// If a derived class has additional information about other referenced symbols, it can
        /// expose the locations of those symbols in a general way, so they can be reported along
        /// with the error.
        /// </summary>
        public virtual IReadOnlyList<Location> AdditionalLocations => SpecializedCollections.EmptyReadOnlyList<Location>();

        /// <summary>
        /// Get the message id (for example "CS1001") for the message. This includes both the error number
        /// and a prefix identifying the source.
        /// </summary>
        public virtual string MessageIdentifier => _messageProvider.GetIdForErrorCode(_errorCode);

        /// <summary>
        /// Get the text of the message in the given language.
        /// </summary>
        public virtual string GetMessage(IFormatProvider? formatProvider = null)
        {
            // Get the message and fill in arguments.
            var message = _messageProvider.LoadMessage(_errorCode, formatProvider as CultureInfo);
            if (string.IsNullOrEmpty(message))
            {
                return string.Empty;
            }

            if (_arguments.Length == 0)
            {
                return message;
            }

            return string.Format(formatProvider, message, GetArgumentsToUse(formatProvider));
        }

        protected object[] GetArgumentsToUse(IFormatProvider? formatProvider)
        {
            object[]? argumentsToUse = null;
            for (var i = 0; i < _arguments.Length; i++)
            {
                if (_arguments[i] is DiagnosticInfo embedded)
                {
                    argumentsToUse = InitializeArgumentListIfNeeded(argumentsToUse);
                    argumentsToUse[i] = embedded.GetMessage(formatProvider);
                    continue;
                }
            }

            return argumentsToUse ?? _arguments;
        }

        private object[] InitializeArgumentListIfNeeded(object[]? argumentsToUse)
        {
            if (argumentsToUse != null)
            {
                return argumentsToUse;
            }

            var newArguments = new object[_arguments.Length];
            Array.Copy(_arguments, newArguments, newArguments.Length);

            return newArguments;
        }

        internal object[] Arguments => _arguments;

        internal CommonMessageProvider MessageProvider => _messageProvider;

        // TODO (tomat): remove
        public override string? ToString() => ToString(null);

        public string ToString(IFormatProvider? formatProvider) => ((IFormattable) this).ToString(null, formatProvider);

        string IFormattable.ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(formatProvider, "{0}: {1}",
                _messageProvider.GetMessagePrefix(MessageIdentifier, Severity, IsWarningAsError, formatProvider as CultureInfo),
                GetMessage(formatProvider));
        }

        public sealed override int GetHashCode()
        {
            var hashCode = _errorCode;
            for (var i = 0; i < _arguments.Length; i++)
            {
                hashCode = Hash.Combine(_arguments[i], hashCode);
            }

            return hashCode;
        }

        public sealed override bool Equals(object? obj)
        {
            var result = false;

            if (obj is DiagnosticInfo other &&
                other._errorCode == _errorCode &&
                other.GetType() == GetType())
            {
                if (_arguments.Length == other._arguments.Length)
                {
                    result = true;
                    for (var i = 0; i < _arguments.Length; i++)
                    {
                        if (!Equals(_arguments[i], other._arguments[i]))
                        {
                            result = false;
                            break;
                        }
                    }
                }
            }

            return result;
        }

        private string? GetDebuggerDisplay()
        {
            // There aren't message resources for our internal error codes, so make
            // sure we don't call ToString for those.
            return Code switch
            {
                InternalErrorCode.Unknown => "Unresolved DiagnosticInfo",
                InternalErrorCode.Void => "Void DiagnosticInfo",
                _ => ToString(),
            };
        }

        /// <summary>
        /// For a DiagnosticInfo that is lazily evaluated, this method evaluates it
        /// and returns a non-lazy DiagnosticInfo.
        /// </summary>
        internal virtual DiagnosticInfo GetResolvedInfo() =>
            // We should never call GetResolvedInfo on a non-lazy DiagnosticInfo
            throw ExceptionUtilities.Unreachable;
    }
}
