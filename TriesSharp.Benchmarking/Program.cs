using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using TriesSharp.Tests.UnitTestProject1.Benchmarks;

var config = DefaultConfig.Instance
    .WithSummaryStyle(SummaryStyle.Default.WithMaxParameterColumnWidth(100))
    .AddDiagnoser(new MemoryDiagnoser(new MemoryDiagnoserConfig()));
BenchmarkSwitcher.FromAssembly(typeof(TrieBenchmarks).Assembly).Run(args, config);
