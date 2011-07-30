// ROBOLOGO
// Copyright (C) 2011 max.kaufmann@gmail.com
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using RoboLogo.Lang;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RoboLogo {
	class MainClass {
		
		public static void Main (string[] args) {
			// a quick interpreter test program (for-loop)
			var interpreter = new Interpreter(StupidCompiler(@"
				set i 0
				hello
				set i i+1
				branch i<10 1 4
			"));
			while(interpreter.ExecuteNextInstruction()) {}
		}
		
		//---------------------------------------------------------------------
		// Implementation of a "stupid" assembly compiler for testing the 
		// interpreter separate from the regular compiler
		//---------------------------------------------------------------------
		
		static Instruction[] StupidCompiler(string src) {
			var buffer = new List<Instruction>();
			foreach(var line in src.Split('\n')) {
				var trim = line.Trim();
				if (trim.Length	 > 0) {
					var tokens = trim.Split(' ');
					switch(tokens[0]) {
						case "set":
							buffer.Add( new SetInstruction(tokens[1], StupidExpressionParser(tokens[2])) );
							break;
						case "hello":
							buffer.Add( new ActionInstruction(arg=>Console.WriteLine("Hello, World"), new NullExpression()) );
							break;
						case "branch":
							buffer.Add( new BranchInstruction(StupidExpressionParser(tokens[1]), int.Parse(tokens[2]), int.Parse(tokens[3])) );
							break;
					}
				}
			}
			return buffer.ToArray();
		}
		
		static readonly Dictionary<char, BinaryOperation> kOpLookup = InitOpTable();
		
		static Dictionary<char, BinaryOperation> InitOpTable() {
			var result = new Dictionary<char, BinaryOperation>();
			result.Add('+', BinaryOperation.Add);
			result.Add('-', BinaryOperation.Subtract);
			result.Add('*', BinaryOperation.Multiply);
			result.Add('/', BinaryOperation.Divide);
			result.Add('&', BinaryOperation.And);
			result.Add('|', BinaryOperation.Or);
			result.Add('=', BinaryOperation.Equals);
			result.Add('>', BinaryOperation.GreaterThan);
			result.Add('<', BinaryOperation.LessThan);
			return result;
		}
		
		static Expression StupidExpressionParser(string expr) {
			int index = 0;
			int literal;
			while(!kOpLookup.ContainsKey(expr[index])) { 
				index++; 
				if (index == expr.Length) {
					// not a binary operation
					return int.TryParse(expr, out literal) ? (Expression) new LiteralExpression(literal) : (Expression) new VariableExpression(expr);
				}
			}
			var left = expr.Substring(0, index);
			var right = expr.Substring(index+1);
			return new BinaryOperationExpression(
				kOpLookup[expr[index]],
				int.TryParse(left, out literal) ? (Expression) new LiteralExpression(literal) : (Expression) new VariableExpression(left),
				int.TryParse(right, out literal) ? (Expression) new LiteralExpression(literal) : (Expression) new VariableExpression(right)
			);
		}
	}
}

