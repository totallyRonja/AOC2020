using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Day19 {
	public static class Program {
		static void Main(string[] args) {
			string[] lines = File.ReadAllLines("input.txt");
			int split = lines.Select((line, i) => (line, i))
				.First(line => string.IsNullOrWhiteSpace(line.line)).i;

			var rules = ParseRules(lines.Take(split));
			int messages = lines.Skip(split + 1)
				.Count(message => {
					int[] match = rules[0].Match(message, 0);
					return match != null && match.Any(m => m == message.Length);
				});
			Console.WriteLine($"There are {messages} messages that match rule 0");

			string[] newRules = new []{"8: 42 | 42 8", "11: 42 31 | 42 11 31"};
			rules = ParseRules(lines.Take(split).Union(newRules));
			messages = lines.Skip(split + 1)
				.Count(message => {
					int[] match = rules[0].Match(message, 0);
					return match != null && match.Any(m => m == message.Length);
				});
			Console.WriteLine($"There are {messages} messages that match rule 0 (with updated rules)");
		}

		static Rule[] ParseRules(IEnumerable<string> ruleText) {
			int maxRule = ruleText.Max(line => int.Parse(line.Substring(0, line.IndexOf(':'))));
			Rule[] rules = new Rule[maxRule+1];

			var defRegex = new Regex(@"^([0-9]+): (.*)$");
			var charRegex =  new Regex(@"""(\w)""");
			var followingRegex = new Regex(@"( [0-9]+)+");
			foreach (string line in ruleText) {
				var outline = defRegex.Match(line);
				int index = int.Parse(outline.Groups[1].Value);
				string body = outline.Groups[2].Value;
				var charMatch = charRegex.Match(body);
				if (charMatch.Success) {
					rules[index] = new LetterRule{Letter = charMatch.Groups[1].Value[0]};
					continue;
				}

				var followMatch = followingRegex.Matches(line);
				if (!followMatch.Any())
					throw new Exception();
				var ruleIndices = followMatch
					.Select(match => match.Value.Split(" ", StringSplitOptions.RemoveEmptyEntries)
						.Select(int.Parse).ToArray());
				var followRules = ruleIndices
					.Select<int[], Rule>(indices => new FollowingRules {RuleIndices = indices, Rules = rules}).ToArray();
				if (followRules.Length == 1)
					rules[index] = followRules[0];
				else
					rules[index] = new MultiRule {Rules = followRules};
			}

			return rules;
		}

		interface Rule {
			int[] Match(string input, int position);
		}

		class LetterRule : Rule {
			public char Letter;
			public int[] Match(string input, int position) => position < input.Length && input[position] == Letter ? new[]{1} : null;
		}

		class MultiRule : Rule {
			public Rule[] Rules;
			
			//try to match all rules and return the first one that returns a valid result
			public int[] Match(string input, int position) => Rules 
				.Select(rule => rule.Match(input, position)) //match all
				.Where(match => match != null && match.Any()) //get valid matches
				.SelectMany(rule => rule).ToArray(); //flatten
		}

		class FollowingRules : Rule {
			public int[] RuleIndices;
			public Rule[] Rules;
			public int[] Match(string input, int position) {
				return Match(input, position, 0);
			}
			int[] Match(string input, int position, int index) {
				int rule = RuleIndices[index];
				int[] matches = Rules[rule].Match(input, position);
				if(matches == null || !matches.Any())
					return null;

				//last one just returns matches
				if (index == RuleIndices.Length - 1)
					return matches;
				
				//earlier ones return matches plus lower matches
				var positions = new List<int>();
				foreach (int match in matches.Where(match => position + match <= input.Length)) {
					var nextMatch = Match(input, position + match, index + 1)?.Select(res => res + match);
					if(nextMatch != null)
						positions.AddRange(nextMatch);
				}
				return positions.ToArray();
			}
		}
	}
}