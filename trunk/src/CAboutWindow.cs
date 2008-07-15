/*
This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; version 3 of the License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

Created by Thomas Beck at 17:50 13.12.2007
*/


using System;
using Gtk;
using Gdk;
using Glade;
using GnomeArtNG;
using Mono.Unix;

namespace GnomeArtNG
{
	
	
	public class CAboutWindow : CWindow
	{
		private static string windowName="AboutWindow";
		private static string windowTitle = Catalog.GetString("About")+" "+ Catalog.GetString("Gnome-Art-NG");	
		
		//AboutWindow
		[Widget] Gtk.Label AboutVersion;
		[Widget] Gtk.Button AboutCloseButton;
		[Widget] Gtk.Image AboutImage;
		
		public CAboutWindow(CConfiguration config,bool ShowWindow):base(config, windowName, windowTitle, WindowShowType.wstNo) {
			AboutVersion.Text = CConfiguration.Version;
			AboutCloseButton.Clicked+=new EventHandler(OnAboutCloseButtonClicked);
			AboutImage.Pixbuf = CUtility.GetPixbuf("./images/gnome.png", config);
			if(ShowWindow)
				mainWindow.ShowAll();
		}
		
		private void OnAboutCloseButtonClicked (object sender, EventArgs b){
			Close();
		}
	}
}
