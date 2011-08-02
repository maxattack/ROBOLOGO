using System;
namespace RoboLogo.Lang {
	
	public class Block {
		public int instructionIndex;
		
		public Block(int index) {
			instructionIndex = index;
		}
	}
	
	public class WhileBlock : Block {
		public BranchInstruction branch;
		
		public WhileBlock(BranchInstruction branch, int index) : base(index) {
			this.branch = branch;
		}
	}
}

