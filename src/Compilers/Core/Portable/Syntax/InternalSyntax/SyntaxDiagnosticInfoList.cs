// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.CodeAnalysis.Syntax.InternalSyntax
{
    // Avoid implementing IEnumerable so we do not get any unintentional boxing.
    internal struct SyntaxDiagnosticInfoList
    {
        private readonly GreenNode _node;

        internal SyntaxDiagnosticInfoList(GreenNode node)
        {
            _node = node;
        }

        public Enumerator GetEnumerator() => new(_node);

        internal bool Any(Func<DiagnosticInfo, bool> predicate)
        {
            var enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (predicate(enumerator.Current))
                    return true;
            }

            return false;
        }

        public struct Enumerator
        {
            private struct NodeIteration
            {
                internal readonly GreenNode Node;
                internal int DiagnosticIndex;
                internal int SlotIndex;

                internal NodeIteration(GreenNode node)
                {
                    Node = node;
                    SlotIndex = -1;
                    DiagnosticIndex = -1;
                }
            }

            private NodeIteration[]? _stack;
            private int _count;

            public DiagnosticInfo Current { get; private set; }

            internal Enumerator(GreenNode node)
            {
                Current = null!;
                _stack = null;
                _count = 0;
                if (node != null && node.ContainsDiagnostics)
                {
                    _stack = new NodeIteration[8];
                    PushNodeOrToken(node);
                }
            }

            public bool MoveNext()
            {
                while (_count > 0)
                {
                    var diagIndex = _stack![_count - 1].DiagnosticIndex;
                    var node = _stack[_count - 1].Node;
                    var diags = node.GetDiagnostics();
                    if (diagIndex < diags.Length - 1)
                    {
                        diagIndex++;
                        Current = diags[diagIndex];
                        _stack[_count - 1].DiagnosticIndex = diagIndex;
                        return true;
                    }

                    var slotIndex = _stack[_count - 1].SlotIndex;
                tryAgain:
                    if (slotIndex < node.SlotCount - 1)
                    {
                        slotIndex++;
                        var child = node.GetSlot(slotIndex);
                        if (child == null || !child.ContainsDiagnostics)
                        {
                            goto tryAgain;
                        }

                        _stack[_count - 1].SlotIndex = slotIndex;
                        PushNodeOrToken(child);
                    }
                    else
                    {
                        Pop();
                    }
                }

                return false;
            }

            private void PushNodeOrToken(GreenNode node)
            {
                if (node.IsToken)
                {
                    PushToken(node);
                }
                else
                {
                    Push(node);
                }
            }

            private void PushToken(GreenNode token)
            {
                var trailing = token.GetTrailingTriviaCore();
                if (trailing != null)
                {
                    Push(trailing);
                }

                Push(token);
                var leading = token.GetLeadingTriviaCore();
                if (leading != null)
                {
                    Push(leading);
                }
            }

            private void Push(GreenNode node)
            {
                LorettaDebug.Assert(_stack is not null);
                if (_count >= _stack.Length)
                {
                    var tmp = new NodeIteration[_stack.Length * 2];
                    Array.Copy(_stack, tmp, _stack.Length);
                    _stack = tmp;
                }

                _stack[_count] = new NodeIteration(node);
                _count++;
            }

            private void Pop() => _count--;
        }
    }
}
