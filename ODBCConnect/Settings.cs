using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Cubewise.Query
{
	/// <summary>
	/// Description of Settings.
	/// </summary>
	public class Settings
	{
		
		TripleDESStringEncryptor encypter = new TripleDESStringEncryptor();
		string[] oracleIgnoredSchemaArray;
		string oracleIgnoredSchemas;
		
		public Settings()
		{
			TreeViewWidth = 300;
			GridHeight = 200;
			oracleIgnoredSchemas = "SYS,SYSTEM,CTXSYS,MDSYS,OLAPSYS,XDB,EXFSYS";
			QueryResultMaximum = 200;
            CommandTimeout = 120;
            Delimiter = ',';
		}
		
		public static Settings Load(string path)
		{
			if(!File.Exists(path))
				return new Settings();
					
			XmlSerializer xml = new XmlSerializer(typeof(Settings));
			using(TextReader reader = new StreamReader(path, System.Text.Encoding.ASCII))
			{
				Settings result = (Settings)xml.Deserialize(reader);
				return result;
			}	
		}
			
		public string DSN { get; set; }
		public string SQLStatement { get; set; }
		public string OutputPath { get; set; }
		public string UserName { get; set; }
		public string EncryptedPassword { get; set; }
		public int TreeViewWidth { get; set; }
		public bool TreeViewVisible { get; set; }
		public int GridHeight { get; set; }
		public bool GridVisible { get; set; }
		public int QueryResultMaximum { get; set; }
        public int CommandTimeout { get; set; }
        public char Delimiter { get; set; }
		
		public string OracleIgnoredSchemas 
		{ 
			get {return oracleIgnoredSchemas;}
		 	set 
		 	{
		 		oracleIgnoredSchemas = value;
		 		oracleIgnoredSchemaArray = null;
		 	}
		}
		
		public string[] OracleIgnoredSchemaArray
		{
			get 
			{
				if(OracleIgnoredSchemas==null)
					return new string[]{};
				
				if(oracleIgnoredSchemaArray!=null)
					return oracleIgnoredSchemaArray;
				
				oracleIgnoredSchemaArray = OracleIgnoredSchemas.Split(',').Select(x => x.Trim()).ToArray();
				return oracleIgnoredSchemaArray;
			}
		}
		
		public void SetPassword (string password)
		{
			if(!string.IsNullOrEmpty(password))
				EncryptedPassword = encypter.EncryptString(password);
			else
				EncryptedPassword = "";
		}
		
		public string Password
		{
			get
			{
				if(!string.IsNullOrEmpty(EncryptedPassword))
					return encypter.DecryptString(EncryptedPassword);
				else
					return "";
			}
		}
		
		public void Save (string path)
		{
			XmlSerializer xml = new XmlSerializer(typeof(Settings));
			using(TextWriter writer = new StreamWriter(path, false, System.Text.Encoding.ASCII))
			{
				xml.Serialize(writer, this);
				writer.Flush();
			}		
		}
		
	}
}
