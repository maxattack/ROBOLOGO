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
			/*
			string test = "";
			while(true) {
				var parser = new ExpressionParser();
				test = Console.ReadLine();
				if (test == "q") { break; }
				Expression exp;
				if (parser.Parse(test, out exp)) {
					Console.WriteLine("{0} = {1}", exp.ToString(), exp.Compute(null));
				} else {
					Console.WriteLine("Bad Expression");
				}
			}
			*/
			// a quick interpreter test program (for-loop)
			
			var interpreter = new Interpreter(StupidCompiler(@"
				set X 0
				hello
				set X X+1
				branch X<10 1 4
			"));
			while(interpreter.ExecuteNextInstruction()) {}
			
		}
		
		//---------------------------------------------------------------------
		// Implementation of a "stupid" assembly compiler for testing the 
		// interpreter separate from the regular compiler
		//---------------------------------------------------------------------
		
		static Instruction[] StupidCompiler(string src) {
			var buffer = new List<Instruction>();
			var parser = new ExpressionParser();
			foreach(var line in src.Split('\n')) {
				var trim = line.Trim();
				if (trim.Length	 > 0) {
					var tokens = trim.Split(' ');
					switch(tokens[0]) {
						case "set":
							buffer.Add(new SetInstruction(tokens[1], parser.Parse(tokens[2])));
							break;
						case "hello":
							buffer.Add(new ActionInstruction(arg=>Console.WriteLine("Hello, World"), new NullExpression()) );
							break;
						case "branch":
							buffer.Add(new BranchInstruction(parser.Parse(tokens[1]), int.Parse(tokens[2]), int.Parse(tokens[3])) );
							break;
					}
				}
			}
			return buffer.ToArray();
		}

	}
}

