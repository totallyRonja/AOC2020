using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Day9 {
	public static class Program {
		static void Main(string[] args) {
			long[] numbers = File.ReadLines("input.txt").Select(long.Parse).ToArray();
			const int preamble = 25;
			
			long[] runningNumbers = numbers.Take(preamble).ToArray();
			int runningNumberIndex = 0;
			int index;
			for (index = 25; index < numbers.Length; index++) {
				long number = numbers[index];
				for (int i1 = runningNumbers.Length - 1; i1 >= 0; i1--) {
					for (int i2 = 0; i2 < i1; i2++) {
						if (runningNumbers[i1] + runningNumbers[i2] == number)
							goto found;
					}
				}

				break;
				found: ;
				runningNumbers[runningNumberIndex++ % runningNumbers.Length] = number;
			}

			long wrongNumber = numbers[index];
			Console.WriteLine($"The first invalid number is {wrongNumber}.");

			long rangeSum = 0;
			int rangeStart = 0; //start inclusive
			int rangeEnd = 0; //end exclusive
			while (true) {
				if (rangeSum < wrongNumber) {
					rangeSum += numbers[rangeEnd];
					rangeEnd++;
				} else if (rangeSum > wrongNumber) {
					rangeSum -= numbers[rangeStart];
					rangeStart++;
				} else {
					break;
				}
			}

			long min = numbers.Skip(rangeStart).Take(rangeEnd - rangeStart).Min();
			long max = numbers.Skip(rangeStart).Take(rangeEnd - rangeStart).Max();

			Console.WriteLine($"The max and min of the range that sums to {wrongNumber} are {max} and {min} respectively adding up to {min + max}");
		}
	}
}