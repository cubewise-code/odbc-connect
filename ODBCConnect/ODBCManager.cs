using System;
using System.Collections;
using Microsoft.Win32;

namespace Cubewise.Query
{
	public enum ODBC_DRIVERS {SQL_SERVER=0};
	/// <summary>
	/// ODBCManager is the class that provides static methods, which provide
	/// access to the various ODBC components such as Drivers List, DSNs List
	/// etc. 
	/// </summary>
	public class ODBCManager
	{
		private const string ODBC_LOC_IN_REGISTRY = "SOFTWARE\\ODBC\\";
		private const string ODBC_INI_LOC_IN_REGISTRY = 
			ODBC_LOC_IN_REGISTRY+"ODBC.INI\\";

		private const string DSN_LOC_IN_REGISTRY = 
			ODBC_INI_LOC_IN_REGISTRY+ "ODBC Data Sources\\";

		private const string ODBCINST_INI_LOC_IN_REGISTRY = 
			ODBC_LOC_IN_REGISTRY+"ODBCINST.INI\\";

		private const string ODBC_DRIVERS_LOC_IN_REGISTRY = 
			ODBCINST_INI_LOC_IN_REGISTRY+"ODBC Drivers\\";

		private ODBCManager()
		{
		}

		/// <summary>
		/// Method that gives the ODBC Drivers installed in the local machine.
		/// </summary>
		/// <returns></returns>
		public static ODBCDriver[] GetODBCDrivers()
		{
			ArrayList driversList = new ArrayList();
			ODBCDriver[] odbcDrivers=null;

			// Get the key for
			// "KHEY_LOCAL_MACHINE\\SOFTWARE\\ODBC\\ODBCINST.INI\\ODBC Drivers\\"
			// (ODBC_DRIVERS_LOC_IN_REGISTRY) that contains all the drivers
			// that are installed in the local machine.
			RegistryKey odbcDrvLocKey = OpenComplexSubKey(Registry.LocalMachine, 
				ODBC_DRIVERS_LOC_IN_REGISTRY, false);

			if (odbcDrvLocKey != null)
			{
				// Get all Driver entries defined in ODBC_DRIVERS_LOC_IN_REGISTRY.
				string [] driverNames =  odbcDrvLocKey.GetValueNames();
				odbcDrvLocKey.Close();
				if (driverNames != null)
				{
					// Foreach Driver entry in the ODBC_DRIVERS_LOC_IN_REGISTRY,
					// goto the Key ODBCINST_INI_LOC_IN_REGISTRY+driver and get
					// elements of the DSN entry to create ODBCDSN objects.
					foreach (string driverName in driverNames)
					{
						ODBCDriver odbcDriver = GetODBCDriver(driverName);
						if (odbcDriver != null)
							driversList.Add(odbcDriver);
					}

					if (driversList.Count>0)
					{
						// Create ODBCDriver objects equal to number of valid objects
						// in the ODBC Drivers ArrayList.
						odbcDrivers = new ODBCDriver[driversList.Count];
						driversList.CopyTo(odbcDrivers,0);
					}
				}
			}
			return odbcDrivers;
		}

		/// <summary>
		/// Method thar returns driver object based on the driver name.
		/// </summary>
		/// <param name="driverName"></param>
		/// <returns>ODBCDriver object</returns>
		public static ODBCDriver GetODBCDriver(string driverName)
		{
			int j=0;
			ODBCDriver odbcDriver = null;
			string [] driverElements = null;
			string [] driverElmVals = null;
			RegistryKey driverNameKey = null;

			// Get the key for ODBCINST_INI_LOC_IN_REGISTRY+dsnName.
			driverNameKey = OpenComplexSubKey(Registry.LocalMachine,
				ODBCINST_INI_LOC_IN_REGISTRY+driverName,false);

			if (driverNameKey != null)
			{
				// Get all elements defined in the above key
				driverElements = driverNameKey.GetValueNames();

				// Create Driver Element values array.
				driverElmVals = new string[driverElements.Length];

				// For each element defined for a typical Driver get
				// its value.
				foreach (string driverElement in driverElements)
				{
					driverElmVals[j] = driverNameKey.GetValue(driverElement).ToString();
					j++;
				}

				// Create ODBCDriver Object.
				odbcDriver = ODBCDriver.ParseForDriver(driverName,
					driverElements, driverElmVals);

				driverNameKey.Close();
			}
			return odbcDriver;
		}
		/// <summary>
		/// Method that gives the System Data Source Name (DSN) entries as
		/// array of ODBCDSN objects.
		/// </summary>
		/// <returns>Array of System DSNs</returns>
		public static ODBCDSN[] GetSystemDSNList()
		{
			return GetDSNList(Registry.LocalMachine);
		}

		/// <summary>
		/// Method that returns one System ODBCDSN Object.
		/// </summary>
		/// <param name="dsnName"></param>
		/// <returns></returns>
		public static ODBCDSN GetSystemDSN(string dsnName)
		{
			return GetDSN(Registry.LocalMachine,dsnName);
		}

		/// <summary>
		/// Method that gives the User Data Source Name (DSN) entries as
		/// array of ODBCDSN objects.
		/// </summary>
		/// <returns>Array of User DSNs</returns>
		public static ODBCDSN[] GetUserDSNList()
		{
			return GetDSNList(Registry.CurrentUser);
		}

		/// <summary>
		/// Method that returns one User ODBCDSN Object.
		/// </summary>
		/// <param name="dsnName"></param>
		/// <returns></returns>
		public static ODBCDSN GetUserDSN(string dsnName)
		{
			return GetDSN(Registry.CurrentUser,dsnName);
		}

		/// <summary>
		/// Method that gives the Data Source Name (DSN) entries as array of
		/// ODBCDSN objects.
		/// </summary>
		/// <returns>Array of DSNs based on the baseKey parameter</returns>
		private static ODBCDSN[] GetDSNList(RegistryKey baseKey)
		{
			ArrayList dsnList = new ArrayList();
			ODBCDSN[] odbcDSNs= new ODBCDSN[]{};

			if (baseKey == null)
				return null;

			// Get the key for (using the baseKey parmetre passed in)
			// "\\SOFTWARE\\ODBC\\ODBC.INI\\ODBC Data Sources\\" (DSN_LOC_IN_REGISTRY)
			// that contains all the configured Data Source Name (DSN) entries.
			RegistryKey dsnNamesKey = OpenComplexSubKey(baseKey, 
				DSN_LOC_IN_REGISTRY, false);

			if (dsnNamesKey != null)
			{
				// Get all DSN entries defined in DSN_LOC_IN_REGISTRY.
				string [] dsnNames =  dsnNamesKey.GetValueNames();
				if (dsnNames != null)
				{
					// Foreach DSN entry in the DSN_LOC_IN_REGISTRY, goto the
					// Key ODBC_INI_LOC_IN_REGISTRY+dsnName and get elements of
					// the DSN entry to create ODBCDSN objects.
					foreach (string dsnName in dsnNames)
					{
						// Get ODBC DSN object.
						ODBCDSN odbcDSN=GetDSN(baseKey,dsnName);
						if(odbcDSN != null)
							dsnList.Add(odbcDSN);
					}
					if (dsnList.Count>0)
					{
						// Create ODBCDSN objects equal to number of valid objects
						// in the DSN ArrayList.
						odbcDSNs = new ODBCDSN[dsnList.Count];
						dsnList.CopyTo(odbcDSNs,0);
					}
				}

				dsnNamesKey.Close();
			}
			return odbcDSNs;
		}

		/// <summary>
		/// Method that gives one ODBC DSN object
		/// </summary>
		/// <param name="baseKey"></param>
		/// <param name="dsnName"></param>
		/// <returns>ODBC DSN object</returns>
		private static ODBCDSN GetDSN(RegistryKey baseKey, string dsnName)
		{
			int j=0;
			string dsnDriverName = null;
			RegistryKey dsnNamesKey = null;
			RegistryKey dsnNameKey = null;
			string [] dsnElements = null;
			string [] dsnElmVals = null;
			ODBCDSN odbcDSN = null;

			// Get the key for (using the baseKey parmetre passed in)
			// "\\SOFTWARE\\ODBC\\ODBC.INI\\ODBC Data Sources\\" (DSN_LOC_IN_REGISTRY)
			// that contains all the configured Data Source Name (DSN) entries.
			dsnNamesKey = OpenComplexSubKey(baseKey, DSN_LOC_IN_REGISTRY, false);

			if (dsnNamesKey != null)
			{
				// Get the name of the driver for which the DSN is 
				// defined.
				dsnDriverName = dsnNamesKey.GetValue(dsnName).ToString();
				dsnNamesKey.Close();
			}

			// Get the key for ODBC_INI_LOC_IN_REGISTRY+dsnName.
			dsnNameKey = OpenComplexSubKey(baseKey,
				ODBC_INI_LOC_IN_REGISTRY+dsnName,false);

			if (dsnNameKey != null)
			{
				// Get all elements defined in the above key
				dsnElements = dsnNameKey.GetValueNames();

				// Create DSN Element values array.
				dsnElmVals = new string[dsnElements.Length];

				// For each element defined for a typical DSN get
				// its value.
				foreach (string dsnElement in dsnElements)
				{
					dsnElmVals[j] = dsnNameKey.GetValue(dsnElement).ToString();
					j++;
				}

				// Create ODBCDSN Object.
				odbcDSN = ODBCDSN.ParseForODBCDSN(dsnName,dsnDriverName,
					dsnElements, dsnElmVals);

				dsnNamesKey.Close();
			}
			return odbcDSN;
		}

		/// <summary>
		/// Method that returns the registry key for a complex string that
		/// represents the key that is to be returned. The 'baseKey' parameter
		/// passed to this method is the root registry key on which the
		/// complex sub key has to be created. The 'complexKeyStr' is the 
		/// stirng representation of the key to be created over the 'baseKey'.
		/// The syntax of the 'complexKeyStr' is "KEY1//KEY2//KEY3//...//".
		/// The "..." in the above syntax represents the repetetion. This
		/// method parses the 'compleKeyStr' parameter value and keep building
		/// the keys following the path of the keys listed in the string. Each
		/// key is built upon its previous key. First Key (KEY1) is built based
		/// on the 'basKey' parameter. Second key (KEY2) is based on the first
		/// key (Key creatd for KEY1) and so on... . The writable parameter 
		/// represents whether final key has to be writable or not.
		/// </summary>
		/// <param name="baseKey"></param>
		/// <param name="complexKeyStr"></param>
		/// <param name="writable"></param>
		/// <returns>RegistryKey For the complex Subkey. </returns>
		public static RegistryKey OpenComplexSubKey(RegistryKey baseKey,
			string complexKeyStr, bool writable)
		{
			int prevLoc=0,currLoc = 0;
			string subKeyStr=complexKeyStr;
			RegistryKey finalKey = baseKey;

			if (baseKey == null)
				return null;

			if (complexKeyStr == null)
				return finalKey;

			// Search for the occurence of "\\" character in the complex string
			// and get all the characters upto "\\" from the start of search
			// point (prevLoc) as the keyString. Open a key out of string 
			// keyString.
			do
			{
				currLoc=complexKeyStr.IndexOf("\\",prevLoc);
				if (currLoc != -1)
				{
					subKeyStr = complexKeyStr.Substring(prevLoc, currLoc-prevLoc);
					prevLoc=currLoc+1;
				}
				else
				{
					subKeyStr = complexKeyStr.Substring(prevLoc);
				}

				if (!subKeyStr.Equals(string.Empty))
					finalKey = finalKey.OpenSubKey(subKeyStr, writable);
			}
			while(currLoc != -1);
			return finalKey;
		}
	}
}
