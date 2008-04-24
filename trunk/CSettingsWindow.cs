/*
This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; version 3 of the License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

//Created by Thomas Beck at 13:50 22.04.2008
*/


using System;
using Gtk;
using Gdk;
using Glade;
using GnomeArtNG;
using Mono.Unix;

namespace GnomeArtNG
{
	public class CSettingsWindow
	{
		private Gtk.Window mainWindow;
		private CConfiguration config;
		public Gtk.Window MainWindow {
			get {return mainWindow;}
		}
		
		//SettingsWindow
		/*[Widget] Gtk.Label SettingsHeadLabel;
		[Widget] Gtk.Label SettingsMainLabel;
		[Widget] Gtk.Button SettingsCancelButton;
		[Widget] Gtk.Label SettingsExtInfoLabel;
		[Widget] Gtk.Expander ExtendedInfoExpander;
		*/
		[Widget] Gtk.Button SettingsOkButton;
		[Widget] Gtk.Button SettingsCancelButton;
		[Widget] Gtk.ComboBox SettingsXmlCb;
		[Widget] Gtk.FileChooser SettingsLocationFc;		
		
		public void Invalidate(){
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration ();
		}
		
		public CSettingsWindow(CConfiguration config, bool ShowWindow)	{
			this.config=config;
			string settingsW="SettingsWindow";
			Glade.XML settingsXml= new Glade.XML (null, "gui.glade", settingsW, null);
			settingsXml.Autoconnect (this);
			mainWindow = (Gtk.Window) settingsXml.GetWidget (settingsW);
			mainWindow.Title = Catalog.GetString("Settings");
			SettingsXmlCb.Active = (int)config.XmlRefresh;
			SettingsLocationFc.SetCurrentFolder(config.ThemesDownloadPath);
			SettingsCancelButton.Clicked+=new EventHandler(OnCancelButtonClicked);
			SettingsOkButton.Clicked+=new EventHandler(OnOkButtonClicked);
			if(ShowWindow) Show();
		}
				
		public void Close(){
			mainWindow.Destroy();
		}
		
		public void Show(){
			mainWindow.ShowAll();
			Invalidate();
		}
		
		private void OnCancelButtonClicked (object sender, EventArgs a){
			Close();
		}

		//Apply all settings (eventually refresh critical things)
		private void OnOkButtonClicked (object sender, EventArgs a){
			config.ThemesDownloadPath=SettingsLocationFc.CurrentFolder;
			config.XmlRefresh = (CConfiguration.XmlRefreshInterval)SettingsXmlCb.Active;
			Close();
		}
	}
}
