using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Loretta.Parsing.AST;
using Loretta.Parsing.AST.Tables;
using Loretta.Parsing.Visitor.ReadWrite;

namespace Loretta.Parsing.Visitor
{
    public partial class SimpleReadWriteTracker : TreeWalkerBase
    {
        private readonly Dictionary<Variable, ReadWriteContainer> _containers = new Dictionary<Variable, ReadWriteContainer>
        {
            [Variable._G] = new GlobalTable ( "_G" )
        };

        public IReadOnlyDictionary<Variable, ReadWriteContainer> Containers => this._containers;

        private Boolean _classifyWritesAsUnconditional = true;

        private ReadWriteContainer? GetSubContainer ( ReadWriteContainer parent, LuaASTNode definer, Object identifier, Boolean createIfNotFound = true )
        {
            if ( identifier is null )
                return null;

            if ( parent[identifier] is ReadWriteContainer container )
            {
                return container;
            }
            else if ( createIfNotFound )
            {
                return parent.AddContainer ( definer, identifier );
            }
            else
            {
                return null;
            }
        }

        private void InitializeContainerWithTable ( ReadWriteContainer table, TableConstructorExpression tableConstructor )
        {
            foreach ( TableField field in tableConstructor.Fields )
            {
                if ( field.KeyType == TableFieldKeyType.None
                     || ( field.KeyType == TableFieldKeyType.Expression && !field.Key!.IsConstant ) )
                {
                    continue;
                }

                var key = field.KeyType switch
                {
                    TableFieldKeyType.Identifier => ( ( IdentifierExpression ) field.Key! ).Identifier,
                    TableFieldKeyType.Expression => field.Key!.ConstantValue,
                    _ => throw new InvalidOperationException ( )
                };

                if ( key is Object && this.GetSubContainer ( table, field, key ) is ReadWriteContainer fieldContainer )
                    this.AddWrite ( fieldContainer, field.Key, field.Value );
            }
        }

        private void AddRead ( Boolean isBeingAliased, Expression expression, Expression? alias, Boolean createIfNotFound = true )
        {
            if ( expression is IndexExpression index )
            {
                if ( index.Type == IndexType.Indexer )
                    this.AddRead ( false, index.Indexer, null, createIfNotFound );
                this.AddRead ( false, index.Indexee, null, createIfNotFound );
            }

            if ( this.GetContainerForTree ( expression, createIfNotFound ) is ReadWriteContainer container )
            {
                container.AddRead ( isBeingAliased, expression, alias );
            }
        }

        private void AddWrite ( Expression expression, Expression value, Boolean createIfNotFound = true )
        {
            if ( this.GetContainerForTree ( expression, createIfNotFound ) is ReadWriteContainer container )
                this.AddWrite ( container, expression, value );
        }

        private void AddWrite ( ReadWriteContainer container, Expression node, Expression value )
        {
            container.AddWrite ( this._classifyWritesAsUnconditional, node, value );

            if ( value is TableConstructorExpression table )
                this.InitializeContainerWithTable ( container, table );
        }

        private void ProcessAssignment ( Expression[] variables, Expression[] values )
        {
            for ( var i = 0; i < variables.Length; i++ )
            {
                Expression variable = variables[i];
                Expression? value = null;
                if ( values.Length > i )
                {
                    value = values[i];
                }

                this.AddWrite ( variable, value ?? ASTNodeFactory.Nil ( ) );
                if ( value is Expression )
                {
                    this.AddRead ( true, value, variable );
                }
            }
        }

        public ReadWriteContainer? GetContainerForVariable ( LuaASTNode definer, Variable variable, Boolean createIfNotFound = true )
        {
            if ( !this._containers.TryGetValue ( variable, out ReadWriteContainer container ) && createIfNotFound )
            {
                container = new ReadWriteContainer ( definer, variable.Identifier );
                this._containers.Add ( variable, container );
            }

            return container;
        }

        public ReadWriteContainer? GetContainerForTree ( IndexExpression index, Boolean createIfNotFound = true )
        {
            if ( index.Type == IndexType.Indexer && !index.Indexer.IsConstant )
                return null;

            var indexer = index.Type switch
            {
                IndexType.Indexer => index.Indexer.ConstantValue,
                _ => ( ( IdentifierExpression ) index.Indexer ).Identifier
            };

            if ( this.GetContainerForTree ( index.Indexee, createIfNotFound ) is ReadWriteContainer container
                 && !( indexer is null ) )
            {
                return this.GetSubContainer ( container, index, indexer, createIfNotFound );
            }

            return null;
        }

        public ReadWriteContainer? GetContainerForTree ( Expression expression, Boolean createIfNotFound = true )
        {
            return expression switch
            {
                IndexExpression index => this.GetContainerForTree ( index, createIfNotFound ),
                IdentifierExpression { Variable: Variable variable } => this.GetContainerForVariable ( expression, variable, createIfNotFound ),
                _ => null
            };
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

        public override void VisitIdentifier ( IdentifierExpression node ) =>
            this.AddRead ( false, node, null );

        public override void VisitIndex ( IndexExpression node ) =>
            this.AddRead ( false, node, null );

        public override void VisitFunctionCall ( FunctionCallExpression node )
        {
            this.VisitNode ( node.Function );
            foreach ( Expression argument in node.Arguments )
            {
                this.AddRead ( true, argument, new UndefinedValueExpression ( ) );
            }
        }

        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        private void WithConditionalWrites<T> ( Action<T> visitor, T visited )
        {
            var classify = this._classifyWritesAsUnconditional;
            this._classifyWritesAsUnconditional = false;
            visitor ( visited );
            this._classifyWritesAsUnconditional = classify;
        }

        public override void VisitFunctionDefinition ( FunctionDefinitionStatement node ) =>
            this.WithConditionalWrites ( node =>
            {
                this.VisitNode ( node.Name );
                foreach ( Expression argument in node.Arguments )
                {
                    this.AddWrite ( argument, new UndefinedValueExpression ( ) );
                    base.VisitNode ( argument );
                }
                this.VisitNode ( node.Body );
            }, node );

        public override void VisitAnonymousFunction ( AnonymousFunctionExpression node ) =>
            this.WithConditionalWrites ( node =>
            {
                foreach ( Expression argument in node.Arguments )
                {
                    this.AddWrite ( argument, new UndefinedValueExpression ( ) );
                    base.VisitNode ( argument );
                }

                this.VisitNode ( node.Body );
            }, node );

        public override void VisitGenericFor ( GenericForLoopStatement node )
        {
            this.WithConditionalWrites ( node =>
            {
                foreach ( IdentifierExpression variable in node.Variables )
                {
                    this.AddWrite ( variable, new UndefinedValueExpression ( ) );
                    base.VisitNode ( variable );
                }

                foreach ( Expression iteratable in node.Expressions )
                {
                    this.VisitNode ( iteratable );
                }
                this.VisitNode ( node.Body );
            }, node );
        }

        public override void VisitIfStatement ( IfStatement node ) =>
            this.WithConditionalWrites ( base.VisitIfStatement, node );

        public override void VisitNumericFor ( NumericForLoopStatement node ) =>
            this.WithConditionalWrites ( node =>
            {
                this.AddWrite ( node.Variable, new UndefinedValueExpression ( ) );
                base.VisitNode ( node.Variable );
                this.VisitNode ( node.Initial );
                this.VisitNode ( node.Final );
                if ( node.Step != null )
                    this.VisitNode ( node.Step );
                this.VisitNode ( node.Body );
            }, node );

        public override void VisitRepeatUntil ( RepeatUntilStatement node ) =>
            this.WithConditionalWrites ( base.VisitRepeatUntil, node );

        public override void VisitWhileLoop ( WhileLoopStatement node ) =>
            this.WithConditionalWrites ( base.VisitWhileLoop, node );
    }
}