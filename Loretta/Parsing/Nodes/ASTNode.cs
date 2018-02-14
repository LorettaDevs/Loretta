using System;
using System.Collections.Generic;
using System.Text;
using GParse.Lexing;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes
{
    public abstract class ASTNode
    {
        /// <summary>
        /// Child nodes
        /// </summary>
        public List<ASTNode> Children { get; } = new List<ASTNode> ( );

        /// <summary>
        /// Parent node
        /// </summary>
        public ASTNode Parent { get; private set; }

        /// <summary>
        /// Range we occupy on the source code
        /// </summary>
        public SourceRange Range { get; private set; }

        /// <summary>
        /// The scope we belong to
        /// </summary>
        public Scope Scope { get; private set; }

        /// <summary>
        /// The tokens we're composed of
        /// </summary>
        public IList<LToken> Tokens { get; }

        /// <summary>
        /// Internal data
        /// </summary>
        public InternalData InternalData { get; } = new InternalData ( );

        /// <summary>
        /// Index we occupy in the parent's list
        /// </summary>
        public Int32 ParentIndex { get; protected set; } = -1;

        protected ASTNode ( ASTNode parent, Scope scope, IList<LToken> tokens )
        {
            this.Tokens = tokens;
            this.Parent = parent;
            this.Scope = scope;
            this.Range = SourceLocation.Max.To ( SourceLocation.Min );
        }

        public override String ToString ( ) => $"{this.GetType ( ).Name}<>";

        public void SetParent ( ASTNode parent )
        {
            if ( this.Parent != null )
            {
                this.Parent.RemoveChild ( this );
            }
            if ( parent != null )
            {
                parent.AddChild ( this );
            }
        }

        public void RecomputeBounds ( )
        {
            Int32 startL = Int32.MaxValue,
                startC = Int32.MaxValue,
                startB = Int32.MaxValue,
                endL = Int32.MinValue,
                endC = Int32.MinValue,
                endB = Int32.MinValue;
            var updatedStart = false;
            var updatedEnd = false;

            foreach ( LToken tok in this.Tokens )
            {
                SourceLocation loc = tok.Location;

                startB = Math.Min ( startB, loc.Byte );
                if ( startB == loc.Byte )
                {
                    startL = loc.Line;
                    startC = loc.Column;
                    updatedStart = true;
                }

                endB = Math.Max ( endB, loc.Byte );
                if ( endB == loc.Byte )
                {
                    endL = loc.Line;
                    endC = loc.Column;
                    updatedEnd = true;
                }

                foreach ( ASTNode child in this.Children )
                {
                    child.RecomputeBounds ( );

                    if ( child.Range.Start != SourceLocation.Max )
                    {
                        startB = Math.Min ( startB, child.Range.Start.Byte );
                        if ( startB == child.Range.Start.Byte )
                        {
                            startL = child.Range.Start.Line;
                            startC = child.Range.Start.Column;
                            updatedStart = true;
                        }

                        endB = Math.Max ( endB, child.Range.End.Byte );
                        if ( endB == child.Range.End.Byte )
                        {
                            endL = child.Range.End.Line;
                            endC = child.Range.End.Column;
                            updatedEnd = true;
                        }
                    }
                }
            }

            if ( !( updatedStart && updatedEnd ) && this.Children.Count != 0 )
                throw new Exception ( "Node couldn't generate start/end data. " + this );

            this.Range = new SourceLocation ( startL, startC, startB )
                .To ( new SourceLocation ( endL, endC, endB ) );
        }

        #region Children Manipulation

        private void ParentChild ( ASTNode child, Int32 idx = -1 )
        {
            child.Parent = this;
            child.ParentIndex = idx != -1 ? this.Children.IndexOf ( child ) : idx;
        }

        /// <summary>
        /// Parents another node
        /// </summary>
        /// <param name="newChild"></param>
        public void AddChild ( ASTNode newChild )
        {
            this.Children.Add ( newChild );
            this.ParentChild ( newChild, this.Children.Count - 1 );
        }

        /// <summary>
        /// Parents many nodes at once
        /// </summary>
        /// <param name="newChildren"></param>
        public void AddChildren ( IEnumerable<ASTNode> newChildren )
        {
            foreach ( ASTNode newChild in newChildren )
                this.AddChild ( newChild );
        }

        /// <summary>
        /// Inserts a child at a specified location
        /// </summary>
        /// <param name="newChild"></param>
        /// <param name="index"></param>
        public void InsertChild ( ASTNode newChild, Int32 index )
        {
            this.Children.Insert ( index, newChild );
            this.ParentChild ( newChild, index );

            // Fix the index of all children after this
            for ( var i = index + 1; i < this.Children.Count; i++ )
                this.Children[i].ParentIndex = i;
        }

        /// <summary>
        /// Inserts a series of children at a specified location
        /// </summary>
        /// <param name="newChildren"></param>
        /// <param name="index"></param>
        public void InsertChildren ( IEnumerable<ASTNode> newChildren, Int32 index )
        {
            foreach ( ASTNode newChild in newChildren )
                this.InsertChild ( newChild, index++ );
        }

        /// <summary>
        /// Replaces a child <see cref="ASTNode" /> by another
        /// </summary>
        /// <param name="oldChild"></param>
        /// <param name="newChild"></param>
        public void ReplaceChild ( ASTNode oldChild, ASTNode newChild )
        {
            // Sanity check
            if ( oldChild.Parent != this )
                throw new Exception ( "ASTNode to be replaced is not a child of this node." );

            this.Children[oldChild.ParentIndex] = newChild;
            this.ParentChild ( newChild, oldChild.ParentIndex );
        }

        /// <summary>
        /// Removes a child node
        /// </summary>
        /// <param name="child"></param>
        public void RemoveChild ( ASTNode child )
        {
            if ( child.Parent != this )
                throw new Exception ( "ASTNode to be removed is not a child of this node." );

            this.Children.Remove ( child );

            for ( var i = 0; i < this.Children.Count; i++ )
                this.Children[i].ParentIndex = i;
        }

        /// <summary>
        /// Replaces this node by a new one in the parent node
        /// </summary>
        /// <param name="newNode"></param>
        public void ReplaceInParent ( ASTNode newNode )
        {
            this.Parent.ReplaceChild ( this, newNode );
        }

        #endregion Children Manipulation
    }
}
