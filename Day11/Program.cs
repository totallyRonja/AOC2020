using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Day11 {
	public static class Program {
		static void Main(string[] args) {
			string[] lines = File.ReadAllLines("input.txt");
			int width = lines[0].Trim().Length;
			var data = lines.SelectMany(line => line)
				.Select(c => c switch { 'L' => State.Empty, '#' => State.Occupied, _ => State.Floor}).ToArray();
			var buffer = new Flat2D<State>(width, lines.Length, data);
			
			Stopwatch sw = new Stopwatch();
			sw.Start();

			SimulateUntilStagnation(ref buffer, false);

			int occupiedSeats = buffer.Count(seat => seat == State.Occupied);
			Console.WriteLine($"When the situation stabilizes there are {occupiedSeats} occupied seats.");
			sw.Stop();
			Console.WriteLine($"First part needed {sw.ElapsedMilliseconds}");
		
			sw.Restart();
			
			buffer.Set(data);
			SimulateUntilStagnation(ref buffer, true);
			
			occupiedSeats = buffer.Count(seat => seat == State.Occupied);
			Console.WriteLine($"When the situation stabilizes there are {occupiedSeats} occupied seats (when looking far).");
			
			sw.Stop();
			Console.WriteLine($"Second part needed {sw.ElapsedMilliseconds}");
		}

		static void SimulateUntilStagnation(ref Flat2D<State> buffer, bool nearest) {
			var targetBuffer = new Flat2D<State>(buffer.Width, buffer.Height);
			bool changed;
			do {
				changed = false;
				for (int x = 0; x < buffer.Width; x++) {
					for (int y = 0; y < buffer.Height; y++) {
						var state = buffer[x, y];
						if (state == State.Floor) {
							targetBuffer[x, y] = State.Floor;
							continue;
						}

						int occupiedNeighbors = nearest switch {
							true => FarOccupiedNeighbors(buffer, x, y), 
							false => DirectOccupiedNeighbors(buffer, x, y),
						};

						targetBuffer[x, y] = state switch {
							State.Empty => occupiedNeighbors == 0 ? State.Occupied : State.Empty,
							State.Occupied => occupiedNeighbors >= (nearest?5:4) ? State.Empty : State.Occupied,
							_ => targetBuffer[x, y],
						};
						changed |= state != targetBuffer[x, y];
					}
				}
				
				//double buffer
				var temp = targetBuffer;
				targetBuffer = buffer;
				buffer = temp;
			} while (changed);
		}
		
		static readonly (int, int)[] Directions = {(-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 1), (1, -1), (1, 0), (1, 1)};

		static int DirectOccupiedNeighbors(Flat2D<State> grid, int x, int y) {
			int occupiedNeighbors = 0;
			foreach ((int dx, int dy) in Directions) {
				int posX = x + dx, posY = y + dy;
				if (grid.InRange(posX, posY) && grid[posX, posY] == State.Occupied)
					occupiedNeighbors++;
			}
			return occupiedNeighbors;
		}

		static (int x, int y)[] rayTargets;
		static int FarOccupiedNeighbors(Flat2D<State> grid, int x, int y) {
			rayTargets ??= new (int x, int y)[grid.Width * grid.Height];
			int occupiedNeighbors = 0;
			foreach ((int dx, int dy) in Directions) {
				(int posX, int posY) = RayTraceChairs(grid, (x, y), (dx, dy));
				if (grid.InRange(posX, posY) && grid[posX, posY] == State.Occupied)
						occupiedNeighbors++;
			}
			return occupiedNeighbors;
		}

		static (int x, int y) RayTraceChairs(Flat2D<State> grid, (int x, int y) origin, (int dx, int dy) direction) {
			var position = origin;
			do {
				position.x += direction.dx;
				position.y += direction.dy;
			} while (grid.InRange(position.x, position.y) && grid[position.x, position.y] == State.Floor);

			return position;
		}
	}

	enum State : byte {
		Floor,
		Empty,
		Occupied
	}

	public class Flat2D<T> : IEnumerable<T> {
		public readonly int Width;
		public readonly int Height;
		T[] arr;
		
		public Flat2D(int width, int height, T[] fromArray) : this(width, height) {
			Set(fromArray);
		}

		public Flat2D(int width, int height) {
			Width = width;
			Height = height;
			arr = new T[width * height];
		}

		public void Set(T[] value) {
			if(Width * Height != value.Length)
				throw new IndexOutOfRangeException();
			value.CopyTo(arr, 0);
		}
		
		public T this[int x, int y] {
			get => arr[y * Width + x];
			set => arr[y * Width + x] = value;
		}

		public bool InRange(int x, int y) {
			return x >= 0 && x < Width && y >= 0 && y < Height;
		}

		public T GetOrDefault(int x, int y) {
			return InRange(x, y) ? arr[ConvertIndex(x, y)] : default;
		}

		int ConvertIndex(int x, int y) {
			return y * Width + x;
		}

		public T[] GetArray() {
			return arr;
		}

		public IEnumerator<T> GetEnumerator() {
			return arr.Cast<T>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return arr.GetEnumerator();
		}
	}
}