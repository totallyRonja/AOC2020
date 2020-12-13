using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Day13 {
	public static class Program {
		static void Main(string[] args) {
			var lines = File.ReadAllLines("input.txt");
			int startTime = int.Parse(lines[0]);
			var busLines = lines[1].Split(",")
				.Select((line, index) => (line: long.TryParse(line, out long bus) ? bus : -1, index))
				.Where(bus => bus.line > 0).Select(bus => (bus.line, invStagger: ((long) -bus.index).Mod(bus.line))).ToArray();

			long nextLine = -1;
			long nextArriveBy = int.MaxValue;
			foreach ((long bus, _) in busLines) {
				long value = bus - (startTime % bus);
				if (value < nextArriveBy) {
					nextLine = bus;
					nextArriveBy = value;
				}
			}
			Console.WriteLine($"the next bus is bus{nextLine} which arrives in {nextArriveBy}," +
			                  $"this means the solution is {nextArriveBy * nextLine}");
			
			//cool technique that only works if everyone is a prime number
			//start at 1
			long time = 1;
			long multiplicands = 1;
			
			foreach (var bus in busLines) {
				//add the multiplicands so far (doesnt change existing stagger) until the new one matches up as well
				//this could probably be even smarter, but this already works pretty darn well
				long diffToStagger = bus.line-(time - bus.invStagger).Mod(bus.line);
				while (diffToStagger != 0) {
					time += multiplicands;
					diffToStagger = (bus.line - (time - bus.invStagger)).Mod(bus.line);
				}
				//add new bus line to multiplicands
				multiplicands *= bus.line;
			}
			Console.WriteLine($"All bus lines being staggered by one happens for the first time at {time}");
		}
		
		static long Mod(this long dividend, long divisor) {
			return (dividend % divisor + divisor) % divisor;
		}
	}
}