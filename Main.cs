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
			/*
			var interpreter = new Interpreter(StupidCompiler(@"
				set X 0
				hello
				set X X+1
				branch (X<10) 1
			"));
			*/
			
			var compiler = new InstructionParser() {
				setColorAction = arg => Console.WriteLine("Setting Color to {0}", arg),
				setThicknessAction = arg => Console.WriteLine("Setting Thickness to {0}", arg),
				setStrokeAction = arg => { if (arg!=0) { Console.WriteLine("Stroke ON"); } else { Console.WriteLine("Stroke OFF"); } },
				moveAction = arg => Console.WriteLine("Moving {0} Units", arg),
				turnAction = arg => Console.WriteLine("Turning {0} Degrees", arg),
			};
			var program = compiler.Parse(@"
				X = (10+32)
				LOOP = 0
				until (LOOP=3)
					set color to blue
					set thickness to 10
					start stroke
					move forward X
					turn left
					move backward 5
					stop stroke
					increment LOOP
				end
			");
			var interpreter = new Interpreter(program);
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
							buffer.Add(new BranchInstruction(parser.Parse(tokens[1]), int.Parse(tokens[2]), int.Parse(tokens[3])));
							break;
					}
				}
			}
			return buffer.ToArray();
		}

	}
}

