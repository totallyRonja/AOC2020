using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Day4 {
	static class Program {
		static void Main(string[] args) {
			IEnumerable<string> lines = File.ReadLines("input.txt");
			
			//swoop passports together into single lines
			var passportTexts = new List<string>();
			var currentPassportData = new StringBuilder();
			foreach (string line in lines) {
				if (string.IsNullOrWhiteSpace(line)) {
					passportTexts.Add(currentPassportData.ToString());
					currentPassportData = currentPassportData.Clear();
					continue;
				}

				if (currentPassportData.Length != 0)
					currentPassportData.Append(" ");
				currentPassportData.Append(line);
			}
			if (currentPassportData.Length != 0) //append last passport if needed
				passportTexts.Add(currentPassportData.ToString());

			//then parse them into fields with keys and values
			Dictionary<string, string>[] passports = passportTexts.Select(
					text => text.Split(" ").ToDictionary(
							part => part.Substring(0, part.IndexOf(':')), 
							part => part.Substring(part.IndexOf(':')+1))).ToArray();

			//here we define the fields and their requirements
			var fields = new Dictionary<string, Func<string, bool>>() {
				{"byr", yearString => int.TryParse(yearString, out int year) && year >= 1920 && year <= 2002 }, 
				{"iyr", yearString => int.TryParse(yearString, out int year) && year >= 2010 && year <= 2020 },
				{"eyr", yearString => int.TryParse(yearString, out int year) && year >= 2020 && year <= 2030 },
				{"hgt", height => {
						//I could make this a one-liner... but no
						if (!int.TryParse(height.Substring(0, height.Length - 2), out int heightVal))
							return false;
						return height.Substring(height.Length - 2) switch {
							"cm" => heightVal >= 150 && heightVal <= 193,
							"in" => heightVal >= 59 && heightVal <= 76,
							_ => false
						};
				}},
				{"hcl", color => color[0] == '#' && color.Length == 7 && color.Skip(1).All(character => "0123456789abcdef".Contains(character))},
				{"ecl", color => new[]{"amb", "blu", "brn", "gry", "grn", "hzl", "oth"}.Contains(color)},
				{"pid", id => int.TryParse(id, out _) && id.Length == 9},
				/*{"cid", validation}*/};
			
			//and do the simple check if the fields exist
			int validPassports = passports.Count(passKeys => fields.Keys.All(
					key => passKeys.Keys.Contains(key)));
			
			Console.WriteLine($"There are {validPassports} valid passports");
			
			//and then if the fields fill the requirements
			int validPassportsStrict = passports.Count(
				passport => fields.All(
					field => passport.TryGetValue(field.Key, out string value) && field.Value(value)));
			
			Console.WriteLine($"There are {validPassportsStrict} valid passports with the stricter measurements");
		}
	}
}