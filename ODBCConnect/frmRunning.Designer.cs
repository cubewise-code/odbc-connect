/*
 * Created by SharpDevelop.
 * User: tryan
 * Date: 20/02/2011
 * Time: 4:16 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace Cubewise.Query
{
	partial class frmRunning
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.butCancel = new System.Windows.Forms.Button();
			this.lblRecord = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// progressBar1
			// 
			this.progressBar1.Location = new System.Drawing.Point(10, 6);
			this.progressBar1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(400, 34);
			this.progressBar1.TabIndex = 0;
			// 
			// butCancel
			// 
			this.butCancel.Location = new System.Drawing.Point(137, 90);
			this.butCancel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.butCancel.Name = "butCancel";
			this.butCancel.Size = new System.Drawing.Size(152, 45);
			this.butCancel.TabIndex = 1;
			this.butCancel.Text = "CANCEL";
			this.butCancel.UseVisualStyleBackColor = true;
			this.butCancel.Click += new System.EventHandler(this.ButCancelClick);
			// 
			// lblRecord
			// 
			this.lblRecord.Location = new System.Drawing.Point(12, 47);
			this.lblRecord.Name = "lblRecord";
			this.lblRecord.Size = new System.Drawing.Size(398, 34);
			this.lblRecord.TabIndex = 2;
			this.lblRecord.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// frmRunning
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(422, 146);
			this.ControlBox = false;
			this.Controls.Add(this.lblRecord);
			this.Controls.Add(this.butCancel);
			this.Controls.Add(this.progressBar1);
			this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmRunning";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Running query....";
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Label lblRecord;
		private System.Windows.Forms.Button butCancel;
		private System.Windows.Forms.ProgressBar progressBar1;
	}
}
