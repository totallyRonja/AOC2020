using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Day14 {
	public static class Program {

		interface Command {}

		class MemCommand : Command {
			public int Address;
			public long Value;
		}

		class MaskCommand : Command {
			public byte[] Mask = new byte[36];
		}

		static void Main(string[] args) {
			string[] lines = File.ReadAllLines("input.txt");
			
			var maskRegex = new Regex(@"mask = ([0-9X]+)");
			var memRegex = new Regex(@"mem\[([0-9]+)\] = ([0-9]+)");
			Command[] parsed = lines.Select<string, Command>(line => {
				var memMatch = memRegex.Match(line);
				if (memMatch.Success) {
					int address = int.Parse(memMatch.Groups[1].Value);
					long value = long.Parse(memMatch.Groups[2].Value);
					return new MemCommand{Address = address, Value = value};
				}
				var maskMatch = maskRegex.Match(line);
				if (maskMatch.Success) {
					string maskString = maskMatch.Groups[1].Value;
					byte[] mask = maskString.Select<char, byte>(character => character switch{'0'=>(byte)0, '1'=>(byte)1, _=>(byte)2})
					.Reverse().ToArray();
					return new MaskCommand{Mask = mask};
				}
				throw new Exception();
			}).ToArray();



			long valueSum = Version1(parsed);
			Console.WriteLine($"The sum of all values after executing the instructions is {valueSum}");

			valueSum = Version2(parsed);
			Console.WriteLine($"The sum of all values after executing the new instructions is {valueSum}");
		}

		static long Version2(Command[] parsed) {
			var memoryValues = new Dictionary<long, long>();
			byte[] mask = null;
			foreach (var command in parsed) {
				switch (command) {
					case MemCommand memCmd:
						List<long> addresses = ApplyMask(memCmd.Address);
						foreach (long address in addresses) {
							memoryValues[address] = memCmd.Value;
						}
						break;
					case MaskCommand maskCmd:
						mask = maskCmd.Mask;
						break;
					default: 
						throw new ArgumentOutOfRangeException();
				}
			}
			
			return memoryValues.Values.Sum();

			List<long> ApplyMask(long value) {
				List<long> addresses = new List<long>{value};
				for (int i = 0; i < mask!.Length; i++) {
					switch (mask[i]) {
						case 0: break;
						case 1:
							for (int j = 0; j < addresses.Count; j++)
								addresses[j] |= 1L << i;
							break;
						default:
							for (int j = addresses.Count - 1; j >= 0; j--) {
								addresses[j] |= 1L << i; //toggle to 1
								addresses.Add(addresses[j] & ~(1L<<i)); //add copy with byte at 0
							}
							break;
					}
				}
				return addresses;
			}
		}

		static long Version1(Command[] parsed) {
			var memoryValues = new Dictionary<int, long>();
			byte[] mask = null;
			foreach (var command in parsed) {
				switch (command) {
					case MemCommand memCmd:
						long value = ApplyMask(memCmd.Value);
						memoryValues[memCmd.Address] = value;
						break;
					case MaskCommand maskCmd:
						mask = maskCmd.Mask;
						break;
					default: 
						throw new ArgumentOutOfRangeException();
				}
			}
			return memoryValues.Values.Sum();
			
			long ApplyMask(long value) {
				for (int i = 0; i < mask!.Length; i++) {
					switch (mask[i]) {
						case 0:
							value &= ~(1L<<i);
							break;
						case 1:
							value |= (1L << i);
							break;
					}
				}
				return value;
			}
		}
	}
}