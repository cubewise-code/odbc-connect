/*
 * Created by SharpDevelop.
 * User: tryan
 * Date: 2/03/2011
 * Time: 8:21 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Cubewise.Query.Schema
{
	/// <summary>
	/// Description of Column.
	/// </summary>
	public class Column: System.Windows.Forms.TreeNode
	{
		public Column()
		{
		}
		
		public string DataType { get; set; }
		public int DataLength { get; set; }
		
		public override string ToString()
		{
			string result = Name;
			if(! string.IsNullOrEmpty(DataType))
				result += " (" + DataType + ")";
			
			return result;
		}
	}
}
