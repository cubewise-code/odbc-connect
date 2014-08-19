using System;

namespace Cubewise.Query
{
	/// <summary>
	/// Summary description for ODBCDSN.
	/// </summary>
	public class ODBCDSN
	{
		private string m_DSNName = null;
		private string m_DSNDriverName = null;
		private string m_DSNDescription = null;
		private string m_DSNServerName = null;
		private string m_DSNDrivr = null;
		private ODBCDSN(string dsnName, string dsnDriverName,
			string description, string server, string driver)
		{
			m_DSNName = dsnName;
			m_DSNDriverName = dsnDriverName;
			m_DSNDescription = description;
			m_DSNServerName = server;
			m_DSNDrivr = driver;
		}

		public static ODBCDSN ParseForODBCDSN(string dsnName, string dsnDriverName,
			string []dsnElements, string[] dsnElmVals)
		{
			ODBCDSN odbcdsn = null;

			if (dsnElements != null && dsnElmVals != null)
			{
				int i=0;
				string description = null;
				string server = null;
				string driver = null;

				// For each element defined for a typical DSN get
				// its value.
				foreach (string dsnElement in dsnElements)
				{
					switch (dsnElement.ToLower())
					{
						case "description":
							description = dsnElmVals[i];
							break;
						case "server":
							server = dsnElmVals[i];
							break;
						case "driver":
							driver = dsnElmVals[i];
							break;
					}
					i++;
				}
				odbcdsn = new ODBCDSN(dsnName, dsnDriverName, 
					description, server, driver); 
			}
			return odbcdsn;
		}

		public string GetDSNName()
		{
			return m_DSNName;
		}

		public string GetDSNDriverName()
		{
			return m_DSNDriverName;
		}

		public string GetDSNDescription()
		{
			return m_DSNDescription;
		}

		public string GetDSNServerName()
		{
			return m_DSNServerName;
		}

		public string GetDSNDriverPath()
		{
			return m_DSNDrivr;
		}

		public override string ToString()
		{
			return GetDSNName();
		}
	}
}
