using System;
using System.IO;
using System.Data;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Data.Odbc;

using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using CsvHelper;

using Cubewise.Query.Schema;


namespace Cubewise.Query
{
	/// <summary>
	/// Description of frmMain.
	/// </summary>
	public partial class frmMain : Form
	{
		
		ODBCDSN currentDSN;
		frmRunning frm = new frmRunning();
		string settingsPath = "Settings.xml";
		Settings settings;

        BackgroundWorker workerExecute = null;
        OdbcCommand com;
        DateTime start;
        Timer timer;
		
		internal TextEditorControl textEditorControl1;
        SourceGrid.Grid grid1;
        FindAndReplaceForm frmFind = new FindAndReplaceForm();
		
		public frmMain()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			EmbeddedSyntaxModeProvider syntax = new EmbeddedSyntaxModeProvider();
			HighlightingManager.Manager.AddSyntaxModeFileProvider(syntax); // Attach to the text editor.
			
			// Load the settings from file
			settings = Settings.Load(settingsPath);
			
			// Load the data sources
			List<ODBCDSN> dsns = new List<ODBCDSN>();
			dsns.AddRange(ODBCManager.GetSystemDSNList());
			dsns.AddRange(ODBCManager.GetUserDSNList());
			
			mnuDataSource.DropDown.Items.Clear();
			
			foreach (var dsn in dsns.OrderBy(x => x.GetDSNName())) {				
				ToolStripMenuItem mnu = new ToolStripMenuItem(string.Format("{0} : {1}", dsn.GetDSNName(), dsn.GetDSNDriverName()));
				mnu.Tag = dsn;	
				mnu.Click += DSNOnClick;
				mnuDataSource.DropDown.Items.Add(mnu);
				
				if(dsn.GetDSNName() == settings.DSN)
				{
					mnu.Checked = true;
					currentDSN = dsn;
					txtStatus.Text = "DSN: " + mnu.Text;
				}
			}
			
			txtUserName.Text = settings.UserName;
			txtPassword.Text = settings.Password;
			txtOutputPath.Text = settings.OutputPath;
			NewEditor();
			myTreeView1.Visible = settings.TreeViewVisible;
			myTreeView1.Width = settings.TreeViewWidth;
					
			splitter1.DoubleClick += new EventHandler(splitter1_DoubleClick);
			
			
		}

		void GridSplitter_DoubleClick(object sender, EventArgs e)
		{

            Splitter splitter = (Splitter)sender;
            SourceGrid.Grid grid = splitter.Tag as SourceGrid.Grid;
            if( grid != null )
                grid.Visible = !grid.Visible;
			
		}

		void splitter1_DoubleClick(object sender, EventArgs e)
		{
			
			myTreeView1.Visible = !myTreeView1.Visible;
	
		}
		
		void DSNOnClick(object sender, EventArgs e)		
		{
			
			foreach (ToolStripMenuItem item in mnuDataSource.DropDown.Items) {
				item.Checked = false;
			}

            ToolStripMenuItem mnu = (ToolStripMenuItem)sender;
			mnu.Checked = true;		
			currentDSN = mnu.Tag as ODBCDSN;
			txtStatus.Text = "DSN: " + mnu.Text;
			myTreeView1.Nodes.Clear();
		}
		
		void MnuExecuteClick(object sender, EventArgs e)
		{

            if (mnuExecute.Text == "Execute")
            {

                if (currentDSN == null)
                {
                    MessageBox.Show("Select a data source", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                lblCurrentStatus.Text = "Running query...";
                this.Cursor = Cursors.WaitCursor;
                textEditorControl1.Cursor = Cursors.WaitCursor;
                start = DateTime.Now;
                mnuExecute.Text = "CANCEL";
                mnuExecute.ForeColor = Color.Red;
                mnuRun.Enabled = false;
                mnuSchema.Enabled = false;
                Application.DoEvents();

                ExecuteArgs args = new ExecuteArgs();

                args.ConString = string.Format("DSN={0};Uid={1};Pwd={2};", currentDSN.GetDSNName(), txtUserName.Text, txtPassword.Text);

                // If there is selected text use that as the sql source or use all of the text
                if (textEditorControl1.ActiveTextAreaControl.SelectionManager.HasSomethingSelected)
                    args.SQL = textEditorControl1.ActiveTextAreaControl.SelectionManager.SelectedText;
                else
                    args.SQL = textEditorControl1.Text;

                timer = new Timer();
                timer.Interval = 1000;
                timer.Tick += delegate (object sender1, EventArgs e1)
                {
                    lblCurrentStatus.Text = "Running query - Elapsed time " + DateTime.Now.Subtract(start).TotalSeconds.ToString("N0") + "s";
                };

                workerExecute = new BackgroundWorker();
                workerExecute.WorkerReportsProgress = true;
                workerExecute.WorkerSupportsCancellation = true;
                workerExecute.DoWork += new DoWorkEventHandler(ExecuteWorker_DoWork);
                workerExecute.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ExecuteWorker_RunWorkerCompleted);

                workerExecute.RunWorkerAsync(args);
                timer.Start();
            }
            else
            {
                com.Cancel();
                workerExecute.CancelAsync();
            }

 
		}

        class ExecuteArgs
        {
            public string ConString { get; set; }
            public string SQL { get; set; }
        }

        void ExecuteWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            ExecuteArgs args = (ExecuteArgs)e.Argument;

            BackgroundWorker worker = (BackgroundWorker)sender;

            IList<IList<object>> result = new List<IList<object>>();

            using (OdbcConnection con = new OdbcConnection(args.ConString))
            {
                con.Open();
                com = new OdbcCommand(args.SQL, con);
                com.CommandTimeout = settings.CommandTimeout;

                using (IDataReader r = com.ExecuteReader())
                {

                    // Write the header
                    IList<object> header = new List<object>();
                    for (int c = 0; c < r.FieldCount; c++)
                    {
                        header.Add(r.GetName(c));
                    }
                    result.Add(header);

                    int i = 0;

                    // Write the values
                    while (r.Read())
                    {

                        i++;
                        IList<object> row = new List<object>();

                        for (int c = 0; c < r.FieldCount; c++)
                        {

                            if (worker.CancellationPending)
                            {
                                e.Cancel = true;
                                return;
                            }

                            if (r.IsDBNull(c))
                                row.Add(null);
                            else if (r.GetFieldType(c) == typeof(DateTime))
                                row.Add(r.GetDateTime(c));
                            else if (r.GetFieldType(c) == typeof(string))
                                row.Add(r[c].ToString().Trim());
                            else
                                row.Add(r[c]);

                        }
                        result.Add(row);

                        if (i >= settings.QueryResultMaximum)
                            break;

                    }
                }

            }

            e.Result = result;

        }

        void ExecuteWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            mnuExecute.Text = "Execute";
            mnuExecute.ForeColor = SystemColors.ControlText;
            mnuRun.Enabled = true;
            mnuSchema.Enabled = true;
            this.Cursor = Cursors.Default;
            textEditorControl1.Cursor = Cursors.Default;
            timer.Stop();

            if (e.Cancelled)
            {
                lblCurrentStatus.Text = string.Format("Query cancelled");
            }
            else if (e.Error != null)
            {
                this.Cursor = Cursors.Default;
                Program.logger.ErrorException("Execute", e.Error);
                lblCurrentStatus.Text = "Error: " + e.Error.Message;
                MessageBox.Show("An error occurred executing sql statement data:\n" + e.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
  
            }
            else if (e.Result != null && e.Result is IList<IList<object>>)
            {

                IList<IList<object>> result = (IList<IList<object>>)e.Result;

                grid1.ClipboardMode = SourceGrid.ClipboardMode.All;
                grid1.Rows.Clear();
                grid1.FixedRows = 1;
                grid1.FixedColumns = 1;
                SourceGrid.Cells.Editors.EditorBase editor = new SourceGrid.Cells.Editors.TextBox(typeof(string));
                editor.EnableEdit = false;

                // Set the column count
                int colCount = result[0].Count;
                grid1.ColumnsCount = colCount + 1; // Extra one for header

                // Write the header
                grid1.Rows.Insert(0);
                grid1[0, 0] = new SourceGrid.Cells.ColumnHeader("");
                for (int c = 0; c < colCount; c++)
                {
                    grid1[0, c + 1] = new SourceGrid.Cells.ColumnHeader(result[0][c]);
                }

                int i = 0;

                // Write the values
                foreach (List<object> row in result.Skip(1))
                {

                    i++;
                    grid1.Rows.Insert(i);
                    grid1[i, 0] = new SourceGrid.Cells.RowHeader(i.ToString());
                    for (int c = 0; c < colCount; c++)
                    {
                        if (row[c] == null)
                            grid1[i, c + 1] = new SourceGrid.Cells.Cell("", typeof(string));
                        else if (row[c] == typeof(DateTime))
                            grid1[i, c + 1] = new SourceGrid.Cells.Cell( ((DateTime)row[c]).ToString("yyyy-MM-dd HH:mm:ss"), typeof(string));
                        else if (row[c] == typeof(string))
                            grid1[i, c + 1] = new SourceGrid.Cells.Cell(row[c].ToString().Trim(), typeof(string));
                        else
                            grid1[i, c + 1] = new SourceGrid.Cells.Cell(row[c], typeof(double));

                        grid1[i, c + 1].Editor = editor;
                    }

                }

                grid1.AutoSizeCells();
                grid1.Visible = true;
                lblCurrentStatus.Text = string.Format("Query completed in {0}s", DateTime.Now.Subtract(start).TotalSeconds);
            }


	

        }
		
		void MnuRunClick(object sender, EventArgs e)
		{
			if(currentDSN == null)
			{
				MessageBox.Show("Select a data source", "Error",  MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

            var args = new RunArgs
            {
                DSN = currentDSN.GetDSNName(),
                UserName = txtUserName.Text,
                Password = txtPassword.Text,
                SQL = textEditorControl1.Text,
                OutputPath = txtOutputPath.Text

            };

            if (string.IsNullOrEmpty(txtOutputPath.Text))
            {
                args.OutputPath = Path.Combine(Program.CurrentDirectory, Path.GetFileNameWithoutExtension(tabControl1.SelectedTab.Text) + ".csv");
            }
			
			
			if(Program.worker == null)
			{
				Program.worker = new System.ComponentModel.BackgroundWorker();
				Program.worker.WorkerReportsProgress = true;
				Program.worker.WorkerSupportsCancellation = true;
				Program.worker.DoWork += new DoWorkEventHandler(RunWorker_DoWork);
				Program.worker.ProgressChanged += new ProgressChangedEventHandler(RunWorker_ProgressChanged);	
				Program.worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunWorker_RunWorkerCompleted);	
			}

			mnuRun.Enabled = false;
            Program.worker.RunWorkerAsync(args);
			frm.Start(CancelHandler);
			frm.ShowDialog();	
		}


        class RunArgs
        {
            public string DSN { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public string SQL { get; set; }
            public string OutputPath { get; set; }
        }


        void RunWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            RunArgs args = (RunArgs)e.Argument;

            Program.Export(true, args.DSN, args.UserName, args.Password, args.SQL, args.OutputPath, settings.Delimiter, e);
        }

        void RunWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState != null)
            {
                frm.SetRecordCount(e.UserState.ToString());
            }
            else
                frm.SetRecordCount("Record: " + e.ProgressPercentage.ToString("N0"));
        }


        void CancelHandler(object sender, EventArgs e)
        {
            if (Program.worker != null)
            {
                Program.worker.CancelAsync();
            }
        }


		void RunWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			frm.Stop();
			mnuRun.Enabled = true;
			if (e.Error != null)
			{
				frm.Stop();
				Timer t = new Timer();
				t.Interval = 200;
				t.Tick += delegate {
					t.Stop();
					MessageBox.Show(string.Format("An error occurred exporting data: {0}\n Refer to the QueryLog.txt for further info.", e.Error.Message) , "Error",  MessageBoxButtons.OK, MessageBoxIcon.Error);
				};
				t.Start();
			}
			else if(!e.Cancelled)
			{
				Timer t = new Timer();
				t.Interval = 200;
				t.Tick += delegate {
					t.Stop();
					MessageBox.Show("Complete!", "Data Export",  MessageBoxButtons.OK, MessageBoxIcon.Information);
				};
				t.Start();
			}
			
		}
		
		void ExitToolStripMenuItemClick(object sender, EventArgs e)
		{
			Application.Exit();
		}
			
		void FrmMainFormClosing(object sender, FormClosingEventArgs e)
		{

			try
			{
				ToSettings();		
				settings.Save(settingsPath);
			}
			catch(Exception ex)
			{
				Program.logger.ErrorException("Save Settings", ex);
			}
		}
		
		void ToSettings()
		{
			string dsn = "";
			if(currentDSN != null)
				dsn = currentDSN.GetDSNName();
			
			settings.DSN = dsn;
			settings.UserName = txtUserName.Text;
			settings.SetPassword (txtPassword.Text);
			settings.OutputPath = txtOutputPath.Text;
			settings.SQLStatement = textEditorControl1.Text;
			settings.TreeViewWidth = myTreeView1.Width;
			settings.TreeViewVisible = myTreeView1.Visible;
			settings.GridHeight = grid1.Height;
			settings.GridVisible = grid1.Visible;
			
			
		}
		
		
		void MnuOpenClick(object sender, EventArgs e)
		{
			
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "SQL Statements|*.sql|All File|*.*";
			dialog.CheckFileExists = true;
			
			if(dialog.ShowDialog() == DialogResult.OK)
			{
				string sqlFileName = dialog.FileName;
				lblFileName.Text = sqlFileName;
				try
				{
					NewEditor();
					textEditorControl1.Text = File.ReadAllText(sqlFileName);
					textEditorControl1.Tag = sqlFileName;
					TabPage page = (TabPage)textEditorControl1.Parent;
					page.Text = Path.GetFileName(sqlFileName);
				}
				catch (Exception ex)
				{
					Program.logger.ErrorException("Open File: " + sqlFileName, ex);
					MessageBox.Show("Unable to open file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
					
		}
		
		void MnuSaveClick(object sender, EventArgs e)
		{
			
			if(textEditorControl1.Tag == null)
			{
				MnuSaveAsClick(null, null);
				return;
			}
			
			string sqlFileName = textEditorControl1.Tag.ToString();
			
			try
			{
				File.WriteAllText(sqlFileName, textEditorControl1.Text);
			}
			catch (Exception ex)
			{
				Program.logger.ErrorException("Save File: " + sqlFileName, ex);
				MessageBox.Show("Unable to save file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			
		}
		
		void MnuSaveAsClick(object sender, EventArgs e)
		{
			SaveFileDialog dialog = new SaveFileDialog();
			dialog.Filter = "SQL Statements|*.sql|All File|*.*";
			dialog.CheckFileExists = false;
			
			if(dialog.ShowDialog() == DialogResult.OK)
			{
				string sqlFileName = dialog.FileName;
				lblFileName.Text = sqlFileName;
				try
				{
					File.WriteAllText(sqlFileName, textEditorControl1.Text);
					TabPage page = (TabPage)textEditorControl1.Parent;
					page.Text = Path.GetFileName(sqlFileName);
				}
				catch (Exception ex)
				{
					Program.logger.ErrorException("Open File: " + sqlFileName, ex);
					MessageBox.Show("Unable to Save As: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}
		
		void MnuNewClick(object sender, EventArgs e)
		{
			NewEditor();
		}
		
		void NewEditor()
		{

			TextEditorControl txt = new TextEditorControl();
			txt.ContextMenuStrip = ctxEditor;
			txt.Dock = System.Windows.Forms.DockStyle.Fill;
			txt.IsReadOnly = false;
			txt.SetHighlighting("SQL"); // Activate the highlighting, use the name from the SyntaxDefinition node.
			txt.TextChanged += new EventHandler(TextEditor_TextChanged);

            SourceGrid.Grid grid = new SourceGrid.Grid();
            grid.ContextMenuStrip = this.ctxResults;
            grid.Dock = System.Windows.Forms.DockStyle.Bottom;
            grid.EnableSort = true;
            //grid.Location = new System.Drawing.Point(220, 293);
            //grid.MinimumSize = new System.Drawing.Size(0, 100);
            grid.OptimizeMode = SourceGrid.CellOptimizeMode.ForRows;
            grid.SelectionMode = SourceGrid.GridSelectionMode.Cell;
            //grid.Size = new System.Drawing.Size(774, 200);
            grid.Height = 200;
            grid.TabIndex = 10;
            grid.TabStop = true;
            grid.ToolTipText = "";
            grid.SelectionMode = SourceGrid.GridSelectionMode.Cell;


            Splitter splitter = new Splitter();
            splitter.BackColor = System.Drawing.SystemColors.ActiveBorder;
            splitter.Dock = System.Windows.Forms.DockStyle.Bottom;
            splitter.Location = new System.Drawing.Point(220, 288);
            splitter.Size = new System.Drawing.Size(774, 5);
            splitter.TabIndex = 11;
            splitter.TabStop = false;
            splitter.Tag = grid;
            splitter.DoubleClick += new EventHandler(GridSplitter_DoubleClick);


			
			TabPage page = new TabPage();
			page.Text = string.Format("SQLQuery{0}.sql", tabControl1.TabCount + 1);
			page.Controls.Add(txt);
            page.Controls.Add(splitter);
            page.Controls.Add(grid);
            page.Tag = grid;
			tabControl1.MouseUp += TabPageOnMouseUp;
			tabControl1.TabPages.Add(page);

			tabControl1.SelectTab(page);	
		
            // Set teh current text control and grid
			textEditorControl1 = txt;
            grid1 = grid;
			

		}
		
		private void TabPageOnMouseUp(object sender, MouseEventArgs e)
		{
		    if (e.Button == MouseButtons.Right)
		    {
		        for (int i = 0; i < tabControl1.TabCount; i++)
		        {
		            Rectangle rt = tabControl1.GetTabRect(i);
		            if (e.X > rt.Left && e.X < rt.Right
		                && e.Y > rt.Top && e.Y < rt.Bottom)
		            {
                        if (tabControl1.TabPages.Count > 1)
                            mnuClose.Enabled = true;
                        else
                            mnuClose.Enabled = false;

		                ctxTab.Show(this.tabControl1, e.Location);
		                tabControl1.SelectTab(i);
		            }
		        }
		    }
		}

		void TextEditor_TextChanged(object sender, EventArgs e)
		{
			TextEditorControl txt = (TextEditorControl)sender;
			TabPage page = (TabPage)txt.Parent;
			if(txt.Text.Length > 100)
				page.ToolTipText = txt.Text.Substring(1, 100);
			else
				page.ToolTipText = txt.Text;
		}
		
		void MnuCopyToClipboardClick(object sender, EventArgs e)
		{
			ToSettings();		
			ToClipboard(settings.DSN, settings.SQLStatement, settings.UserName, settings.Password, settings.OutputPath);
		}
		
		void ToClipboard(string dsn, string sql, string user, string pass, string output)
		{
			System.Text.StringBuilder command = new System.Text.StringBuilder();
            string path = typeof(frmMain).Assembly.Location;
			command.AppendLine("# Execute the command to extract data");
            command.AppendFormat("ExecuteCommand('\"{0}\" ", path);
            // Add the parameters
            command.AppendLine("loglevel=Error dsn=\"{0}\" sql=\"{1}\" user=\"{2}\" pass=\"{3}\" output=\"{4}\" delimiter=\"{5}\"', 1);");
			string cmd = string.Format(command.ToString(), 
			                           dsn,
			                           sql.Replace(Environment.NewLine, " ").Replace("'", "''"), 
			                           user, 
			                           pass, 
			                          output,
                                      settings.Delimiter);
			Clipboard.SetText(cmd);
		}
		
		void MnuCopyToClipboardEncryptedClick(object sender, EventArgs e)
		{
			ToSettings();		
			ToClipboard(settings.DSN, settings.SQLStatement, settings.UserName, "###:" + settings.EncryptedPassword, settings.OutputPath);
		}
	
		void MnuCutClick(object sender, EventArgs e)
		{
			SendKeys.Send("^x");
		}
		
		void MnuCopyClick(object sender, EventArgs e)
		{
			if(this.ActiveControl is SourceGrid.Grid)
			{
				GridCopySelectedToClipboard();
			}
			else
				SendKeys.Send("^c");
		}
		
		void MnuPasteClick(object sender, EventArgs e)
		{
			SendKeys.Send("^v");
		}
		
		void MnuSchemaClick(object sender, EventArgs e)
		{
			
			if(currentDSN == null)
			{
				MessageBox.Show("Select a data source", "Error",  MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			
			lblCurrentStatus.Text = "Building schema...";
			this.Cursor = Cursors.WaitCursor;
			Application.DoEvents();
			DateTime start = DateTime.Now;
			mnuSchema.Enabled = false;
			myTreeView1.Nodes.Clear();

            SchemaArgs args = new SchemaArgs();

            args.ConString = string.Format("DSN={0};Uid={1};Pwd={2};", currentDSN.GetDSNName(), txtUserName.Text, txtPassword.Text);
			
			BackgroundWorker worker = new BackgroundWorker();
			worker.WorkerReportsProgress = true;
			worker.WorkerSupportsCancellation = true;
			worker.DoWork += new DoWorkEventHandler(SchemaWorker_DoWork);
			worker.ProgressChanged += new ProgressChangedEventHandler(SchemaWorker_ProgressChanged);
			worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SchemaWorker_RunWorkerCompleted);
			
			worker.RunWorkerAsync(args);

		}

        class SchemaArgs
        {
            public string ConString { get; set; }
        }

		void SchemaWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			this.Cursor = Cursors.Default;
			mnuSchema.Enabled = true;
			if(e.Error != null)
			{
				lblCurrentStatus.Text = "Error building schema.";
				Program.logger.ErrorException("Build Schema", e.Error);
				MessageBox.Show("An error occurred building the schema:\n" + e.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				
			}
			else
			{
				lblCurrentStatus.Text = "Finished building schema.";
				myTreeView1.Visible = true;
			}
		}

		void SchemaWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			Table table = (Table)e.UserState;
			myTreeView1.Nodes.Add(table);
		}

		void SchemaWorker_DoWork(object sender, DoWorkEventArgs e)
		{

            SchemaArgs args = (SchemaArgs)e.Argument; 
			
			BackgroundWorker worker = (BackgroundWorker)sender;

            Program.logger.Debug("Connection string: " + args.ConString);

            using (OdbcConnection con = new OdbcConnection(args.ConString))
			{

				con.Open();
				
				Program.logger.Debug("Getting restrictions");
				DataRow[] restrictionList = con.GetSchema("Restrictions").Select("CollectionName = 'Tables'");
				
								// Build column restrictions
				foreach (DataRow r in restrictionList)
				{
					string colName = r[1].ToString();
					Program.logger.Debug("Restrictions: " + colName);
				}
				
				
				Program.logger.Debug("Getting table list");
				DataTable result = con.GetSchema(OdbcMetaDataCollectionNames.Tables);
				
				myTreeView1.Nodes.Clear();
				
				bool isOracle = currentDSN.GetDSNDriverName().ToLower().Contains("oracle");

                List<DataRow> tables = new List<DataRow>();
                foreach (DataRow tab in result.Rows)
                {
                    tables.Add(tab);
                }
				
				// Write the values
				foreach (DataRow tab in tables.Where( x => ! x.IsNull(2)).OrderBy( x => x[2].ToString()))
				{
					
					if(worker.CancellationPending)
					{
						e.Cancel = true;
						break;
					}
					
					Program.logger.Debug("Getting table properties");
					Table table = new Table();
					if(! tab.IsNull(0))
						table.Database = tab[0].ToString();
					if(! tab.IsNull(1))
						table.Owner = tab[1].ToString();
					if(! tab.IsNull(2))
						table.Name = tab[2].ToString();
					if(! tab.IsNull(3))
						table.Type = tab[3].ToString();
					
					if(isOracle)
					{
						
						if(settings.OracleIgnoredSchemaArray.Contains(table.Owner))
							continue;
					
					}
					
					table.Text = table.ToString();
					table.ImageIndex = 0;
					table.SelectedImageIndex = 1;
										
					// Build column restrictions
					foreach (DataRow r in restrictionList)
					{
						string colName = r[1].ToString();
						int col = result.Columns.IndexOf(colName);
						if(col == -1)
							table.Restrictions.Add(null);
						else if(tab.IsNull(col))
                            table.Restrictions.Add(null);
						else
						{
							string restriction = tab[col].ToString();
                            table.Restrictions.Add(restriction);
						}
					}

                    table.Nodes.Add("Loading...");
					
					worker.ReportProgress(0, table);
				}

			}
		}
		
	
		
		void myTreeView1ItemDrag(object sender, ItemDragEventArgs e)
		{
			if(e.Item is Table)
			{
				Table table = (Table)e.Item;
				table.Expand();
				
				System.Text.StringBuilder sql = new System.Text.StringBuilder();
				sql.Append("SELECT ");
				int lineLength = 0;
				foreach(Column column in table.Nodes)
				{
					if(lineLength > 70)
					{
						lineLength = 0;
						sql.AppendLine();
						sql.Append("\t");
					}
					sql.Append(column.Name);
					sql.Append(", ");
					
					lineLength += column.Name.Length + 2;
				}
				
				// Take away the last comma
				sql.Length-=2;
				
				sql.AppendLine();
				
				sql.Append("FROM ");
				if( !string.IsNullOrEmpty(table.Owner) )
					sql.Append(table.Owner + ".");
				sql.AppendLine(table.Name);
				
				DoDragDrop(sql.ToString(), DragDropEffects.Move | DragDropEffects.Copy);
				
			}
			else if(e.Item is Column)
			{
				Column column = (Column)e.Item;
				DoDragDrop(column.Name, DragDropEffects.Move | DragDropEffects.Copy);
			}
			
  			
		}
		
		
		void myTreeView1NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			TreeNode node = e.Node;
			if(node != null)
			{
				string text = node.Name;
				if(node is Table)
					text = node.ToString();
				
				textEditorControl1.ActiveTextAreaControl.TextArea.InsertString(text);
				textEditorControl1.ActiveTextAreaControl.TextArea.Select();
				node.Expand();
			}
		}

        private void myTreeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {

            if (e.Node.Nodes.Count == 1 && e.Node.Nodes[0].Text == "Loading...")
            {

                Table table = (Table)e.Node;

                table.Nodes.Clear();

                string conString = string.Format("DSN={0};Uid={1};Pwd={2};", currentDSN.GetDSNName(), txtUserName.Text , txtPassword.Text);
			    Program.logger.Debug("Connection string: " + conString);

                using (OdbcConnection con = new OdbcConnection(conString))
                {

                    con.Open();

                    Program.logger.Debug("Restrictions: " + string.Join(",", table.Restrictions.ToArray()));

                    //Get the columns
                    DataTable columns = con.GetSchema(OdbcMetaDataCollectionNames.Columns, table.Restrictions.ToArray());
                    Program.logger.Debug("Getting columns for table: " + table.Name);
                    foreach (DataRow col in columns.Rows)
                    {
                        Column column = new Column();
                        column.Name = col["COLUMN_NAME"].ToString();
                        column.DataType = col["TYPE_NAME"].ToString();
                        column.Text = column.ToString();
                        column.ImageIndex = 2;
                        column.SelectedImageIndex = 3;
                        table.Nodes.Add(column);
                    }

                }



            }
        }
		
		void SelectAllToolStripMenuItemClick(object sender, EventArgs e)
		{
			List<int> rows = new List<int>();
			for(int r = 1; r < grid1.RowsCount; r++)
			{
				rows.Add(r);
			}
			
			List<int> cols = new List<int>();
			for(int c = 1; c < grid1.ColumnsCount; c++)
			{
				cols.Add(c);
			}
			
			GridCopyToClipboard(rows.ToArray(), cols.ToArray());
		}
		
		public void GridCopySelectedToClipboard()
        {
			// Analyze selected range (rows/cols)
            SourceGrid.RangeRegion selRegion = grid1.Selection.GetSelectionRegion();
            int[] selRows = selRegion.GetRowsIndex();
            int[] selCols = selRegion.GetColumnsIndex();
            Array.Sort(selRows);
            Array.Sort(selCols);
            GridCopyToClipboard(selRows, selCols);
            
		}
		
		public void GridCopyToClipboard(int[] selRows, int[] selCols)
        {
            // Declarations
            int minRow = 0, maxRow = 0, minCol = 0, maxCol = 0;
            int rows = 0, cols = 0;

            rows = selRows.Length;
            minRow = selRows[0];
            maxRow = selRows[rows - 1];

            cols = selCols.Length;
            minCol = selCols[0];
            maxCol = selCols[cols - 1];

            // Array tables that allows to find an ouput clipboard range row/col from input cell
            Dictionary<int, int> rowsRefs = new Dictionary<int, int>();
            for (int i = 0; i < rows; i++)
                rowsRefs.Add(selRows[i], i + 1);

            Dictionary<int, int> colsRefs = new Dictionary<int, int>();
            for (int i = 0; i < cols; i++)
                colsRefs.Add(selCols[i], i);

            // Ignore row headers
            if (minCol == 0)
                cols--;
            
            // Copy the headers
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            foreach(int c in selCols)
            {
            	if(grid1[0, c].Value != null)
            		builder.Append(grid1[0, c].Value.ToString());
            	builder.Append("\t");
            }
            builder.AppendLine();
            
            // Copy the data        
            foreach (int r in selRows)
            {
            	foreach(int c in selCols)
            	{
                    if (grid1[r, c].Editor != null)
                    	builder.Append(grid1[r, c].Editor.ValueToString(grid1[r, c].Value));
                    else
                    	builder.Append(grid1[r, c].Value.ToString());
                    builder.Append("\t");
                }
            	builder.AppendLine();
            }

            // New clipboard DataObject
            System.Windows.Forms.DataObject cbDataObject = new System.Windows.Forms.DataObject();
            cbDataObject.SetData("SourceGrid.Grid", this);

            // Copy string object to clipboard
            cbDataObject.SetData(typeof(string), builder.ToString());
            System.Windows.Forms.Clipboard.SetDataObject(cbDataObject);
        }
				
		void MnuOptionsClick(object sender, EventArgs e)
		{
			frmOptions opts = new frmOptions();
			opts.numDisplayResults.Value = settings.QueryResultMaximum;
			opts.txtOracleSchema.Text = settings.OracleIgnoredSchemas;
            opts.txtDelimiter.Text = settings.Delimiter.ToString();
			if(opts.ShowDialog() == DialogResult.OK)
			{
				settings.QueryResultMaximum = (int)opts.numDisplayResults.Value;
				settings.OracleIgnoredSchemas = opts.txtOracleSchema.Text;
                
                if (opts.txtDelimiter.Text.Length > 0)
                    settings.Delimiter = opts.txtDelimiter.Text.First();
                else
                    settings.Delimiter = ',';

				settings.Save(settingsPath);
			}
			
		}
		
		void TabControl1SelectedIndexChanged(object sender, EventArgs e)
		{
			if(tabControl1.SelectedIndex !=  -1)
			{
				textEditorControl1 = (TextEditorControl)tabControl1.SelectedTab.Controls[0];
                grid1 = (SourceGrid.Grid)tabControl1.SelectedTab.Controls[2];
			}
		}
		
		void MnuCloseTabClick(object sender, EventArgs e)
		{
			if(tabControl1.TabPages.Count > 1)
			{
				tabControl1.TabPages.Remove(tabControl1.SelectedTab);
			}
		}

        private void selectAllToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SendKeys.Send("^a");
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmFind.ShowFor(this, false);
        }

        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmFind.ShowFor(this, true);
        }
		
	}
}
