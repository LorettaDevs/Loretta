using System;
using System.Collections.Generic;
using System.Linq;
using Loretta.Parsing.AST;
using Loretta.Parsing.Visitor.ReadWrite;

namespace Loretta.Parsing.Visitor
{
    public class SimpleInliner : TreeFolderBase
    {
        private readonly SimpleReadWriteTracker _readWriteTracker;
        private readonly HashSet<ReadWriteContainer> _added = new HashSet<ReadWriteContainer> ( );
        private readonly List<(ReadWriteContainer Container, LuaASTNode Original, Expression Replacement)> _replacements = new List<(ReadWriteContainer Container, LuaASTNode Original, Expression Replacement)> ( );

        public SimpleInliner ( SimpleReadWriteTracker readWriteTracker )
        {
            this._readWriteTracker = readWriteTracker;

            foreach ( ReadWriteContainer container in readWriteTracker.Containers.Select ( kv => kv.Value ) )
            {
                this.AddAllContainers ( container );
            }
        }

        private void AddAllContainers ( ReadWriteContainer container )
        {
            if ( this._added.Contains ( container ) )
                return;
            this._added.Add ( container );

            foreach ( ReadWriteContainer subContainer in container.Containers.Select ( kv => kv.Value ) )
            {
                this.AddAllContainers ( subContainer );
            }
            this.AddReplacement ( container );
        }

        private Boolean AddReplacement ( ReadWriteContainer container )
        {
            if ( !isReplaceable ( container ) )
                return false;

            this._replacements.Add ( (container, container.Definer, container.Writes.Single ( ).Value) );
            foreach ( Read read in container.Reads )
            {
                if ( read.IsBeingAlised && this._readWriteTracker.GetContainerForTree ( read.Alias!, false ) is ReadWriteContainer subContainer )
                {
                    this.AddReplacement ( subContainer );
                }
            }

            return true;

            Boolean isReplaceable ( ReadWriteContainer container )
            {
                return !this._added.Contains ( container )
                       && !container.HasConditionalWrites
                       && !container.HasIndirectWrites
                       && !container.HasUndefinedWrites
                       && container.Writes.Count == 1
                       // We don't accept any aliases for inlining (might need multiple passes but
                       // it's to annoying (if not impossible) to deal with in a single pass without
                       // CFGs and SSA)
                       && ( this._readWriteTracker.GetContainerForTree ( container.Writes.Single ( ).Node )?.Writes.Count ?? 0 ) == 0
                       ;
            }
        }

        public override LuaASTNode VisitNode ( LuaASTNode node )
        {
            (_, _, Expression replacement) = this._replacements.FirstOrDefault ( r => r.Original == node
                                                                                      || ( node is Expression ex
                                                                                           && this._readWriteTracker.GetContainerForTree ( ex )?.Equals ( r.Container ) is true ) );
            if ( replacement != null )
                return replacement;
            return base.VisitNode ( node );
        }
    }
}