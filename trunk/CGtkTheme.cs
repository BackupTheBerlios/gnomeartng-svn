// /home/neo/Projects/GnomeArtNG/GnomeArtNG/CGtkTheme.cs created with MonoDevelop
// User: neo at 12:15 24.11.2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace GnomeArtNG
{
	
	
	public class CGtkTheme: CTheme
	{
	
		override protected void PreInstallation(CStatusWindow sw){}
		override protected void PostInstallation(CStatusWindow sw){}
		override protected void Installation(CStatusWindow sw){}
		
		public void Install(){}
		override public void Revert(){}
		public CGtkTheme(CConfiguration config):base(config)
		{
		}
	}
}
