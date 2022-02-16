// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

namespace Loretta.CodeAnalysis
{
    public static partial class SyntaxNodeExtensions
    {
        private static readonly ConditionalWeakTable<SyntaxNode, SyntaxAnnotation> s_nodeToIdMap = new();

        private static readonly ConditionalWeakTable<SyntaxNode, CurrentNodes> s_rootToCurrentNodesMap = new();

        internal const string IdAnnotationKind = "Id";

        /// <summary>
        /// Creates a new tree of nodes with the specified nodes being tracked.
        /// 
        /// Use GetCurrentNode on the subtree resulting from this operation, or any transformation of it,
        /// to get the current node corresponding to the original tracked node.
        /// </summary>
        /// <param name="root">The root of the subtree containing the nodes to be tracked.</param>
        /// <param name="nodes">One or more nodes that are descendants of the root node.</param>
        public static TRoot TrackNodes<TRoot>(this TRoot root, IEnumerable<SyntaxNode> nodes!!)
            where TRoot : SyntaxNode
        {

            // create an id for each node
            foreach (var node in nodes)
            {
                if (!IsDescendant(root, node))
                {
                    throw new ArgumentException(CodeAnalysisResources.InvalidNodeToTrack);
                }

                s_nodeToIdMap.GetValue(node, n => new SyntaxAnnotation(IdAnnotationKind));
            }

            return root.ReplaceNodes(nodes, (n, r) => n.HasAnnotation(GetId(n)!) ? r : r.WithAdditionalAnnotations(GetId(n)!));
        }

        /// <summary>
        /// Creates a new tree of nodes with the specified nodes being tracked.
        /// 
        /// Use GetCurrentNode on the subtree resulting from this operation, or any transformation of it,
        /// to get the current node corresponding to the original tracked node.
        /// </summary>
        /// <param name="root">The root of the subtree containing the nodes to be tracked.</param>
        /// <param name="nodes">One or more nodes that are descendants of the root node.</param>
        public static TRoot TrackNodes<TRoot>(this TRoot root, params SyntaxNode[] nodes)
            where TRoot : SyntaxNode =>
            TrackNodes(root, (IEnumerable<SyntaxNode>) nodes);

        /// <summary>
        /// Gets the nodes within the subtree corresponding to the original tracked node.
        /// Use TrackNodes to start tracking nodes.
        /// </summary>
        /// <param name="root">The root of the subtree containing the current node corresponding to the original tracked node.</param>
        /// <param name="node">The node instance originally tracked.</param>
        public static IEnumerable<TNode> GetCurrentNodes<TNode>(this SyntaxNode root, TNode node!!)
            where TNode : SyntaxNode =>
            GetCurrentNodeFromTrueRoots(GetRoot(root), node).OfType<TNode>();

        /// <summary>
        /// Gets the node within the subtree corresponding to the original tracked node.
        /// Use TrackNodes to start tracking nodes.
        /// </summary>
        /// <param name="root">The root of the subtree containing the current node corresponding to the original tracked node.</param>
        /// <param name="node">The node instance originally tracked.</param>
        public static TNode? GetCurrentNode<TNode>(this SyntaxNode root, TNode node)
            where TNode : SyntaxNode =>
            GetCurrentNodes(root, node).SingleOrDefault();

        /// <summary>
        /// Gets the nodes within the subtree corresponding to the original tracked nodes.
        /// Use TrackNodes to start tracking nodes.
        /// </summary>
        /// <param name="root">The root of the subtree containing the current nodes corresponding to the original tracked nodes.</param>
        /// <param name="nodes">One or more node instances originally tracked.</param>
        public static IEnumerable<TNode> GetCurrentNodes<TNode>(this SyntaxNode root, IEnumerable<TNode> nodes!!)
            where TNode : SyntaxNode
        {
            var trueRoot = GetRoot(root);

            foreach (var node in nodes)
            {
                foreach (var newNode in GetCurrentNodeFromTrueRoots(trueRoot, node).OfType<TNode>())
                {
                    yield return newNode;
                }
            }
        }

        private static IReadOnlyList<SyntaxNode> GetCurrentNodeFromTrueRoots(SyntaxNode trueRoot, SyntaxNode node)
        {
            var id = GetId(node);
            if (id is not null)
            {
                CurrentNodes tracked = s_rootToCurrentNodesMap.GetValue(trueRoot, r => new CurrentNodes(r));
                return tracked.GetNodes(id);
            }
            else
            {
                return SpecializedCollections.EmptyReadOnlyList<SyntaxNode>();
            }
        }

        private static SyntaxAnnotation? GetId(SyntaxNode original)
        {
            s_nodeToIdMap.TryGetValue(original, out var id);
            return id;
        }

        private static SyntaxNode GetRoot(SyntaxNode node)
        {
            while (true)
            {
                while (node.Parent != null)
                {
                    node = node.Parent;
                }

                if (!node.IsStructuredTrivia)
                {
                    return node;
                }
                else
                {
                    node = ((IStructuredTriviaSyntax) node).ParentTrivia.Token.Parent!;
                    LorettaDebug.Assert(node is not null);
                }
            }
        }

        private static bool IsDescendant(SyntaxNode root, SyntaxNode node)
        {
            while (node != null)
            {
                if (node == root)
                {
                    return true;
                }

                if (node.Parent != null)
                {
                    node = node.Parent;
                }
                else if (!node.IsStructuredTrivia)
                {
                    break;
                }
                else
                {
                    node = ((IStructuredTriviaSyntax) node).ParentTrivia.Token.Parent!;
                    LorettaDebug.Assert(node is not null);
                }
            }

            return false;
        }

        private class CurrentNodes
        {
            private readonly Dictionary<SyntaxAnnotation, IReadOnlyList<SyntaxNode>> _idToNodeMap;

            public CurrentNodes(SyntaxNode root)
            {
                // there could be multiple nodes with same annotation if a tree is rewritten with
                // same node injected multiple times.
                var map = new Dictionary<SyntaxAnnotation, List<SyntaxNode>>();

                foreach (var node in root.GetAnnotatedNodesAndTokens(IdAnnotationKind).Select(n => n.AsNode()!))
                {
                    LorettaDebug.Assert(node is not null);
                    foreach (var id in node.GetAnnotations(IdAnnotationKind))
                    {
                        if (!map.TryGetValue(id, out var list))
                        {
                            list = new List<SyntaxNode>();
                            map.Add(id, list);
                        }

                        list.Add(node);
                    }
                }

                _idToNodeMap = map.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<SyntaxNode>) ImmutableArray.CreateRange(kv.Value));
            }

            public IReadOnlyList<SyntaxNode> GetNodes(SyntaxAnnotation id)
            {
                if (_idToNodeMap.TryGetValue(id, out var nodes))
                {
                    return nodes;
                }
                else
                {
                    return SpecializedCollections.EmptyReadOnlyList<SyntaxNode>();
                }
            }
        }
    }
}
