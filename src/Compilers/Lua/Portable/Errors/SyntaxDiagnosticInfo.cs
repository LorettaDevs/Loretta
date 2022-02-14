namespace Loretta.CodeAnalysis.Lua
{
    internal class SyntaxDiagnosticInfo : DiagnosticInfo
    {
        static SyntaxDiagnosticInfo()
        {
            ObjectBinder.RegisterTypeReader(typeof(SyntaxDiagnosticInfo), r => new SyntaxDiagnosticInfo(r));
        }

        internal readonly int Offset;
        internal readonly int Width;

        internal SyntaxDiagnosticInfo(int offset, int width, ErrorCode code, params object[] args)
            : base(Lua.MessageProvider.Instance, (int) code, args)
        {
            LorettaDebug.Assert(width >= 0);
            Offset = offset;
            Width = width;
        }

        internal SyntaxDiagnosticInfo(int offset, int width, ErrorCode code)
            : this(offset, width, code, Array.Empty<object>())
        {
        }

        internal SyntaxDiagnosticInfo(ErrorCode code, params object[] args)
            : this(0, 0, code, args)
        {
        }

        internal SyntaxDiagnosticInfo(ErrorCode code)
            : this(0, 0, code)
        {
        }

        public SyntaxDiagnosticInfo WithOffset(int offset) => new(offset, Width, (ErrorCode) Code, Arguments);

        #region Serialization

        protected override void WriteTo(ObjectWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteInt32(Offset);
            writer.WriteInt32(Width);
        }

        protected SyntaxDiagnosticInfo(ObjectReader reader)
            : base(reader)
        {
            Offset = reader.ReadInt32();
            Width = reader.ReadInt32();
        }

        #endregion
    }
}
