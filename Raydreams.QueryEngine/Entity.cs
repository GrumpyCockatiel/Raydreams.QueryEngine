using System;
using System.Collections.Generic;

namespace Raydreams.QueryEngine
{
	/// <summary>Encapsulates the data for an entity node in the attributes tree.</summary>
	[Serializable()]
	public class Entity : Element, IEnumerable<Element>
	{
		#region [Fields]

		private string[] _parentFields = null;
		private string[] _childFields = null;
		private JoinType _join = JoinType.Inner;
		private List<Element> _children = null;

		#endregion [Fields]

		#region [Constructor]

		/// <summary>Constructor</summary>
		public Entity( string displayName, string dataField )
			: base( displayName, dataField )
		{ }

		#endregion [Constructor]

		#region [Properties]

		/// <summary>Get or set the schema field ID that is this entity's parent's primary key.</summary>
		public string ParentIdField
		{
			get { return (this._parentFields == null) ? null : this._parentFields[0]; }
			//set { this._id = ( String.IsNullOrEmpty( value ) ) ? null : value; }
		}

		/// <summary>Get or set the schema field ID that is foreign key field to this entity's parent.</summary>
		public string FkField
		{
			get { return ( this._childFields == null ) ? null : this._childFields[0]; }
			//set { this._fk = ( String.IsNullOrEmpty( value ) ) ? null : value; }
		}

		/// <summary>Gets or sets the schema field IDs of this entity's parent.</summary>
		/// <remarks></remarks>
		public string[] ParentIdFields
		{
			get { return this._parentFields; }
			set { this._parentFields = ( value == null || value.Length < 1 ) ? null : value; }
		}

		/// <summary>Gets or sets the schema field IDs for this entity - the child.</summary>
		/// <remarks></remarks>
		public string[] ChildIdFields
		{
			get { return this._childFields; }
			set { this._childFields = ( value == null || value.Length < 1 ) ? null : value; }
		}

		/// <summary>Gets or sets how this entity joins with its parent.</summary>
		public JoinType Join
		{
			get { return this._join; }
			set { this._join = value; }
		}

		/// <summary>Returns the number of children contained by this entity</summary>
		public int Count
		{
			get
			{
				if ( this._children == null )
					return 0;

				return this._children.Count;
			}
		}

		/// <summary>Gets the collection of children as a separate list.</summary>
		/// <remarks>Need to change this to just GET the actual list which will return the iterator but not allow changing the list or setting it to null.</remarks>
		public List<Element> Children
		{
			get
			{
				if ( this.Count < 1 )
					return null;

				return new List<Element>( this );
			}
		}

		#endregion [Properties]

		#region [Methods]

		/// <summary>Add a child element to this entity.</summary>
		/// <param name="child">Child element to be added.</param>
		public void AddChild( Element child )
		{
			if ( child == null )
				return;

			// create the neighbor list if it does not exist
			if ( this._children == null )
				this._children = new List<Element>();

			// add the node
			this._children.Add( child );

			// create a reference from the child to the parent
			child.Parent = this;
		}

		/// <summary>Adds a collection of child elements to the entity.</summary>
		/// <param name="collection">The collection of children to add.</param>
		public void AddRange(IEnumerable<Element> collection)
		{
			foreach ( Element e in collection )
				this.AddChild( e );
		}

		/// <summary>Removes all the children of this entity.</summary>
		public void Clear()
		{
			if ( this._children != null )
				this._children.Clear();
		}

		/// <summary>Sorts this entity's children based on the specified comparer.</summary>
		public void SortChildren( IComparer<Element> sorter )
		{
			this._children.Sort( sorter );
		}

		#endregion [Methods]

		#region [Enumerators]

		/// <summary></summary>
		public IEnumerator<Element> GetEnumerator()
		{
			return this._children.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		#endregion [Enumerators]
	}

	/// <summary>Enumerates how an entity joins with its parent</summary>
	public enum JoinType : byte
	{
		/// <summary>Inner join.</summary>
		Inner = 0,
		/// <summary>Left outer join.</summary>
		LeftOuter,
		/// <summary>Right outer join.</summary>
		RightOuter,
		/// <summary>Full outer join.</summary>
		FullOuter
	}
}
