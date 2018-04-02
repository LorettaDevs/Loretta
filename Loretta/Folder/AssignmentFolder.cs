using System;
using Loretta.Parsing.Nodes;
using Loretta.Parsing.Nodes.Functions;
using Loretta.Parsing.Nodes.Indexers;
using Loretta.Parsing.Nodes.Loops;
using Loretta.Parsing.Nodes.Variables;

namespace Loretta.Folder
{
    // WIP: Do not use.
    public class AssignmentFolder : BaseASTFolder
    {
        protected override ASTNode FoldMemberExpression ( MemberExpression node, params Object[] args )
        {
            if ( node.Parent is IndexExpression || node.Parent is MemberExpression || node.Parent is VariableExpression )
                return node;
            ASTNode val = node.InternalData.GetValue<ASTNode> ( "aa.valueAtTime", null );
            return val ?? node;
        }

        protected override ASTNode FoldIndexExpression ( IndexExpression node, params Object[] args )
        {
            if ( node.Parent is IndexExpression || node.Parent is MemberExpression || node.Parent is VariableExpression )
                return node;
            ASTNode val = node.InternalData.GetValue<ASTNode> ( "aa.valueAtTime", null );
            return val ?? node;
        }

        protected override ASTNode FoldVariableExpression ( VariableExpression node, params Object[] args )
        {
            if ( node.Parent is IndexExpression || node.Parent is MemberExpression || node.Parent is VariableExpression )
                return node;
            ASTNode val = node.InternalData.GetValue<ASTNode> ( "aa.valueAtTime", null );
            return val ?? node;
        }

        #region Function Calls
        // No folding bases

        protected override ASTNode FoldFunctionCallExpression ( FunctionCallExpression node, params Object[] args )
        {
            node.SetArguments ( this.FoldNodeList ( node.Arguments ) );
            return node;
        }

        protected override ASTNode FoldStringFunctionCallExpression ( StringFunctionCallExpression node, params Object[] args )
        {
            return node;
        }

        protected override ASTNode FoldTableFunctionCallExpression ( TableFunctionCallExpression node, params Object[] args )
        {
            return node;
        }

        #endregion Function Calls

        #region Fors

        protected override ASTNode FoldForGenericStatement ( ForGenericStatement node, params Object[] args )
        {
            node.SetBody ( ( StatementList ) this.Fold ( node.Body ) );
            return node;
        }

        protected override ASTNode FoldForNumericStatement ( ForNumericStatement node, params Object[] args )
        {
            if ( node.InitialExpression != null )
            {
                ASTNode init = this.Fold ( node.InitialExpression );
                if ( init != null )
                    node.SetInitialExpression ( init );
                else
                    throw new Exception ( "Cannot have a numeric for statement without an initial expression." );
            }
            else
                throw new Exception ( "Cannot have a numeric for statement without an initial expression." );

            if ( node.FinalExpression != null )
            {
                ASTNode final = this.Fold ( node.FinalExpression );
                if ( final != null )
                    node.SetFinalExpression ( final );
                else
                    throw new Exception ( "Cannot have a numeric for statement without a final expression." );
            }
            else
                throw new Exception ( "Cannot have a numeric for statement without a final expression." );

            if ( node.IncrementExpression != null )
                node.SetIncrementExpression ( this.Fold ( node.IncrementExpression ) );

            if ( node.Body == null )
                throw new Exception ( "Cannot have a numeric for statement with a null body." );
            node.SetBody ( this.FoldStatementList ( node.Body ) );
            return node;
        }

        #endregion Fors
    }
}
