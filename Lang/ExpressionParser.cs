using System;
using System.Collections.Generic;

namespace RoboLogo.Lang {
	
	/// <summary>
	/// Parses statements into expression trees
	/// </summary>
	public class ExpressionParser {
		enum TokenType { Operator, Element, SubExpression }

		struct Token {
			public TokenType type;
			public string expr;
			public int opId;
			
			public Token(TokenType t, string s) { type = t; expr = s; opId=-1; }
			public Token(int op, string s) { type = TokenType.Operator; expr = s; opId = op; }
		}
		
		Dictionary<string,BinaryOperation> mBinaryOpTable = new Dictionary<string, BinaryOperation>();
		Dictionary<string,UnaryOperation> mUnaryOpTable = new Dictionary<string, UnaryOperation>();
		string[] mOps = { "+", "-", "*", "/", "and", "or", "=", ">", "<", "!" };
		
		public ExpressionParser() {
			mBinaryOpTable.Add("+", BinaryOperation.Add);
			mBinaryOpTable.Add("-", BinaryOperation.Subtract);
			mBinaryOpTable.Add("*", BinaryOperation.Multiply);
			mBinaryOpTable.Add("/", BinaryOperation.Divide);
			mBinaryOpTable.Add("and", BinaryOperation.And);
			mBinaryOpTable.Add("or", BinaryOperation.Or);
			mBinaryOpTable.Add("=", BinaryOperation.Equals);
			mBinaryOpTable.Add(">", BinaryOperation.GreaterThan);
			mBinaryOpTable.Add("<", BinaryOperation.LessThan);
			mUnaryOpTable.Add("-", UnaryOperation.Negate);
			mUnaryOpTable.Add("!", UnaryOperation.Complement);
		}
		
		public Expression Parse(string expr) {
			if (expr.Length > 256) {
				return null;
			}
			
			{ // verify parenthesis consistency
				int pcount = 0;
				for(int i=0; i<expr.Length; ++i) {
					if (expr[i] == '(') {
						pcount++;
					} else if (expr[i] == ')') {
						--pcount;
						if (pcount < 0) { 
							return null;
						}
					}
				}
				if (pcount != 0) { 
					return null;
				}
			}
			
			expr = expr.Trim();
			Expression result;
			if (ParseSubExpression(expr, out result)) {
				return result;
			}
			return null;
		}
		
		//---------------------------------------------------------------------
		// PARSING METHODS
		//---------------------------------------------------------------------
		
		
		bool ParseSubExpression(string expr, out Expression result) {
			List<Token> tokens;
			if (!Tokenize(expr, out tokens)) {
				result = null;
				return false;
			}
			return ParseTokenSlice(tokens, 0, tokens.Count, out result);
		}
		
		bool ParseTokenSlice(List<Token> tokens, int start, int len, out Expression result) {
			result = null;
			if (len == 0) {
				return false;
			} 
			if (len == 1) {
				switch(tokens[start].type) {
					case TokenType.SubExpression: return ParseSubExpression(tokens[start].expr, out result);
					case TokenType.Element: return ParseElement(tokens[start].expr, out result);
					default: return false;
				}
			} 
			int opIndex;
			if (FindHighestPriorityOperator(tokens, start, len, out opIndex)) {
				if (opIndex == start) {
					Expression right;
					if (!ParseTokenSlice(tokens, start+1, len-1, out right)) { return false; }
					result = new UnaryOperationExpression(mUnaryOpTable[mOps[tokens[opIndex].opId]], right);
					return true;
				} else {
					Expression left, right;
					if (!ParseTokenSlice(tokens, start, opIndex-start, out left)) { return false; }
					if (!ParseTokenSlice(tokens, opIndex+1, start + len - opIndex - 1, out right)) { return false; }
					result = new BinaryOperationExpression(mBinaryOpTable[mOps[tokens[opIndex].opId]], left, right);
					return true;
				}
			} else {
				return false;
			}
		}
		
		bool ParseElement(string elem, out Expression result) {
			result = null;
			elem = elem.Trim();
			if (elem.Length == 0) { return false; }
			if (elem[0] == '(') {
				elem = elem.Substring(1, elem.Length-2).Trim();
			}
			int val;
			if (int.TryParse(elem, out val)) {
				result = new LiteralExpression(val);
			} else {
				// verify variable name
				for(int i=0; i<elem.Length; ++i) {
					if (!char.IsUpper(elem[i]) || elem[i] == '_' || char.IsNumber(elem[i])) {
						return false;
					}
				}
				result = new VariableExpression(elem);
			}
			return true;
		}
		
		//---------------------------------------------------------------------
		// HELPER METHODS
		//---------------------------------------------------------------------
		
		bool Tokenize(string expr, out List<Token> result) {
			if (expr[0] == '(' && expr[expr.Length-1] == ')') {
				expr = expr.Substring(1, expr.Length-2);
			}
			int i=0;
			int opIndex = 0;
			int opId = 0;
			result = new List<Token>(expr.Length);
			while(i<expr.Length) {
				if (expr[i] == '(') {
					int pCount = 1;
					int start = i;
					while(pCount > 0) {
						i++;
						if (expr[i] == '(') { pCount++; }
						else if (expr[i] == ')') { pCount--; }
					}
					result.Add(new Token(TokenType.SubExpression, expr.Substring(start, i-start+1).Trim()));
					++i;
				} else if (FindNextOperatorStartingAt(expr, i, out opIndex, out opId)) {
					if (opIndex > i) {
						result.Add(new Token(TokenType.Element, expr.Substring(i, opIndex-i).Trim()));
					}
					int opLen = mOps[opId].Length;
					result.Add(new Token(opId, expr.Substring(opIndex, opLen).Trim()));
					i = opIndex + opLen;
				} else if (i < expr.Length) {
					var str = expr.Substring(i).Trim();
					if (str.Length > 0) {
						result.Add(new Token(TokenType.Element, expr.Substring(i).Trim()));
					}
					i = expr.Length;
				}
			}
			return true;
		}
		
		bool FindNextOperatorStartingAt(string expr, int i, out int result, out int opId) {
			while(i < expr.Length) {
				for(int j=0; j<mOps.Length; ++j) {
					var opToken = mOps[j];
					if (i+opToken.Length < expr.Length && expr.Substring(i, opToken.Length) == opToken) {
						result = i;
						opId = j;
						return true;
					}
				}
				++i;
			}
			result = 0;
			opId = 0;
			return false;
		}
		
		bool FindHighestPriorityOperator(List<Token> tokens, int start, int len, out int tokenIndex) {
			int opEnum = -1;
			tokenIndex = -1;
			for(int i=start; i<start+len; ++i) {
				if (tokens[i].type == TokenType.Operator) {
					int val = 0;
					if (i == 0 || tokens[i-1].type == TokenType.Operator) {
						val = (int) mUnaryOpTable[mOps[tokens[i].opId]];
					} else {
						val = (int) mBinaryOpTable[mOps[tokens[i].opId]];
					}
					if (tokenIndex == -1 || val < opEnum) { 
						opEnum = val;
						tokenIndex = i;
					}
				}
			}
			return tokenIndex != -1;
		}
	}
	
}

