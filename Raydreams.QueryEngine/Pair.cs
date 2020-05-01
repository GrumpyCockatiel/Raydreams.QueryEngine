using System;

namespace Raydreams.Collections
{
	/// <summary>Similar to System.Web.UI.Pair class only using generics.</summary>
    /// <remarks>This can easily be repalced now by a simple tuple</remarks>
	[Serializable()]
	public class Pair<T>
	{
		private T _first = default(T);
		private T _second = default(T);

		/// <summary>Constructor</summary>
		public Pair(T f, T s)
		{
			this._first = f;
			this._second = s;
		}

		/// <summary>Parameterless constructor for XML serialization.</summary>
		private Pair() : this(default(T), default(T))
		{ }

		/// <summary>Get or set the first item in the pair.</summary>
		public T First
		{
			get { return this._first; }
			set { this._first = value; }
		}

		/// <summary>Get or set the second item in the pair.</summary>
		public T Second
		{
			get { return this._second; }
			set { this._second = value; }
		}

	}
}