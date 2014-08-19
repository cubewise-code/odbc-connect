/*
 * Created by SharpDevelop.
 * User: tryan
 * Date: 3/03/2011
 * Time: 10:28 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;

namespace Cubewise.Query
{
	/// <summary>
	/// Description of MyTreeView.
	/// </summary>
	[Serializable]
	public class MyTreeView: TreeView
	{
		public MyTreeView()
		{
		}
		
		protected override void DefWndProc(ref Message m) 
        {
            if (m.Msg == 515) 
            { 
                /* WM_LBUTTONDBLCLK */
            }
            else
                base.DefWndProc(ref m);
        }
	}
}
