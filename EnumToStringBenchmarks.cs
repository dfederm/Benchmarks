using System;
using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    [MemoryDiagnoser]
    public class EnumToStringBenchmarks
    {
        private static TestEnum[] enumData = new[]
        {
            TestEnum.None,
            TestEnum.ValueA,
            TestEnum.ValueB,
            TestEnum.ValueH,
            TestEnum.ValueI,
            TestEnum.ValueA,
            TestEnum.ValueB,
        };

        private static TestFlags[] flagsData = new[]
        {
            TestFlags.ValueA,
            TestFlags.ValueA | TestFlags.ValueB,
            TestFlags.ValueB | TestFlags.ValueH,
            TestFlags.ValueD | TestFlags.ValueH | TestFlags.ValueI,
            TestFlags.ValueA,
            TestFlags.ValueB | TestFlags.ValueC,
        };

        [Benchmark]
        public int EnumToString()
        {
            int count = 0;
            for (int i = 0; i < enumData.Length; i++)
            {
                count += enumData[i].ToString().Length;
            }

            return count;
        }

        [Benchmark]
        public int EnumCachedToStringConvert()
        {
            int count = 0;
            for (int i = 0; i < enumData.Length; i++)
            {
                count += Enum<TestEnum>.ToString(enumData[i]).Length;
            }

            return count;
        }

        [Benchmark]
        public int EnumCachedToStringIntCast()
        {
            int count = 0;
            for (int i = 0; i < enumData.Length; i++)
            {
                count += Enum<TestEnum>.ToString((int)enumData[i]).Length;
            }

            return count;
        }

        // This measures the one-time cost per enum type of using this optimization.
        [Benchmark]
        public string[] EnumCreateStringCache() => Enum<TestEnum>.CreateStringCache();

        [Benchmark]
        public int FlagsToString()
        {
            int count = 0;
            for (int i = 0; i < flagsData.Length; i++)
            {
                count += flagsData[i].ToString().Length;
            }

            return count;
        }

        [Benchmark]
        public int FlagsCachedToStringConvert()
        {
            int count = 0;
            for (int i = 0; i < flagsData.Length; i++)
            {
                count += Enum<TestFlags>.ToString(flagsData[i]).Length;
            }

            return count;
        }

        [Benchmark]
        public int FlagsCachedToStringIntCast()
        {
            int count = 0;
            for (int i = 0; i < flagsData.Length; i++)
            {
                count += Enum<TestFlags>.ToString((int)flagsData[i]).Length;
            }

            return count;
        }

        // This measures the one-time cost per enum type of using this optimization.
        [Benchmark]
        public string[] FlagsCreateStringCache() => Enum<TestFlags>.CreateStringCache();
    }

    public enum TestEnum
    {
        None,
        ValueA,
        ValueB,
        ValueC,
        ValueD,
        ValueE,
        ValueF,
        ValueG,
        ValueH,
        ValueI,
    }

    [Flags]
    public enum TestFlags
    {
        None = 0,
        ValueA = 1 << 0,
        ValueB = 1 << 1,
        ValueC = 1 << 2,
        ValueD = 1 << 3,
        ValueE = 1 << 4,
        ValueF = 1 << 5,
        ValueG = 1 << 6,
        ValueH = 1 << 7,
        ValueI = 1 << 8,
    }

    public static class Enum<T> where T : Enum
    {
        private static readonly string[] strings = CreateStringCache();

        public static string ToString(int intValue) => strings[intValue];

        public static string ToString(T value) => strings[Convert.ToInt32(value)];

        internal static string[] CreateStringCache()
        {
            Type type = typeof(T);
            string[] names = Enum.GetNames(type);
            if (type.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0)
            {
                var values = (uint[])Enum.GetValues(type);
                uint maxValue = values[0];
                foreach (uint value in values)
                {
                    if (value > maxValue)
                    {
                        maxValue = value;
                    }
                }

                var strings = new string[2 * maxValue];

                // Special-case the 0 value (usually "None" or something).
                strings[0] = names[0];

                // This is a fancy algorithm to compute a flags ToString value.
                // For a given value composed of one or more flags, the ToString value
                // is the name of the value of the highest order bit prepended by the full name
                // of the bits below it. Eg. for the value 7 (0x111), the string representation is
                // the string representation of 4 (0x100) prepended by the string representation of
                // 3 (0x011). This does some bit twiddling to figure the bits out and reuses the
                // string representations of earlier values. Note that zero also needs to be special-cased.
                int highestBitPos = 0;
                for (uint i = 1; i < strings.Length; i++)
                {
                    // Get the highest bit position
                    // Since we're counting up in order, we can start from the result of the last value.
                    uint val = i >> highestBitPos;
                    val >>= 1;
                    while (val > 0)
                    {
                        val >>= 1;
                        highestBitPos++;
                    }

                    // Get the lower bits by masking out the higher bit.
                    uint lowerBits = i & ~(uint)(1 << highestBitPos);

                    // If the lower bits are zero, the value is a power of 2 and so we should just use the
                    // name at that bit position plus one to account for the zero flag name.
                    // Else, prepend the string representation of the lower bits combined (already computed earlier).
                    strings[i] = lowerBits == 0
                        ? names[highestBitPos + 1]
                        : strings[lowerBits] + ", " + names[highestBitPos + 1];
                }

                return strings;
            }
            else
            {
                return names;
            }
        }
    }
}
