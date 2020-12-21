using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Day20 {
	public static class Program {
		static void Main(string[] args) {
			string[] lines = File.ReadAllLines("input.txt");
			var idRegex = new Regex(@"^Tile ([0-9]+):$");
			int tileSize = lines.Skip(1).First().Length;

			//get tiles
			var tiles = new List<Tile>();
			var tileInProgress = new Tile {Data = new Flat2D<bool>(tileSize, tileSize)};
			int tileLine = -1;
			foreach (string line in lines) {
				if (string.IsNullOrWhiteSpace(line)) {
					if(tileInProgress.Id >= 0)
						tiles.Add(tileInProgress);
					tileInProgress = new Tile {Data = new Flat2D<bool>(tileSize, tileSize)};
					tileLine = -1;
					continue;
				}
				if (tileLine < 0) {
					var idMatch = idRegex.Match(line);
					tileInProgress.Id = int.Parse(idMatch.Groups[1].Value);
				} else {
					bool[] lineData = line.Select(letter => letter == '#').ToArray();
					lineData.CopyTo(tileInProgress.Data.GetArray(), tileLine * tileSize);
				}
				tileLine++;
			}
			if(tileInProgress.Id >= 0)
				tiles.Add(tileInProgress);

			//extract edges
			(int start, int walk)[] edges = {(0, 1), (0, tileSize), (tileSize*(tileSize-1), 1), (tileSize-1, tileSize)};
			foreach (Tile tile in tiles) {
				tile.Edges = new bool[4][];
				for (int i = 0; i < tile.Edges.Length; i++) {
					var edgeParam = edges[i];
					bool[] edge = Enumerable.Range(0, tileSize)
						.Select(num => tile.Data.GetArray()[edgeParam.start + edgeParam.walk * num]).ToArray();
					tile.Edges[i] = edge;
				}
			}
			
			//find corners
			(Tile tile, bool[] neighbor)[] tileNeighbors = tiles.Select(tile => (tile, new []{false, false, false, false})).ToArray();
			for (int i = 0; i < tiles.Count; i++) {
				Tile tile = tiles[i];
				for (int j = i+1; j < tiles.Count; j++) {
					Tile other = tiles[j];
					var edge = MatchingEdge(tile, other);
					if (edge != null) {
						tileNeighbors[i].neighbor[edge.Value.Item1] = true; //set side neighbor true
						tileNeighbors[j].neighbor[edge.Value.Item2] = true; //set opposite true
					}
				}
			}

			Console.WriteLine(tileNeighbors.Max(tn => tn.neighbor.Count(n => n)));
			Console.WriteLine(tileNeighbors.Count(tn => tn.neighbor.Count(n => n) == 3));
			var corners = tileNeighbors.Where(tn => tn.neighbor.Count(n => n) == 2).ToArray();
			long result = corners.Aggregate(1L, (prod, corner) => prod * corner.tile.Id);
			Console.WriteLine($"The corners {string.Join(", ", corners.Select(c => c.tile.Id))} multiplied result in {result}");

			int imageSize = (int) Math.Sqrt(tiles.Count);
			var remainingTiles = tiles.ToHashSet();
			remainingTiles.Remove(corners.First().tile);
			var orderedImage = new Flat2D<(Tile tile, (int rot, bool flipX, bool flipY) trans)>(imageSize, imageSize);
			var firstCorner = corners.First();
			int rot = Enumerable.Range(0, 4).First(n => !firstCorner.neighbor[n] && !firstCorner.neighbor[(n + 1) % 4]);
			//rot = (4 - rot) % 4;
			orderedImage[0, 0] = (firstCorner.tile, (rot, false, false));
			for (int y = 0; y < imageSize; y++) {
				for (int x = 0; x < imageSize; x++) {
					//skip already set ones (should only be first one)
					if(orderedImage[x, y].tile != null)
						continue;
					
					if (x == 0) {
						//first one in a row only depends on the one above it
						var neighbor = orderedImage[x, y - 1];
						var match = remainingTiles.Select(tile => (tile, MatchingEdge(tile, neighbor.tile)))
							.First(e => e.Item2 != null && ApplyTransform(e.Item2.Value.Item2, neighbor.Item2) == 2);
						Debug.Assert(match.Item2 != null, "match.Item2 != null");
						int newRot = (4 - match.Item2.Value.Item1) % 4;
						orderedImage[x, y] = (match.tile, (newRot, false, false));
						remainingTiles.Remove(match.tile);
					} else {
						//otherwise on the one on the left
						var neighbor = orderedImage[x - 1, y];
						var neighborEdge = ApplyTransform(3, neighbor.trans);
						//find tile to the right of neighbor
						var match = remainingTiles.Select(tile => (tile, match: MatchingEdge(tile, neighbor.tile
						.Edges[neighborEdge]))).First(e => e.match.HasValue);
						Debug.Assert(match.match != null, "match.Item2 != null");
						int newRot = match.match.Value;

						bool flipY = false;
						if (y > 0) {
							//if it doesnt match vertically, flip it
							var vertMatch = MatchingEdge(match.tile, orderedImage[x, y - 1].tile)!.Value;
							if (ApplyTransform(vertMatch.first, (newRot, false, false)) != 0)
								flipY = true;
							//if we're in line 2, also adjust line 1
							if(y == 1)
								if (ApplyTransform(vertMatch.second, (newRot, false, false)) != 2) {
									var previous = orderedImage[x, y - 1];
									previous.Item2.flipY = true;
									orderedImage[x, y - 1] = previous;
								}
						}
						
						orderedImage[x, y] = (match.tile, (newRot, false, flipY));
						remainingTiles.Remove(match.tile);

						//if we connected to the left to a field starting a row, flip it
						if (x == 1 && ApplyTransform(match.Item2.Value, neighbor.Item2) == 3) {
							var previous = orderedImage[x - 1, y];
							previous.Item2.flipX = true;
							orderedImage[x - 1, y] = previous;
						}
					}
					Console.WriteLine(Vis(orderedImage, tileSize));
				}
			}
			Console.WriteLine(orderedImage);
		}

		static string Vis(Flat2D<(Tile tile, (int rot, bool flipX, bool flipY))> tiles, int tileSize) {
			var builder = new StringBuilder();
			for (int y = 0; y < tiles.Height; y++) {
				for (int localY = 0; localY < tileSize; localY++) {
					for (int x = 0; x < tiles.Width; x++) {
						var tile = tiles[x, y];
						if (tile.tile == null)
							builder.Append(string.Join("",Enumerable.Range(0, 10).Select(_=>"?")));
						else
							for (int localX = 0; localX < tileSize; localX++) {
								(int posX, int posY) = ApplyTransform((localX, localY), tile.Item2);
								(posX, posY) = (posX.Mod(tileSize), posY.Mod(tileSize));
								builder.Append(tile.tile.Data[posX, posY]?"#":".");
							}
						builder.Append(" ");
					}
					builder.Append("\n");
				}
				builder.Append("\n");
			}

			return builder.ToString();
		}

		static int ApplyTransform(int direction, (int rot, bool flipX, bool flipY) transform) {
			direction += transform.rot;
			if (transform.flipY && direction % 2 == 0)
				direction += 2;
			if (transform.flipX && direction % 2 == 1)
				direction += 2;
			return direction % 4;
		}
		
		static (int x, int y) ApplyTransform((int x, int y) pos, (int rot, bool flipX, bool flipY) transform) {
			for (int i = 0; i < transform.rot; i++) {
				pos = (pos.y, -pos.x);
			}
			if (transform.flipY)
				pos = (pos.x, -pos.y);
			if (transform.flipX)
				pos = (pos.x, -pos.y);
			return pos;
		}

		static (int first, int second)? MatchingEdge(Tile first, Tile second) {
			for (int i = 0; i < first.Edges.Length; i++) {
				bool[] edge = first.Edges[i];
				int? otherEdge = MatchingEdge(second, edge);
				if (otherEdge != null)
					return (i, otherEdge.Value);
			}
			return null;
		}
		
		static int? MatchingEdge(Tile tile, bool[] compEdge) {
			for (int i = 0; i < tile.Edges.Length; i++) {
				bool[] edge = tile.Edges[i];
				if (edge.SequenceEqual(compEdge) || edge.Reverse().SequenceEqual(compEdge))
					return i;
			}
			return null;
		}

		static int Mod(this int dividend, int divisor) {
			return (dividend % divisor + divisor) % divisor;
		}

		class Tile {
			public int Id = -1;
			public Flat2D<bool> Data;
			public bool[][] Edges;
			
			public string Vis => ToString();

			public override string ToString() {
				var builder = new StringBuilder();
				for (int localY = 0; localY < Data.Height; localY++) {
					for (int localX = 0; localX < Data.Width; localX++) {
						(int posX, int posY) = (localX.Mod(Data.Width), localY.Mod(Data.Height));
						builder.Append(Data[posX, posY]?"#":".");
					}
					builder.Append("\n");
				}
				return builder.ToString();
			}
		}
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