namespace Benchmarks
{
    using System;
    using System.Text;
    using BenchmarkDotNet.Attributes;

    [MemoryDiagnoser]
    public class StringBuilderBenchmarks
    {
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private string[] items;

        [Params(100, 1000, 10000)]
        public int NumItems;

        [Params(128, 1024)]
        public int InitialCapacity;

        [GlobalSetup]
        public void Setup()
        {
            var random = new Random(27);

            this.items = new string[this.NumItems];
            for (var i = 0; i < this.NumItems; i++)
            {
                var len = random.Next(10, 20);
                var chars = new char[len];
                for (var j = 0; j < len; j++)
                {
                    chars[j] = Chars[random.Next(Chars.Length)];
                }

                this.items[i] = new string(chars);
            }
        }

        [Benchmark]
        public string AppendFormat()
        {
            var sb = new StringBuilder(this.InitialCapacity);

            for (var i = 0; i < this.items.Length; i++)
            {
                sb.AppendFormat("[{0}]", this.items[i]);
            }

            return sb.ToString();
        }

        [Benchmark]
        public string AppendStr()
        {
            var sb = new StringBuilder(this.InitialCapacity);

            for (var i = 0; i < this.items.Length; i++)
            {
                sb.Append("[");
                sb.Append(this.items[i]);
                sb.Append("]");
            }

            return sb.ToString();
        }

        [Benchmark]
        public string AppendChars()
        {
            var sb = new StringBuilder(this.InitialCapacity);

            for (var i = 0; i < this.items.Length; i++)
            {
                sb.Append('[');
                sb.Append(this.items[i]);
                sb.Append(']');
            }

            return sb.ToString();
        }
    }
}
