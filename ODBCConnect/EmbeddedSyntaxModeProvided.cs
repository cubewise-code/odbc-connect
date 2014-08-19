// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

using ICSharpCode.TextEditor.Document;

namespace Cubewise.Query
{
	
	public class EmbeddedSyntaxModeProvider : ISyntaxModeFileProvider
	{
		List<SyntaxMode> _syntaxModes = new List<SyntaxMode>();
		Assembly _assembly;
		
		public ICollection<SyntaxMode> SyntaxModes {
			get {
				return _syntaxModes;
			}
		}
		
		public EmbeddedSyntaxModeProvider(Assembly assembly)
		{
			_assembly = assembly;
			
			foreach(string resName in assembly.GetManifestResourceNames())
			{
				if( resName.EndsWith(".xshd") )
				{
					Stream syntaxModeStream = assembly.GetManifestResourceStream(resName);
					XmlDocument xml = new XmlDocument();
					xml.Load(syntaxModeStream);
					XmlNodeList nodes = xml.SelectNodes("/SyntaxDefinition");
					if(nodes.Count > 0)
					{
						XmlAttributeCollection attrs = nodes[0].Attributes;
						SyntaxMode syntax = new SyntaxMode(resName, attrs.GetNamedItem("name").Value, attrs.GetNamedItem("extensions").Value);
						_syntaxModes.Add(syntax);
					}
				}
				
			}	
		}
		
		public EmbeddedSyntaxModeProvider(): this(typeof(EmbeddedSyntaxModeProvider).Assembly)
		{
	
		}
		
		public XmlTextReader GetSyntaxModeFile(SyntaxMode syntaxMode)
		{
			return new XmlTextReader(_assembly.GetManifestResourceStream(syntaxMode.FileName));
		}
		
		public void UpdateSyntaxModeList()
		{
			// resources don't change during runtime
		}
	}
}
