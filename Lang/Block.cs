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
			iterator = "REPEAT_ITERATOR_"+ sForCount;
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
		}
		
	}
}

