using System;
using System.Linq;

namespace Day15 {
	public static class Program {
		static void Main(string[] args) {
			var input = new []{0,1,4,13,15,12,16};

			int play2020 = SpeakingGame(input, 2020);
			Console.WriteLine($"The 2020th number spoken is {play2020}");

			int play30m = SpeakingGame(input, 30000000);
			Console.WriteLine($"The 20 millionth number spoken is {play30m}");
		}

		static int SpeakingGame(int[] input, int duration) {
			var numbers = input.SkipLast(1).Select((value, index) => (value, index))
				.ToDictionary(value => value.value, value => value.index);
			int count = input.Length;
			int lastNumber = input.Last();
			while (count < duration) {
				int newNumber = numbers.TryGetValue(lastNumber, out int value) ? (count - 1 - value) : 0;
				numbers[lastNumber] = count - 1;
				lastNumber = newNumber;
				count++;
			}

			return lastNumber;
		}
	}
}