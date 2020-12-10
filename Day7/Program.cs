using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Day7 {
	public static class Program {
		static void Main(string[] args) {
			string[] input = File.ReadAllLines("input.txt");
			Dictionary<string, (int amount, string color)[]> rules = input.ToDictionary(
				line => Regex.Match(line, @"(\w+ \w+) bags?").Groups[1].Value,
				line => Regex.Matches(line, @"([0-9]+) (\w+ \w+) bags?")
					.Select(match => (int.Parse(match.Groups[1].Value), match.Groups[2].Value)).ToArray());

			var containShinyGold = new HashSet<string>();
			int startBagCount;
			do {
				startBagCount = containShinyGold.Count;
				foreach ((string container, (int amount, string color)[] contains) in rules) {
					if (contains.Any(bag => bag.color == "shiny gold") ||
					    contains.Select(bag => bag.color).Intersect(containShinyGold).Any()) {
						containShinyGold.Add(container);
					}
				}
			} while (startBagCount != containShinyGold.Count);
			
			Console.WriteLine($"There are {containShinyGold.Count} types of bags that can contain shiny gold bags.");

			var bags = new Stack<(int amount, string color)>();
			bags.Push((1, "shiny gold"));
			int totalBags = 0;
			while (bags.Any()) {
				var currentBags = bags.Pop();
				foreach (var contained in rules[currentBags.color]) {
					totalBags += currentBags.amount * contained.amount;
					bags.Push((currentBags.amount * contained.amount, contained.color));
				}
			}
			Console.WriteLine($"There are {totalBags} bags inside the shiny gold bag.");
		}
	}
}