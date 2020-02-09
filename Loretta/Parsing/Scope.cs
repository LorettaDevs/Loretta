using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Loretta.Lexing;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing
{
    public class Scope
    {
        public enum FindMode
        {
            /// <summary>
            /// Only checks the current scope
            /// </summary>
            CheckSelf,

            /// <summary>
            /// Checks parents until we hit a function scope
            /// </summary>
            CheckFunctionScope,

            /// <summary>
            /// Checks all parents
            /// </summary>
            CheckParents,
        }

        public Scope? Parent { get; }

        //private readonly List<Variable> variables = new List<Variable> ( );
        private readonly Dictionary<String, Variable> _variables = new Dictionary<String, Variable> ( );

        //private readonly List<Label> labels = new List<Label> ( );
        private readonly Dictionary<String, GotoLabel> _gotoLabels = new Dictionary<String, GotoLabel> ( );

        // Yes, you can technically edit the dictionary like this, but when one wants to abuse an
        // API they will find a way to do it.
        public IEnumerable<Variable> Variables => this._variables.Values;

        public IEnumerable<GotoLabel> GotoLabels => this._gotoLabels.Values;

        public Boolean IsFunctionScope { get; }

        public Scope ( Boolean isFunctionScope )
        {
            this.Parent = null;
            this.IsFunctionScope = isFunctionScope;
        }

        public Scope ( Scope parent, Boolean isFunctionScope )
        {
            this.Parent = parent;
            this.IsFunctionScope = isFunctionScope;
        }

        #region TryFindVariable

        public Boolean TryFindVariable ( LuaToken identifier, FindMode findMode, [NotNullWhen ( true )] out Variable? variable )
        {
            if ( identifier.Type != LuaTokenType.Identifier || !( identifier.Value is String value ) )
                throw new ArgumentException ( "Token is not an identifier", nameof ( identifier ) );

            return this.TryFindVariable ( value, findMode, out variable );
        }

        public Boolean TryFindVariable ( String identifier, FindMode findMode, [NotNullWhen ( true )] out Variable? variable )
        {
            if ( this._variables.TryGetValue ( identifier, out variable ) )
                return true;

            if ( ( ( findMode == FindMode.CheckFunctionScope && !this.IsFunctionScope ) || findMode == FindMode.CheckParents ) && this.Parent != null )
                return this.Parent.TryFindVariable ( identifier, findMode, out variable );

            variable = null;
            return false;
        }

        #endregion TryFindVariable

        #region GetVariable

        public Variable GetVariable ( LuaToken identifier, FindMode findMode )
        {
            if ( identifier.Type != LuaTokenType.Identifier || !( identifier.Value is String value ) )
                throw new ArgumentException ( "Token is not an identifier", nameof ( identifier ) );
            return this.GetVariable ( value, findMode );
        }

        public Variable GetVariable ( String identifier, FindMode findMode )
        {
            if ( !this.TryFindVariable ( identifier, findMode, out Variable? variable ) )
            {
                variable = new Variable ( identifier, this );
                this._variables.Add ( identifier, variable );
            }

            return variable;
        }

        #endregion GetVariable

        #region FindLabel

        public GotoLabel? FindLabel ( LuaToken labelIdentifier, FindMode findMode )
        {
            if ( labelIdentifier.Type != LuaTokenType.Identifier || !( labelIdentifier.Value is String value ) )
                throw new ArgumentException ( "Token is not an identifier", nameof ( labelIdentifier ) );

            return this.FindLabel ( value, findMode );
        }

        public GotoLabel? FindLabel ( String labelIdentifier, FindMode findMode )
        {
            if ( !this.TryFindLabel ( labelIdentifier, findMode, out GotoLabel? label ) )
                return null;
            return label;
        }

        #endregion FindLabel

        #region TryFindLabel

        public Boolean TryFindLabel ( LuaToken labelIdentifier, FindMode findMode, [NotNullWhen ( true )] out GotoLabel? label )
        {
            if ( labelIdentifier.Type != LuaTokenType.Identifier || !( labelIdentifier.Value is String value ) )
                throw new ArgumentException ( "Token is not am identifier", nameof ( labelIdentifier ) );

            return this.TryFindLabel ( value, findMode, out label );
        }

        public Boolean TryFindLabel ( String labelIdentifier, FindMode findMode, [NotNullWhen ( true )] out GotoLabel? label )
        {
            if ( this._gotoLabels.TryGetValue ( labelIdentifier, out label ) )
                return true;

            if ( ( ( findMode == FindMode.CheckFunctionScope && !this.IsFunctionScope ) || findMode == FindMode.CheckParents ) && this.Parent != null )
                return this.Parent.TryFindLabel ( labelIdentifier, findMode, out label );

            label = null;
            return false;
        }

        #endregion TryFindLabel

        #region GetLabel

        public GotoLabel GetLabel ( LuaToken labelIdentifier, FindMode findMode )
        {
            if ( labelIdentifier.Type != LuaTokenType.Identifier || !( labelIdentifier.Value is String value ) )
                throw new ArgumentException ( "Token is not an identifier", nameof ( labelIdentifier ) );
            return this.GetLabel ( value, findMode );
        }

        public GotoLabel GetLabel ( String labelIdentifier, FindMode findMode )
        {
            if ( !this.TryFindLabel ( labelIdentifier, findMode, out GotoLabel? label ) )
            {
                label = new GotoLabel ( this, labelIdentifier );
                this._gotoLabels.Add ( labelIdentifier, label );
            }

            return label;
        }

        #endregion GetLabel
    }
}