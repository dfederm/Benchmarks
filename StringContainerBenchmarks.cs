namespace Benchmarks
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using BenchmarkDotNet.Attributes;

    [MemoryDiagnoser]
    public class StringContainerBenchmarks
    {
        private const string Format = "Item1: {0} Item2: {1}";

        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private string[] items1;

        private string[] items2;

        [Params(100, 1000, 10000)]
        public int NumItems;

        [GlobalSetup]
        public void Setup()
        {
            var random = new Random(27);

            this.items1 = new string[this.NumItems];
            this.items2 = new string[this.NumItems];

            for (var i = 0; i < this.NumItems; i++)
            {
                var len1 = random.Next(10, 20);
                var len2 = random.Next(10, 20);

                var chars1 = new char[len1];
                var chars2 = new char[len2];

                for (var j = 0; j < len1; j++)
                {
                    chars1[j] = Chars[random.Next(Chars.Length)];
                }

                for (var j = 0; j < len2; j++)
                {
                    chars2[j] = Chars[random.Next(Chars.Length)];
                }

                this.items1[i] = new string(chars1);
                this.items2[i] = new string(chars2);
            }
        }

        [Benchmark]
        public string ImmediateStringCreation()
        {
            var sb = new StringBuilder();

            foreach (var holder in this.GetStringHolders())
            {
                sb.Append(holder.Str);
            }

            return sb.ToString();
        }

        [Benchmark]
        public string DeferredStringCreation()
        {
            var sb = new StringBuilder();

            foreach (var holder in this.GetFormatHolders())
            {
                sb.AppendFormat(holder.Format, holder.Params);
            }

            return sb.ToString();
        }

        [Benchmark]
        public string ThunkedStringCreation()
        {
            var sb = new StringBuilder();

            foreach (var holder in this.GetThunkHolders())
            {
                holder.Thunk(sb);
            }

            return sb.ToString();
        }

        private IEnumerable<StringHolder> GetStringHolders()
        {
            for (var i = 0; i < this.NumItems; i++)
            {
                yield return new StringHolder { Str = string.Format(Format, this.items1[i], this.items2[i]) };
            }
        }

        private IEnumerable<FormatHolder> GetFormatHolders()
        {
            for (var i = 0; i < this.NumItems; i++)
            {
                yield return new FormatHolder { Format = Format, Params = new[] { this.items1[i], this.items2[i] } };
            }
        }

        private IEnumerable<ThunkHolder> GetThunkHolders()
        {
            for (var i = 0; i < this.NumItems; i++)
            {
                var item1 = this.items1[i];
                var item2 = this.items2[i];
                yield return new ThunkHolder
                {
                    Thunk = sb =>
                    {
                        // "Item1: {0} Item2: {1}"
                        sb.Append("Item1: ");
                        sb.Append(item1);
                        sb.Append(" Item2: ");
                        sb.Append(item2);
                    }
                };
            }
        }

        private sealed class StringHolder
        {
            public string Str;
        }

        private sealed class FormatHolder
        {
            public string Format;
            public string[] Params;
        }

        private sealed class ThunkHolder
        {
            public Action<StringBuilder> Thunk;
        }
    }
}
