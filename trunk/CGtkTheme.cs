// /home/neo/Projects/GnomeArtNG/GnomeArtNG/CGtkTheme.cs created with MonoDevelop
// User: neo at 12:15 24.11.2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace GnomeArtNG
{
	
	//TODO: Noch zu implementieren! 
	public class CGtkTheme: CTheme
	{
	
		override protected void PreInstallation(CStatusWindow sw){}
		override protected void PostInstallation(CStatusWindow sw){}
		override protected void Installation(CStatusWindow sw){}
		
		public void Install(){
			throw new Exception(Catalog.GetString("Not yet implemented!!"));
		}
		override public void Revert(){
			throw new Exception(Catalog.GetString("Not yet implemented!!"));
		}
		
		public CGtkTheme(CConfiguration config):base(config)
		{
			
		}
	}
}
