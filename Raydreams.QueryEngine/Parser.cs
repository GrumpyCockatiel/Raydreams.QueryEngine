using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Raydreams.QueryEngine
{
	/// <summary>Parses an input SQL Statement into a parse tree and reforms it as SQL text.</summary>
	/// <remarks>Consult Wikipedia entries for RPN http://en.wikipedia.org/wiki/Reverse_Polish_notation and Shunting Yard http://en.wikipedia.org/wiki/Shunting_yard_algorithm for algorithm details.  Possibly build this functionality into the Statement Class or make a singleton class.  Certainly could use a lot of work.</remarks>
	public class Parser
	{
		private delegate string Sql( List<Lexem> inList );

		#region [Fields]
		
		private static Dictionary<JoinType, string> _joinStrs = null;
		private Dictionary<string, int> _markers = new Dictionary<string, int>();

		/// <summary>Private delegate for converting the parsed list of lexems to SQL query.</summary>
		private Sql _sql = null;
		
		#endregion [Fields]

		#region [Constructors]

		/// <summary>Static Constructor.</summary>
		static Parser()
		{
			_joinStrs = new Dictionary<JoinType, string>();
			_joinStrs.Add( JoinType.Inner, "JOIN" );
			_joinStrs.Add( JoinType.FullOuter, "FULL OUTER JOIN" );
			_joinStrs.Add( JoinType.LeftOuter, "LEFT OUTER JOIN" );
			_joinStrs.Add( JoinType.RightOuter, "RIGHT OUTER JOIN" );
		}

		#endregion [Constructors]

		#region [Properties]

		/// <summary>Gets or sets a collection of pointers into the string query.</summary>
		public Dictionary<string, int> Markers
		{
			get { return this._markers; }
			set { this._markers = value; }
		}

		#endregion [Properties]

		#region [Methods]

		/// <summary>Gets a marker by the string key.</summary>
		public int GetMarkerByKey( string key )
		{
			if ( this._markers == null )
				return -1;

			if (this._markers.ContainsKey( key ) )
				return this._markers[key];

			return -1;
		}

		/// <summary>Parse an input infix expression using the shunting yard algorithm.</summary>
		/// <param name="inList">List of lexems to parse in infix notation.</param>
		/// <returns>Returns a parsed list of lexems in prefix notation.</returns>
		public List<Lexem> Parse( List<Lexem> inList )
		{
			if ( inList == null || inList.Count < 1 )
				return null;

			// create a queue to hold the statement in postfix order
			List<Lexem> outList = new List<Lexem>( inList.Count / 2 );

			// create stack to hold operators and parenthesis
			Stack<Lexem> opStack = new Stack<Lexem>();

			// iterate the input list and perform the shunting yard algorithm
			foreach ( Lexem current in inList )
			{
				//switch ( current.GetType().Name )
				//{
				//    case "FilterExpression":
				//        break;

				//    default:
				//        break;
				//}

				// add expressions directly to the output
				if ( current.Token.Type == TokenType.Expression )
					outList.Add( current );

				// put operators on the stack
				else if ( current.Token.Type == TokenType.And || current.Token.Type == TokenType.Or )
				{
					// push on to the operator stack if
					// operator stack is empty
					// the top of the stack is a left parenthesis
					// the operator on the top of the stack has less priority than this operator
					if ( opStack.Count < 1 || opStack.Peek().Token.Type == TokenType.LeftParenthesis 
						|| opStack.Peek().Token < current.Token )
						opStack.Push( current );
					// pop operators with higher precedence
					else
					{
						while ( opStack.Count > 0 && opStack.Peek().Token > current.Token )
							//outList.Enqueue( opStack.Pop() );
							outList.Add( opStack.Pop() );

						opStack.Push( current );
					}
				}

				// push left parenthesis
				else if ( current.Token.Type == TokenType.LeftParenthesis )
					opStack.Push( current );

				// on right parenthesis, pop all operators until a left parenthesis
				else if ( current.Token.Type == TokenType.RightParenthesis )
				{
					Lexem popped = opStack.Pop();

					while ( popped.Token.Type != TokenType.LeftParenthesis )
					{
						//outList.Enqueue( popped );
						outList.Add( popped );
						popped = opStack.Pop();
					}
				}
				else
					//outList.Enqueue( current );
					outList.Add( current );

			} // end foreach lexem

			// pop remaing items off the stack
			while ( opStack.Count > 0 )
				//outList.Enqueue( opStack.Pop() );
				outList.Add( opStack.Pop() );

			// put the outlist into a parse tree

			return outList;
		}

		/// <summary>Builds the SQL string from a list of lexems.</summary>
		/// <param name="inList">List of parsed lexems in prefix notation.</param>
		/// <returns>Corresponding SQL statement.</returns>
		public string ConstructSql( List<Lexem> inList )
		{
			// set which SQL Construction routine to use
			this._sql = new Sql( BuildSql1 );
			return this._sql( inList );
		}

		/// <summary>Original SQL constructor method.</summary>
		private string BuildSql1( List<Lexem> inList )
		{
			if ( inList == null || inList.Count < 1 )
				return null;

			// keep a reference to the top most root node
			string rootNode = null;

			// iterate the list and extract all the expressions
			StringCollection tables = new StringCollection();
			StringCollection joins = new StringCollection();
			Stack<string> conditions = new Stack<string>();

			foreach ( Lexem current in inList )
			{
				// deal with filter expressions
				if ( current is FilterExpression )
				{
					FilterExpression exp = current as FilterExpression;

					// get the root node once
					if ( String.IsNullOrEmpty( rootNode ) )
						rootNode = exp.Attribute.Root.DataSource;

					// add all table names to the collection
					Entity curPar = exp.Attribute.Parent;
					while ( curPar != null )
					{
						if ( curPar.DataSource != null && !tables.Contains( curPar.DataSource ) )
							tables.Add( curPar.DataSource );

						curPar = curPar.Parent;
					}

					// construct join strings
					curPar = exp.Attribute.Parent;
					while ( curPar != null && !curPar.IsRoot() )
                    {
						// get all keys
						int fieldNum = curPar.ParentIdFields.Length;

						// last chance to handle a field num mismatch - should be delt with before we ever get here
						if ( curPar.ChildIdFields.Length != fieldNum )
							throw new System.Exception("There is a mismatch in the number of fields defining the parent and child. Double check the XML.");

						// only need to loop for composite keys, most will only do this loop once
						for ( int i = 0; i < fieldNum; ++i )
						{
							string join = String.Format( "{0}.{1} = ", curPar.DataSource, curPar.ChildIdFields[i] );
							join += String.Format( "{0}.{1}", curPar.Parent.DataSource, curPar.ParentIdFields[i] );
							joins.Add( join );
						}
						
						// traverse up the tree
						curPar = curPar.Parent;
					}

					// get the SQL verson of the filter and push it onto the stack
					conditions.Push( exp.ToSql() );
				}
				else if ( current.Token.Type == TokenType.And || current.Token.Type == TokenType.Or )
				{
					string expJoin = String.Format( "( {0} {1} {2} )", conditions.Pop(), current.Token.GetSymbol( 0 ).ToUpper(), conditions.Pop() );
					conditions.Push( expJoin );
				}
				else
					continue;
			}

			// put the SQL statement together

			// add the header part
			StringBuilder sql = new StringBuilder();
			sql.AppendFormat( "SELECT DISTINCT {0}.* FROM ", rootNode );

			string separator = ", ";
			foreach (string s in tables)
				sql.AppendFormat( "{0}{1}", s, separator );
			
			sql.Length -= separator.Length;

			sql.Append(" WHERE ");

			// add any joins to parent tables if there are any
			if ( joins.Count > 0 )
			{
				separator = " AND ";
				foreach ( string s in joins )
					sql.AppendFormat( "{0}{1}", s, separator );
			}

			// mark the start of the filters
			this._markers.Add( "FILTER_START", sql.Length );

			// add field conditionals
			foreach ( string s in conditions )
				sql.AppendFormat( " {0} ", s);
			
			// close out and return the statement
			return sql.ToString().Trim();
		}

		/// <summary>SQL constructor method.</summary>
		/// <remarks>Trying to use INNER JOIN syntax on this version so can switch between INNER, OUTER and FULL.</remarks>
		private string BuildSql2( List<Lexem> inList)
		{
			// validate the input list
			if ( inList == null || inList.Count < 1 )
				return null;

			// keep a reference to the top most root node for convenience
			string rootNode = null;

			// setup state stacks and strings to build upon
			Stack<string> filters = new Stack<string>();
			List<Entity> tables = new List<Entity>();

			// now iterate the lexems and build up the state stacks correctly
			foreach ( Lexem current in inList )
			{
				// deal with filter expressions
				switch ( current.Token.Type )
				{
					case TokenType.Expression:
						#region [Filter Expression]
						{
							// cast the lexem as a Filter Expression and validate
							FilterExpression express = current as FilterExpression;

							if ( express == null )
								continue;

							// get a reference to the root node just once
							if ( String.IsNullOrEmpty( rootNode ) )
								rootNode = express.Attribute.Root.DataSource;

							// create a list of parent entities
							//Entity curPar = express.Attribute.Parent;
							//while ( curPar != null )
							//{
							//    if ( !String.IsNullOrEmpty(curPar.DataSource) && !tables.Keys.Contains( curPar ) )
							//        tables.Add( curPar, false );

							//    curPar = curPar.Parent;
							//}

							// get this attribute's path and create the join, marking each table that is used
							List<Entity> path = express.Attribute.Path;

							// add each entity to the tables list once
							foreach ( Entity e in express.Attribute.Path )
							{
								if ( !tables.Contains( e ) )
								{
									tables.Add( e );
								}
							}

							//foreach ( Entity e in path )
							//{
							//    // if e is already used, skip it

							//    // else, do a join string

							//}

							// get the SQL verson of the filter and push it onto the stack
							filters.Push( express.ToSql() );
						}
						#endregion [Filter Expression]
						break;

					case TokenType.And:
					case TokenType.Or:
						#region [Boolean Token]
						{
							// join to filters and push them back on the stack
							string expJoin = String.Format( "( {0} {1} {2} )", 
								filters.Pop(), current.Token.GetSymbol( 0 ).ToUpper(), filters.Pop() );
							
							filters.Push( expJoin );
						}
						#endregion [Boolean Token]
						break;

					// ignore any unknown tokens
					default:
						break;
				}
			}

			// finally, construct the SQL string
			//StringBuilder sql = new StringBuilder();
			//sql.AppendFormat( "SELECT DISTINCT {0}.* FROM ", rootNode );

			//foreach ( KeyValuePair<Entity, bool> pair in tables )
			//    sql.AppendFormat( "{0}", pair.Key.DataSource );

			//return sql.ToString();

			// build the SQL
			SqlStatement sql = new SqlStatement();
			sql.SelectClause = String.Format( "DISTINCT {0}.*", rootNode);

			// add joins


			// add filters
			sql.WhereClause = String.Join( " ", filters.ToArray() );

			return sql.Query;
		}

		/// <summary>Generates a SELECT [fields] part of query from the specified input list of attributes.</summary>
		public string BuildSelectFields( List<Attribute> inList )
		{
			// validate the input list
			if ( inList == null || inList.Count < 1 )
				return null;

			List<string> fields = new List<string>();

			// iterate each field
			foreach ( Attribute attr in inList )
			{
				fields.Add( String.Format( "{0}.{1}", attr.Parent.DataSource, attr.DataSource ) );
			}

			return String.Join( ",", fields.ToArray() );
		}

		/// <summary></summary>
		public string BuildSelectWithNoFilters( List<Attribute> inList )
		{
			// validate the input list
			if ( inList == null || inList.Count < 1 )
				return null;

			StringCollection fields = new StringCollection();
			StringCollection joins = new StringCollection();
			StringCollection tables = new StringCollection();

			// iterate all the attributes
			foreach ( Attribute attr in inList )
			{
				Entity curPar = attr.Parent;

				// get all the table names above this attribute
				while ( curPar != null )
				{
					if ( curPar.DataSource != null && !tables.Contains( curPar.DataSource ) )
						tables.Add( curPar.DataSource );

					curPar = curPar.Parent;
				}

				// construct join strings
				curPar = attr.Parent;
				
				while ( curPar != null && !curPar.IsRoot() )
				{
					string join = String.Format( "{0}.{1} = ", curPar.DataSource, curPar.FkField );
					join += String.Format( "{0}.{1}", curPar.Parent.DataSource, curPar.ParentIdField );
					joins.Add( join );

					curPar = curPar.Parent;
				}

				// finally add the fields
				fields.Add( String.Format( "{0}.{1} {2}", attr.Parent.DataSource, attr.DataSource, attr.DisplayText ) );
			}

			// build the sql
            // select 
            string separator = ", ";
            StringBuilder sql = new StringBuilder();
			StringBuilder sqlSelect = new StringBuilder();
            foreach (string s in fields)
                sqlSelect.AppendFormat("{0}{1}", s, separator );

            sql.AppendFormat("SELECT DISTINCT {0} ", sqlSelect );

            //from
            StringBuilder sqlFrom = new StringBuilder();
            foreach (string s in tables)
                sqlFrom.AppendFormat("{0}{1}", s, separator);

            sql.AppendFormat("FROM {0} ", sqlFrom); 

            //where 
            if (joins.Count > 0)
            {
                StringBuilder sqlWhere = new StringBuilder();
                separator = " AND ";
                foreach (string s in joins)
                    sqlWhere.AppendFormat("{0}{1}", s, separator);

                sql.AppendFormat("WHERE {0}", sqlWhere );
            }
            

            
            //sql.AppendFormat( "SELECT DISTINCT {0} FROM ", inList[0].Root.DataSource );

			
            //foreach ( string s in tables )
            //    sql.AppendFormat( "{0}{1}", s, separator );

            //sql.Length -= separator.Length;

            //sql.Append( " WHERE " );

            //// add any joins to parent tables if there are any
            //if ( joins.Count > 0 )
            //{
            //    separator = " AND ";
            //    foreach ( string s in joins )
            //        sql.AppendFormat( "{0}{1}", s, separator );
            //}

			// close out and return the statement
			return sql.ToString().Trim();
		}

        /// <summary></summary>
        public string BuildSelectWithFilters(List<Attribute> inList, string searchSql)
        {
            // validate the input list
            if (inList == null || inList.Count < 1)
                return null;

            StringCollection fields = new StringCollection();
            StringCollection joins = new StringCollection();
            StringCollection tables = new StringCollection();

            //parse search sql
            ParseJoinsFromSql(ref joins, ref tables, searchSql);
            

            // iterate all the attributes
            foreach (Attribute attr in inList)
            {
                Entity curPar = attr.Parent;

                // get all the table names above this attribute
                while (curPar != null)
                {
                    if (curPar.DataSource != null && !tables.Contains(curPar.DataSource))
                        tables.Add(curPar.DataSource);

                    curPar = curPar.Parent;
                }

                // construct join strings
                curPar = attr.Parent;

                while (curPar != null && !curPar.IsRoot())
                {
                    string join = String.Format("{0}.{1} = ", curPar.DataSource, curPar.FkField);
                    join += String.Format("{0}.{1}", curPar.Parent.DataSource, curPar.ParentIdField);
                    joins.Add(join);

                    curPar = curPar.Parent;
                }

                // finally add the fields
                //string x = @"";
                
                fields.Add(String.Format("{0}.{1} {2}", attr.Parent.DataSource, attr.DataSource, @"""" + attr.DisplayText + @""""));
                //fields.Add(String.Format("{0}.{1}", attr.Parent.DataSource, attr.DataSource));
            }

            // build the sql
            // select 
            string separator = ", ";
            StringBuilder sql = new StringBuilder();
            string sqlSelect = string.Empty;
            string[] sSelect = new string[fields.Count];
            fields.CopyTo(sSelect, 0);
            sqlSelect = string.Join(separator, sSelect);
            
            sql.AppendFormat("SELECT DISTINCT {0} ", sqlSelect);

            //from
            string sqlFrom = string.Empty;
            string[] sFrom = new string[tables.Count];
            tables.CopyTo(sFrom, 0);
            sqlFrom = string.Join(separator, sFrom);
            
            sql.AppendFormat("FROM {0} ", sqlFrom);

            //where 
            if (joins.Count > 0)
            {
                string sqlWhere = string.Empty;
                separator = " AND ";
                string[] sWhere = new string[joins.Count];
                joins.CopyTo(sWhere, 0);
                sqlWhere = string.Join(separator, sWhere);
                
                sql.AppendFormat("WHERE {0}", sqlWhere);
            }

            // close out and return the statement
            return sql.ToString().Trim();
        }

        private void ParseJoinsFromSql(ref StringCollection joins, ref StringCollection tables, string searchSql)
        {
            {
                int posFrom, posWhere;
                string From = string.Empty;
                string Where = string.Empty;

                posFrom = searchSql.IndexOf(" FROM ", 0);
                if (posFrom > 0)
                {
                    posWhere = searchSql.IndexOf(" WHERE ", posFrom);
                    if (posWhere > 0)
                    {
                        From = searchSql.Substring(posFrom + 6, (posWhere - (posFrom + 6)));
                        Where = searchSql.Substring(posWhere + 7, (searchSql.Length - posWhere) - 7);

                        //parse tables
                        char[] delimiterCharsTbl = {','};
                        char[] trimChars = { ' ' };
                        string[] tbls = From.Split(delimiterCharsTbl);
                        foreach (string s in tbls)
                            tables.Add(s.Trim(trimChars));

                        //parse join conditions
                        string[] delimiterStrWhr = {" AND "};
                        string[] jns = Where.Split(delimiterStrWhr, System.StringSplitOptions.RemoveEmptyEntries);
                        foreach (string s in jns)
                            joins.Add(s.Trim(trimChars));
                    }
                }
            }
        }

        

        private StringCollection ParseJoinsFromSql(string searchSql)
        {
            throw new Exception("The method or operation is not implemented.");
        }

		#endregion [Methods]
	}
}
