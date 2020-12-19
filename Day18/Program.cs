using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Day18 {
	public static class Program {
		static void Main(string[] args) {
			string[] lines = File.ReadAllLines("input.txt");
			var tokens = lines.Select(Tokenizer).ToArray();
			long resultSum = tokens.Select(Parse).Sum(expression => expression.Parse());
			Console.WriteLine($"The sum of math is {resultSum}");
			resultSum = tokens.Select(ParseAdvanced).Sum(expression => expression.Parse());
			Console.WriteLine($"The sum of advanced math is {resultSum}");
		}

		static List<Token> Tokenizer(string code) {
			var tokens = new List<Token>();
			for (int index = 0; index < code.Length; index++) {
				char character = code[index];
				if(character == ' ')
					continue;
				var token = character switch {
					'(' => new Token{Type = TokenType.ParenOpen},
					')' => new Token{Type = TokenType.ParenClose},
					'+' => new Token{Type = TokenType.Add},
					'*' => new Token{Type = TokenType.Mul},
					_ => new Token{Type = TokenType.Int, Value = int.Parse(character.ToString())},
				};
				tokens.Add(token);
			}

			return tokens;
		}

		static Expression ParseAdvanced(List<Token> tokens) {
			Expression result = null;
			var priorityStack = new Stack<Expression>();
			foreach (Token token in tokens) {
				switch (token.Type) {
					case TokenType.ParenOpen:
						priorityStack.Push(result);
						result = null;
						break;
					case TokenType.ParenClose:
						result = FillExpression(priorityStack.Pop(), new Parenthesis{Child = result});
						break;
					case TokenType.Add:
						var left = result;
						if (result is BinaryOp binRes) {
							left = binRes.Right;
							binRes.Right = null;
						}
						result = FillExpression(result, new BinaryOp{Left = left, Right = null, Operation = BinOp.Add});
						break;
					case TokenType.Mul: 
						result = new BinaryOp{Left = result, Right = null, Operation = BinOp.Mul};
						break;
					case TokenType.Int:
						result = FillExpression(result, new Const {Value = token.Value});
						break;
					default: throw new Exception("Invalid token type");
				}
			}
			return result;
		}
		
		static Expression Parse(List<Token> tokens) {
			Expression result = null;
			var priorityStack = new Stack<Expression>();
			foreach (Token token in tokens) {
				switch (token.Type) {
					case TokenType.ParenOpen:
						priorityStack.Push(result);
						result = null;
						break;
					case TokenType.ParenClose:
						result = FillExpression(priorityStack.Pop(), new Parenthesis{Child = result});
						break;
					case TokenType.Add: 
						result = new BinaryOp{Left = result, Right = null, Operation = BinOp.Add};
						break;
					case TokenType.Mul: 
						result = new BinaryOp{Left = result, Right = null, Operation = BinOp.Mul};
						break;
					case TokenType.Int:
						result = FillExpression(result, new Const {Value = token.Value});
						break;
					default: throw new Exception("Invalid token type");
				}
			}
			return result;
		}

		static Expression FillExpression(Expression parent, Expression child) {
			if (parent is BinaryOp binParent) {
				binParent.Right = FillExpression(binParent.Right, child);
				return parent;
			}
			return child;
		}
	}

	public enum TokenType {
		Mul,
		Add,
		Int,
		ParenOpen,
		ParenClose,
	}

	public struct Token {
		public TokenType Type;
		public int Value;
	}

	public abstract class Expression {
		public abstract long Parse();
	}

	public class Const: Expression{
		public long Value;
		public override string ToString() {
			return Value.ToString();
		}

		public override long Parse() {
			return Value;
		}
	}

	public class Parenthesis : Expression {
		public Expression Child;
		public override long Parse() => Child.Parse();
		public override string ToString() {
			return $"{Child}";
		}
	}

	public enum BinOp {
		Mul,
		Add,
	}

	public class BinaryOp : Expression {
		public Expression Left;
		public Expression Right;
		public BinOp Operation;

		public override string ToString() {
			return $"({Left} {Operation switch {BinOp.Add => "+", BinOp.Mul => "*", _ => "???"}} {Right})";
		}

		public override long Parse() {
			return Operation switch {
				BinOp.Mul => Left.Parse() * Right.Parse(),
				BinOp.Add => Left.Parse() + Right.Parse(),
				_ => throw new ArgumentOutOfRangeException()
			};
		}
	}
}