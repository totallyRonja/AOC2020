using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Day17 {
	public static class Program {
		static void Main(string[] args) {
			var input = File.ReadAllLines("input.txt").SelectMany((line, indexY) => line
				.Select((character, indexX) => (x: indexX, y:indexY, valid:character == '#')))
				.Where(pos => pos.valid).Select(pos => (pos.x, pos.y)).ToArray();

			int sim3d = Sim3d(input);
			Console.WriteLine($"After 6 steps there are {sim3d} active cubes in the 3d simulation");
			
			int sim4d = Sim4d(input);
			Console.WriteLine($"After 6 steps there are {sim4d} active cubes in the 4d simulation");
		}
		
		static int Sim4d(IEnumerable<(int x, int y)> input) {
			var activeCubes = input.Select(pos => (pos.x, pos.y, 0, 0)).ToHashSet();
			var neighborDirections = Enumerable.Range(-1, 3)
				.SelectMany(x => Enumerable.Range(-1, 3)
					.SelectMany(y => Enumerable.Range(-1, 3)
						.SelectMany(z => Enumerable.Range(-1, 3)
							.Select(w => (x, y, z, w)))))
				.Where(dir => dir.x != 0 || dir.y != 0 || dir.z != 0 || dir.w != 0).ToArray();
			var neighborMap = new Dictionary<(int x, int y, int z, int w), int>();
			for (int i = 0; i < 6; i++) {
				neighborMap.Clear();
				//to make sure lonely cubes know they're lonely
				foreach (var cube in activeCubes) {
					neighborMap.Add(cube, 0);
				}
				//collect data
				foreach (var cube in activeCubes) {
					foreach (var direction in neighborDirections) {
						var pos = cube.Plus(direction);
						neighborMap[pos] = neighborMap.TryGetValue(pos, out int neighbors) ? neighbors + 1 : 1;
					}
				}
				//apply changes
				foreach (((int x, int y, int z, int w) key, int value) in neighborMap) {
					if (value < 2 || value > 3)
						activeCubes.Remove(key);
					if (value == 3)
						activeCubes.Add(key);
				}
			}

			return activeCubes.Count;
		}

		static int Sim3d(IEnumerable<(int x, int y)> input) {
			var activeCubes = input.Select(pos => (pos.x, pos.y, 0)).ToHashSet();
			var neighborDirections = Enumerable.Range(-1, 3)
				.SelectMany(x => Enumerable.Range(-1, 3)
					.SelectMany(y => Enumerable.Range(-1, 3)
						.Select(z => (x, y, z))))
				.Where(dir => dir.x != 0 || dir.y != 0 || dir.z != 0).ToArray();
			var neighborMap = new Dictionary<(int x, int y, int z), int>();
			for (int i = 0; i < 6; i++) {
				neighborMap.Clear();
				//to make sure lonely cubes know they're lonely
				foreach (var cube in activeCubes) {
					neighborMap.Add(cube, 0);
				}
				foreach (var cube in activeCubes) {
					foreach (var direction in neighborDirections) {
						var pos = cube.Plus(direction);
						neighborMap[pos] = neighborMap.TryGetValue(pos, out int neighbors) ? neighbors + 1 : 1;
					}
				}

				foreach (((int x, int y, int z) key, int value) in neighborMap) {
					if (value < 2 || value > 3)
						activeCubes.Remove(key);
					if (value == 3)
						activeCubes.Add(key);
				}
			}

			return activeCubes.Count;
		}
		
		static (int x, int y, int z) Plus(this (int x, int y, int z) value, (int x, int y, int z) other) {
			return (value.x + other.x, value.y + other.y, value.z + other.z);
		}
		
		static (int x, int y, int z, int w) Plus(this (int x, int y, int z, int w) value, (int x, int y, int z, int w) other) {
			return (value.x + other.x, value.y + other.y, value.z + other.z, value.w + other.w);
		}
	}
}