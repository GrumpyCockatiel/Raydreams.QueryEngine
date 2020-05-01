using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Raydreams.QueryEngine
{
	/// <summary>This is just a stub class that will hopefully lead to a better class to manage custom control state.  See the SaveControlState and LoadControlState methods in custom controls.</summary>
	[Serializable()]
	[XmlRoot("Query")]
	public class Statement : IXmlSerializable
	{
		#region [Fields]

		private List<QueryEngine.Lexem> _prelexs = null;
		private List<QueryEngine.Lexem> _postlexs = null;
		private SchemaTree _schema = null;
		private List<Collections.Pair<QueryEngine.Lexem>> _parens = null;
		private string _sql = null;
		private string _name = null;
		private string _cai = null;
		private bool _isPublic = true;
		private Dictionary<string, int> _markers = null;

		#endregion [Fields]

		#region [Constructor]

		/// <summary>Constructor</summary>
		public Statement()
		{ }

		#endregion [Constructor]

		#region [Properties]

		/// <summary>Gets or sets whether the query is public or not.</summary>
		public bool IsPublic
		{
			get { return this._isPublic; }
			set { this._isPublic = value; }
		}

		/// <summary>Gets or sets the descriptive name of the query.</summary>
		public string Name
		{
			get { return this._name; }
			set { this._name = value; }
		}

		/// <summary>Gets or sets the CAI of the user who owns the query.</summary>
		public string Cai
		{
			get { return this._cai; }
			set { this._cai = value; }
		}

		/// <summary>Gets or sets a collection of pointers into the string query.</summary>
		public Dictionary<string, int> QueryMarkers
		{
			get { return this._markers; }
			set { this._markers = value; }
		}

		/// <summary>Gets or sets the schema tree referenced by the attributes in the list of lexems.</summary>
		public SchemaTree SchemaTree
		{
			get { return this._schema; }
			set { this._schema = value; }
		}

		/// <summary>Gets or sets the list of lexems in this statement.</summary>
		public List<Lexem> RawQuery
		{
			get { return this._prelexs; }
			set { this._prelexs = value; }
		}

		/// <summary>Gets or sets the list of lexems once they are parsed into a postfix notation.</summary>
		public List<Lexem> ParsedQuery
		{
			get { return this._postlexs; }
			set { this._postlexs = value; }
		}

		/// <summary>Gets or sets the SQL query created from the lexem list.</summary>
		public string SqlQuery
		{
			get { return this._sql; }
			set { this._sql = value; }
		}

		/// <summary>Gets or sets the list of parenthesis pairs.</summary>
		public List<Collections.Pair<Lexem>> Parenthesis
		{
			get { return this._parens; }
			set { this._parens = value; }
		}

		/// <summary>Gets the depth of the deepest attribute used in the query.</summary>
		public int QueryDepth
		{
			get
			{
				// init depth
				int depth = 0;

				foreach ( Lexem lex in this.RawQuery )
				{
					if ( lex is FilterExpression )
					{
						if ( ( (FilterExpression)lex ).Attribute.Depth > depth )
							depth = ( (FilterExpression)lex ).Attribute.Depth;
					}
				}

				return depth;
			}
		}

		#endregion [Properties]

		#region [Methods]

		/// <summary>Get an escaped version of the SQL statement.</summary>
		protected static string EscapeSql( string str )
		{
			return str.Replace( @"'", @"''" );
		}

		/// <summary>Remove escape characters from the specified SQL statement.</summary>
		protected static string UnescapeSql( string sql )
		{
			return sql.Replace( @"''", @"'" );
		}

		/// <summary>Serialize this instance to XML.</summary>
		public virtual void WriteXml( XmlWriter writer )
		{
			// add the application this query is for
			writer.WriteStartAttribute( "application" );
			writer.WriteString( this.SchemaTree.Application );
			writer.WriteEndAttribute();

			// add the produced SQL query if you don't want to have to reparse
			writer.WriteStartElement( "Sql" );
			writer.WriteString( EscapeSql(this.SqlQuery) );
			writer.WriteEndElement();

			// write the collection of unparsed lexems
			writer.WriteStartElement( "Lexems" );
			
			foreach ( QueryEngine.Lexem lex in this._prelexs )
			{
				Type type = lex.GetType();
				object[] attr = type.GetCustomAttributes( true );
				//System.Xml.Serialization.XmlRootAttribute
				string rootName = type.Name;

				writer.WriteStartElement( rootName );
				lex.WriteXml( writer );
				writer.WriteEndElement();
			}
			
			writer.WriteEndElement();
		}

		/// <summary>Create from XML.</summary>
		public virtual void ReadXml( XmlReader reader )
		{
			reader.MoveToContent();

			// see if there is an app schema associated with this query
			// DOES NOT generate the whole schema tree, just the name, reassociate the tree with property using the name
			string schemaName = reader.GetAttribute( "application" );
			if ( !String.IsNullOrEmpty( schemaName ) )
			{
				this.SchemaTree = new SchemaTree( null );
				this.SchemaTree.Application = schemaName;
			}

			// move to SQL node
			reader.ReadToDescendant("Sql");
			this.SqlQuery = UnescapeSql(reader.ReadElementContentAsString());

			// Should be on Lexems now
			if ( reader.Name == "Lexems" && !reader.IsEmptyElement )
			{
				this.RawQuery = new List<Raydreams.QueryEngine.Lexem>( 1 );
			}

			// read each lexem
			while ( reader.Read() )
			{
				if ( reader.NodeType == XmlNodeType.Element )
				{
					if ( reader.Name == "Lexem" )
					{
						// create a new lexem holder object
						QueryEngine.Lexem lex = new QueryEngine.Lexem(QueryEngine.TokenType.Null);
						lex.ReadXml( reader );

						// add to the lexem collection
						if ( lex.Token.Type != QueryEngine.TokenType.Null )
							this.RawQuery.Add( lex );
					}
					else if ( reader.Name == "FilterExpression" )
					{
						// create a new lexem holder object
						QueryEngine.FilterExpression lex = new QueryEngine.FilterExpression( null, null, null );
						lex.ReadXml( reader );

						// add to the lexem collection
						if ( lex.Attribute != null && lex.Operator != null && lex.Operand != null )
							this.RawQuery.Add( lex );
					}
				}
			}

			reader.Read();
			reader.Read();

			// deserialize the lexems
			//reader.ReadToFollowing( "Lexems" );

			//// read each child lexem or filter expression
			//XmlReader inner = reader.ReadSubtree();
			//while ( inner.Read() )
			//{
			//    int x = 5;
			//}
		}

		/// <summary>Schema info for this instance.</summary>
		public virtual XmlSchema GetSchema()
		{
			return ( null );
		}

		#endregion [Methods]
	}
}
