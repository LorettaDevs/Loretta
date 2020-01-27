using System;
using System.Collections.Generic;
using System.Linq;
using Loretta.Parsing.AST;
using Loretta.Parsing.AST.Tables;
using Loretta.Parsing.Visitor.ReadWrite;

namespace Loretta.Parsing.Visitor
{
    public class SimpleReadWriteTracker : TreeWalkerBase
    {
        private readonly IDictionary<Variable, ReadWriteContainer> _containers = new Dictionary<Variable, ReadWriteContainer> ( );

        private Boolean _classifyWritesAsUnconditional = true;

        private ReadWriteContainer GetSubContainer ( ReadWriteContainer parent, Object identifier, Boolean createIfNotFound = true, ReadWriteContainer proxied = null )
        {
            if ( identifier is null )
                return null;

            if ( parent[identifier] is ReadWriteContainer container )
            {
                return container;
            }
            else if ( createIfNotFound )
            {
                if ( proxied is ReadWriteContainer )
                {
                    return parent.AddContainerProxy ( identifier, proxied );
                }
                else
                {
                    return parent.AddContainer ( identifier );
                }
            }
            else
            {
                return null;
            }
        }

        public ReadWriteContainer GetContainerForVariable ( Variable variable, Boolean createIfNotFound = true, ReadWriteContainer proxied = null )
        {
            if ( !this._containers.TryGetValue ( variable, out ReadWriteContainer container ) && createIfNotFound )
            {
                if ( proxied is ReadWriteContainer )
                    container = new ReadWriteContainerProxy ( variable.Identifier, proxied );
                else
                    container = new ReadWriteContainer ( variable.Identifier );

                this._containers.Add ( variable, container );
            }

            return container;
        }

        public ReadWriteContainer GetContainerForTree ( IndexExpression index, Boolean createIfNotFound = true, ReadWriteContainer proxied = null )
        {
            if ( index.Type == IndexType.Indexer && !index.Indexer.IsConstant )
                return null;

            var indexer = index.Type switch
            {
                IndexType.Indexer => index.Indexer.ConstantValue,
                _ => ( ( IdentifierExpression ) index.Indexer ).Identifier
            };

            ReadWriteContainer container = this.GetContainerForTree ( index.Indexee, createIfNotFound, null );
            return this.GetSubContainer ( container, indexer, createIfNotFound, proxied );
        }

        public ReadWriteContainer GetContainerForTree ( Expression expression, Boolean createIfNotFound = true, ReadWriteContainer proxied = null )
        {
            return expression switch
            {
                IndexExpression index => this.GetContainerForTree ( index, createIfNotFound, proxied ),
                IdentifierExpression identifier => this.GetContainerForVariable ( identifier.Variable, createIfNotFound, proxied ),
                _ => null
            };
        }

        private void InitializeContainerWithTable ( ReadWriteContainer table, TableConstructorExpression tableConstructor )
        {
            foreach ( TableField field in tableConstructor.Fields )
            {
                if ( field.KeyType == TableFieldKeyType.None || ( field.KeyType == TableFieldKeyType.Expression && !field.Key.IsConstant ) ) continue;

                var key = field.KeyType switch
                {
                    TableFieldKeyType.Identifier => ( ( IdentifierExpression ) field.Key ).Identifier,
                    TableFieldKeyType.Expression => field.Key.ConstantValue,
                    _ => throw new InvalidOperationException ( )
                };

                if ( this.GetSubContainer ( table, key ) is ReadWriteContainer fieldContainer )
                {
                    fieldContainer.AddWrite ( this._classifyWritesAsUnconditional, field.Key, field.Value );
                    if ( field.Value is TableConstructorExpression fieldTableConstructor )
                    {
                        this.InitializeContainerWithTable ( fieldContainer, fieldTableConstructor );
                    }
                }
            }
        }

        private void ProcessAssignment ( Expression[] variables, Expression[] values )
        {
            for ( var i = 0; i < variables.Length; i++ )
            {
                Expression value = null;
                if ( values.Length > i )
                {
                    value = values[i];
                }

                if ( this.GetContainerForTree ( value, false ) is ReadWriteContainer valueContainer )
                    valueContainer.AddRead ( true, value );

                Expression variable = variables[i];
                ReadWriteContainer variableContainer = this.GetContainerForTree ( variable, true );

                variableContainer.AddWrite ( this._classifyWritesAsUnconditional, variable, value );
                if ( value is TableConstructorExpression table )
                {
                    this.InitializeContainerWithTable ( variableContainer, table );
                }
            }
        }

        public override void VisitAssignment ( AssignmentStatement node )
        {
            Expression[] variables = node.Variables.ToArray ( );
            Expression[] values = node.Values.ToArray ( );
            this.ProcessAssignment ( variables, values );
        }

        public override void VisitLocalVariableDeclaration ( LocalVariableDeclarationStatement node )
        {
            IdentifierExpression[] identifiers = node.Identifiers.ToArray ( );
            Expression[] values = node.Values.ToArray ( );
            this.ProcessAssignment ( identifiers, values );
        }

        public override void VisitFunctionDefinition ( FunctionDefinitionStatement node )
        {
            var classify = this._classifyWritesAsUnconditional;
            this._classifyWritesAsUnconditional = false;
            base.VisitFunctionDefinition ( node );
            this._classifyWritesAsUnconditional = classify;
        }

        public override void VisitAnonymousFunction ( AnonymousFunctionExpression node )
        {
            var classify = this._classifyWritesAsUnconditional;
            this._classifyWritesAsUnconditional = false;
            base.VisitAnonymousFunction ( node );
            this._classifyWritesAsUnconditional = classify;
        }

        public override void VisitGenericFor ( GenericForLoopStatement node )
        {
            var classify = this._classifyWritesAsUnconditional;
            this._classifyWritesAsUnconditional = false;
            base.VisitGenericFor ( node );
            this._classifyWritesAsUnconditional = classify;
        }

        public override void VisitIfStatement ( IfStatement node )
        {
            var classify = this._classifyWritesAsUnconditional;
            this._classifyWritesAsUnconditional = false;
            base.VisitIfStatement ( node );
            this._classifyWritesAsUnconditional = classify;
        }

        public override void VisitNumericFor ( NumericForLoopStatement node )
        {
            var classify = this._classifyWritesAsUnconditional;
            this._classifyWritesAsUnconditional = false;
            base.VisitNumericFor ( node );
            this._classifyWritesAsUnconditional = classify;
        }

        public override void VisitRepeatUntil ( RepeatUntilStatement node )
        {
            var classify = this._classifyWritesAsUnconditional;
            this._classifyWritesAsUnconditional = false;
            base.VisitRepeatUntil ( node );
            this._classifyWritesAsUnconditional = classify;
        }

        public override void VisitWhileLoop ( WhileLoopStatement node )
        {
            var classify = this._classifyWritesAsUnconditional;
            this._classifyWritesAsUnconditional = false;
            base.VisitWhileLoop ( node );
            this._classifyWritesAsUnconditional = classify;
        }
    }
}