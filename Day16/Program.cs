using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Day16 {
	public static class Program {
		static void Main(string[] args) {
			var watch = new Stopwatch();
			watch.Start();
			
			var lines = File.ReadAllLines("input.txt");

			var rules = new List<(string name, (int min, int max)[] ranges)>();
			int[] myTicket = null;
			var otherTickets = new List<(int[] fields, int[] invalid)>();

			var rangeRegex = new Regex(@"([0-9]+)-([0-9]+)");
			var ruleNameRegex = new Regex(@"(.*):");
			int part = 0;
			for (int i = 0; i < lines.Length; i++) {
				string line = lines[i];
				if (string.IsNullOrWhiteSpace(line)) {
					part++; //next part
					i++; //skip description line
					continue;
				}
				switch (part) {
					case 0://rules
						rules.Add((ruleNameRegex.Match(line!).Groups[1].Value, rangeRegex.Matches(line!)
							.Select(match => (int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value))).ToArray()));
						break;
					case 1: //my ticket
						myTicket = line!.Split(",").Select(int.Parse).ToArray();
						break;
					case 2: //nearby tickets
						var fields = line!.Split(",").Select(int.Parse).ToArray();
						var invalidFields = fields.Select((field, index) => (field, index))
							.Where(field => rules.All(rule => rule!.ranges.All(
								range => field.field < range.min || field.field > range.max)))
							.Select(field => field.index).ToArray();
						otherTickets.Add((fields, invalidFields));
						break;
				}
			}

			int illegalSum = otherTickets.Sum(ticket => ticket!.invalid.Sum(invalid => ticket.fields[invalid]));
			Console.WriteLine($"The sum of invalid values is {illegalSum}");

			otherTickets = otherTickets.Where(ticket => !ticket!.invalid.Any()).ToList(); //throw out invalid tickets

			var categoryNames = rules.Select(rule => rule.name).ToArray();
			var categoryOrder = Enumerable.Repeat(categoryNames, categoryNames.Length)
				.Select(names => new HashSet<string>(names)).ToArray();

			foreach (var ticket in otherTickets) {
				for (int i = 0; i < ticket.fields.Length; i++) {
					int field = ticket.fields[i];
					var possibleFields = rules.Where(rule => rule!.ranges
							.Any(range => field >= range.min && field <= range.max))
						.Select(rule => rule.name);
					categoryOrder[i].IntersectWith(possibleFields);
				}
			}

			var cleaned = new HashSet<string>();
			string[] toClean;
			do {
				toClean = categoryOrder.Where(set => set.Count == 1 && !cleaned.Contains(set.First()))
					.Select(set => set.First()).ToArray();
				foreach (var set in categoryOrder) {
					if(set.Count <= 1)
						continue;
					set.ExceptWith(toClean);
				}
				cleaned.UnionWith(toClean);
			} while (toClean.Any());

			categoryNames = categoryOrder.Select(cat => cat.FirstOrDefault()).ToArray();

			long departureProduct = categoryNames.Select((cat, index) => (cat, index))
				.Where(cat => cat!.cat.StartsWith("departure")).Select(cat => myTicket![cat.index])
				.Aggregate(1L, (product, myValue) => product * myValue);
			
			Console.WriteLine(watch.ElapsedMilliseconds);
			
			Console.WriteLine($"The product of all fields starting with \"Departure\" is {departureProduct}");
		}
	}
}