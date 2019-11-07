using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Open.ChannelExtensions.Tests
{
	public static partial class BasicTests
	{
		const int testSize1 = 10000001;
		const int testSize2 = 30000001;

		[Theory]
		[InlineData(testSize1)]
		[InlineData(testSize2)]
		public static async Task ReadAll(int testSize)
		{
			var range = Enumerable.Range(0, testSize);
			var result = new List<int>(testSize);

			var sw = Stopwatch.StartNew();
			var total = await range
				.ToChannel()
				.ReadAll(i => result.Add(i));
			sw.Stop();

			Console.WriteLine("ReadAll(): {0}", sw.Elapsed);
			Console.WriteLine();

			Assert.Equal(testSize, result.Count);
			Assert.True(result.SequenceEqual(range));
			result.Clear();
		}

		[Theory]
		[InlineData(testSize1)]
		[InlineData(testSize2)]
		public static async Task ReadAllAsync(int testSize)
		{
			var range = Enumerable.Range(0, testSize);
			var result = new List<int>(testSize);

			var sw = Stopwatch.StartNew();
			var total = await range
				.ToChannel()
				.ReadAllAsync(i =>
				{
					result.Add(i);
					return new ValueTask();
				});
			sw.Stop();

			Console.WriteLine("Channel.ReadAllAsync(): {0}", sw.Elapsed);
			Console.WriteLine();

			Assert.Equal(testSize, result.Count);
			Assert.True(result.SequenceEqual(range));
			result.Clear();
		}

		[Theory]
		[InlineData(testSize1, 51)]
		[InlineData(testSize1, 5001)]
		[InlineData(testSize2, 51)]
		[InlineData(testSize2, 5001)]
		public static async Task BatchThenJoin(int testSize, int batchSize)
		{
			var range = Enumerable.Range(0, testSize);
			var result1 = new List<List<int>>(testSize / batchSize + 1);

			{
				var sw = Stopwatch.StartNew();
				var total = await range
					.ToChannel()
					.Batch(batchSize)
					.ReadAll(i => result1.Add(i));
				sw.Stop();

				Console.WriteLine("Channel.Batch({1}): {0}", sw.Elapsed, batchSize);
				Console.WriteLine();

				var r = result1.SelectMany(e => e).ToList();
				Assert.Equal(testSize, r.Count);
				Assert.True(r.SequenceEqual(range));
			}

			{
				var result2 = new List<int>(testSize);
				var sw = Stopwatch.StartNew();
				var total = await result1
					.ToChannel()
					.Join()
					.ReadAll(i => result2.Add(i));
				sw.Stop();

				Console.WriteLine("Channel.Join(): {0}", sw.Elapsed);
				Console.WriteLine();

				Assert.Equal(testSize, result2.Count);
				Assert.True(result2.SequenceEqual(range));
				result2.Clear();
				result2.TrimExcess();
			}

			result1.Clear();
			result1.TrimExcess();
		}

		[Theory]
		[InlineData(testSize1, 51)]
		[InlineData(testSize1, 5001)]
		[InlineData(testSize2, 51)]
		[InlineData(testSize2, 5001)]
		public static async Task BatchJoin(int testSize, int batchSize)
		{
			var range = Enumerable.Range(0, testSize);
			var result = new List<int>(testSize);

			var sw = Stopwatch.StartNew();
			var total = await range
				.ToChannel()
				.Batch(batchSize)
				.Join()
				.ReadAll(i => result.Add(i));
			sw.Stop();

			Console.WriteLine("Channel.Batch({1}).Join(): {0}", sw.Elapsed, batchSize);
			Console.WriteLine();

			Assert.Equal(testSize, result.Count);
			Assert.True(result.SequenceEqual(range));
		}
	}
}