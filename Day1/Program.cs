using System;
using System.IO;
using System.Linq;

namespace Day1 {
	static class Program {
		static void Main(string[] args) {
			string[] input = File.ReadAllLines("input.txt");
			int[] numbers = input.Select(int.Parse).ToArray();

			for (int index1 = 0; index1 < numbers.Length; index1++) {
				int first = numbers[index1];
				for (int index2 = 0; index2 < index1; index2++) {
					int second = numbers[index2];
					if (first + second == 2020) {
						Console.WriteLine($"{first} * {second} = {first * second}");
					}
				}
			}

			for (int index1 = 0; index1 < numbers.Length; index1++) {
				int first = numbers[index1];
				for (int index2 = 0; index2 < index1; index2++) {
					int second = numbers[index2];
					for (int index3 = 0; index3 < index2; index3++) {
						int third = numbers[index3];
						if (first + second + third == 2020) {
							Console.WriteLine($"{first} * {second} * {third} = {first * second * third}");
						}
					}
				}
			}

		}

	}
}