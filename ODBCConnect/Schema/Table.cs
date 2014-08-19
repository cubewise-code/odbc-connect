/*
 * Created by SharpDevelop.
 * User: tryan
 * Date: 2/03/2011
 * Time: 8:21 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

namespace Cubewise.Query.Schema
{
	/// <summary>
	/// Description of Table.
	/// </summary>
	public class Table: System.Windows.Forms.TreeNode
	{
		public Table()
		{
            Restrictions = new List<string>();
		}
		
		public string Database { get; set; }
		public string Owner { get; set; }
		public string Type { get; set; }
        internal List<string> Restrictions { get; private set; }
		
		public override string ToString()
		{
			string result = "";
			if(string.IsNullOrEmpty(Owner))
				result += Name;
			else
				result += Owner + "." + Name;

			return result;
		}
	}
}
