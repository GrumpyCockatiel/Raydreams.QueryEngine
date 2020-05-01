using System;
using System.Collections.Generic;

namespace Raydreams.QueryEngine
{
	/// <summary>Enumerates the operand value types.</summary>
	public enum ValueTypeName : byte
	{
		/// <summary>Value type is a string.</summary>
		String = 0,
		/// <summary>Value type is boolean.</summary>
		Boolean,
		/// <summary>Value type is an enumerated bit list.</summary>
		Enum,
		/// <summary>Value type is a number - integer or float.</summary>
		Number,
		/// <summary>Value type is a date and timestamp.</summary>
		DateTime
	}

	/// <summary>Abstract class from which all operand types descend.</summary>
	/// <remarks>To avoid confusion, should have been called OperandDataType.</remarks>
	[Serializable()]
	public abstract class ValueType
	{
		#region [Factory Methods]

		/// <summary>ValueType factory method using a string.</summary>
		public static ValueType NewValueType( string type )
		{
			type = type.ToLower();

			if ( type.Substring( 0, 4 ) == "bool" )
				return NewValueType( ValueTypeName.Boolean );
			if ( type.Substring( 0, 4 ) == "enum" )
				return NewValueType( ValueTypeName.Enum );
			else if ( type.Substring( 0, 3 ) == "str" )
				return NewValueType( ValueTypeName.String );
			else if ( type.Substring( 0, 3 ) == "num" )
				return NewValueType( ValueTypeName.String );
			else if ( type  == "datetime" )
				return NewValueType( ValueTypeName.DateTime );
			else
				return NewValueType( ValueTypeName.String );
		}

		/// <summary>ValueType factory method using enumeration.</summary>
		public static ValueType NewValueType( ValueTypeName type )
		{
			ValueType op = null;

			switch ( type )
			{
				case ValueTypeName.Boolean:
					op = new BoolValueType();
					break;

				case ValueTypeName.Enum:
					op = new EnumValueType();
					break;

				case ValueTypeName.Number:
					op = new NumberValueType();
					break;

				case ValueTypeName.DateTime:
					op = new DateTimeValueType();
					break;

				case ValueTypeName.String:
				default:
					op = new StringValueType();
					break;
			}

			return op;
		}

		/// <summary>Get the value type enumeration for this instance.</summary>
		public abstract ValueTypeName Type { get; }

		#endregion [Factory Methods]
	}

	/// <summary>Encapsulates the data for a string operand in the attributes tree.</summary>
	[Serializable()]
	public class StringValueType : ValueType
	{
		private string filter = null;

		/// <summary>Constructor</summary>
		public StringValueType()
		{ }

		/// <summary>Constructor</summary>
		public StringValueType( string filter )
		{
			this.Filter = filter;
		}

		/// <summary>Get the value type name for this instance.</summary>
		public override ValueTypeName Type
		{
			get { return ValueTypeName.String; }
		}

		/// <summary>Get or set the regular expression for valid values.</summary>
		public string Filter
		{
			get { return this.filter; }
			set { this.filter = ( String.IsNullOrEmpty( value ) ) ? null : value; }
		}

	}

	/// <summary>Encapsulates the data for a enum operand in the attributes tree.</summary>
	[Serializable()]
	public class EnumValueType : ValueType
	{
		private List<EnumItem> items = null;
		private bool stringItems = true;

		/// <summary>Constructor</summary>
		public EnumValueType()
		{ }

		/// <summary>Constructor</summary>
		public EnumValueType( List<EnumItem> list )
		{
			this.Items = list;
		}

		/// <summary>Get the value type name for this instance.</summary>
		public override ValueTypeName Type
		{
			get { return ValueTypeName.Enum; }
		}

		/// <summary>Get or set the list of enumeration values.</summary>
		public List<EnumItem> Items
		{
			get { return this.items; }
			set { this.items = value; }
		}

		/// <summary>Get or set whether item values are reads as strings or numbers.</summary>
		public bool IsItemsString
		{
			get { return this.stringItems; }
			set { this.stringItems = value; }
		}
	}

	/// <summary>Encapsulates the data for a bool operand in the attributes tree.</summary>
	[Serializable()]
	public class BoolValueType : EnumValueType
	{
		public BoolValueType()
		{
			base.Items = new List<EnumItem>( 2 );
			base.Items.Add( new EnumItem("true", "1", 0) );
			base.Items.Add( new EnumItem( "false", "0", 1 ) );
			base.IsItemsString = false;
		}

		/// <summary>Get the value type name for this instance.</summary>
		public override ValueTypeName Type
		{
			get { return ValueTypeName.Boolean; }
		}
	}

	/// <summary>Encapsulates the data for a bool operand in the attributes tree.</summary>
	[Serializable()]
	public class NumberValueType : ValueType
	{
		/// <summary>Get the value type name for this instance.</summary>
		public override ValueTypeName Type
		{
			get { return ValueTypeName.Number; }
		}
	}

	/// <summary>Encapsulates the data for a datetime operand in the attributes tree.</summary>
	[Serializable()]
	public class DateTimeValueType : ValueType
	{
		/// <summary>Get the value type name for this instance.</summary>
		public override ValueTypeName Type
		{
			get { return ValueTypeName.DateTime; }
		}
	}

	/// <summary>Items of the enumeration value type.</summary>
	[Serializable()]
	public class EnumItem : IComparable
	{
		private string text = null;
		private string value = null;
		private int order = 0;

		///<summary>Constructor</summary>
		public EnumItem( string text, string value ) : this(text, value, 0)
		{}

		///<summary>Constructor</summary>
		public EnumItem( string text, string value, int order )
		{
			this.Text = text;
			this.Value = value;
			this.Order = order;
		}

		/// <summary>Gets or sets the text displayed for this item.</summary>
		public string Text
		{
			get { return this.text; }
			set { this.text = ( String.IsNullOrEmpty( value ) ) ? null : value; }
		}

		/// <summary>Gets or sets the value of this item.</summary>
		public string Value
		{
			get { return this.value; }
			set { this.value = ( String.IsNullOrEmpty( value ) ) ? null : value; }
		}

		/// <summary>Gets or sets the sort order value for this item.</summary>
		public int Order
		{
			get { return this.order; }
			set { this.order = value; }
		}

		#region IComparable Members

		public int CompareTo( object obj )
		{
			EnumItem to = obj as EnumItem;

			if ( to == null )
				throw new ArgumentException( "object is not a EnumItem" );
			
			return this.Order.CompareTo( to.Order );
		}

		#endregion
	}
}
