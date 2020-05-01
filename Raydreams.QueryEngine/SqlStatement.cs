using System;
using System.Collections.Generic;
using System.Text;

namespace Raydreams.QueryEngine
{
	/// <summary>Breaks a SQL Statement up into each clause so it is easier to customize the final statement.</summary>
	public class SqlStatement
	{
		#region [Fields]

		private string _select = null;
		private string _from = null;
		private string _where = null;
		private Dictionary<string, int> _markers = new Dictionary<string, int>();

		#endregion [Fields]

		#region [Constructors]

		#endregion [Constructors]

		#region [Properties]

		/// <summary></summary>
		public string SelectClause
		{
			get { return this._select; }
			set { this._select = value; }
		}

		/// <summary></summary>
		public string FromClause
		{
			get { return this._from; }
			set { this._from = value; }
		}

		/// <summary></summary>
		public string WhereClause
		{
			get { return this._where; }
			set { this._where = value; }
		}

		/// <summary></summary>
		public string Query
		{
			get { return String.Format("SELECT {0} FROM {1} WHERE {2}", this._select, this._from, this._where); }
		}

		#endregion [Properties]

		#region [Methods]

		/// <summary></summary>
		public bool AddMarker( string key, int idx )
		{
			if ( String.IsNullOrEmpty( key ) )
				return false;

			this._markers.Add( key, idx);

			return true;
		}

		/// <summary></summary>
		public int GetMarker( string key )
		{
			if ( this._markers.ContainsKey( key ) )
				return this._markers[key];

			return -1;
		}

		#endregion [Methods]
	}
}
