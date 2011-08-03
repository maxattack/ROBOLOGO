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
	
	public abstract class Block {
		public List<Instruction> buffer;
		
		public Block(List<Instruction> buffer) {
			this.buffer = buffer;
		}
		
		public abstract void End();
	}
	
	public class IfBlock : Block {
		public BranchInstruction branch;
		public int branchIndex;
		public JumpInstruction elseJump = null;
		
		public IfBlock(Expression condition, List<Instruction> buffer) : base(buffer) {
			branchIndex = buffer.Count;
			branch = new BranchInstruction(condition, branchIndex+1, -1);
			buffer.Add(branch);
		}
		
		public bool ElseIf(Expression condition) {
			return false;
		}
		
		public bool Else() {
			if (elseJump != null) { return false; }
			elseJump = new JumpInstruction(-1);
			buffer.Add(elseJump);
			branch.indexFalse = buffer.Count;
			return true;
		}
		
		override public void End() {
			if (elseJump != null) {
				elseJump.index = buffer.Count;
			} else {
				branch.indexFalse = buffer.Count;
			}
		}
		
		// todo add elseif, else
	}
	
	public class WhileBlock : Block {
		public BranchInstruction branch;
		public int branchIndex;
		
		public WhileBlock(Expression condition, List<Instruction> buffer) : base(buffer) {
			branchIndex = buffer.Count;
			branch = new BranchInstruction(condition, branchIndex+1, -1);
			buffer.Add(branch);
		}
		
		override public void End() {
			buffer.Add(new JumpInstruction(branchIndex));
			branch.indexFalse = buffer.Count;
		}
	}
	
	public class ForBlock : Block {
		static int sForCount = 0;
		public string iterator;
		public SetInstruction initializer;
		public BranchInstruction branch;
		public int branchIndex;
		
		public ForBlock(Expression initialValue, List<Instruction> buffer) : base(buffer) {
			sForCount++;
			iterator = "_iterator_"+ sForCount; // won't collide with names that are parse-able
			buffer.Add(new SetInstruction(iterator, initialValue));
			branchIndex = buffer.Count;
			branch = new BranchInstruction(
				new BinaryOperationExpression(
					BinaryOperation.GreaterThan,
					new VariableExpression(iterator),
					new LiteralExpression(0)),
				branchIndex+1, -1
			);
			buffer.Add(branch);
		}
		
		override public void End() {
			sForCount--;
			buffer.Add(new SetInstruction(
				iterator,
				new BinaryOperationExpression(
					BinaryOperation.Add,
					new VariableExpression(iterator), 
					new LiteralExpression(-1)
				)
			));
			buffer.Add(new JumpInstruction(branchIndex));
			branch.indexFalse = buffer.Count;
			buffer.Add(new UnsetInstruction(iterator));			
		}
		
	}
}

