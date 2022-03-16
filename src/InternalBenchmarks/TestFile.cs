using Loretta.CodeAnalysis.Text;

namespace Loretta.InternalBenchmarks
{
    public sealed class TestFile
    {
        public TestFile(string name, SourceText text)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public static TestFile Load(string path)
        {
            SourceText text;
            using (var stream = File.OpenRead(path))
                text = SourceText.From(stream);
            return new TestFile(Path.GetFileName(path), text);
        }

        public string Name { get; }
        public SourceText Text { get; }
        public string Contents => Text.ToString();

        public override string ToString() => Name;
    }
}
