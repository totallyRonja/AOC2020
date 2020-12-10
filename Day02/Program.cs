using System;
using System.IO;
using System.Linq;

namespace Day2 {
	static class Program {
		static void Main(string[] args) {
			string[] input = File.ReadAllLines("input.txt");
			//parse input to numbers
			var parsed = input.Select(line => {
				string[] parts = line.Split(" ");
				string password = parts[2];
				string[] minMax = parts[0].Split("-");
				int min = int.Parse(minMax[0]);
				int max = int.Parse(minMax[1]);
				char character = parts[1][0];
				return (min, max, character, password);
			}).ToArray();

			//do range check
			int validCount = parsed.Count(
					password => password.password!.Count(
					letter => letter == password.character).IsBetweenInclusive(password.min, password.max));
			
			Console.WriteLine($"{validCount} valid passwords!");
			
			//do poke check
			int newValidCount = parsed.Count(
				pw => pw.password.GetOrNull(pw.min - 1) == pw.character ^ pw.password.GetOrNull(pw.max - 1) == pw.character);
			
			Console.WriteLine($"{newValidCount} valid passwords with the new rules!");
		}
		
		static bool IsBetweenInclusive(this int i, int min, int max) {
			return i >= min && i <= max;
		}

		static char? GetOrNull(this string value, int index) {
			if (index < 0 || index >= value.Length)
				return null;
			return value[index];
		}

	}
}