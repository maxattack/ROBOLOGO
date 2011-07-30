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
	/// Interface for all instructions.  Conditional expressions are treated as expr!=0 numeric expressions
	/// </summary>
	public abstract class Expression {
		public abstract int Compute(Interpreter interp);
	}
	
	/// <summary>
	/// Implemented Binary Operations
	/// </summary>
	public enum BinaryOperation { Add, Subtract, Multiply, Divide, And, Or, Equals, GreaterThan, LessThan }
	
	/// <summary>
	/// Implemented Unary Operations
	/// </summary>
	public enum UnaryOperation { Negate, Complement }
	
	/// <summary>
	/// Null expression just returns 0
	/// </summary>
	public class NullExpression : Expression {
		override public int Compute(Interpreter interp) {
			return 0;
		}
	}
	
	/// <summary>
	/// A literal expression evaluates to a constant
	/// </summary>
	public class LiteralExpression : Expression {
		int mValue;
		
		public LiteralExpression(int val) { 
			mValue = val; 
		}
		
		override public int Compute(Interpreter interp) { 
			return mValue; 
		}
	}
	
	/// <summary>
	/// A variable expression looks up a value in the current executing context
	/// </summary>
	public class VariableExpression : Expression {
		string mName;
		
		public VariableExpression(string name) {
			mName = name;
		}
		
		override public int Compute(Interpreter interp) {
			int result;
			if (!interp.GetVariable(mName, out result)) {
				Console.WriteLine("Undefined Variable Expression");
			}
			return result;
		}
	}
	
	/// <summary>
	/// Implementation of all binary operations
	/// </summary>
	public class BinaryOperationExpression : Expression {
		BinaryOperation mOp;
		Expression mLeft;
		Expression mRight;
		
		public BinaryOperationExpression(BinaryOperation op, Expression left, Expression right) {
			mOp = op;
			mLeft = left;
			mRight = right;
			
		}
		
		override public int Compute(Interpreter interp) {
			switch(mOp) {
				case BinaryOperation.Add: return mLeft.Compute(interp) + mRight.Compute(interp);
				case BinaryOperation.Subtract: return mLeft.Compute(interp) - mRight.Compute(interp);
				case BinaryOperation.Multiply: return mLeft.Compute(interp) * mRight.Compute(interp);
				case BinaryOperation.Divide: 
					int r = mRight.Compute(interp);
					if (r == 0) {
						Console.WriteLine("Divide By Zero");
						return 0;
					} else {
						return mLeft.Compute(interp) / r;
					}
				case BinaryOperation.And: return (mLeft.Compute(interp) != 0) && (mRight.Compute(interp) != 0) ? 1 : 0;
				case BinaryOperation.Or: return (mLeft.Compute(interp) != 0) || (mRight.Compute(interp) != 0) ? 1 : 0;
				case BinaryOperation.Equals: return mLeft.Compute(interp) == mRight.Compute(interp) ? 1 : 0;
				case BinaryOperation.GreaterThan: return mLeft.Compute(interp) > mRight.Compute(interp) ? 1 : 0;
				case BinaryOperation.LessThan: return mLeft.Compute(interp) < mRight.Compute(interp) ? 1 : 0;
				default: return 0;
			}
		}
	}
	
	/// <summary>
	/// Implementation of all unary operations
	/// </summary>
	public class UnaryOperationExpression : Expression {
		UnaryOperation mOp;
		Expression mExpr;
		
		public UnaryOperationExpression(UnaryOperation op, Expression exp) {
			mOp = op;
			mExpr = exp;
		}
		
		override public int Compute(Interpreter interp) {
			switch(mOp) {
				case UnaryOperation.Negate: return -mExpr.Compute(interp);
				case UnaryOperation.Complement: return mExpr.Compute(interp) != 0 ? 0 : 1;
				default: return 0;
			}
		}
	}
	
	
	
}

