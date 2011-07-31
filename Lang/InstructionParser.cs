using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RoboLogo.Lang {
	
	public class InstructionParser {
		public Action<int> setColorAction;
		public Action<int> setThicknessAction;
		public Action<int> setStrokeAction;
		public Action<int> moveAction;

		enum TokenType { Expression, Keyword }

		delegate State State(Token t);
		
		struct Token {
			public TokenType type;
			public string data;
		}

		TextReader mReader;
		StringBuilder mBuilder = new StringBuilder();
		State mState;
		List<Instruction> mScratchpad;
		ExpressionParser mExpParser;
		static readonly string[] sPalette = { "red", "green", "blue" };
		
		public InstructionParser() : this(new ExpressionParser()) {
		}
		
		public InstructionParser(ExpressionParser exp) {
			mExpParser = exp;
		}
		
		public Instruction[] Parse(string src) {
			return Parse(new StringReader(src));
		}
		
		public Instruction[] Parse(TextReader reader) {
			Token token;
			mReader = reader;
			mScratchpad = new List<Instruction>();
			mState = Idle;
			while(mState != null && ReadNextToken(out token)) {
				mState = mState(token);
				if (mState == null) { 
					mReader = null;
					mBuilder.Length = 0;
					mScratchpad.Clear();
					return null; 
				}
			}
			mReader = null;
			var result = mScratchpad.ToArray();
			mBuilder.Length = 0;
			mScratchpad.Clear();
			return result;
		}
		
		//---------------------------------------------------------------------
		// STATES
		//---------------------------------------------------------------------
		
		State Idle(Token t) {
			if (t.type == TokenType.Keyword) {
				switch(t.data) {
					case "set": return SawSet;
					case "start": return SawStart;
					case "stop": return SawStop;
					case "move": return SawMove;
					default: return null;
				}
			}
			return null;
		}
		
		State SawSet(Token t) {
			if (t.type == TokenType.Keyword) {
				switch(t.data) {
					case "color": return SawColor;
					case "thickness": return SawThickness;
				}
			}
			return null;
		}
		
		State SawColor(Token t) {
			if (t.type == TokenType.Keyword && t.data == "to") {
				return ExpectingColor;
			}
			return null;
		}
		
		State ExpectingColor(Token t) {
			if (t.type == TokenType.Keyword) {
				for(int i=0; i<sPalette.Length; ++i) {
					if (sPalette[i] == t.data) {
						mScratchpad.Add(new ActionInstruction(setColorAction, new LiteralExpression(i)));
						return Idle;
					}
				}
			} else {
				var exp = mExpParser.Parse(t.data);
				if (exp == null) { return null; }
				mScratchpad.Add(new ActionInstruction(setColorAction, exp));
				return Idle;
			}
			return null;
		}
		
		State SawThickness(Token t) {
			if (t.type == TokenType.Keyword && t.data == "to") {
				return ExpectingThickness;
			}
			return null;
		}
		
		State ExpectingThickness(Token t) {
			if (t.type == TokenType.Expression) {
				var exp = mExpParser.Parse(t.data);
				if (exp == null) { return null; }
				mScratchpad.Add(new ActionInstruction(setThicknessAction, exp));
				return Idle;
			}
			return null;
		}
		
		State SawStart(Token t) {
			if (t.type == TokenType.Keyword && t.data == "stroke") {
				mScratchpad.Add(new ActionInstruction(setStrokeAction, new LiteralExpression(1)));
				return Idle;
			}
			return null;
		}
		
		State SawStop(Token t) {
			if (t.type == TokenType.Keyword && t.data == "stroke") {
				mScratchpad.Add(new ActionInstruction(setStrokeAction, new LiteralExpression(0)));
				return Idle;
			}
			return null;
		}
		
		State SawMove(Token t) {
			if (t.type == TokenType.Keyword) {
				switch(t.data) {
					case "forward": return ExpectingForwardMove;
					case "backward": return ExpectingBackwardMove;
				}
			}
			return null;
		}
		
		State ExpectingForwardMove(Token t) {
			if (t.type == TokenType.Expression) {
				var exp = mExpParser.Parse(t.data);
				if (exp == null) { return null; }
				mScratchpad.Add(new ActionInstruction(moveAction, exp));
				return Idle;
			}
			return null;
		}
		
		State ExpectingBackwardMove(Token t) {
			if (t.type == TokenType.Expression) {
				var exp = mExpParser.Parse(t.data);
				if (exp == null) { return null; }
				exp = new UnaryOperationExpression(UnaryOperation.Negate, exp);
				mScratchpad.Add(new ActionInstruction(moveAction, exp));
				return Idle;
			}
			return null;
		}

		//---------------------------------------------------------------------
		// TOKENIZATION
		//---------------------------------------------------------------------
		
		bool ReadNextToken(out Token token) {
			mBuilder.Length = 0;
			char letter;
			token = new Token();
			// go passed whitespace characters
			do {
				if (!ReadNextChar(out letter)) { return false; }
			} while(char.IsWhiteSpace(letter));
			// what is next?
			if (letter == '(') {
				// reading an expression
				int pCount = 1;
				while(pCount > 0) {
					if (!ReadNextChar(out letter)) { return false; }
					if (letter == ')') { pCount--; }
					if (pCount > 0) {
						if (letter == '(') { ++pCount; }
						mBuilder.Append(letter);
					}
				}
				token.type = TokenType.Expression;
				token.data = mBuilder.ToString();
				return true;
			} else if (char.IsLetter(letter) || char.IsNumber(letter)) {
				if (char.IsNumber(letter)) {
					// reading a numeric literal
					token.type = TokenType.Expression;
					mBuilder.Append(letter);
					PeekNextChar(out letter);
					while(char.IsNumber(letter)) {
						if (!ReadNextChar(out letter)) {
							token.data = mBuilder.ToString();
							return true;
						}
						mBuilder.Append(letter);
					}
					token.data = mBuilder.ToString();
					return true;
				} else if (char.IsUpper(letter)) {
					// reading a variable name
					token.type = TokenType.Expression;
					PullAnotherWord:
					while(!char.IsWhiteSpace(letter)) {
						mBuilder.Append(letter);
						if (!ReadNextChar(out letter)) {
							goto BailOutOfVariableName;
						}
					}
					if (letter == ' ' && PeekNextChar(out letter) && char.IsLetter(letter) && char.IsUpper(letter)) {
						ReadNextChar(out letter);
						goto PullAnotherWord;
					}
					BailOutOfVariableName:
					token.data = mBuilder.ToString();
					return true;
				} else {
					// reading a keyword
					token.type = TokenType.Keyword;
					while(!char.IsWhiteSpace(letter)) {
						mBuilder.Append(letter);
						if (!ReadNextChar(out letter)) {
							goto BailOutOfKeyword;
						}
					}
					BailOutOfKeyword:
					token.data = mBuilder.ToString();
					return true;
				}
			}
			// found something weird, stop tokenizing
			return false;
		}
		
		//---------------------------------------------------------------------
		// FILTERING
		//---------------------------------------------------------------------
		
		// TODO: treat comment as "whitespace"
		
		bool PeekNextChar(out char c) {
			int next = mReader.Peek();
			if (next == -1) {
				c = '\n';
				return false;
			}
			c = Convert.ToChar(next);
			// ignore comments
			while(c == '"') {
				do {
					mReader.Read();
					next = mReader.Read();
					if (next == -1) {
						c = '\n';
						return false;
					}
					c = Convert.ToChar(next);
				} while(c != '"');
				next = mReader.Peek();
				if (next == -1) {
					c = '\n';
					return false;
				}
				c = Convert.ToChar(next);
			}
			return true;
		}
		
		bool ReadNextChar(out char c) {
			int next = mReader.Read();
			if (next == -1) {
				c = '\n';
				return false;
			}
			c = Convert.ToChar(next);
			// ignore comments
			while(c == '"') {
				do {
					next = mReader.Read();
					if (next == -1) {
						c = '\n';
						return false;
					}
					c = Convert.ToChar(next);
				} while(c != '"');
				next = mReader.Read();
				if (next == -1) {
					c = '\n';
					return false;
				}
			}
			return true;
		}
	}
}

