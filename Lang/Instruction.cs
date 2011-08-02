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

namespace RoboLogo.Lang {
	
	/// <summary>
	/// Interface for all instructions.
	/// </summary>
	public abstract class Instruction {
		public abstract void Execute(Interpreter interp);
	}
	
	/// <summary>
	/// Implementation of a jump instruction
	/// </summary>
	public class JumpInstruction : Instruction {
		public int index;
		
		public JumpInstruction(int index) { 
			this.index = index; 
		}
		
		override public void Execute(Interpreter interp) { 
			interp.Goto(index); 
		}
	}
	
	/// <summary>
	/// Implementation of a set-global variable instruction
	/// </summary>
	public class SetInstruction : Instruction {
		string mName;
		Expression mExpr;
		
		public SetInstruction(string name, Expression exp) {
			mName = name;
			mExpr = exp;
		}
		
		override public void Execute(Interpreter interp) {
			interp.SetVariable(mName, mExpr.Compute(interp));
		}
	}
	
	/// <summary>
	/// Implemention of a branch instruciton, used to implement conditionals and loops
	/// </summary>
	public class BranchInstruction : Instruction {
		public Expression condition;
		public int indexTrue;
		public int indexFalse;
		
		public BranchInstruction(Expression condition, int jumpTrue, int jumpFalse) {
			this.condition = condition;
			this.indexTrue = jumpTrue;
			this.indexFalse = jumpFalse;
		}
		
		override public void Execute(Interpreter interp) {
			interp.Goto(condition.Compute(interp) == 0 ? indexFalse : indexTrue);
		}
	}
	
	/// <summary>
	/// Implementation of a custom action instruction, for binding to user-space C# code
	/// </summary>
	public class ActionInstruction : Instruction {
		Action<int> mAction;
		Expression mExpr;

		public ActionInstruction(Action<int> action, Expression exp) {
			mAction = action;
			mExpr = exp;
		}
		
		override public void Execute(Interpreter interp) {
			mAction(mExpr.Compute(interp));
		}
	}

}

