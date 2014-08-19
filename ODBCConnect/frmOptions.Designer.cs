/*
 * Created by SharpDevelop.
 * User: tryan
 * Date: 7/03/2011
 * Time: 8:20 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace Cubewise.Query
{
	partial class frmOptions
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
            this.label1 = new System.Windows.Forms.Label();
            this.numDisplayResults = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.txtOracleSchema = new System.Windows.Forms.TextBox();
            this.butOK = new System.Windows.Forms.Button();
            this.butCANCEL = new System.Windows.Forms.Button();
            this.txtDelimiter = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numDisplayResults)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(24, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(159, 29);
            this.label1.TabIndex = 0;
            this.label1.Text = "Max. Display Results:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numDisplayResults
            // 
            this.numDisplayResults.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numDisplayResults.Location = new System.Drawing.Point(189, 14);
            this.numDisplayResults.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numDisplayResults.Name = "numDisplayResults";
            this.numDisplayResults.Size = new System.Drawing.Size(104, 22);
            this.numDisplayResults.TabIndex = 1;
            this.numDisplayResults.ThousandsSeparator = true;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(3, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(180, 29);
            this.label2.TabIndex = 2;
            this.label2.Text = "Oracle Schema\'s to Ignore:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtOracleSchema
            // 
            this.txtOracleSchema.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOracleSchema.Location = new System.Drawing.Point(189, 47);
            this.txtOracleSchema.Name = "txtOracleSchema";
            this.txtOracleSchema.Size = new System.Drawing.Size(283, 22);
            this.txtOracleSchema.TabIndex = 3;
            // 
            // butOK
            // 
            this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOK.Location = new System.Drawing.Point(246, 114);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(109, 37);
            this.butOK.TabIndex = 6;
            this.butOK.Text = "OK";
            this.butOK.UseVisualStyleBackColor = true;
            this.butOK.Click += new System.EventHandler(this.ButOKClick);
            // 
            // butCANCEL
            // 
            this.butCANCEL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCANCEL.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCANCEL.Location = new System.Drawing.Point(361, 114);
            this.butCANCEL.Name = "butCANCEL";
            this.butCANCEL.Size = new System.Drawing.Size(109, 37);
            this.butCANCEL.TabIndex = 7;
            this.butCANCEL.Text = "CANCEL";
            this.butCANCEL.UseVisualStyleBackColor = true;
            this.butCANCEL.Click += new System.EventHandler(this.ButCANCELClick);
            // 
            // txtDelimiter
            // 
            this.txtDelimiter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDelimiter.Location = new System.Drawing.Point(189, 82);
            this.txtDelimiter.Name = "txtDelimiter";
            this.txtDelimiter.Size = new System.Drawing.Size(21, 22);
            this.txtDelimiter.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(27, 79);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(154, 29);
            this.label3.TabIndex = 4;
            this.label3.Text = "Export Delimiter:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // frmOptions
            // 
            this.AcceptButton = this.butOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCANCEL;
            this.ClientSize = new System.Drawing.Size(484, 160);
            this.Controls.Add(this.txtDelimiter);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.butCANCEL);
            this.Controls.Add(this.butOK);
            this.Controls.Add(this.txtOracleSchema);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numDisplayResults);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(2000, 250);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 173);
            this.Name = "frmOptions";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Options";
            ((System.ComponentModel.ISupportInitialize)(this.numDisplayResults)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		internal System.Windows.Forms.NumericUpDown numDisplayResults;
		internal System.Windows.Forms.TextBox txtOracleSchema;
		private System.Windows.Forms.Button butCANCEL;
		private System.Windows.Forms.Button butOK;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
        internal System.Windows.Forms.TextBox txtDelimiter;
        private System.Windows.Forms.Label label3;
	}
}
