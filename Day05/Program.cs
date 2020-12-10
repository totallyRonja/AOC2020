using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Day5 {
	static class Program {
		static void Main(string[] args) {
			string[] passes = File.ReadAllLines("input.txt");
			int[] seatIDs = passes.Select(pass => {
				int row = pass.Take(7).Select((character, index) => character=='B'?2.Pow(6-index):0).Sum();
				int column = pass.Skip(7).Select((character, index) => character=='R'?2.Pow(2-index):0).Sum();
				return row * 8 + column;
			}).ToArray();
			Console.WriteLine($"The highest seat id is {seatIDs.Max()}.");

			var possibleSeats = new bool[8*128];
			foreach (int id in seatIDs) {
				possibleSeats[id] = true;
			}
			int mySeat = Enumerable.Range(1, possibleSeats.Length - 2).First(
				id => !possibleSeats[id] && possibleSeats[id-1] && possibleSeats[id+1]);
			Console.WriteLine($"My seat is {mySeat}.");
		}
		
		public static int Pow(this int bas, int exp)
		{
			return Enumerable
				.Repeat(bas, exp)
				.Aggregate(1, (a, b) => a * b);
		}
	}
}