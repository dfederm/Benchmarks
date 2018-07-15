namespace Benchmarks
{
    using System.Collections.Generic;
    using BenchmarkDotNet.Attributes;

    [MemoryDiagnoser]
    public class BitMapBenchmarks
    {
        [Params(100, 1000, 10000)]
        public int NumItems;

        [Benchmark]
        public int BitMap()
        {
            // Create
            var numItems = this.NumItems;
            var list = new List<long>(numItems);
            for (var i = 0; i < numItems; i++)
            {
                list.Add(Encode(i, i));
            }

            // Access
            var sum = 0;
            for (var i = 0; i < list.Count; i++)
            {
                var (a, b) = Decode(list[i]);
                sum += a + b;
            }

            return sum;
        }

        private static long Encode(int left, int right) => (long)left << 32 | (uint)right;

        private static (int, int) Decode(long value) => ((int)(value >> 32), (int)(value & 0xffffffffL));

        [Benchmark]
        public int BasicStruct()
        {
            // Create
            var numItems = this.NumItems;
            var list = new List<BasicStructImpl>(numItems);
            for (var i = 0; i < numItems; i++)
            {
                list.Add(new BasicStructImpl { a = i, b = i });
            }

            // Access
            var sum = 0;
            for (var i = 0; i < list.Count; i++)
            {
                var item = list[i];
                sum += item.a + item.b;
            }

            return sum;
        }

        private struct BasicStructImpl
        {
            public int a;
            public int b;
        }
    }
}
