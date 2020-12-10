using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Day8 {
	public static class Program {
		enum Instruction {
			Acc,
			Jmp,
			Nop
		}
		
		static void Main(string[] args) {
			//preprocess
			string[] input = File.ReadAllLines("input.txt");
			(Instruction operation, int argument, bool executed)[] instructions = input.Select(line => {
				string[] parts = line.Split(" ");
				Enum.TryParse<Instruction>(parts[0], true, out var operation);
				int argument = int.Parse(parts[1]);
				return (operation, argument, false);
			}).ToArray();

			//run
			long accumulator = 0;
			long instructionPointer = 0;
			while (true) {
				var instruction = instructions[instructionPointer];
				if(instruction.executed)
					break;
				instructions[instructionPointer].executed = true;
				switch (instruction.operation) {
					case Instruction.Acc:
						accumulator += instruction.argument;
						instructionPointer++;
						break;
					case  Instruction.Jmp:
						instructionPointer += instruction.argument;
						break;
					case Instruction.Nop:
						instructionPointer++;
						break;
					default: throw new ArgumentOutOfRangeException();
				}
			}
			Console.WriteLine($"When repeating an instruction for the first time the accumulator is at {accumulator}.");
			
			//reset everything
			for (int i = 0; i < instructions.Length; i++) {
				instructions[i].executed = false;
			}
			accumulator = 0;
			instructionPointer = 0;
			long changedInstruction = -1;
			long accumulatorCopy = -1;
			var taintedPath = new Stack<long>();

			//run
			while (instructionPointer < instructions.Length) {
				var instruction = instructions[instructionPointer];
				//if we loop
				if (instruction.executed) {
					//if we loop and already tried changing anything up to this point, stuffs broken
					if (changedInstruction < 0)
						throw new Exception();
					//go back and try the real thing
					while (taintedPath.TryPop(out long tainted)) {
						instructions[tainted].executed = false;
					}
					instructionPointer = changedInstruction;
					changedInstruction = -1;
					instruction = instructions[instructionPointer];
					accumulator = accumulatorCopy;
				}
				
				//if we're trying a changed instruction, remember what we walked over
				if(changedInstruction >= 0)
					taintedPath.Push(instructionPointer);

				//if we see a instruction we havent tried changing yet, lets try that!
				if (!instruction.executed && changedInstruction < 0) {
					switch (instruction.operation) {
						case Instruction.Jmp:
							changedInstruction = instructionPointer;
							accumulatorCopy = accumulator;
							instruction.operation = Instruction.Nop;
							break;
						case Instruction.Nop:
							changedInstruction = instructionPointer;
							accumulatorCopy = accumulator;
							instruction.operation = Instruction.Jmp;
							break;
					}
				}
				
				//actually execute
				instructions[instructionPointer].executed = true;
				switch (instruction.operation) {
					case Instruction.Acc:
						accumulator += instruction.argument;
						instructionPointer++;
						break;
					case  Instruction.Jmp:
						instructionPointer += instruction.argument;
						break;
					case Instruction.Nop:
						instructionPointer++;
						break;
					default: throw new ArgumentOutOfRangeException();
				}
			}
			
			Console.WriteLine($"When fixing the instruction in line {changedInstruction} the accumulator ends on {accumulator}.");
		}
	}
}