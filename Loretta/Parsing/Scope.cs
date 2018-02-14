using System;
using System.Collections;
using System.Collections.Generic;
using GParse.Lexing;
using GParse.Parsing.Errors;

namespace Loretta.Parsing
{
    public class Scope
    {
        public GLuaParser Parser { get; }

        public Scope Parent { get; }

        public Boolean IsRoot { get; }

        public List<Variable> Locals { get; } = new List<Variable> ( );

        public Dictionary<String, Label> Labels { get; } = new Dictionary<String, Label> ( );

        public InternalData InternalData { get; } = new InternalData ( );

        public SourceLocation StartPosition { get; private set; }

        public SourceLocation EndPosition { get; private set; }

        public Scope ( GLuaParser Parser, Scope Parent = null )
        {
            this.Parser = Parser;
            this.Parent = Parent;
            this.IsRoot = Parent != null;
        }

        public void Start ( )
        {
            SourceLocation location = this.Parser.GetLocation ( );
            this.StartPosition = location;

            this.Parser.PushScope ( this );
        }

        public void Finish ( )
        {
            SourceLocation location = this.Parser.GetLocation ( );
            this.EndPosition = location;

            if ( this.Parser.PopScope ( ) != this )
                throw new ParseException ( location, "Scope popped from parser != this, shitcode alert" );
        }

        /// <summary>
        /// Returns the last local variable with name
        /// <paramref name="name" /> or null if nothing is found
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private Variable FindLocalVariable ( String name )
        {
            for ( var i = this.Locals.Count - 1; i >= 0; i-- )
            {
                if ( this.Locals[i].Name == name )
                    return this.Locals[i];
            }

            return null;
        }

        /// <summary>
        /// Returns the global variable with the name provided or
        /// null if it doesn't exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private Variable FindGlobalVariable ( String name )
        {
            return this.Parser.Environment.Globals.ContainsKey ( name ) ? this.Parser.Environment.Globals[name] : null;
        }

        /// <summary>
        /// Creates a global variable in the parser's
        /// <see cref="LuaEnvironment" /> and returns it
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private Variable CreateGlobalVariable ( String name )
        {
            var var = new Variable ( this, name, false );
            this.Parser.Environment.Globals[name] = var;
            return var;
        }

        /// <summary>
        /// Creates a local variable and inserts it into the
        /// <see cref="Locals" /> list.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public Variable CreateLocalVariable ( String Name )
        {
            var variable = new Variable ( this, Name, true );
            this.Locals.Add ( variable );
            return variable;
        }

        /// <summary>
        /// Attempts to find a variable with the provided name and
        /// creates a global variable if the variable is not found
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Variable GetVariable ( String name )
        {
            Variable var = this.FindLocalVariable ( name );

            if ( var == null )
                var = this.FindGlobalVariable ( name );

            if ( var == null )
                var = this.CreateGlobalVariable ( name );

            return var;
        }

        public Label GetLabel ( String value )
        {
            if ( this.Labels.ContainsKey ( value ) )
                return this.Labels[value];

            if ( this.Parent != null )
            {
                Label label = this.Parent.GetLabel ( value );
                if ( label != null )
                    return label;
            }

            if ( this.InternalData.GetValue ( "isFunction", false ) )
            {
                var label = new Label ( this, value );
                this.Labels[value] = label;
                return label;
            }

            return null;
        }
    }
}
