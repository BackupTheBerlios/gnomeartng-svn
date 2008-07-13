/*
This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; version 3 of the License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
*/
//
// CWindow.cs created with MonoDevelop
// Thomas Beck at 14:54 07.07.2008

using System;
using Gtk;
using Gdk;
using Glade;

namespace GnomeArtNG
{
	
	public class CWindow
	{
		protected Gtk.Window mainWindow;
		protected CConfiguration config;
		public Gtk.Window MainWindow {
			get {return mainWindow;}
		}
		
		public CWindow(CConfiguration config, string WindowName, string WindowTitle, bool ShowWindow) {
			this.config = config;
			string win = WindowName;
			Glade.XML settingsXml = new Glade.XML (null, "gui.glade", win, null);
			settingsXml.Autoconnect (this);
			mainWindow = (Gtk.Window) settingsXml.GetWidget (win);
			mainWindow.Title = WindowTitle;
			if(ShowWindow)
				Show();
		}
		//Gtk.Object[] HideArray
		protected void Show() {
			mainWindow.ShowAll();
			Invalidate();
		}
		
		public void Invalidate(){
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration ();
		}
		
		public void Close(){
			mainWindow.Destroy();
		}
	}
}
