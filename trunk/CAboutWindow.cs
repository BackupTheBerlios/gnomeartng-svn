/*
This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; version 2 of the License.

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

namespace GnomeArtNG
{
	
	
	public class CAboutWindow
	{
		
		private Gtk.Window mainWindow;
		public Gtk.Window MainWindow {
			get {return mainWindow;}
		}
		
		//AboutWindow
		//[Widget] Gtk.Label AboutHeadLabel;
		[Widget] Gtk.Label AboutVersion;
		[Widget] Gtk.Button AboutCloseButton;
		[Widget] Gtk.Image AboutImage;
		
		public CAboutWindow(string Version,bool ShowWindow)	{
			string aboutW="AboutWindow";
			Glade.XML aboutXml= new Glade.XML (null, "gui.glade", aboutW, null);
			aboutXml.Autoconnect (this);
			mainWindow = (Gtk.Window) aboutXml.GetWidget (aboutW);
			AboutVersion.Text=Version;
			AboutCloseButton.Clicked+=new EventHandler(OnAboutCloseButtonClicked);
			AboutImage.Pixbuf = new Gdk.Pixbuf("./gnome.png");
			if(ShowWindow)
				mainWindow.ShowAll();
		}
		
		private void OnAboutCloseButtonClicked (object sender, EventArgs b){
			mainWindow.Destroy();
		}
	}
}
