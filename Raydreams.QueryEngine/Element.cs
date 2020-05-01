using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Raydreams.QueryEngine
{
	/// <summary>Abstract schema node class.</summary>
	[Serializable()]
	public class Element
	{
		#region [Fields]

		private string _name = null;
		private string _field = null;
		protected Entity _parent = null;

		#endregion [Fielda]

		#region [Constructor]

		/// <summary>Constructor</summary>
		private Element()
			: this( null, null )
		{ }

		/// <summary>Constructor</summary>
		public Element( string name, string field )
		{
			this.DisplayText = name;
			this.DataSource = field;
		}

		#endregion [Constructor]

		#region [Properties]

		/// <summary>Gets or sets the UI text for the data source object name.</summary>
		public string DisplayText
		{
			get { return this._name; }
			set { this._name = ( String.IsNullOrEmpty( value ) ) ? null : value; }
		}

		/// <summary>Gets or sets the schema object name such as a field or table name.</summary>
		public string DataSource
		{
			get { return this._field; }
			set { this._field = ( String.IsNullOrEmpty( value ) ) ? null : value; }
		}

		/// <summary>Gets or sets this element's parent element for faster back references.</summary>
		[XmlIgnore()]
		public Entity Parent
		{
			get { return this._parent; }
			set { this._parent = value; }
		}

		/// <summary>Gets the depth of this element.</summary>
		public int Depth
		{
			get
			{
				int depth = 0;
				Element current = this._parent;
				while ( current != null )
				{
					current = current.Parent;
					++depth;
				}

				return depth;
			}
		}

		/// <summary>Gets the element that is the root of the subtree this element is in.</summary>
		public Element Root
		{
			get
			{
				Element current = this;
				while ( current.Parent != null )
					current = current.Parent;

				return current;
			}
		}

		/// <summary>Gets the path from this element back to the root.  Root element is in index zero.  This element is NOT included so parent of this element is in the last element.  All Entity elements since only Entities can be parents.</summary>
		[XmlIgnore()]
		public List<Entity> Path
		{
			get
			{
				if ( this.IsRoot() )
					return null;

				// init the array
				List<Entity> path = new List<Entity>();

				// iterate up from this entity to the parent
				Entity curPar = this.Parent;

				while ( curPar != null )
				{
					path.Add( curPar );
					curPar = curPar.Parent;
				}

				path.Reverse();

				return path;
			}
		}

		#endregion [Properties]

		#region [Methods]

		/// <summary>Returns whether this entity is the root entity or not.</summary>
		/// <remarks>In hierarchel data there can only be one root entity.</remarks>
		public virtual bool IsRoot()
		{
			return ( this._parent == null );
		}

		/// <summary>Returns whether this entity is a leaf element or not.  Might be an Entity with no attributes or other entities.</summary>
		/// <remarks>Since only entities have children, test for type.</remarks>
		public virtual bool IsLeaf()
		{
			// cast as an Entity
			Entity test = this as Entity;

			// if not Entity, then Attribute and thus leaf
			if ( test == null )
				return true;

			// if entity with no children, then leaf
			return ( test.Count > 0 ) ? false : true;
		}

		/// <summary>Return whether or not this entity has children.</summary>
		public virtual bool HasChildren()
		{ 
			return !this.IsLeaf();
		}

		/// <summary>Recurses the subtree looking for a node that matches the specified path.</summary>
		/// <param name="path">The path starting from the parent node using the DataSource property as the path values.</param>
		/// <param name="separator">The character used to separate nodes in the path.</param>
		/// <param name="ignoreCase">Whether or not to ignore casing when searching.</param>
		public Element FindElement( string path, char separator, bool ignoreCase )
		{
			// set the comparison rule
			StringComparison rule = ( ignoreCase ) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

			// get the next node data source value
			StringCollection ids = new StringCollection();
			ids.AddRange( path.Split( new char[] { separator }, StringSplitOptions.RemoveEmptyEntries ) );
			
			// if the path array is now empty, then no path was specified
			if ( ids.Count < 1 )
				return null;

			// put this node as the root of a new subtree
			List<Element> nodes = new List<Element>( 1 );
			nodes.Add( this );

			// init the found flag to false
			bool found = false;

			// need to be careful we have exit conditions
			while ( nodes != null && nodes.Count > 0 && ids.Count > 0 )
			{
				foreach ( Element currentNode in nodes )
				{
					// path match found
					if ( currentNode.DataSource.Equals(ids[0], rule ) )
					{
						found = true;

						// last path item so found
						if ( ids.Count == 1 )
							return currentNode;

						// given path was longer than the subtree so wrong
						if ( !(currentNode is Entity) )
							return null;

						nodes = ( (Entity)currentNode ).Children;
						break;
					} // end if

				} // end foreach

				// none of the nodes matched the current path so there's no match at all
				if ( !found )
					return null;

				// remove a path id value
				ids.RemoveAt( 0 );

				// reset found flag
				found = false;

			} // end while

			// if the while loop was exited, then the given path never mathced any path in the subtree
			return null;
		}

		#endregion [Methods]
	}

	/// <summary>Class to compare two elements based on the ElementSortOption value.  Default is AsRead if no constructor parameter.</summary>
	/// <remarks>AsRead does not sort correctly.  Returning 0 does not leave them in the order read.</remarks>
	public class ElementComparer : IComparer<Element>
	{
		#region [Fields]

		private ElementSortOption _sortBy = ElementSortOption.AsRead;

		#endregion [Fields]

		#region [Constructor]

		/// <summary>Default constructor.</summary>
		public ElementComparer() : this(ElementSortOption.AsRead)
		{ }

		/// <summary>Constructor.</summary>
		public ElementComparer(ElementSortOption sortBy)
		{ this._sortBy = sortBy; }

		#endregion [Constructor]

		/// <summary>Compare to elements based on the ElementSortOption.</summary>
		public int Compare( Element x, Element y )
		{
			// null is always less than a non-null
			if ( x == null )
				return ( y == null ) ? 0 : -1;

			// x is not null, but y is then x is greater
			if ( y == null )
				return 1;

			// neither is null
			if ( this._sortBy == ElementSortOption.Alphabetical  )
			{
				return x.DisplayText.CompareTo( y.DisplayText );
			}
			else if ( this._sortBy == ElementSortOption.AttributesFirstAsRead 
				|| this._sortBy == ElementSortOption.EntitiesFirstAsRead )
			{
				// set the type to compare
				Type major = typeof( QueryEngine.Attribute );
				if ( this._sortBy == ElementSortOption.EntitiesFirstAsRead )
					major = typeof( QueryEngine.Entity );

				if ( x.GetType() == major )
				{
					if ( y is Attribute )
						return 0;
					else
						return -1;
				}
				else
				{
					if ( y.GetType() == major )
						return 1;
					else
						return 0;
				}
			}
			else if ( this._sortBy == ElementSortOption.AttributesFirstAlphabetical 
				|| this._sortBy == ElementSortOption.EntitiesFirstAlphabetical )
			{
				// set the type to compare
				Type major = typeof( QueryEngine.Attribute );
				if ( this._sortBy == ElementSortOption.EntitiesFirstAsRead )
					major = typeof( QueryEngine.Entity );

				if ( x.GetType() == major )
				{
					if ( y is Attribute )
						return x.DisplayText.CompareTo( y.DisplayText );
					else
						return -1;
				}
				else
				{
					if ( y.GetType() == major )
						return 1;
					else
						return x.DisplayText.CompareTo( y.DisplayText );
				}
			}
			else
				return 0;
		}
	}


}
