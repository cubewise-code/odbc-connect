/*
 * Created by SharpDevelop.
 * User: tryan
 * Date: 20/02/2011
 * Time: 4:16 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Cubewise.Query
{
	/// <summary>
	/// Description of frmRunning.
	/// </summary>
	public partial class frmRunning : Form
	{
		
		Timer timer = new Timer();
		int endCount = 0;
		EventHandler cancelHandler;
		DateTime start;
				
		public frmRunning()
		{

			InitializeComponent();
			
			timer.Tick += new EventHandler(timer_Tick);
		}

		void timer_Tick(object sender, EventArgs e)
		{
			//timer.Interval = (int)(timer.Interval * 1.01);
			
			if(progressBar1.Value >= progressBar1.Maximum)
			{
				endCount++;
				if(endCount > 3)
				{
					endCount = 0;
					progressBar1.Value = 0;
				}
			}
			
            if( progressBar1.Value < progressBar1.Maximum )
			    progressBar1.Value++;
		}		
		
		void ButCancelClick(object sender, EventArgs e)
		{
			cancelHandler(this, e);
			butCancel.Enabled = false;
			lblRecord.Text = "Cancelling export...";
			timer.Stop();
		}
		
		public void Start(EventHandler cancelHandler)
		{
			this.cancelHandler = cancelHandler;
			timer.Interval = 50;
			progressBar1.Value = 0;
			progressBar1.Maximum = 500;
			
			start = DateTime.Now;
			butCancel.Enabled = true;
			
			timer.Start();
		}
		
		public void Stop()
		{
			timer.Stop();
			this.Close();
		}
		
		public void SetRecordCount(string message)
		{
			lblRecord.Text = "Elapsed: " + DateTime.Now.Subtract(start).TotalSeconds.ToString("N0") + "s - " + message;
		}
	}
}
