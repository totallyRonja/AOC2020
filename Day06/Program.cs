using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Day6 {
	public static class Program {
		static void Main(string[] args) {
			string input = File.ReadAllText("input.txt");
			string[][] groups = input.Split("\n\n").Select(
				group => group.Split("\n").Where(
					answers => !string.IsNullOrWhiteSpace(answers)).ToArray()).ToArray();

			int anyAnswered = groups.Select(
				group => group.Skip(1).Aggregate(group[0] as IEnumerable<char>, 
					(union, answers) => union.Union(answers)).Count()).Sum();
			
			int allAnswered = groups.Select(
				group => group.Skip(1).Aggregate(group[0] as IEnumerable<char>, 
					(intersection, answers) => intersection.Intersect(answers)).Count()).Sum();
			
			Console.WriteLine($"The sum of different answers per group is {anyAnswered}.");
			Console.WriteLine($"The sum of overlapping answers per group is {allAnswered}.");
		}
	}
}