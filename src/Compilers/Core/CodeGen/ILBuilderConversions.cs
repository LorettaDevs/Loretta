// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.CodeGen
{
    internal partial class ILBuilder
    {
        public void EmitNumericConversion(Loretta.Cci.PrimitiveTypeCode fromPredefTypeKind, Loretta.Cci.PrimitiveTypeCode toPredefTypeKind, bool @checked)
        {
            bool fromUnsigned = fromPredefTypeKind.IsUnsigned();

            switch (toPredefTypeKind)
            {
                case Loretta.Cci.PrimitiveTypeCode.Int8:
                    switch (fromPredefTypeKind)
                    {
                        case Loretta.Cci.PrimitiveTypeCode.Int8:
                            break; // NOP
                        default:
                            if (@checked)
                                this.EmitOpCode(fromUnsigned ? ILOpCode.Conv_ovf_i1_un : ILOpCode.Conv_ovf_i1);
                            else
                                this.EmitOpCode(ILOpCode.Conv_i1);
                            break;
                    }
                    break;

                case Loretta.Cci.PrimitiveTypeCode.UInt8:
                    switch (fromPredefTypeKind)
                    {
                        case Loretta.Cci.PrimitiveTypeCode.UInt8:
                            break; // NOP
                        default:
                            if (@checked)
                                this.EmitOpCode(fromUnsigned ? ILOpCode.Conv_ovf_u1_un : ILOpCode.Conv_ovf_u1);
                            else
                                this.EmitOpCode(ILOpCode.Conv_u1);
                            break;
                    }
                    break;

                case Loretta.Cci.PrimitiveTypeCode.Int16:
                    switch (fromPredefTypeKind)
                    {
                        case Loretta.Cci.PrimitiveTypeCode.Int8:
                        case Loretta.Cci.PrimitiveTypeCode.UInt8:
                        case Loretta.Cci.PrimitiveTypeCode.Int16:
                            break; // NOP
                        default:
                            if (@checked)
                                this.EmitOpCode(fromUnsigned ? ILOpCode.Conv_ovf_i2_un : ILOpCode.Conv_ovf_i2);
                            else
                                this.EmitOpCode(ILOpCode.Conv_i2);
                            break;
                    }
                    break;

                case Loretta.Cci.PrimitiveTypeCode.Char:
                case Loretta.Cci.PrimitiveTypeCode.UInt16:
                    switch (fromPredefTypeKind)
                    {
                        case Loretta.Cci.PrimitiveTypeCode.UInt8:
                        case Loretta.Cci.PrimitiveTypeCode.UInt16:
                        case Loretta.Cci.PrimitiveTypeCode.Char:
                            break; // NOP
                        default:
                            if (@checked)
                                this.EmitOpCode(fromUnsigned ? ILOpCode.Conv_ovf_u2_un : ILOpCode.Conv_ovf_u2);
                            else
                                this.EmitOpCode(ILOpCode.Conv_u2);
                            break;
                    }
                    break;

                case Loretta.Cci.PrimitiveTypeCode.Int32:
                    switch (fromPredefTypeKind)
                    {
                        case Loretta.Cci.PrimitiveTypeCode.Int8:
                        case Loretta.Cci.PrimitiveTypeCode.UInt8:
                        case Loretta.Cci.PrimitiveTypeCode.Int16:
                        case Loretta.Cci.PrimitiveTypeCode.UInt16:
                        case Loretta.Cci.PrimitiveTypeCode.Int32:
                        case Loretta.Cci.PrimitiveTypeCode.Char:
                            break; // NOP
                        case Loretta.Cci.PrimitiveTypeCode.UInt32:
                            if (@checked)
                                this.EmitOpCode(ILOpCode.Conv_ovf_i4_un);
                            break; // NOP in unchecked
                        default:
                            if (@checked)
                                this.EmitOpCode(fromUnsigned ? ILOpCode.Conv_ovf_i4_un : ILOpCode.Conv_ovf_i4);
                            else
                                this.EmitOpCode(ILOpCode.Conv_i4);
                            break;
                    }
                    break;

                case Loretta.Cci.PrimitiveTypeCode.UInt32:
                    switch (fromPredefTypeKind)
                    {
                        case Loretta.Cci.PrimitiveTypeCode.UInt8:
                        case Loretta.Cci.PrimitiveTypeCode.UInt16:
                        case Loretta.Cci.PrimitiveTypeCode.UInt32:
                        case Loretta.Cci.PrimitiveTypeCode.Char:
                            break; // NOP
                        case Loretta.Cci.PrimitiveTypeCode.Int8:
                        case Loretta.Cci.PrimitiveTypeCode.Int16:
                        case Loretta.Cci.PrimitiveTypeCode.Int32:
                            if (@checked)
                                this.EmitOpCode(ILOpCode.Conv_ovf_u4);
                            break; // NOP in unchecked
                        default:
                            if (@checked)
                                this.EmitOpCode(fromUnsigned ? ILOpCode.Conv_ovf_u4_un : ILOpCode.Conv_ovf_u4);
                            else
                                this.EmitOpCode(ILOpCode.Conv_u4);
                            break;
                    }
                    break;

                case Loretta.Cci.PrimitiveTypeCode.IntPtr:
                    switch (fromPredefTypeKind)
                    {
                        case Loretta.Cci.PrimitiveTypeCode.IntPtr:
                            break; // NOP
                        case Loretta.Cci.PrimitiveTypeCode.Int8:
                        case Loretta.Cci.PrimitiveTypeCode.Int16:
                        case Loretta.Cci.PrimitiveTypeCode.Int32:
                            this.EmitOpCode(ILOpCode.Conv_i); // potentially widening, so not NOP
                            break;
                        case Loretta.Cci.PrimitiveTypeCode.UInt8:
                        case Loretta.Cci.PrimitiveTypeCode.UInt16:
                        case Loretta.Cci.PrimitiveTypeCode.Char:
                            // Doesn't actually matter whether we sign extend, because
                            // bit 32 can't be set in any of these types.
                            this.EmitOpCode(ILOpCode.Conv_u); // potentially widening, so not NOP
                            break;
                        case Loretta.Cci.PrimitiveTypeCode.UInt32:
                            if (@checked)
                                this.EmitOpCode(ILOpCode.Conv_ovf_i_un);
                            else
                                // Don't want to sign extend if this is a widening conversion.
                                this.EmitOpCode(ILOpCode.Conv_u); // potentially widening, so not NOP
                            break;
                        case Loretta.Cci.PrimitiveTypeCode.Pointer:
                        case Loretta.Cci.PrimitiveTypeCode.FunctionPointer:
                            if (@checked)
                                goto default;
                            break; // NOP
                        default:
                            if (@checked)
                                this.EmitOpCode(fromUnsigned ? ILOpCode.Conv_ovf_i_un : ILOpCode.Conv_ovf_i);
                            else
                                this.EmitOpCode(ILOpCode.Conv_i);
                            break;
                    }
                    break;

                case Loretta.Cci.PrimitiveTypeCode.UIntPtr:
                    switch (fromPredefTypeKind)
                    {
                        case Loretta.Cci.PrimitiveTypeCode.UIntPtr:
                        case Loretta.Cci.PrimitiveTypeCode.Pointer:
                        case Loretta.Cci.PrimitiveTypeCode.FunctionPointer:
                            break; // NOP
                        case Loretta.Cci.PrimitiveTypeCode.UInt8:
                        case Loretta.Cci.PrimitiveTypeCode.UInt16:
                        case Loretta.Cci.PrimitiveTypeCode.UInt32:
                        case Loretta.Cci.PrimitiveTypeCode.Char:
                            this.EmitOpCode(ILOpCode.Conv_u); // potentially widening, so not NOP
                            break;
                        case Loretta.Cci.PrimitiveTypeCode.Int8:
                        case Loretta.Cci.PrimitiveTypeCode.Int16:
                        case Loretta.Cci.PrimitiveTypeCode.Int32:
                            if (@checked)
                                this.EmitOpCode(ILOpCode.Conv_ovf_u);
                            else
                                this.EmitOpCode(ILOpCode.Conv_i); // potentially widening, so not NOP
                            break;
                        default:
                            if (@checked)
                                this.EmitOpCode(fromUnsigned ? ILOpCode.Conv_ovf_u_un : ILOpCode.Conv_ovf_u);
                            else
                                this.EmitOpCode(ILOpCode.Conv_u);
                            break;
                    }
                    break;

                case Loretta.Cci.PrimitiveTypeCode.Int64:
                    switch (fromPredefTypeKind)
                    {
                        case Loretta.Cci.PrimitiveTypeCode.Int64:
                            break; //NOP
                        case Loretta.Cci.PrimitiveTypeCode.Int8:
                        case Loretta.Cci.PrimitiveTypeCode.Int16:
                        case Loretta.Cci.PrimitiveTypeCode.Int32:
                        case Loretta.Cci.PrimitiveTypeCode.IntPtr:
                            this.EmitOpCode(ILOpCode.Conv_i8); // sign extend
                            break;
                        case Loretta.Cci.PrimitiveTypeCode.UInt8:
                        case Loretta.Cci.PrimitiveTypeCode.UInt16:
                        case Loretta.Cci.PrimitiveTypeCode.UInt32:
                        case Loretta.Cci.PrimitiveTypeCode.Char:
                            this.EmitOpCode(ILOpCode.Conv_u8); // 0 extend
                            break;
                        case Loretta.Cci.PrimitiveTypeCode.Pointer:
                        case Loretta.Cci.PrimitiveTypeCode.FunctionPointer:
                        case Loretta.Cci.PrimitiveTypeCode.UIntPtr:
                            if (@checked)
                                this.EmitOpCode(ILOpCode.Conv_ovf_i8_un);
                            else
                                this.EmitOpCode(ILOpCode.Conv_u8); // 0 extend if unchecked
                            break;
                        case Loretta.Cci.PrimitiveTypeCode.UInt64:
                            if (@checked)
                                this.EmitOpCode(ILOpCode.Conv_ovf_i8_un);
                            break; // NOP in unchecked
                        default:
                            RoslynDebug.Assert(fromPredefTypeKind.IsFloatingPoint());
                            if (@checked)
                                this.EmitOpCode(ILOpCode.Conv_ovf_i8);
                            else
                                this.EmitOpCode(ILOpCode.Conv_i8);
                            break;
                    }
                    break;

                case Loretta.Cci.PrimitiveTypeCode.UInt64:
                    switch (fromPredefTypeKind)
                    {
                        case Loretta.Cci.PrimitiveTypeCode.UInt64:
                            break; //NOP
                        case Loretta.Cci.PrimitiveTypeCode.UInt8:
                        case Loretta.Cci.PrimitiveTypeCode.UInt16:
                        case Loretta.Cci.PrimitiveTypeCode.UInt32:
                        case Loretta.Cci.PrimitiveTypeCode.Pointer:
                        case Loretta.Cci.PrimitiveTypeCode.FunctionPointer:
                        case Loretta.Cci.PrimitiveTypeCode.UIntPtr:
                        case Loretta.Cci.PrimitiveTypeCode.Char:
                            this.EmitOpCode(ILOpCode.Conv_u8); // 0 extend
                            break;
                        case Loretta.Cci.PrimitiveTypeCode.Int8:
                        case Loretta.Cci.PrimitiveTypeCode.Int16:
                        case Loretta.Cci.PrimitiveTypeCode.Int32:
                        case Loretta.Cci.PrimitiveTypeCode.IntPtr:
                            if (@checked)
                                this.EmitOpCode(ILOpCode.Conv_ovf_u8);
                            else
                                this.EmitOpCode(ILOpCode.Conv_i8); // sign extend if unchecked
                            break;
                        case Loretta.Cci.PrimitiveTypeCode.Int64:
                            if (@checked)
                                this.EmitOpCode(ILOpCode.Conv_ovf_u8);
                            break; // NOP in unchecked
                        default:
                            RoslynDebug.Assert(fromPredefTypeKind.IsFloatingPoint());
                            if (@checked)
                                this.EmitOpCode(ILOpCode.Conv_ovf_u8);
                            else
                                this.EmitOpCode(ILOpCode.Conv_u8);
                            break;
                    }
                    break;

                case Loretta.Cci.PrimitiveTypeCode.Float32:
                    switch (fromPredefTypeKind)
                    {
                        case Loretta.Cci.PrimitiveTypeCode.UInt32:
                        case Loretta.Cci.PrimitiveTypeCode.UInt64:
                            this.EmitOpCode(ILOpCode.Conv_r_un);
                            break;
                    }
                    this.EmitOpCode(ILOpCode.Conv_r4);
                    break;

                case Loretta.Cci.PrimitiveTypeCode.Float64:
                    switch (fromPredefTypeKind)
                    {
                        case Loretta.Cci.PrimitiveTypeCode.UInt32:
                        case Loretta.Cci.PrimitiveTypeCode.UInt64:
                            this.EmitOpCode(ILOpCode.Conv_r_un);
                            break;
                    }
                    this.EmitOpCode(ILOpCode.Conv_r8);
                    break;

                case Loretta.Cci.PrimitiveTypeCode.Pointer:
                case Loretta.Cci.PrimitiveTypeCode.FunctionPointer:
                    if (@checked)
                    {
                        switch (fromPredefTypeKind)
                        {
                            case Loretta.Cci.PrimitiveTypeCode.UInt8:
                            case Loretta.Cci.PrimitiveTypeCode.UInt16:
                            case Loretta.Cci.PrimitiveTypeCode.UInt32:
                                this.EmitOpCode(ILOpCode.Conv_u);
                                break;
                            case Loretta.Cci.PrimitiveTypeCode.UInt64:
                                this.EmitOpCode(ILOpCode.Conv_ovf_u_un);
                                break;
                            case Loretta.Cci.PrimitiveTypeCode.Int8:
                            case Loretta.Cci.PrimitiveTypeCode.Int16:
                            case Loretta.Cci.PrimitiveTypeCode.Int32:
                            case Loretta.Cci.PrimitiveTypeCode.Int64:
                                this.EmitOpCode(ILOpCode.Conv_ovf_u);
                                break;
                            case Loretta.Cci.PrimitiveTypeCode.IntPtr:
                                this.EmitOpCode(ILOpCode.Conv_ovf_u);
                                break;
                            case Loretta.Cci.PrimitiveTypeCode.UIntPtr:
                                break; // NOP
                            default:
                                throw ExceptionUtilities.UnexpectedValue(fromPredefTypeKind);
                        }
                    }
                    else
                    {
                        switch (fromPredefTypeKind)
                        {
                            case Loretta.Cci.PrimitiveTypeCode.UInt8:
                            case Loretta.Cci.PrimitiveTypeCode.UInt16:
                            case Loretta.Cci.PrimitiveTypeCode.UInt32:
                            case Loretta.Cci.PrimitiveTypeCode.UInt64:
                            case Loretta.Cci.PrimitiveTypeCode.Int64:
                                this.EmitOpCode(ILOpCode.Conv_u);
                                break;
                            case Loretta.Cci.PrimitiveTypeCode.Int8:
                            case Loretta.Cci.PrimitiveTypeCode.Int16:
                            case Loretta.Cci.PrimitiveTypeCode.Int32:
                                // This matches dev10.  Presumably, we're using conv_i,
                                // rather than conv_u, to sign-extend the value.
                                this.EmitOpCode(ILOpCode.Conv_i);
                                break;
                            case Loretta.Cci.PrimitiveTypeCode.IntPtr:
                            case Loretta.Cci.PrimitiveTypeCode.UIntPtr:
                                break; // NOP
                            default:
                                throw ExceptionUtilities.UnexpectedValue(fromPredefTypeKind);
                        }
                    }
                    break;

                default:
                    throw ExceptionUtilities.UnexpectedValue(toPredefTypeKind);
            }
        }
    }
}
