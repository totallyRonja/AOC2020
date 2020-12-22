using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;

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
			(int start, int walk)[] edges = {(0, 1), (tileSize*(tileSize-1), -tileSize), (tileSize*tileSize-1, -1), (tileSize-1, tileSize)};
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
			var corners = tileNeighbors.Where(tn => tn.neighbor.Count(n => n) <= 2).ToArray();
			long cornerProduct = corners.Aggregate(1L, (prod, corner) => prod * corner.tile.Id);
			Console.WriteLine($"The corners {string.Join(", ", corners.Select(c => c.tile.Id))} multiplied result in {cornerProduct}");

			int imageSize = (int) Math.Sqrt(tiles.Count);
			var remainingTiles = tiles.ToHashSet();
			remainingTiles.Remove(corners.First().tile);
			var orderedImage = new Flat2D<Tile>(imageSize, imageSize);
			var firstCorner = corners.First();
			int rot = Enumerable.Range(0, 4).First(n => !firstCorner.neighbor[n] && !firstCorner.neighbor[(n + 1) % 4]);
			rot = (rot - 1) % 4;
			firstCorner.tile.Trans.rot = rot;
			firstCorner.tile.Trans.flipY = true;
			orderedImage[0, 0] = firstCorner.tile;
			for (int y = 0; y < imageSize; y++) {
				for (int x = 0; x < imageSize; x++) {
					//skip already set ones (should only be first one)
					if(orderedImage[x, y] != null)
						continue;
					
					if (x == 0) {
						//first one in a row only depends on the one above it
						var neighbor = orderedImage[x, y - 1];
						var neighborEdge = ApplyTransform(2, neighbor.Trans);
						var match = remainingTiles
							.Select(tile => (tile, match: MatchingEdge(tile, neighbor.Edges[neighborEdge])))
							.First(e => e.match.HasValue);
						Debug.Assert(match.Item2 != null, "match.Item2 != null");
						int newRot = match.Item2.Value;
						match.tile.Trans.rot = newRot;
						orderedImage[x, y] = match.tile;
						remainingTiles.Remove(match.tile);
					} else {
						//otherwise on the one on the left
						var neighbor = orderedImage[x - 1, y];
						var neighborEdge = ApplyTransform(3, neighbor.Trans);
						//find tile to the right of neighbor
						var match = remainingTiles.Select(tile => (tile, match: MatchingEdge(tile, neighbor
							.Edges[neighborEdge]))).FirstOrDefault(e => e.match.HasValue);
						if (!match.match.HasValue && x == 1)
						{
							//flip to other side and try backside
							neighborEdge = (neighborEdge + 2) % 4;
							match = remainingTiles.Select(tile => (tile, match: MatchingEdge(tile, neighbor
								.Edges[neighborEdge]))).First(e => e.match.HasValue);
							//flip prev tile
							var previous = orderedImage[x - 1, y];
							previous.Trans.flipX = true;
							orderedImage[x - 1, y] = previous;
						}
						Debug.Assert(match.match != null, "match.Item2 != null");
						int newRot = (match.match.Value - 1).Mod(4);
						
						match.tile.Trans.rot = newRot;

						bool flipY = false;
						if (y > 0) {
							//if it doesnt match vertically, flip it
							var vertMatch = MatchingEdge(orderedImage[x, y - 1], 
								match.tile.Edges[ApplyTransform(0, match.tile.Trans)]).HasValue;
							if (!vertMatch)
								flipY = true;
							//if we're in line 2, also adjust line 1
							if (y == 1)
							{
								vertMatch = MatchingEdge(match.tile,
										orderedImage[x, y - 1].Edges[ApplyTransform(2, orderedImage[x, y - 1].Trans)])
									.HasValue;
								if (!vertMatch)
								{
									var previous = orderedImage[x, y - 1];
									previous.Trans.flipY = true;
									orderedImage[x, y - 1] = previous;
								}
							}
						}

						match.tile.Trans.flipY = flipY;
						orderedImage[x, y] = match.tile;
						remainingTiles.Remove(match.tile);
					}
					//Console.WriteLine(Vis(orderedImage, tileSize));
				}
			}
			//Console.WriteLine(orderedImage);
			int resultSize = imageSize * (tileSize - 2);
			var result = new Flat2D<char>(resultSize, resultSize);
			for (int y = 0; y < imageSize; y++)
			{
				for (int x = 0; x < imageSize; x++)
				{
					for (int localY = 1; localY < tileSize - 1; localY++)
					{
						for (int localX = 1; localX < tileSize - 1; localX++)
						{
							(int x, int y) targetPos = (x * (tileSize-2) + (localX-1), y * (tileSize-2) + (localY-1));
							(int x, int y) sourcePos = ApplyTransform((localX, localY), orderedImage[x, y].Trans, tileSize);
							result[targetPos.x, targetPos.y] = orderedImage[x, y].Data[sourcePos.x, sourcePos.y]?'#':'.';
						}
					}
				}
			}

			var monster = new []
			{
				@"                  _ ",
				@"\    /\    /\    /0\",
				@" \  /  \  /  \  /   ",
			};
			var monsterPositions = monster
				.SelectMany((line, yIndex) => line
					.Select((letter, xIndex) => (x: xIndex, y: yIndex, l: letter))
					.Where(letter => letter.l != ' ')
					.Select(val => (val.x, val.y))).ToArray();
			(int x, int y) monsterSize = (monster.First().Length, monster.Length);
			var transformations = Enumerable.Range(0, 4)
				.SelectMany(rot => Enumerable.Range(0, 4)
					.Select(flip => (rot, flip<2, flip%2==0)));
			int monsterCount = 0;
			(int, bool, bool) validTrans = default;
			foreach (var trans in transformations)
			{
				validTrans = trans;
				for (int x = 0; x < resultSize - monsterSize.x; x++)
				{
					for (int y = 0; y < resultSize - monsterSize.y; y++)
					{
						if (monsterPositions.All(monsterPos =>
						{
							var pos = ApplyTransform((x + monsterPos.x, y + monsterPos.y), trans, resultSize);
							return result[pos.x, pos.y] == '#';
						}))
						{
							monsterCount++;
							foreach (var monsterPos in monsterPositions)
							{
								var pos = ApplyTransform((x + monsterPos.x, y + monsterPos.y), trans, resultSize);
								result[pos.x, pos.y] = monster[monsterPos.y][monsterPos.x];
							}
						}
					}
				}
				if(monsterCount > 0)
					break;
			}
			//var resBuilder = new StringBuilder();
			for (int y = 0; y < result.Height; y++)
			{
				for (int x = 0; x < result.Width; x++)
				{
					var pos = ApplyTransform((x, y), validTrans, resultSize);
					var pixel = result[pos.x, pos.y];
					Console.ForegroundColor = pixel switch
					{
						'.' => ConsoleColor.Blue,
						'#' => ConsoleColor.White,
						'0' => ConsoleColor.Red,
						_ => ConsoleColor.Green,
					};
					pixel = pixel switch
					{
						'.' => '~',
						_ => pixel,
					};
					Console.Write(pixel);
				}
				Console.WriteLine();
			}
			Console.WriteLine();
			Console.ResetColor();

			var solidCount = result.Count(pixel => pixel == '#');
			Console.WriteLine(solidCount);
		}

		private static string Vis(Flat2D<char> result)
		{
			var builder = new StringBuilder();
			for (int y = 0; y < result.Height; y++)
			{
				for (int x = 0; x < result.Width; x++)
				{
					builder.Append(result[x, y]);
				}
				builder.AppendLine();
			}
			return builder.ToString();
		}

		static string Vis(Flat2D<Tile> tiles, int tileSize) {
			var builder = new StringBuilder();
			for (int y = 0; y < tiles.Height; y++) {
				for (int localY = 0; localY < tileSize; localY++) {
					for (int x = 0; x < tiles.Width; x++) {
						var tile = tiles[x, y];
						if (tile == null)
							builder.Append(string.Join("",Enumerable.Range(0, 10).Select(_=>"?")));
						else
							for (int localX = 0; localX < tileSize; localX++) {
								(int posX, int posY) = ApplyTransform((localX, localY), tile.Trans, tileSize);
								(posX, posY) = (posX.Mod(tileSize), posY.Mod(tileSize));
								builder.Append(tile.Data[posX, posY]?"#":".");
							}
						builder.Append(' ');
					}
					builder.Append('\n');
				}
				builder.Append('\n');
			}

			return builder.ToString();
		}

		static int ApplyTransform(int direction, (int rot, bool flipX, bool flipY) transform) {
			
			if (transform.flipY && direction % 2 == 0)
				direction += 2;
			if (transform.flipX && direction % 2 == 1)
				direction += 2;
			direction += transform.rot;
			return direction % 4;
		}
		
		static (int x, int y) ApplyTransform((int x, int y) pos, (int rot, bool flipX, bool flipY) transform, int  size) {
			
			if (transform.flipY)
				pos = (pos.x, size - 1 - pos.y);
			if (transform.flipX)
				pos = (size - 1 - pos.x, pos.y);
			for (int i = 0; i < transform.rot; i++) {
				pos = (pos.y, size - 1 - pos.x);
			}
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
			public (int rot, bool flipX, bool flipY) Trans;
			
			public string Vis => ToString();

			public override string ToString() {
				var builder = new StringBuilder();
				builder.Append("  ");
				builder.Append(string.Join("", Edges[ApplyTransform(0, Trans)].Select(vis => vis ? '#' : '.')));
				builder.AppendLine();
				builder.AppendLine();
				for (int localY = 0; localY < Data.Height; localY++)
				{
					builder.Append(Edges[ApplyTransform(1, Trans)][Data.Height - 1 - localY] ? '#' : '.');
					builder.Append(' ');
					for (int localX = 0; localX < Data.Width; localX++)
					{
						(int posX, int posY) = ApplyTransform((localX, localY), Trans, Data.Width);
						builder.Append(Data[posX, posY]?'#':'.');
					}
					builder.Append(' ');
					builder.Append(Edges[ApplyTransform(3, Trans)][localY] ? '#' : '.');
					builder.AppendLine();
				}

				builder.AppendLine();
				builder.Append("  ");
				builder.Append(string.Join("", Edges[ApplyTransform(2, Trans)].Reverse().Select(vis => vis ? '#' : '.')));
				builder.AppendLine();
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