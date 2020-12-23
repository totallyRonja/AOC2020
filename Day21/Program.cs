using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Day21
{
    public static class Program
    {
        static void Main(string[] args)
        {
            var parseRegex = new Regex(@"^([\w ]+) \(contains ([\w ,]+)\)$");
            var lines = File.ReadAllLines("input.txt").Select(line =>
            {
                var match = parseRegex.Match(line);
                var ingredients = match.Groups[1].Value
                    .Split(" ", StringSplitOptions.RemoveEmptyEntries);
                var allergens = match.Groups[2].Value.Split(",")
                    .Select(a => a.Trim()).ToArray();
                return (ingredients, allergens);
            }).ToArray();
            var allergenNames = new Dictionary<string, HashSet<string>>();
            foreach (var line in lines)
            {
                foreach (var allergen in line.allergens)
                {
                    if (allergenNames.ContainsKey(allergen)) {
                        var possibleNames = allergenNames[allergen];
                        possibleNames.IntersectWith(line.ingredients);
                    } else {
                        allergenNames.Add(allergen, new HashSet<string>(line.ingredients));
                    }
                }
            }

            //count "definitely not allergen" entries
            var allergenFreeEntries = lines.Sum(line => line.ingredients
                .Count(i => !allergenNames
                    .Any(al => al.Value.Contains(i))));
            
            Console.WriteLine(allergenFreeEntries);

            //resolve allergen names
            var resolvedNames = allergenNames.ToArray();
            var toRemove = resolvedNames.Where(name => name.Value.Count == 1).ToArray();
            while (toRemove.Length < resolvedNames.Length)
            {
                foreach (var names in resolvedNames)
                {
                    if(names.Value.Count > 1)
                        names.Value.ExceptWith(toRemove.SelectMany(rem => rem.Value));
                }
                toRemove = resolvedNames.Where(name => name.Value.Count == 1).ToArray();
            }

            //order by allergen name and print
            resolvedNames = resolvedNames.OrderBy(name => name.Key).ToArray();
            Console.WriteLine(string.Join(",", resolvedNames.Select(name => name.Value.First())));
        }
    }
}