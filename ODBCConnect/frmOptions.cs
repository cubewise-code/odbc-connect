using System;
using System.Drawing;
using System.Windows.Forms;

namespace Cubewise.Query
{
	/// <summary>
	/// Description of frmOptions.
	/// </summary>
	public partial class frmOptions : Form
	{
		public frmOptions()
		{
			InitializeComponent();		
		}
		
		void ButOKClick(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
		
		void ButCANCELClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}
