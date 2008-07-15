/*
This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; version 2 of the License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

Thomas Beck at 12:15 24.11.2007
*/
				
			
using System;
using Mono.Unix;

namespace GnomeArtNG
{
	
	//TODO: Noch zu implementieren! 
	public class CGtkTheme: CTheme
	{
	
		override protected void PreInstallation(CStatusWindow sw){}
		override protected void PostInstallation(CStatusWindow sw){}
		override protected void Installation(CStatusWindow sw){
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
