using System;
using System.Xml.Serialization;

namespace Raydreams.QueryEngine
{
	/// <summary>Encapsulates the data for an attribute node in the attributes tree.</summary>
	[Serializable()]
	public class Attribute : Element
	{
		#region [Fields]

		private ValueType _operand = null;
		private ElementFlag _options = ElementFlag.None;

		#endregion [Fields]

		#region [Constructor]

		/// <summary>Constructor</summary>
		private Attribute() : base( null, null )
		{ }

		/// <summary>Constructor</summary>
		public Attribute( string name, string field ) : base(name, field)
		{ }

		/// <summary>Constructor</summary>
		public Attribute( string name, string field, ValueType type ) : base( name, field )
		{
			this._operand = type;
		}

		#endregion [Constructor]

		#region [Properties]

		/// <summary>Gets or sets this attribute's operand value type.</summary>
		[XmlIgnore()]
		public ValueType OperandType
		{
			get { return this._operand; }
			set { this._operand = value; }
		}

		/// <summary>Gets or sets this attribute's optional flags.</summary>
		[XmlIgnore()]
		public ElementFlag Options
		{
			get { return this._options; }
			set { this._options = value; }
		}

		#endregion [Properties]

	}

	/// <summary>Additional flag options to apply</summary>
	[FlagsAttribute()]
	public enum ElementFlag : byte
	{
		/// <summary>No flags are applied to this element.</summary>
		None = 0,
		/// <summary>Do not apply any functions to the field or value such as LOWER(field_name) = LOWER(value).</summary>
		NoFunctions = 1
	}
}
