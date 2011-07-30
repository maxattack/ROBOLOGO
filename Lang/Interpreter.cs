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

using System;
using System.Collections.Generic;

namespace RoboLogo.Lang {
	
	/// <summary>
	/// The interpreter is the facade-class for the virtual machine.  It maintains an instruciton buffer,
	/// an instruction pointer, and a dictionary of global variables.
	/// </summary>
	
	public class Interpreter {
		Instruction[] mInstructions;
		int mCurrentInstruction;
		int mNextInstruction;
		Dictionary<string, int> mEnvironment = new Dictionary<string, int>();
		
		public Interpreter (Instruction[] instructions) {
			mInstructions = instructions;
			mCurrentInstruction = 0;
		}
		
		public bool ExecuteNextInstruction() {
			if (mCurrentInstruction >= mInstructions.Length) { return false; }
			mNextInstruction = mCurrentInstruction + 1;
			mInstructions[mCurrentInstruction].Execute(this);
			mCurrentInstruction = mNextInstruction;
			return true;
		}
		
		internal void Goto(int n) {
			mNextInstruction = n;
		}
		
		internal void SetVariable(string name, int val) {
			if (!mEnvironment.ContainsKey(name)) {
				mEnvironment.Add(name, val);
			} else {
				mEnvironment[name] = val;
			}
		}
		
		internal bool GetVariable(string name, out int val) {
			if (!mEnvironment.ContainsKey(name)) {
				val = 0;
				return false;
			} else {
				val = mEnvironment[name];
				return true;
			}
		}
	}
	
}

