// /home/neo/Projects/GnomeArtNG/GnomeArtNG/CBackgroundImage.cs created with MonoDevelop
// User: neo at 12:20 24.11.2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace GnomeArtNG
{
	
	public class CBackgroundImage
	{
		
		public string URL = "";
		public string Type;
		public int xResolution=1024;
		public int yResolution=768;
		public int imageNr=0;
		
		public CBackgroundImage(){

		}
		
		//In version2 dafür gedacht, das Hintergrundbild auf eine beliebige Größe zu stutzen 
		public void Cut(){
		
		}
	}
}
