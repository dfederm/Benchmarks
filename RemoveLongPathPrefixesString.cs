using BenchmarkDotNet.Attributes;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Benchmarks;

public partial class RemoveLongPathPrefixesString
{
    [Params(false, true)]
    public bool IsLongPath;

    private string InputPath;

    [GlobalSetup]
    public void Setup()
    {
        InputPath = IsLongPath
            ? @"\\?\C:\Foo\Bar\Baz\Foo\Bar\Baz\Foo\Bar\Baz\Foo\Bar\Baz\Foo\Bar\Baz\Foo\Bar\Baz\Foo\Bar\Baz\Foo\Bar\Baz\Foo\Bar\Baz\Foo\Bar\Baz\Foo\Bar\Baz\Foo\Bar\Baz\Foo\Bar\Baz\Foo\Bar\Baz"
            : @"C:\Foo\Bar\Baz\Foo\Bar\Baz\Foo\Bar\Baz";
    }

    [Benchmark]
    public string StartsWithSubstring()
    {
        string path = InputPath;

        string pattern1 = @"\\?\";
        string pattern2 = @"\??\";
        if (path.StartsWith(pattern1, StringComparison.OrdinalIgnoreCase))
        {
            return path.Substring(pattern1.Length);
        }
        if (path.StartsWith(pattern2, StringComparison.OrdinalIgnoreCase))
        {
            return path.Substring(pattern2.Length);
        }

        return path;
    }

    [Benchmark]
    public string StartsWithSubstring2()
    {
        string path = InputPath;

        if (path.StartsWith(@"\\?\", StringComparison.OrdinalIgnoreCase)
           || path.StartsWith(@"\??\", StringComparison.OrdinalIgnoreCase))
        {
            return path.Substring(4);
        }

        return path;
    }

    [Benchmark]
    public string ShortCircuit()
    {
        string path = InputPath;

        if (path.Length == 0 || path[0] != '\\')
        {
            return path;
        }

        if (path.StartsWith(@"\\?\", StringComparison.OrdinalIgnoreCase)
           || path.StartsWith(@"\??\", StringComparison.OrdinalIgnoreCase))
        {
            return path.Substring(4);
        }

        return path;
    }

    [Benchmark]
    public string ShortCircuitOptimized()
    {
        string path = InputPath;
        if (path.Length < 4 || path[0] != '\\')
        {
            return path;
        }

        // We already checked index 0
        ReadOnlySpan<char> span = path.AsSpan(1, 3);
        if (span.SequenceEqual(['\\', '?', '\\'])
           || span.SequenceEqual(['?', '?', '\\']))
        {
            return path.Substring(4);
        }

        return path;
    }
}
