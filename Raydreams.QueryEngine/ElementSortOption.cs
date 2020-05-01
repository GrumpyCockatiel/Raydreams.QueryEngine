using System;
using System.Collections.Generic;
using System.Text;

namespace Raydreams.QueryEngine
{
	/// <summary>How a combination of attributes and entities defined at the same level are sorted in the display.</summary>
	public enum ElementSortOption : byte
	{
		/// <summary>Insert the nodes in the order they are read from the schema.</summary>
		AsRead = 0,
		/// <summary>Primary sorting by attributes first and then tables, with secondary sorting by the order in which they are read.</summary>
		AttributesFirstAsRead,
		/// <summary>Primary sorting by tables first and then attributes, with secondary sorting by the order in which they are read.</summary>
		EntitiesFirstAsRead,
		/// <summary>Primary, alphabetical sorting only.</summary>
		Alphabetical,
		/// <summary>Primary sorting by attributes first and then tables, with secondary alphabetical sorting.</summary>
		AttributesFirstAlphabetical,
		/// <summary>Primary sorting by tables first and then attributes, with secondary alphabetical sorting.</summary>
		EntitiesFirstAlphabetical
	}
}
