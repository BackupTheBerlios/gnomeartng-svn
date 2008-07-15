/*
This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; version 2 of the License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

Thomas Beck at 12:20 24.11.2007
*/

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
