// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Loretta.CodeAnalysis.Operations;
using Loretta.CodeAnalysis.PooledObjects;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.FlowAnalysis
{
    internal sealed partial class ControlFlowGraphBuilder
    {
        private class RegionBuilder
        {
            public ControlFlowRegionKind Kind;
            public RegionBuilder? Enclosing { get; private set; } = null;
            public readonly ITypeSymbol? ExceptionType;
            public BasicBlockBuilder? FirstBlock = null;
            public BasicBlockBuilder? LastBlock = null;
            public ArrayBuilder<RegionBuilder>? Regions = null;
            public ImmutableArray<ILocalSymbol> Locals;
            public ArrayBuilder<(IMethodSymbol, ILocalFunctionOperation)>? LocalFunctions = null;
            public ArrayBuilder<CaptureId>? CaptureIds = null;
            public readonly bool IsStackSpillRegion;
#if DEBUG
            private bool _aboutToFree = false;
#endif 

            public RegionBuilder(ControlFlowRegionKind kind, ITypeSymbol? exceptionType = null, ImmutableArray<ILocalSymbol> locals = default, bool isStackSpillRegion = false)
            {
                RoslynDebug.Assert(!isStackSpillRegion || (kind == ControlFlowRegionKind.LocalLifetime && locals.IsDefaultOrEmpty));
                Kind = kind;
                ExceptionType = exceptionType;
                Locals = locals.NullToEmpty();
                IsStackSpillRegion = isStackSpillRegion;
            }

            [MemberNotNullWhen(false, nameof(FirstBlock), nameof(LastBlock))]
            public bool IsEmpty
            {
                get
                {
                    RoslynDebug.Assert((FirstBlock == null) == (LastBlock == null));
#pragma warning disable CS8775 // LastBlock is null when exiting, verified equivalent to FirstBlock above
                    return FirstBlock == null;
#pragma warning restore CS8775
                }
            }

            [MemberNotNullWhen(true, nameof(Regions))]
            public bool HasRegions => Regions?.Count > 0;
            [MemberNotNullWhen(true, nameof(LocalFunctions))]
            public bool HasLocalFunctions => LocalFunctions?.Count > 0;
            [MemberNotNullWhen(true, nameof(CaptureIds))]
            public bool HasCaptureIds => CaptureIds?.Count > 0;

#if DEBUG
            public void AboutToFree() => _aboutToFree = true;
#endif

            [MemberNotNull(nameof(CaptureIds))]
            public void AddCaptureId(int captureId)
            {
                RoslynDebug.Assert(Kind != ControlFlowRegionKind.Root);

                if (CaptureIds == null)
                {
                    CaptureIds = ArrayBuilder<CaptureId>.GetInstance();
                }

                CaptureIds.Add(new CaptureId(captureId));
            }

            public void AddCaptureIds(ArrayBuilder<CaptureId>? others)
            {
                RoslynDebug.Assert(Kind != ControlFlowRegionKind.Root);

                if (others == null)
                {
                    return;
                }

                if (CaptureIds == null)
                {
                    CaptureIds = ArrayBuilder<CaptureId>.GetInstance();
                }

                CaptureIds.AddRange(others);
            }

            [MemberNotNull(nameof(LocalFunctions))]
            public void Add(IMethodSymbol symbol, ILocalFunctionOperation operation)
            {
                RoslynDebug.Assert(!IsStackSpillRegion);
                RoslynDebug.Assert(Kind != ControlFlowRegionKind.Root);
                RoslynDebug.Assert(symbol.MethodKind == MethodKind.LocalFunction);

                if (LocalFunctions == null)
                {
                    LocalFunctions = ArrayBuilder<(IMethodSymbol, ILocalFunctionOperation)>.GetInstance();
                }

                LocalFunctions.Add((symbol, operation));
            }

            public void AddRange(ArrayBuilder<(IMethodSymbol, ILocalFunctionOperation)>? others)
            {
                RoslynDebug.Assert(Kind != ControlFlowRegionKind.Root);

                if (others == null)
                {
                    return;
                }

                RoslynDebug.Assert(others.All(((IMethodSymbol m, ILocalFunctionOperation _) tuple) => tuple.m.MethodKind == MethodKind.LocalFunction));

                if (LocalFunctions == null)
                {
                    LocalFunctions = ArrayBuilder<(IMethodSymbol, ILocalFunctionOperation)>.GetInstance();
                }

                LocalFunctions.AddRange(others);
            }

            [MemberNotNull(nameof(Regions))]
            public void Add(RegionBuilder region)
            {
                if (Regions == null)
                {
                    Regions = ArrayBuilder<RegionBuilder>.GetInstance();
                }

#if DEBUG
                RoslynDebug.Assert(region.Enclosing == null || (region.Enclosing._aboutToFree && region.Enclosing.Enclosing == this));
#endif 
                region.Enclosing = this;
                Regions.Add(region);

#if DEBUG
                ControlFlowRegionKind lastKind = Regions.Last().Kind;
                switch (Kind)
                {
                    case ControlFlowRegionKind.FilterAndHandler:
                        RoslynDebug.Assert(Regions.Count <= 2);
                        RoslynDebug.Assert(lastKind == (Regions.Count < 2 ? ControlFlowRegionKind.Filter : ControlFlowRegionKind.Catch));
                        break;

                    case ControlFlowRegionKind.TryAndCatch:
                        if (Regions.Count == 1)
                        {
                            RoslynDebug.Assert(lastKind == ControlFlowRegionKind.Try);
                        }
                        else
                        {
                            RoslynDebug.Assert(lastKind == ControlFlowRegionKind.Catch || lastKind == ControlFlowRegionKind.FilterAndHandler);
                        }
                        break;

                    case ControlFlowRegionKind.TryAndFinally:
                        RoslynDebug.Assert(Regions.Count <= 2);
                        if (Regions.Count == 1)
                        {
                            RoslynDebug.Assert(lastKind == ControlFlowRegionKind.Try);
                        }
                        else
                        {
                            RoslynDebug.Assert(lastKind == ControlFlowRegionKind.Finally);
                        }
                        break;

                    default:
                        RoslynDebug.Assert(lastKind != ControlFlowRegionKind.Filter);
                        RoslynDebug.Assert(lastKind != ControlFlowRegionKind.Catch);
                        RoslynDebug.Assert(lastKind != ControlFlowRegionKind.Finally);
                        RoslynDebug.Assert(lastKind != ControlFlowRegionKind.Try);
                        break;
                }
#endif
            }

            public void Remove(RegionBuilder region)
            {
                RoslynDebug.Assert(region.Enclosing == this);
                RoslynDebug.Assert(Regions != null);

                if (Regions.Count == 1)
                {
                    RoslynDebug.Assert(Regions[0] == region);
                    Regions.Clear();
                }
                else
                {
                    Regions.RemoveAt(Regions.IndexOf(region));
                }

                region.Enclosing = null;
            }

            public void ReplaceRegion(RegionBuilder toReplace, ArrayBuilder<RegionBuilder> replaceWith)
            {
                RoslynDebug.Assert(toReplace.Enclosing == this);
                RoslynDebug.Assert(toReplace.FirstBlock!.Ordinal <= replaceWith.First().FirstBlock!.Ordinal);
                RoslynDebug.Assert(toReplace.LastBlock!.Ordinal >= replaceWith.Last().LastBlock!.Ordinal);
                RoslynDebug.Assert(Regions != null);

                int insertAt;

                if (Regions.Count == 1)
                {
                    RoslynDebug.Assert(Regions[0] == toReplace);
                    insertAt = 0;
                }
                else
                {
                    insertAt = Regions.IndexOf(toReplace);
                }

                int replaceWithCount = replaceWith.Count;
                if (replaceWithCount == 1)
                {
                    RegionBuilder single = replaceWith[0];
                    single.Enclosing = this;
                    Regions[insertAt] = single;
                }
                else
                {
                    int originalCount = Regions.Count;
                    Regions.Count = replaceWithCount - 1 + originalCount;

                    for (int i = originalCount - 1, j = Regions.Count - 1; i > insertAt; i--, j--)
                    {
                        Regions[j] = Regions[i];
                    }

                    foreach (RegionBuilder region in replaceWith)
                    {
                        region.Enclosing = this;
                        Regions[insertAt++] = region;
                    }
                }

                toReplace.Enclosing = null;
            }

            [MemberNotNull(nameof(FirstBlock), nameof(LastBlock))]
            public void ExtendToInclude(BasicBlockBuilder block)
            {
                RoslynDebug.Assert(block != null);
                RoslynDebug.Assert((Kind != ControlFlowRegionKind.FilterAndHandler &&
                              Kind != ControlFlowRegionKind.TryAndCatch &&
                              Kind != ControlFlowRegionKind.TryAndFinally) ||
                              Regions!.Last().LastBlock == block);

                if (FirstBlock == null)
                {
                    RoslynDebug.Assert(LastBlock == null);

                    if (!HasRegions)
                    {
                        FirstBlock = block;
                        LastBlock = block;
                        return;
                    }

                    FirstBlock = Regions.First().FirstBlock;
                    RoslynDebug.Assert(FirstBlock != null);
                    RoslynDebug.Assert(Regions.Count == 1 && Regions.First().LastBlock == block);
                }
                else
                {
                    RoslynDebug.Assert(LastBlock!.Ordinal < block.Ordinal);
                    RoslynDebug.Assert(!HasRegions || Regions.Last().LastBlock!.Ordinal <= block.Ordinal);
                }

                LastBlock = block;
            }

            public void Free()
            {
#if DEBUG
                RoslynDebug.Assert(_aboutToFree);
#endif 
                Enclosing = null;
                FirstBlock = null;
                LastBlock = null;
                Regions?.Free();
                Regions = null;
                LocalFunctions?.Free();
                LocalFunctions = null;
                CaptureIds?.Free();
                CaptureIds = null;
            }

            public ControlFlowRegion ToImmutableRegionAndFree(ArrayBuilder<BasicBlockBuilder> blocks,
                                                              ArrayBuilder<IMethodSymbol> localFunctions,
                                                              ImmutableDictionary<IMethodSymbol, (ControlFlowRegion region, ILocalFunctionOperation operation, int ordinal)>.Builder localFunctionsMap,
                                                              ImmutableDictionary<IFlowAnonymousFunctionOperation, (ControlFlowRegion region, int ordinal)>.Builder? anonymousFunctionsMapOpt,
                                                              ControlFlowRegion? enclosing)
            {
#if DEBUG
                RoslynDebug.Assert(!_aboutToFree);
#endif 
                RoslynDebug.Assert(!IsEmpty);

                int localFunctionsBefore = localFunctions.Count;

                if (HasLocalFunctions)
                {
                    foreach ((IMethodSymbol method, IOperation _) in LocalFunctions)
                    {
                        localFunctions.Add(method);
                    }
                }

                ImmutableArray<ControlFlowRegion> subRegions;

                if (HasRegions)
                {
                    var builder = ArrayBuilder<ControlFlowRegion>.GetInstance(Regions.Count);

                    foreach (RegionBuilder region in Regions)
                    {
                        builder.Add(region.ToImmutableRegionAndFree(blocks, localFunctions, localFunctionsMap, anonymousFunctionsMapOpt, enclosing: null));
                    }

                    subRegions = builder.ToImmutableAndFree();
                }
                else
                {
                    subRegions = ImmutableArray<ControlFlowRegion>.Empty;
                }

                CaptureIds?.Sort((x, y) => x.Value.CompareTo(y.Value));

                var result = new ControlFlowRegion(Kind, FirstBlock.Ordinal, LastBlock.Ordinal, subRegions,
                                                   Locals,
                                                   LocalFunctions?.SelectAsArray(((IMethodSymbol, ILocalFunctionOperation) tuple) => tuple.Item1) ?? default,
                                                   CaptureIds?.ToImmutable() ?? default,
                                                   ExceptionType,
                                                   enclosing);

                if (HasLocalFunctions)
                {
                    foreach ((IMethodSymbol method, ILocalFunctionOperation operation) in LocalFunctions)
                    {
                        localFunctionsMap.Add(method, (result, operation, localFunctionsBefore++));
                    }
                }

                int firstBlockWithoutRegion = FirstBlock.Ordinal;

                foreach (ControlFlowRegion region in subRegions)
                {
                    for (int i = firstBlockWithoutRegion; i < region.FirstBlockOrdinal; i++)
                    {
                        setRegion(blocks[i]);
                    }

                    firstBlockWithoutRegion = region.LastBlockOrdinal + 1;
                }

                for (int i = firstBlockWithoutRegion; i <= LastBlock.Ordinal; i++)
                {
                    setRegion(blocks[i]);
                }

#if DEBUG
                AboutToFree();
#endif 
                Free();
                return result;

                void setRegion(BasicBlockBuilder block)
                {
                    RoslynDebug.Assert(block.Region == null);
                    block.Region = result;

                    // Populate the map of IFlowAnonymousFunctionOperation nodes, if we have any
                    if (anonymousFunctionsMapOpt != null)
                    {
                        (ImmutableDictionary<IFlowAnonymousFunctionOperation, (ControlFlowRegion region, int ordinal)>.Builder map, ControlFlowRegion region) argument = (anonymousFunctionsMapOpt, result);

                        if (block.HasStatements)
                        {
                            foreach (IOperation o in block.StatementsOpt)
                            {
                                AnonymousFunctionsMapBuilder.Instance.Visit(o, argument);
                            }
                        }

                        AnonymousFunctionsMapBuilder.Instance.Visit(block.BranchValue, argument);
                    }
                }
            }

            private sealed class AnonymousFunctionsMapBuilder :
                OperationVisitor<(ImmutableDictionary<IFlowAnonymousFunctionOperation, (ControlFlowRegion region, int ordinal)>.Builder map, ControlFlowRegion region), IOperation>
            {
                public static readonly AnonymousFunctionsMapBuilder Instance = new AnonymousFunctionsMapBuilder();

                public override IOperation? VisitFlowAnonymousFunction(
                    IFlowAnonymousFunctionOperation operation,
                    (ImmutableDictionary<IFlowAnonymousFunctionOperation, (ControlFlowRegion region, int ordinal)>.Builder map, ControlFlowRegion region) argument)
                {
                    argument.map.Add(operation, (argument.region, argument.map.Count));
                    return base.VisitFlowAnonymousFunction(operation, argument);
                }

                internal override IOperation? VisitNoneOperation(IOperation operation, (ImmutableDictionary<IFlowAnonymousFunctionOperation, (ControlFlowRegion region, int ordinal)>.Builder map, ControlFlowRegion region) argument)
                {
                    return DefaultVisit(operation, argument);
                }

                public override IOperation? DefaultVisit(
                    IOperation operation,
                    (ImmutableDictionary<IFlowAnonymousFunctionOperation, (ControlFlowRegion region, int ordinal)>.Builder map, ControlFlowRegion region) argument)
                {
                    foreach (IOperation child in ((Operation)operation).ChildOperations)
                    {
                        Visit(child, argument);
                    }

                    return null;
                }
            }
        }
    }
}
