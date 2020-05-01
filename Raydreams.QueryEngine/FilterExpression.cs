using System;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Raydreams.QueryEngine
{
	/// <summary>Encapsulates a SQL filter operation that is created by the UI.</summary>
	[Serializable()]
	[XmlRoot( "FilterExpression" )]
	public class FilterExpression : Lexem
	{
		#region [Fields]

		/// <summary>LHS operand</summary>
		private Attribute _attr = null;
		/// <summary>Operator</summary>
		private FilterOperator _op = null;
		/// <summary>RHS operand</summary>
		private string _operand = null;

		#endregion [Fields]

		#region [Constructors]

		/// <summary>Constructor</summary>
		public FilterExpression( Attribute a, FilterOperatorType type, string rhs ) : this( a, FilterOperator.GetOperator(type), rhs )
		{}

		/// <summary>Constructor</summary>
		public FilterExpression( Attribute a, FilterOperator op, string rhs ) : base (TokenType.Expression)
		{
			this._attr = a;
			this._op = op;
			this._operand = rhs;
		}

		/// <summary>Constructor</summary>
		/// <remarks>Required for XML Serialization.</remarks>
		private FilterExpression() : this(null, null, null)
		{
		}

		#endregion [Constructors]

		#region [Properties]

		/// <summary>Gets or sets the attribute to be filtered.</summary>
		public Attribute Attribute
		{
			get { return this._attr; }
			set { this._attr = value; }
		}

		/// <summary>Set the filter operator.</summary>
		public FilterOperator Operator
		{
			get { return this._op; }
		}

		/// <summary>Set the RHS operand value.</summary>
		/// <remarks>This is a string value, not the operand type.</remarks>
		public string Operand
		{
			get { return this._operand; }
		}

		#endregion [Properties]

		#region [Methods]

		/// <summary>Formats a SQL string from this instance for use with Oracle.</summary>
		/// <remarks>Consider using IFormattable to control formatting.</remarks>
		public string ToSql()
		{
			StringBuilder sql = new StringBuilder();

			// switch on the operator type
			switch ( this.Operator.Value )
			{
					// Greater Than
				case FilterOperatorType.GreaterThan:
					if ( this.Attribute.OperandType is DateTimeValueType )
					{
						DateTime dt = DateTime.Parse( this.Operand );
						sql.AppendFormat( "{0}.{1} > '{2}'", this.Attribute.Parent.DataSource, this.Attribute.DataSource, dt.ToString( "dd-MMM-yyyy" ) );
					}
					else
						sql.AppendFormat( "{0} > {1}", this.Attribute.DataSource, this.Operand );
					break;

				// Less Than
				case FilterOperatorType.LessThan:
					if ( this.Attribute.OperandType is DateTimeValueType )
					{
						DateTime dt = DateTime.Parse( this.Operand );
						sql.AppendFormat( "{0}.{1} < '{2}'", this.Attribute.Parent.DataSource, this.Attribute.DataSource, dt.ToString( "dd-MMM-yyyy" ) );
					}
					else
						sql.AppendFormat( "{0} < {1}", this.Attribute.DataSource, this.Operand );
					break;

				// Greater Than Or Equal
				case FilterOperatorType.GreaterThanOrEqual:
					if ( this.Attribute.OperandType is DateTimeValueType )
					{
						DateTime dt = DateTime.Parse( this.Operand );
						sql.AppendFormat( "{0}.{1} >= '{2}'", this.Attribute.Parent.DataSource, this.Attribute.DataSource, dt.ToString( "dd-mmm-yyyy" ) );
					}
					else
						sql.AppendFormat( "{0} >= {1}", this.Attribute.DataSource, this.Operand );
					break;

				// Less Than Or Equal
				case FilterOperatorType.LessThanOrEqual:
					if ( this.Attribute.OperandType is DateTimeValueType )
					{
						DateTime dt = DateTime.Parse( this.Operand );
						sql.AppendFormat( "{0}.{1} <= '{2}'", this.Attribute.Parent.DataSource, this.Attribute.DataSource, dt.ToString( "dd-MMM-yyyy" ) );
					}
					else
						sql.AppendFormat( "{0} <= {1}", this.Attribute.DataSource, this.Operand );
					break;

				// Equal
				case FilterOperatorType.Equal:
					if ( this.Attribute.OperandType is DateTimeValueType )
					{
						DateTime dt = DateTime.Parse( this.Operand );
						sql.AppendFormat( "{0}.{1} = '{2}'", this.Attribute.Parent.DataSource, this.Attribute.DataSource, dt.ToString( "dd-MMM-yyyy" ) );
					}
					else if ( this.Attribute.OperandType is StringValueType && ( this.Attribute.Options & ElementFlag.NoFunctions ) == ElementFlag.NoFunctions )
					{
						sql.AppendFormat( "{0}.{1} = '{2}'", this.Attribute.Parent.DataSource, this.Attribute.DataSource, this.Operand );
					}
					else if ( this.Attribute.OperandType is StringValueType
				   || this.Attribute.OperandType == null
				   || ( this.Attribute.OperandType is EnumValueType && ( (EnumValueType)this.Attribute.OperandType ).IsItemsString ) )
					{
						sql.AppendFormat( "LOWER({0}.{1}) = LOWER('{2}')", this.Attribute.Parent.DataSource, this.Attribute.DataSource, this.Operand );
					}
					else
						sql.AppendFormat( "{0}.{1} = {2}", this.Attribute.Parent.DataSource, this.Attribute.DataSource, this.Operand );
					break;

				// Inequal
				case FilterOperatorType.Inequal:
					if ( this.Attribute.OperandType is DateTimeValueType )
					{
						DateTime dt = DateTime.Parse( this.Operand );
						sql.AppendFormat( "{0}.{1} <> '{2}'", this.Attribute.Parent.DataSource, this.Attribute.DataSource, dt.ToString( "dd-MMM-yyyy" ) );
					}
					else if ( this.Attribute.OperandType is StringValueType && ( this.Attribute.Options & ElementFlag.NoFunctions ) == ElementFlag.NoFunctions )
					{
						sql.AppendFormat( "{0}.{1} <> '{2}'", this.Attribute.Parent.DataSource, this.Attribute.DataSource, this.Operand );
					}
					else if ( this.Attribute.OperandType is StringValueType
						|| this.Attribute.OperandType == null
						|| ( this.Attribute.OperandType is EnumValueType && ( (EnumValueType)this.Attribute.OperandType ).IsItemsString ) )
					{
						sql.AppendFormat( "LOWER({0}.{1}) <> LOWER('{2}')", this.Attribute.Parent.DataSource, this.Attribute.DataSource, this.Operand );
					}
					else
						sql.AppendFormat( "{0}.{1} <> {0}", this.Attribute.Parent.DataSource, this.Attribute.DataSource, this.Operand );
					break;

				// Contains
				case FilterOperatorType.Contains:
					if ( (this.Attribute.Options & ElementFlag.NoFunctions) == ElementFlag.NoFunctions )
						sql.AppendFormat( "{0}.{1} LIKE '%{2}%' ", this.Attribute.Parent.DataSource, this.Attribute.DataSource, this.Operand );
					else
					sql.AppendFormat( "LOWER({0}.{1}) LIKE '%{2}%' ", this.Attribute.Parent.DataSource, this.Attribute.DataSource, this.Operand.ToLower() );
					break;

				// Begins With
				case FilterOperatorType.BeginsWith:
					if ( (this.Attribute.Options & ElementFlag.NoFunctions) == ElementFlag.NoFunctions )
						sql.AppendFormat( "{0}.{1} LIKE '{2}%' ", this.Attribute.Parent.DataSource, this.Attribute.DataSource, this.Operand );
					else
						sql.AppendFormat( "LOWER({0}.{1}) LIKE '{2}%' ", this.Attribute.Parent.DataSource, this.Attribute.DataSource, this.Operand.ToLower() );
					break;

				// Ends With
				case FilterOperatorType.EndsWith:
					if ( (this.Attribute.Options & ElementFlag.NoFunctions) == ElementFlag.NoFunctions )
						sql.AppendFormat( "{0}.{1} LIKE '%{2}' ", this.Attribute.Parent.DataSource, this.Attribute.DataSource, this.Operand );
					else
						sql.AppendFormat( "LOWER({0}.{1}) LIKE '%{2}' ", this.Attribute.Parent.DataSource, this.Attribute.DataSource, this.Operand.ToLower() );
					break;
				
				// is null or empty
				case FilterOperatorType.IsNullOrEmpty:
					sql.AppendFormat( "{0}.{1} IS NULL ", this.Attribute.Parent.DataSource, this.Attribute.DataSource );
					if ( this.Attribute.OperandType is StringValueType || this.Attribute.OperandType is EnumValueType && ( (EnumValueType)this.Attribute.OperandType ).IsItemsString )
						sql.AppendFormat( "OR {0}.{1} = '' ", this.Attribute.Parent.DataSource, this.Attribute.DataSource );
					break;

				// is not null or empty
				case FilterOperatorType.NotIsNullOrEmpty:
					sql.AppendFormat( "({0}.{1} IS NOT NULL ", this.Attribute.Parent.DataSource, this.Attribute.DataSource );
					if (this.Attribute.OperandType is StringValueType || this.Attribute.OperandType is EnumValueType && ((EnumValueType)this.Attribute.OperandType).IsItemsString)
						sql.AppendFormat("OR {0}.{1} <> '' )", this.Attribute.Parent.DataSource, this.Attribute.DataSource);
					else
						sql.Append(" )");
					break;

				default:
					break;
			}

			return sql.ToString();
		}

		/// <summary>Serialize this instance to XML.</summary>
		public override void WriteXml( XmlWriter writer )
		{
			// write the operator
			writer.WriteStartAttribute( "field" );
			if ( this.Attribute != null && this.Attribute.Parent != null )
				writer.WriteString( String.Format( "{0}.{1}", this.Attribute.Parent.DataSource, this.Attribute.DataSource ) );
			writer.WriteEndAttribute();

			// write the operator
			writer.WriteStartAttribute( "operator" );
			writer.WriteString( Enum.Format( typeof( FilterOperatorType ), this.Operator.Value, "g" ) );
			writer.WriteEndAttribute();

			// write the operand
			writer.WriteStartAttribute( "operand" );
			writer.WriteString( this.Operand );
			writer.WriteEndAttribute();

			// write the expected operand value type
		}

		/// <summary>Create from XML.</summary>
		public override void ReadXml( XmlReader reader )
		{
			// get the data source table and field
			string[] ds = reader.GetAttribute( "field" ).Split( new char[] { '.' }, 2, StringSplitOptions.RemoveEmptyEntries );
			
			// set the operand
			this._operand = reader.GetAttribute( "operand" );

			// set the operator
			string op = reader.GetAttribute( "operator" );
			FilterOperatorType type = (FilterOperatorType)Enum.Parse( typeof( FilterOperatorType ), op );
			this._op = FilterOperator.GetOperator( type );

			// create a new attribute & parent element entity
			QueryEngine.Attribute attr = new QueryEngine.Attribute( ds[1], ds[1] );
			QueryEngine.Entity parent = new QueryEngine.Entity( ds[0], ds[0] );
			attr.Parent = parent;
			this._attr = attr;
		}

		/// <summary>Converts the expression to a display string.</summary>
		/// <remarks>Only used internally to put some value on the Lexem base class.  Maybe just override ToString or the Value property.</remarks>
		private void FormatValue()
		{
			StringBuilder sb = new StringBuilder( this._attr.DisplayText );

			sb.AppendFormat( " {0} {1}", this._op.DisplayText, this._operand );

			this._value = sb.ToString();
		}

		#endregion [Methods]
	}


}
