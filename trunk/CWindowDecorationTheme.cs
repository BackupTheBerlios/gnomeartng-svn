// /home/neo/Projects/Monodevelop-Projects/GnomeArtNG/CWindowDecorationTheme.cs created with MonoDevelop
// User: neo at 00:11 03.12.2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace GnomeArtNG
{
	
	
	public class CWindowDecorationTheme:CTheme
	{
		override protected void PreInstallation(CStatusWindow sw){}
		override protected void PostInstallation(CStatusWindow sw){}
		override protected void Installation(CStatusWindow sw){}
		
		public void Install(){}
		
		override public void Revert(){}
		public CWindowDecorationTheme(CConfiguration config):base(config)
		{
		}
	}
}
