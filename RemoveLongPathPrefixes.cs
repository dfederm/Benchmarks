using BenchmarkDotNet.Attributes;
using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace Benchmarks;

public partial class RemoveLongPathPrefixes
{
    [Params(false, true)]
    public bool IsLongPath;

    private ReadOnlyMemory<char> InputPath;

    [GlobalSetup]
    public void Setup()
    {
        InputPath = IsLongPath
            ? @"\\?\C:\Foo\Bar\Baz\Foo\Bar\Baz\Foo\Bar\Baz\Foo\Bar\Baz\Foo\Bar\Baz\Foo\Bar\Baz\Foo\Bar\Baz\Foo\Bar\Baz\Foo\Bar\Baz\Foo\Bar\Baz\Foo\Bar\Baz\Foo\Bar\Baz\Foo\Bar\Baz\Foo\Bar\Baz".AsMemory()
            : @"C:\Foo\Bar\Baz\Foo\Bar\Baz\Foo\Bar\Baz".AsMemory();
    }

    [Benchmark]
    public ReadOnlySpan<char> StringsAsSpans()
    {
        ReadOnlySpan<char> path = InputPath.Span;

        ReadOnlySpan<char> pattern1 = @"\\?\".AsSpan();
        ReadOnlySpan<char> pattern2 = @"\??\".AsSpan();
        if (path.StartsWith(pattern1, StringComparison.OrdinalIgnoreCase))
        {
            return path.Slice(pattern1.Length);
        }
        if (path.StartsWith(pattern2, StringComparison.OrdinalIgnoreCase))
        {
            return path.Slice(pattern2.Length);
        }

        return path;
    }

    [Benchmark]
    public ReadOnlySpan<char> StringsAsSpans2()
    {
        ReadOnlySpan<char> path = InputPath.Span;

        if (path.StartsWith(@"\\?\".AsSpan(), StringComparison.OrdinalIgnoreCase)
           || path.StartsWith(@"\??\".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            return path.Slice(4);
        }

        return path;
    }

    [Benchmark]
    public ReadOnlySpan<char> LiteralSpans()
    {
        ReadOnlySpan<char> path = InputPath.Span;

        if (path.StartsWith(['\\', '\\', '?', '\\'], StringComparison.Ordinal)
           || path.StartsWith(['\\', '?', '?', '\\'], StringComparison.Ordinal))
        {
            return path.Slice(4);
        }

        return path;
    }

    [Benchmark]
    public ReadOnlySpan<char> ShortCircuit()
    {
        ReadOnlySpan<char> path = InputPath.Span;

        if (path.Length == 0 || path[0] != '\\')
        {
            return path;
        }

        if (path.StartsWith(['\\', '\\', '?', '\\'], StringComparison.Ordinal)
           || path.StartsWith(['\\', '?', '?', '\\'], StringComparison.Ordinal))
        {
            return path.Slice(4);
        }

        return path;
    }

    [Benchmark]
    public ReadOnlySpan<char> ShortCircuitOptimized()
    {
        ReadOnlySpan<char> path = InputPath.Span;

        if (path.Length < 4 || path[0] != '\\')
        {
            return path;
        }

        // We already checked index 0
        ReadOnlySpan<char> s = path.Slice(1, 3);
        if (s.SequenceEqual(['\\', '?', '\\'])
           || s.SequenceEqual(['?', '?', '\\']))
        {
            return path.Slice(4);
        }

        return path;
    }
}
