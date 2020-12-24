using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Day24
{
    public static class Program
    {
        enum Dir
        {
            East,
            SouthEast,
            SouthWest,
            West,
            NorthWest,
            NorthEast,
        }
        static void Main(string[] args)
        {
            var directionRegex = new Regex(@"(ne|nw|se|sw|e|w)");
            var dirDict = new Dictionary<string, Dir>
            {
                {"e", Dir.East},
                {"se", Dir.SouthEast},
                {"sw", Dir.SouthWest},
                {"w", Dir.West},
                {"nw", Dir.NorthWest},
                {"ne", Dir.NorthEast},
            };
            var lines = File.ReadLines("input.txt")
                .Select(line => directionRegex.Matches(line)
                    .Select(dirMatch => dirDict[dirMatch.Value]).ToArray()).ToArray();
            
            var blackTiles = new HashSet<(int, int, int)>();
            foreach (var line in lines)
            {
                var target = WalkDirections(line);
                blackTiles.SymmetricExceptWith(Enumerable.Repeat(target,1));
            }
            Console.WriteLine($"At the end of all instructions there are {blackTiles.Count} black tiles");

            var tileList = blackTiles.ToList();
            CellularAutomaton(tileList);
            
            Console.WriteLine($"After running the Automaton for 100 days, there are {tileList.Count} black tiles");
        }

        private static void CellularAutomaton(List<(int x, int y, int z)> blackTiles)
        {
            var neighbors = new Dictionary<(int, int, int), (int amount, bool isBlack)>();
            for (int i = 0; i < 100; i++) {
                neighbors.Clear();
                foreach (var tile in blackTiles)
                {
                    neighbors.TryGetValue(tile, out var ownTile);
                    ownTile.isBlack = true;
                    neighbors[tile] = ownTile;
                    foreach (var dir in (Dir[]) Enum.GetValues(typeof(Dir))) {
                        var neighborPos = Walk(tile, dir);
                        neighbors.TryGetValue(neighborPos, out var neighborTile);
                        neighborTile.amount++;
                        neighbors[neighborPos] = neighborTile;
                    }
                }
                blackTiles.Clear();
                foreach (var (pos, data) in neighbors)
                {
                    if ((!data.isBlack && data.amount == 2) || //with with 2 neighbors flips
                        (data.isBlack && (data.amount > 0 && data.amount <= 2))) //black with 1-2 neighbors stays
                        blackTiles.Add(pos);
                }
                //Console.WriteLine(blackTiles.Count);
            }
        }

        private static (int x, int y, int z) WalkDirections(Dir[] directions) => 
            directions.Aggregate((x: 0, y: 0, z: 0), (current, direction) => Walk(current, direction));

        private static (int x, int y, int z) Walk((int x, int y, int z) from, Dir direction) =>
            direction switch
            {
                Dir.East      => (from.x + 1, from.y - 1, from.z),
                Dir.SouthEast => (from.x, from.y - 1, from.z + 1),
                Dir.SouthWest => (from.x - 1, from.y, from.z + 1),
                Dir.West      => (from.x - 1, from.y + 1, from.z),
                Dir.NorthWest => (from.x, from.y + 1, from.z - 1),
                Dir.NorthEast => (from.x + 1, from.y, from.z - 1),
                _ => throw new Exception(),
            };
    }
}