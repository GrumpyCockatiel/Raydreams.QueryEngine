using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Raydreams.QueryEngine
{
	/// <summary>Enumerates the various types of operations.  Does not include negation operations since they are equivalent to a negation boolean operation on the whole expression.</summary>
	public enum FilterOperatorType
	{
		/// <summary>No operator.</summary>
		None = 0, // unary
		/// <summary>Equivalent to operator.</summary>
		Equal, // binary
		/// <summary>Not equivalent to operator.</summary>
		Inequal, // binary
		/// <summary>Less than operator.</summary>
		LessThan, // binary
		/// <summary>Greater than operator.</summary>
		GreaterThan, // binary
		/// <summary>Less than or equal to operator.</summary>
		LessThanOrEqual, // binary
		/// <summary>Greater than or equal to operator.</summary>
		GreaterThanOrEqual, // binary
		/// <summary>Contains operator.</summary>
		Contains, // binary
		/// <summary>Begins with operator.</summary>
		BeginsWith, // binary
		/// <summary>Ends with operator.</summary>
		EndsWith, // binary
		/// <summary>Is null or empty operator.</summary>
		IsNullOrEmpty, // unary
		/// <summary>Is not null or not empty.</summary>
		NotIsNullOrEmpty // unary
	}

	/// <summary>A keyed collection of FilterOperator objects.</summary>
	public class FilterOperatorCollection : KeyedCollection<FilterOperatorType, FilterOperator>
	{
		public FilterOperatorCollection() : base() { }

		protected override FilterOperatorType GetKeyForItem( FilterOperator item )
		{
			return item.Value;
		}
	}

	/// <summary>Encapsulates an actual instance of an operator.</summary>
	/// <remarks>Operator instances are static.</remarks>
	/// <remarks>Filter operators need to ultimately become Tokens and Lexems as well.</remarks>
	[Serializable()]
	public class FilterOperator
	{
		/// <summary>The internal static dictionary of operators.</summary>
		private static readonly FilterOperatorCollection _opList = null;

		private FilterOperatorType _value = FilterOperatorType.None;
		private string _text = null;

		/// <summary>Static Constructor.</summary>
		static FilterOperator()
		{
			_opList = new FilterOperatorCollection();
			_opList.Add( new FilterOperator( FilterOperatorType.Equal, "=" ) );
			_opList.Add( new FilterOperator( FilterOperatorType.Inequal, "<>" ) );
			_opList.Add( new FilterOperator( FilterOperatorType.LessThan, "<" ) );
			_opList.Add( new FilterOperator( FilterOperatorType.GreaterThan, ">" ) );
			_opList.Add( new FilterOperator( FilterOperatorType.LessThanOrEqual, "<=" ) );
			_opList.Add( new FilterOperator( FilterOperatorType.GreaterThanOrEqual, ">=" ) );
			_opList.Add( new FilterOperator( FilterOperatorType.Contains, "contains" ) );
			_opList.Add( new FilterOperator( FilterOperatorType.BeginsWith, "begins with" ) );
			_opList.Add( new FilterOperator( FilterOperatorType.EndsWith, "ends with" ) );
			_opList.Add( new FilterOperator( FilterOperatorType.None, " " ) );
			_opList.Add( new FilterOperator( FilterOperatorType.IsNullOrEmpty, "is null or empty" ) );
			_opList.Add( new FilterOperator( FilterOperatorType.NotIsNullOrEmpty, "is not (null or empty)" ) );
		}

		/// <summary>Get the operator types dictionary.</summary>
		public static FilterOperatorCollection OperatorList
		{
			//get { return FilterOperator.opList; }
			get { return FilterOperator._opList; }
		}

		#region [Constructor]
		
		/// <summary>Constructor</summary>
		private FilterOperator( FilterOperatorType type )
		{
			this._value = type;
		}

		/// <summary>Constructor</summary>
		private FilterOperator( FilterOperatorType type, string displayText )
		{
			this._value = type;
			this._text = displayText;
		}

		#endregion [Constructor]

		#region [Properties]

		/// <summary>Gets the instance operator by type.</summary>
		public static FilterOperator GetOperator(FilterOperatorType type)
		{
			return FilterOperator._opList[type];
		}

		/// <summary>Get the type of filter operator.</summary>
		public FilterOperatorType Value
		{
			get { return this._value; }
		}

		/// <summary>Get the display text for this operator.</summary>
		/// <remarks>Returns text from static list until constructor is made private.</remarks>
		public string DisplayText
		{
			get { return FilterOperator._opList[this._value]._text; }
		}

		#endregion [Properties]
	}
}
