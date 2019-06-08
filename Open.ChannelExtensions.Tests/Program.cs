﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Open.ChannelExtensions.Tests
{
	class Program
	{
		static async Task Main()
		{
			const int repeat = 50;
			const int concurrency = 4;

			{
				Console.WriteLine("Standard DataFlow operation test...");
				var sw = Stopwatch.StartNew();
				var block = new ActionBlock<int>(async i => await Delay(i));
				foreach (var i in Enumerable.Range(0, repeat))
					block.Post(i);
				block.Complete();
				await block.Completion;
				sw.Stop();
				Console.WriteLine(sw.Elapsed);
				Console.WriteLine();
			}

			{
				Console.WriteLine("Batch + join test 1...");
				var sw = Stopwatch.StartNew();
				var range = Enumerable
					.Range(0, 10000000);

				var result = new List<int>(10000000);

				var total = await range
					.ToChannel()
					.Batch(5000)
					.Join()
					.ReadAll(i=> result.Add(i));

				sw.Stop();
				Debug.Assert(result.SequenceEqual(range));
				Console.WriteLine(sw.Elapsed);
				Console.WriteLine();
			}

			{
				Console.WriteLine("Batch + join test 2...");
				var sw = Stopwatch.StartNew();
				var range = Enumerable
					.Range(0, 10000000);

				var result = new List<int>(10000000);

				var total = await range
					.ToChannel()
					.Batch(50)
					.Join()
					.ReadAll(i => result.Add(i));

				sw.Stop();
				Debug.Assert(result.SequenceEqual(range));
				Console.WriteLine(sw.Elapsed);
				Console.WriteLine();
			}

			{
				Console.WriteLine("Standard Channel filter test...");
				var sw = Stopwatch.StartNew();
				var total = await Enumerable
					.Repeat((Func<int, ValueTask<int>>)Delay, repeat)
					.Select((t, i) => t(i))
					.ToChannelAsync(singleReader: true)
					.Filter(i => i % 2 == 0)
					.ReadAll(Dummy);
				sw.Stop();
				Debug.Assert(total == repeat / 2);
				Console.WriteLine(sw.Elapsed);
				Console.WriteLine();
			}

			{
				Console.WriteLine("Concurrent DataFlow operation test...");
				var sw = Stopwatch.StartNew();
				var block = new ActionBlock<int>(async i => await Delay(i), new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = concurrency });
				foreach (var i in Enumerable.Range(0, repeat))
					block.Post(i);
				block.Complete();
				await block.Completion;
				sw.Stop();
				Console.WriteLine(sw.Elapsed);
				Console.WriteLine();
			}

			{
				Console.WriteLine("Concurrent Channel operation test...");
				var sw = Stopwatch.StartNew();
				await Enumerable
					.Repeat((Func<int, ValueTask<int>>)Delay, repeat)
					.Select((t, i) => t(i))
					.ToChannelAsync(singleReader: false, maxConcurrency: concurrency)
					.ReadAllConcurrently(4, Dummy);
				sw.Stop();
				Console.WriteLine(sw.Elapsed);
				Console.WriteLine();
			}

			{
				Console.WriteLine("Pipe operation test...");
				var sw = Stopwatch.StartNew();
				var total = await Enumerable
					.Repeat((Func<int, ValueTask<int>>)Delay, repeat)
					.Select((t, i) => t(i))
					.ToChannelAsync()
					.Pipe(i => i * 2)
					.ReadAll(Dummy);
				sw.Stop();
				Debug.Assert(total == repeat);
				Console.WriteLine(sw.Elapsed);
				Console.WriteLine();
			}

			{
				Console.WriteLine("Transform operation test...");
				var sw = Stopwatch.StartNew();
				await Enumerable
					.Repeat((Func<int, ValueTask<int>>)Delay, repeat)
					.Select((t, i) => t(i))
					.ToChannelAsync()
					.Transform(i => i * 2L)
					.ReadAll(Dummy);
				sw.Stop();
				Console.WriteLine(sw.Elapsed);
				Console.WriteLine();
			}

		}

		static void Dummy(int i)
		{

		}

		static void Dummy(long i)
		{

		}

		static async ValueTask<int> Delay(int i)
		{
			await Task.Delay(100);
			return i;
		}
	}
}
