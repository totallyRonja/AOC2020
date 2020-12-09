using System;
using System.IO;

namespace Day3 {
	static class Program {
		static void Main(string[] args) {

			int trees = WalkTrees(3, 1);
			Console.WriteLine($"Bonked {trees} trees!");

			var directions = new(int x, int y)[]{(1, 1), (3, 1), (5, 1), (7, 1), (1, 2)};
			long collectiveTrees = 1;
			foreach ((int x, int y) in directions) {
				collectiveTrees = collectiveTrees * WalkTrees(x, y);
			}
			Console.WriteLine($"Bonked {collectiveTrees} trees overall!");
		}

		static int WalkTrees(int x, int y) {
			string[] input = File.ReadAllLines("input.txt");
			int position = 0;
			int trees = 0;
			for (int i = 0; i < input.Length; i+=y) {
				char terrain = input[i].GetWrapped(position);
				if (terrain == '#')
					trees++;
				position += x;
			}
			return trees;
		}

		static char GetWrapped(this string value, int index) {
			index = index.Modulo(value.Length);
			return value[index];
		}

		static int Modulo(this int dividend, int divisor) {
			return (dividend % divisor + divisor) % divisor;
		}
		
	}
	
}