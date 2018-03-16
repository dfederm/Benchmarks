namespace Benchmarks
{
    using System;
    using System.Linq;
    using System.Reflection;
    using BenchmarkDotNet.Running;

    public class Program
    {
        public static int Main(string[] args)
        {
            var benchmarks = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.Name.EndsWith("Benchmarks"))
                .ToArray();

            string choice;
            if (args.Length == 0)
            {
                Console.WriteLine("Available Benchmarks:");
                for (var i = 0; i < benchmarks.Length; i++)
                {
                    Console.WriteLine($"\t{i}: {benchmarks[i].Name}");
                }

                Console.Write("Choose a benchmark to run (index or name): ");
                choice = Console.ReadLine();
            }
            else
            {
                choice = args[0];
            }

            Type type;
            if (int.TryParse(choice, out var index))
            {
                if (index < 0 || index >= benchmarks.Length)
                {
                    Console.WriteLine($"Invalid index: {index}. Must be between 0 and {benchmarks.Length - 1}");
                    return 1;
                }

                type = benchmarks[index];
            }
            else
            {
                type = benchmarks.FirstOrDefault(benchmark => benchmark.Name.Equals(choice, StringComparison.OrdinalIgnoreCase));
                if (type == null)
                {
                    Console.WriteLine($"Could not find benchmark: {choice}");
                    return 1;
                }
            }

            BenchmarkRunner.Run(type);
            return 0;
        }
    }
}