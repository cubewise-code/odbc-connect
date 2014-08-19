using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data;
using System.Data.Odbc;
using System.Runtime.InteropServices;
using System.Linq;

using CsvHelper;

using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Layouts;
using NLog.LayoutRenderers;
using NLog.Win32.Targets;

namespace Cubewise.Query
{
	class Program
	{
		
		[DllImport("user32.dll")]        
        public static extern IntPtr FindWindow(string lpClassName,string lpWindowName);   
     
        [DllImport("user32.dll")]       
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
		
		internal static Logger logger;
        internal static string CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        internal static BackgroundWorker worker;
        internal static IDictionary<string, string> args = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static void CreateLogger(string loglevel)
        {

            // Create Tab format
            CsvLayout layout = new CsvLayout();
            layout.Delimiter = CsvLayout.ColumnDelimiterMode.Tab;
            layout.Columns.Add(new CsvColumn("date", "${longdate}"));
            layout.Columns.Add(new CsvColumn("level", "${level}"));
            layout.Columns.Add(new CsvColumn("message", "${message}"));
            layout.Columns.Add(new CsvColumn("exception", "${exception:format=tostring}"));


            FileTarget target = new FileTarget();
            target.FileName = "${basedir}/QueryLog.txt";
            target.CompiledLayout = layout;
            target.ArchiveFileName = "${basedir}/archives/Querylog.{#####}.txt";
            target.ArchiveAboveSize = 1000 * 1024; // archive files greater than 10 KB
            target.ArchiveNumbering = FileTarget.ArchiveNumberingMode.Sequence;

            LogLevel level;

            switch (loglevel.ToLower())
            {
                case "trace":
                    level = LogLevel.Trace;
                    break;
                case "debug":
                    level = LogLevel.Debug;
                    break;
                case "warn":
                    level = LogLevel.Warn;
                    break;
                case "error":
                    level = LogLevel.Error;
                    break;
                case "fatal":
                    level = LogLevel.Fatal;
                    break;
                default:
                    level = LogLevel.Info;
                    break;
            }

            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, level);

            logger = LogManager.GetLogger("Query");

        }

		
		[STAThread]
		public static int Main(string[] args)
		{
			
			Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(AppDomain_CurrentDomain_UnhandledException);
		
			for (int i = 0; i < args.Length; i++)
            {
            	string[] argParts = args[i].Split(new char[]{'='}, 2);

                if (argParts.Length == 1)
                {
                    Program.args.Add(argParts[0], "");
                }
                else if (argParts.Length == 2)
                {
                	string name = argParts[0];
                	string val = argParts[1];
                    Program.args.Add(name, val);
                }
            }
				
			// Setup logging
			if(Program.args.ContainsKey("loglevel"))
				CreateLogger(Program.args["loglevel"]);
			else
				CreateLogger("Error");	
			
			if (args.Length == 0 || (args.Length == 1 && args[0].StartsWith("loglevel", StringComparison.CurrentCultureIgnoreCase)))
			{
			    Console.Title = "ODBC Connect";
	            setConsoleWindowVisibility(false, Console.Title);             
				
	            logger.Info("Starting application");
	            
	            Application.EnableVisualStyles();
	            Application.SetCompatibleTextRenderingDefault(false);
	            Application.Run(new frmMain());
	            
	            return 0;

				
			}
			else
			{
	            
	            return RunCommandLine();
	            				
			}
					
		}

		static void AppDomain_CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			if(logger != null)
				logger.ErrorException("An thread exception error occurred", (Exception)e.ExceptionObject);
			else
			{
				if (!System.Diagnostics.EventLog.SourceExists("Query"))
					System.Diagnostics.EventLog.CreateEventSource("Query", "Application");
		
				System.Diagnostics.EventLog log = new System.Diagnostics.EventLog("Application");
				log.Source = "Query";
				log.WriteEntry(e.ExceptionObject.ToString());
			}
		}
		
		static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
			if(logger != null)
            	logger.ErrorException("An thread exception error occurred", e.Exception);
			else
			{
				if (!System.Diagnostics.EventLog.SourceExists("Query"))
					System.Diagnostics.EventLog.CreateEventSource("Query", "Application");
		
				System.Diagnostics.EventLog log = new System.Diagnostics.EventLog("Application");
				log.Source = "Query";
				log.WriteEntry(e.Exception.ToString());
			}
        }
		
		static int RunCommandLine()
		{	
			
			if(! args.ContainsKey("user"))
				args.Add("user","");
			
			if(! args.ContainsKey("pass"))
				args.Add("pass","");
			
			int result = 0;
			
			if(! args.ContainsKey("dsn"))
			{
				logger.Error("The command line argument 'dsn' must be provided");
				result = 1;
			}
			
			if(! args.ContainsKey("sql"))
			{
				logger.Error("The command line argument 'sql' must be provided");
				result = 1;
			}
			
			if(! args.ContainsKey("output"))
			{
				logger.Error("The command line argument 'output' must be provided");
				result = 1;
			}
			
			if(result > 0)
			{
				Console.WriteLine("An error occurred, refer to QueryLog.txt for details");
				return result;
			}
			
			// Decrypt the password
			if(args["pass"].StartsWith("###:"))
			{
				TripleDESStringEncryptor encrypt = new TripleDESStringEncryptor();
				string encryptedPass = args["pass"];
				if(encryptedPass.Length > 4)
				{
					string decryptedPass = encrypt.DecryptString(args["pass"].Substring(4));
					args["pass"] = decryptedPass;
				}
				else 
					args["pass"] = "";
			}

            char delimiter = ',';
            if (args.ContainsKey("delimiter"))
            {
                delimiter = args["delimiter"].First();
            }
			
			result = Export(false, args["dsn"], args["user"], args["pass"], args["sql"], args["output"], delimiter, null);
			
			logger.Info("Result: " + result.ToString());
			
			if(result > 0)
				Console.WriteLine("An error occurred, refer to QueryLog.txt for details");
			
			return result;
			
		}
		
		static void setConsoleWindowVisibility(bool visible, string title)       
        {             
            // below is Brandon's code            
            //Sometimes System.Windows.Forms.Application.ExecutablePath works for the caption depending on the system you are running under.           
            IntPtr hWnd = FindWindow(null, title); 
         
            if (hWnd != IntPtr.Zero)            
            {               
                if (!visible)                   
                    //Hide the window                    
                    ShowWindow(hWnd, 0); // 0 = SW_HIDE                
                else                   
                     //Show window again                    
                    ShowWindow(hWnd, 1); //1 = SW_SHOWNORMA           
             }        
        }
		
		
		internal static int Export(bool gui, string dsn, string userName, string password, string sql, string outputPath, char delimiter, DoWorkEventArgs e)
		{

            try
            {
                if (File.Exists(outputPath))
                    File.Delete(outputPath);

            }
            catch (Exception ex)
            {
                logger.ErrorException("Unable to delete file: " + outputPath, ex);
                return 1;
            }

			logger.Info("DSN: " + dsn);
			string conString = string.Format("DSN={0};Uid={1};Pwd={2};", dsn, userName, password);
			logger.Debug("Connection string: " + conString);
			using(OdbcConnection con = new OdbcConnection(conString))
			{
				try
				{
					logger.Debug("Opening database connection");
					con.Open();
					OdbcCommand com = new OdbcCommand(sql, con);
								
					logger.Debug("Executing command");
					
					if(worker != null)
					{
						worker.ReportProgress(1, "Executing sql statement");
					}
					
					logger.Info("Running SQL statement:\n" + sql);
					using(IDataReader r = com.ExecuteReader())
					{
						logger.Debug("Opening text writer");
						
						if(gui)
						{
							worker.ReportProgress(1, "Outputting records");
						}
						using (TextWriter tw = new StreamWriter(outputPath, false, System.Text.Encoding.UTF8))
				       	{

                            CsvWriterOptions options = new CsvWriterOptions();
                            options.HasHeaderRecord = true;
                            options.Delimiter = delimiter;
							
							CsvWriter writer = new CsvWriter(tw, options);
							
							logger.Debug("Writing headers");
							// Write the header
							for(int i = 0; i < r.FieldCount; i++) {
								writer.WriteField(r.GetName(i));
							}

                            writer.NextRecord();
							
							int count = 0;
							int lastCount = 0;
							int gap = 1000;
							if(!gui)
								gap = 10000;
							
							Random rnd = new Random();
							
							logger.Debug("Writing data...");
							// Write the values
							while (r.Read()) {
								
								if(gui && worker.CancellationPending)
								{
									e.Cancel = true;
									break;
								}
								
								count++;

								
								//logger.Debug("Writing record: " + count.ToString("N0"));
								
								// Output all fields
								for(int i = 0; i < r.FieldCount; i++) {
									if(r.IsDBNull(i))
										writer.WriteField("");
									else if(r.GetFieldType(i) == typeof(DateTime))
										writer.WriteField(r.GetDateTime(i).ToString("yyyy-MM-dd HH:mm:ss"));
									else
										writer.WriteField(r[i].ToString().Trim());
								}

                                writer.NextRecord();
								
								if(count >= lastCount + (gap + gap * rnd.NextDouble()))
								{
									if(gui)
									{
										worker.ReportProgress(count);										
									}
									else 
									{
										Console.Write(count.ToString ("N0") + "   ");
									}
									
									lastCount = count;
								}
							}

                            tw.Flush();

				       	}
					}
					
					if(gui)
					{
						worker.ReportProgress(0);
					}

					
					logger.Debug("Process complete");
					
				}
				catch(Exception ex)
				{				
					Program.logger.ErrorException("Export", ex);
					if(gui)
					{
						throw ex;
					}	
					return 1;
				}
			}
			
			return 0;
		}
		        
		        
	}
}