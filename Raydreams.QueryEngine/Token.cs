using System;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

namespace Raydreams.QueryEngine
{
	/// <summary>Types of tokens.</summary>
	public enum TokenCategory : byte
	{
		/// <summary>Token is a member of all other tokens.</summary>
		Miscellaneous = 0,
		/// <summary>Token is a boolean operator.</summary>
		BoolOperator,
		/// <summary>Token is a comparison operator.</summary>
		CompareOperator,
		/// <summary>Token is an expression.</summary>
		Expression,
		/// <summary>Token is an association symbol.</summary>
		Association
	}

	/// <summary>Types of lexems.</summary>
	public enum TokenType : byte
	{
		/// <summary>The token is unknown.</summary>
		Null = 0,
		/// <summary>The token is the OR operator.</summary>
		Or = 1,
		/// <summary>The token is the AND operator.</summary>
		And = 2,
		/// <summary>The token is the NOT operator.</summary>
		Not = 3,
		/// <summary>The token is the left explicit association symbol.</summary>
		LeftParenthesis = 8,
		/// <summary>The token is the right explicit association symbol.</summary>
		RightParenthesis = 9,
		/// <summary>The token is an expression.</summary>
		Expression = 16
	}

	/// <summary>A keyed collection of Token objects.</summary>
	internal class TokenCollection : KeyedCollection<int, Token>
	{
		public TokenCollection() : base() { }

		protected override int GetKeyForItem( Token item )
		{
			return (int)item.Type;
		}
	}

	/// <summary>Encapsulates the metadata for each type of token.  Each instance of a lexem should be associated with a token.</summary>
	[Serializable()]
	public class Token : IFormattable
	{
		#region [Fields]

		private static readonly TokenCollection _list = null;

		private TokenCategory _category = TokenCategory.Miscellaneous;
		private StringCollection _symbols = null;
		private int _precedence = 0;
		private TokenType _type = TokenType.Null;

		#endregion [Fields]

		#region [Constructors]

		/// <summary>Static constructor</summary>
		/// <remarks>Remember that this list is re-created on each postback and instances of the same type may have a different hash code than previously (i.e. don't depend on the hash code for referential equivalence).  All token instances are created statically.</remarks>
		static Token()
		{
			// construct the static collection
			_list = new TokenCollection();
			_list.Add( new Token( TokenCategory.BoolOperator, TokenType.And, 2, "and", "x" ) );
			_list.Add( new Token( TokenCategory.BoolOperator, TokenType.Or, 1, "or", "+" ) );
			_list.Add( new Token( TokenCategory.BoolOperator, TokenType.Not, 3, "not", "!" ) );
			_list.Add( new Token( TokenCategory.Association, TokenType.RightParenthesis, 9, ")", "]" ) );
			_list.Add( new Token( TokenCategory.Association, TokenType.LeftParenthesis, 8, "(", "[" ) );
			_list.Add( new Token( TokenCategory.Expression, TokenType.Expression, 16 ) );
			_list.Add( new Token( TokenCategory.Miscellaneous, TokenType.Null, 0, "null" ) );
		}

		/// <summary>Private Constructor</summary>
		private Token( TokenCategory cat, TokenType t, int p )
			: this( cat, t, p, null )
		{ }

		/// <summary>Private Constructor</summary>
		private Token( TokenCategory cat, TokenType t, int p, params string[] syms )
		{
			this._category = cat;
			this._precedence = p;
			this._type = t;

			if ( syms != null )
			{
				this._symbols = new StringCollection();

				foreach ( string s in syms )
					_symbols.Add( s.Trim() );
			}
		}

		#endregion [Constructors]

		#region [Properties]

		/// <summary>Gets the category this token belongs to.</summary>
		public TokenCategory Category
		{
			get { return this._category; }
		}

		/// <summary>Gets this token's type</summary>
		public TokenType Type
		{
			get { return this._type; }
		}

		/// <summary>Gets the default symbol (first one) for this token if any.</summary>
		public string DefaultSymbol
		{
			get { return this.GetSymbol( 0 ); }
		}

		/// <summary>Gets this token's precedence compared to all other tokens.</summary>
		public int Precedence
		{
			get { return this._precedence; }
		}

		#endregion [Properties]

		#region [Methods]

		/// <summary>Get the token by the given type from the static list.</summary>
		public static Token GetToken( TokenType type )
		{
			return _list[(int)type];
		}

		/// <summary>Get the token represented by the string symbol from the static list.</summary>
		public static Token GetToken( string symbol )
		{
			foreach ( Token t in _list)
				if ( t.HasSymbol( symbol ) )
					return t;

			return null;
		}

		/// <summary>Whether or not the instance of a token is represented by the input symbol.</summary>
		public bool HasSymbol( string s )
		{
			if ( this._symbols == null )
				return false;

			return this._symbols.Contains( s );
		}

		/// <summary>Get one of the symbols that represent the token by index.</summary>
		public static string GetSymbol( TokenType t, int i )
		{
			return GetToken( t ).GetSymbol( i );
		}

		/// <summary>Get one of the symbols that represent the token by index.</summary>
		public string GetSymbol( int i )
		{
			if ( this._symbols == null || i < 0 || i > this._symbols.Count - 1 )
				return null;

			return this._symbols[i];
		}

		/// <summary>Format the token as a string</summary>
		/// <param name="format">String format.  Use "t" to return the enumeration type name or "s" for the token's first symbol (usually the default).</param>
		public string ToString( string format, IFormatProvider formatProvider )
		{
			format = format.Trim().ToLower();

			if ( format == "s" )
				return this.GetSymbol( 0 );

			// return the type numeration name as a string
			return Enum.Format( typeof( TokenType ), this, "d" );

			// unrecognized format
			// throw new FormatException( String.Format( "Invalid format string: '{0}'.", format ) );
		}

		#endregion [Methods]

		#region [Operators]

		/// <summary>Token comparison is by precedence.</summary>
		public static bool operator >( Token a, Token b )
		{ return a.Precedence > b.Precedence; }

		/// <summary>Token comparison is by precedence.</summary>
		public static bool operator <( Token a, Token b )
		{ return a.Precedence < b.Precedence; }

		/// <summary>Tokens with the same type are equivalent.</summary>
		public static bool operator ==( Token a, Token b )
		{
			return a.Type == b.Type;
		}

		/// <summary>Tokens of different types are not equivalent.</summary>
		public static bool operator !=( Token a, Token b )
		{ return !(a.Type == b.Type); }

		/// <summary>Equals should still be referential, not value, which this is.</summary>
		public override bool Equals( object obj )
		{
			if ( obj is Token )
				return (this == (Token)obj);
			else
				return false;
		}

		/// <summary>No change.</summary>
		/// <remarks>Should override and return Hash(attributes).</remarks>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion [Operators]

	}
}
