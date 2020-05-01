using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Raydreams.QueryEngine
{

	/// <sumary>The instance of a token or lexem.</sumary>
	/// <remarks>A lexem is an instance of a token.  Token's are categories of lexems.  The lexem can be an operator, a filter expression or grouping token such as a parenthesis. Some tokens are constrained to certain values (symbols) but may have more than one representation (e.g. plus or +) are both symbols for the ADD token.  Other tokens may have a much larger, though still finite set (e.g. VARIABLE may be any number of valid strings though still finite). Consider the boolean data type, with a value domain of true or false, and a lexem domain of {T,F,1,0}.</remarks>
	[Serializable()]
	[XmlRoot( "Lexem" )]
	public class Lexem : IXmlSerializable
	{
		#region [Fields]

		protected string _value = null;
		private Token _tok = QueryEngine.Token.GetToken( TokenType.Null );
		
		#endregion [Fields]

		#region [Constructors]

		/// <summary>Constructor</summary>
		public Lexem( TokenType type, string val )
		{
			this._value = (val == String.Empty) ? null : val;
			this._tok = QueryEngine.Token.GetToken( type );
		}

		/// <summary>Constructor</summary>
		public Lexem( TokenType type ) : this( type, null )
		{ }

		/// <summary>Constructor</summary>
		/// <remarks>Required for XML Serialization.</remarks>
		private Lexem( ) : this( TokenType.Null, null )
		{ }

		#endregion [Constructors]

		#region [Properties]

		/// <summary>Gets or sets the value of this lexem.</summary>
		/// <remarks>A lexem's value should be one of the possible token representations unless the token has none, then it can be any string or a value defined by the subclass.</remarks>
		public string Value
		{
			get { return this._value; }
			set { this._value = ( value == String.Empty ) ? null : value; }
		}

		/// <summary>Gets or sets the token to which this lexem belongs.</summary>
		public Token Token
		{
			get { return this._tok; }
		}

		#endregion [Properties]

		#region [Operators]

		/// <summary></summary>
		public static bool operator >( Lexem a, Lexem b )
		{ return a.Token > b.Token; }

		/// <summary></summary>
		public static bool operator <( Lexem a, Lexem b )
		{ return a.Token < b.Token; }

		#endregion [Operators]

		#region [Methods]

		/// <summary>Serialize this instance to XML.</summary>
		public virtual void WriteXml( XmlWriter writer )
		{
			// write the token type
			writer.WriteStartAttribute( "type" );
			writer.WriteString( Enum.Format(typeof( TokenType ), this.Token.Type, "g") );
			writer.WriteEndAttribute();

			// write the value
			writer.WriteStartAttribute( "value" );
			writer.WriteString( this.Value );
			writer.WriteEndAttribute();
		}

		/// <summary>Create from XML.</summary>
		public virtual void ReadXml( XmlReader reader )
		{
			string typeStr = reader.GetAttribute( "type" );
			this.Value = reader.GetAttribute( "value" );

			TokenType type = (TokenType)Enum.Parse( typeof( TokenType ), typeStr );
			this._tok = Token.GetToken( type );
		}

		/// <summary>Schema info for this instance.</summary>
		public virtual XmlSchema GetSchema()
		{
			return ( null );
		}

		#endregion [Methods]
	}

}
