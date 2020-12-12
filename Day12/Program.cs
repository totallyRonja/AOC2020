using System;
using System.IO;
using System.Linq;

namespace Day12 {
	public static class Program {
		enum Actions {
			North,
			South,
			East,
			West,
			Left,
			Right,
			Forward,
		}
		static void Main(string[] args) {
			var lines = File.ReadAllLines("input.txt");
			var commands = lines.Select(line => (action: line[0] switch {
				'N' => Actions.North,
				'S' => Actions.South,
				'E' => Actions.East,
				'W' => Actions.West,
				'L' => Actions.Left,
				'R' => Actions.Right,
				'F' => Actions.Forward,
				_ => throw new ArgumentException(),
			}, arg: int.Parse(line.Substring(1)))).ToArray();

			(int x, int y)[] directions = {(1, 0), (0, -1), (-1, 0), (0, 1)};

			var target = commands.Aggregate((pos: (x: 0, y: 0), dir: 0), (curr, command) => command.action switch {
				Actions.North => ((curr.pos.x, curr.pos.y + command.arg), curr.dir),
				Actions.South => ((curr.pos.x, curr.pos.y - command.arg), curr.dir),
				Actions.East => ((curr.pos.x + command.arg, curr.pos.y), curr.dir),
				Actions.West => ((curr.pos.x - command.arg, curr.pos.y), curr.dir),
				Actions.Left => ((curr.pos.x, curr.pos.y), (curr.dir - command.arg/90).Mod(4)),
				Actions.Right => ((curr.pos.x, curr.pos.y), (curr.dir + command.arg/90).Mod(4)),
				Actions.Forward => ((curr.pos.x + directions[curr.dir].x * command.arg, curr.pos.y + directions[curr.dir].y* command.arg), curr.dir),
				_ => curr,
			});
			
			Console.WriteLine($"The ship arrives at {target.pos.x},{target.pos.y} which has a manhattan distance " +
			                  $"of {target.pos.Manhattan()}");
			
			var newTarget = commands.Aggregate((pos: (x: 0, y: 0), wp: (x: 10, y: 1)), (curr, command) => command.action 
			switch {
				Actions.North => (curr.pos, (curr.wp.x, curr.wp.y + command.arg)),
				Actions.South => (curr.pos, (curr.wp.x, curr.wp.y - command.arg)),
				Actions.East => (curr.pos, (curr.wp.x + command.arg, curr.wp.y)),
				Actions.West => (curr.pos, (curr.wp.x - command.arg, curr.wp.y)),
				Actions.Left => (curr.pos, curr.wp.Rot(-command.arg/90)),
				Actions.Right => (curr.pos, curr.wp.Rot(command.arg/90)),
				Actions.Forward => ((curr.pos.x + curr.wp.x * command.arg, curr.pos.y + curr.wp.y * command.arg), curr.wp),
				_ => curr,
			});
			
			Console.WriteLine($"With the new rules the ship arrives at {newTarget.pos.x},{newTarget.pos.y} which has a manhattan distance " +
			                  $"of {newTarget.pos.Manhattan()}");
		}

		static int Manhattan(this (int x, int y) value) {
			return Math.Abs(value.x) + Math.Abs(value.y);
		}
		
		static (int x, int y) Rot(this (int x, int y) value, int steps) {
			for (int i = 0; i < steps.Mod(4); i++) {
				value = (value.y, -value.x);
			}
			return value;
		}

		static int Mod(this int dividend, int divisor) {
			return (dividend % divisor + divisor) % divisor;
		}
	}
}