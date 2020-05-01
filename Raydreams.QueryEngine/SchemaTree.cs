using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace Raydreams.QueryEngine
{
	/// <summary>An n-way tree of query schema elements. Parses an in memory XML document for each of the various entities and attributes as well as their value types.</summary>
	/// <remarks>Trees have inorder, preorder, postorder and level-order traversal methods.</remarks>
	[XmlRoot( ElementName = "schema", Namespace = "urn:rqe-common-raydreams" )]
	[Serializable()]
	public class SchemaTree : IXmlSerializable
	{
		#region [Fields]

		private static readonly string SchemaFileFullName = "raydreams.Resources.rqe.xsd";

		private Entity _root = null;
		private string _application = null;

		#endregion [Fields]

		#region [Constructors]

		/// <summary>Parameterless constructor for use with serializer.</summary>
		private SchemaTree()
		{ }

		/// <summary>Wrap an already existing tree with the SchemaTree object.  Does not checking on the tree so this is just a hack and should be deprecated.</summary>
		public SchemaTree( Entity root )
		{
			this._root = root;
		}

		/// <summary>Creates a SchemaTree instance from a UTF-8 Encoded XML Document.</summary>
		public static SchemaTree Create(XmlDocument xml)
		{
			XmlSerializer xs = new XmlSerializer( typeof(SchemaTree) );
			System.IO.MemoryStream stream = new System.IO.MemoryStream( System.Text.ASCIIEncoding.UTF8.GetBytes(xml.OuterXml));
			XmlReader xr = XmlReader.Create( stream );
			return (SchemaTree)xs.Deserialize( xr );
		}

		#endregion [Constructors]

		#region [Properties]

		/// <summary>Get the root element in the tree which must be an entity.</summary>
		public Entity Root
		{
			get { return this._root; }
			private set { this._root = value; }
		}

		/// <summary>Gets or sets the application ID that this schema is associated with.</summary>
		public string Application
		{
			set { this._application = value; }
			get { return this._application; }
		}

		/// <summary>Static version of the get schema method that temporarily instantiates a SchemaTree.</summary>
		public static XmlSchema Schema
		{
			get
			{
				SchemaTree temp = new SchemaTree();
				return temp.GetSchema();
			}
		}

		#endregion [Properties]

		#region [Methods]

		/// <summary>Traverse the schema tree looking for an attribute matching the specified data source field names.</summary>
		/// <param name="child">Name of the attribute (child) data source.</param>
		/// <param name="parent">Name of the parent data source.</param>
		public Attribute FindAttribute( string parent, string child, bool ignoreCase )
		{
			return (Attribute)FindElement( parent, child, ignoreCase );
		}

		/// <summary>Traverse the schema tree looking for an element matching the specified data source field names.</summary>
		/// <param name="parent">Name of the parent data source.</param>
		/// <param name="child">Name of the child data source.</param>
		/// <param name="ignoreCase">Whether or not to ignore casing when searching.</param>
		public Element FindElement( string parent, string child, bool ignoreCase )
		{
			// set the comparison rule
			StringComparison rule = ( ignoreCase ) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

			// if the input element has no parent it is the root
			if ( String.IsNullOrEmpty( parent ) && child.Equals(this._root.DataSource, rule) )
				return this._root;

			// init the locals
			Stack<Element> toVisit = new Stack<Element>();
			Element current = null;

			// init with all the root children
			foreach ( Element elem in this._root )
				toVisit.Push( elem );

			// visit every node until you find a match
			while ( toVisit.Count > 0 )
			{
				current = toVisit.Pop();

				// check for a match
				if ( child.Equals( current.DataSource, rule ) &&
					parent.Equals( current.Parent.DataSource, rule ) )
				{
					return current;
				}

				// add an entity's children and move on
				if ( !current.IsLeaf() )
				{
					foreach ( Element elem in (Entity)current )
						toVisit.Push( elem );
				}
			}

			// no match ever found
			return null;
		}

		/// <summary>Traverse the schema tree looking for an element matching the specified input path.</summary>
		/// <param name="path">String path to match.</param>
		/// <param name="separator">Separator character used in the path.</param>
		/// <param name="ignoreCase">Whether or not to ignore casing when searching.</param>
		public Element FindElement( string path, char separator, bool ignoreCase )
		{
			return this._root.FindElement( path, separator, ignoreCase );
		}

		/// <summary>Sorts the entire tree based on the specified comparer.  Calls a list sort on each child list.</summary>
		public void Sort( IComparer<Element> sorter )
		{
			// init the locals
			Stack<Entity> toVisit = new Stack<Entity>();
			Entity current = null;

			// init the stack with the root
			toVisit.Push( this._root );

			while ( toVisit.Count > 0 )
			{
				current = toVisit.Pop();

				if ( current.HasChildren() )
				{
					current.SortChildren( sorter );

					foreach ( Element child in current )
					{
						if ( child is Entity )
							toVisit.Push( (Entity)child );
					}
				}
			}
		}

		/// <summary>Deserializes XML into a SchemaTree instance.</summary>
		public void ReadXml( XmlReader reader )
		{
			// stack to hold parent nodes
			Stack<Entity> stack = new Stack<Entity>();

			if ( reader.MoveToContent() != XmlNodeType.Element && reader.Name != "schema" )
				return;

			this.Application = reader.GetAttribute( "application" );

			// move to root entity node and init
			bool results = reader.ReadToDescendant( "entity" );
			string name = reader.GetAttribute( "name" );
			string field = reader.GetAttribute( "table" );
			this._root = new Entity( name, field );

			Entity current = this._root;

			// recurse tree from the root
			while ( reader.Read() )
			{
				// process start elements
				if ( reader.NodeType == XmlNodeType.Element )
				{
					if ( reader.Name == "entity" )
					{
						// create a new entity
						name = reader.GetAttribute( "name" );
						field = reader.GetAttribute( "table" );

						// push the current entity on to a stack
						stack.Push( current );
						current = new Entity( name, field );

						#region [old code]
						//current.FkField = reader.GetAttribute( "fkField" );
						//current.ParentIdField = reader.GetAttribute( "parentIdField" );
						#endregion [old code]

						#region [new code]
						// split the parent and child fields by ',' - yea I know this is bad XML design
						current.ParentIdFields = reader.GetAttribute( "parentIdField" ).Replace( " ", null ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
						current.ChildIdFields = reader.GetAttribute( "fkField" ).Replace( " ", null ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
						//current.FkField = current.ChildIdFields[0];
						//current.ParentIdField = current.ParentIdFields[0];
						#endregion [new code]

						// get the optional join attribute
						string joinValue =  reader.GetAttribute( "join" );
						if ( joinValue != null)
							current.Join = (JoinType)Enum.Parse( typeof( JoinType ), joinValue, true );
					}
					else if ( reader.Name == "attribute" )
					{
						// get the attribute's properties
						name = reader.GetAttribute( "name" );
						field = reader.GetAttribute( "field" );
						string flagsStr = reader.GetAttribute( "flags" );

						// move to the value type node
						do { reader.Read(); } while (reader.NodeType != XmlNodeType.Element);

						// reference to a new value type
						ValueType vt = null;

						// deserialize the value type information
						#region [process each value type]
						switch ( reader.Name )
						{
							// boolean operand value type
							case "boolean":
								vt = new BoolValueType();
								break;

							// number operand value type
							case "number":
								vt = new NumberValueType();
								if ( reader.HasAttributes )
								{
									// set min/max
								}
								break;

							// string operand value type
							case "string":
								vt = new StringValueType();
								if ( reader.HasAttributes )
									( (StringValueType)vt ).Filter = reader.GetAttribute( "filter" );
								break;

							// datetime operand value type
							case "dateTime":
								vt = new DateTimeValueType();
								break;

							// enum operand value type
							case "enum":
								vt = new EnumValueType();
								bool stringItems = Convert.ToBoolean( reader.GetAttribute( "stringitems" ) );
								( (EnumValueType)vt ).IsItemsString = stringItems;
								( (EnumValueType)vt ).Items = new List<EnumItem>();

								// read each item
								while ( reader.NodeType != XmlNodeType.EndElement || reader.Name != "enum" )
								{
									reader.Read();

									if ( reader.Name == "item" )
									{
										EnumItem item = new EnumItem( reader.GetAttribute( "text" ),
											reader.GetAttribute( "value" ) );
										string order = reader.GetAttribute( "order" );
										if ( !String.IsNullOrEmpty( order ) )
											item.Order = Convert.ToInt32( order );
										( (EnumValueType)vt ).Items.Add( item );
									}
								}
								break;

							default:
								break;
						}
						#endregion

						// add this attribute to the current entity
						Attribute attr = new Attribute( name, field, vt );
						if ( !String.IsNullOrEmpty(flagsStr) )
							attr.Options = (ElementFlag)Enum.Parse( typeof( ElementFlag ), flagsStr, true );
						current.AddChild( attr );

						// move to end element
						reader.ReadToNextSibling( "dummy" );
					}
					else
						continue;
				}
				// process end elements
				else if ( reader.NodeType == XmlNodeType.EndElement )
				{
					if ( reader.Name == "entity" )
					{
						if ( current.Equals( this._root ) )
							continue;

						// add current as a child of the previous entity
						Entity parent = stack.Pop();
						parent.AddChild( current );
						current = parent;
					}
				}
				else
					continue;
			}
		}

		/// <summary>Serializes a SchemaTree instance to XML.</summary>
		public void WriteXml( XmlWriter writer )
		{
			throw new Exception( "The method or operation is not implemented." );
		}

		/// <summary>Returns the valid schema for the SchemaTree object.</summary>
		public XmlSchema GetSchema()
		{
			System.Reflection.Assembly ass = System.Reflection.Assembly.GetExecutingAssembly();
			System.IO.Stream stream = ass.GetManifestResourceStream( SchemaFileFullName );

			if ( stream == null )
				throw new System.Exception("Could not read SchemaTree schema resource from assemby.");
			
			return XmlSchema.Read( stream, null );
		}

		#endregion [Methods]
	}
}
