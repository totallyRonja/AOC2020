using System;
using System.Diagnostics;
using System.Linq;

namespace Day15 {
	public static class Program {
		static void Main(string[] args) {
			var input = new []{0,1,4,13,15,12,16};

			int play2020 = SpeakingGame(input, 2020);
			Console.WriteLine($"The 2020th number spoken is {play2020}");

			var watch = new Stopwatch();
			watch.Start();
			int play30m = SpeakingGame(input, 30000000);
			Console.WriteLine(watch.ElapsedMilliseconds);
			Console.WriteLine($"The 20 millionth number spoken is {play30m}");
		}

		static int SpeakingGame(int[] input, int duration) {
			var numbers = new int[duration];
			for (int index = 0; index < input.Length-1; index++) {
				int i = input[index];
				numbers[i] = index;
			}
			int count = input.Length;
			int lastNumber = input.Last();
			while (count < duration) {
				int newNumber = numbers[lastNumber];
				if (newNumber != 0)
					newNumber = count - 1 - newNumber;
				numbers[lastNumber] = count - 1;
				lastNumber = newNumber;
				count++;
			}

			return lastNumber;
		}
	}
}