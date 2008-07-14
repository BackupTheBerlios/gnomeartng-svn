/*
This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; version 3 of the License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
*/
// Thomas Beck at 14:54 07.07.2008

using System;
using Gtk;
using Gdk;
using Glade;

namespace GnomeArtNG
{
	public enum WindowShowType:int{
		wstNo,
		wstNormal,
		wstModal
	}
	
	public class CWindow
	{
		protected Gtk.Window mainWindow;
		protected CConfiguration config;
		protected bool IsShownModal = false;
		public Gtk.Window MainWindow {
			get {return mainWindow;}
		}
		
		public CWindow(CConfiguration config, string WindowName, string WindowTitle, WindowShowType ShowType) {
			this.config = config;
			string win = WindowName;
			Glade.XML settingsXml = new Glade.XML (null, "gui.glade", win, null);
			settingsXml.Autoconnect (this);
			mainWindow = (Gtk.Window) settingsXml.GetWidget (win);
			mainWindow.Title = WindowTitle;
			mainWindow.DeleteEvent += new DeleteEventHandler(OnWindowDeleteEvent);
			IsShownModal = (ShowType == WindowShowType.wstModal);
			switch (ShowType){
			case WindowShowType.wstModal: ShowModal(); break;
			case WindowShowType.wstNormal: Show(); break;
			case WindowShowType.wstNo: break;
			}
					
		}
		protected void OnWindowDeleteEvent (object sender, DeleteEventArgs a) {
			Close();
		}
		
		//Gtk.Object[] HideArray
		protected void Show() {
			mainWindow.ShowAll();
			Invalidate();
		}
		
		protected void onDestroyHandler(object Sender, DestroyEventArgs args){
			Close();
		}

		public void ShowModal(){
			mainWindow.ShowAll();
			IsShownModal = true;
			Application.Run();
		}
		
		public void Invalidate(){
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration ();
		}
		
		public void Close(){
			if (IsShownModal)
				Application.Quit();
			mainWindow.Destroy();
		}
	}
}
