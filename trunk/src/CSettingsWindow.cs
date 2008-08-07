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
using System.IO;

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
		[Widget] Gtk.CheckButton SettingsUpdateCb;
		[Widget] Gtk.Button SettingsOkButton;
		[Widget] Gtk.Button SettingsCancelButton;
		[Widget] Gtk.ComboBox SettingsXmlCb;
		[Widget] Gtk.FileChooser SettingsLocationFc;
		[Widget] Gtk.Label SettingsGeneralHeadlineLabel;
		[Widget] Gtk.RadioButton SettingsNoProxyRb;
		[Widget] Gtk.RadioButton SettingsProxyActiveRb;
		[Widget] Gtk.RadioButton SettingsProxySystemActiveRb;		
		[Widget] Gtk.Entry SettingsProxyPort;
		[Widget] Gtk.Entry SettingsProxySystemPort;
		[Widget] Gtk.Entry SettingsProxyAddress;
		[Widget] Gtk.Entry SettingsProxySystemAddress;


		public void Invalidate(){
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration ();
		}
		
		public CSettingsWindow(CConfiguration config, bool ShowWindow)	{
			ProxyAttrStruct proxy;
			this.config=config;
			string settingsW="SettingsWindow";
			Glade.XML settingsXml= new Glade.XML (null, "gui.glade", settingsW, null);
			settingsXml.Autoconnect (this);
			mainWindow = (Gtk.Window) settingsXml.GetWidget (settingsW);
			mainWindow.Title = Catalog.GetString("Settings");
			SettingsGeneralHeadlineLabel.Text = "<b>"+Catalog.GetString("General settings")+"</b>";
			SettingsXmlCb.Active = config.XmlRefreshInterval;
			SettingsLocationFc.SetCurrentFolder(config.ThemesDownloadPath);
            //Events
			SettingsCancelButton.Clicked+=new EventHandler(OnCancelButtonClicked);
			SettingsOkButton.Clicked+=new EventHandler(OnOkButtonClicked);
			SettingsNoProxyRb.Clicked+=new EventHandler(OnProxyClicked);
			SettingsProxyActiveRb.Clicked+=new EventHandler(OnProxyClicked);
			//Proxies
			proxy = config.GetProxy(CConfiguration.ProxyType.ptGang);
			SettingsProxyAddress.Text = proxy.Ip;
			SettingsProxyPort.Text = proxy.Port.ToString();
			proxy = config.GetProxy(CConfiguration.ProxyType.ptSystem);
			SettingsProxySystemAddress.Text = proxy.Ip;
			SettingsProxySystemPort.Text = proxy.Port.ToString();
			SettingsProxySystemPort.Sensitive = false;
			SettingsProxySystemAddress.Sensitive = false;
			SettingsProxySystemActiveRb.Active =  (config.ProxyKind == CConfiguration.ProxyType.ptSystem);
			SettingsNoProxyRb.Active = (config.ProxyKind == CConfiguration.ProxyType.ptNone);
			SettingsProxyActiveRb.Active = (config.ProxyKind == CConfiguration.ProxyType.ptGang);
			
			SettingsUpdateCb.Active = !(config.DontBotherForUpdates);
			config.GConfClient.AddNotify(config.GConfPath+"themesDownloadPath",OnThemesPathChangedEventHandler);
			if(ShowWindow) Show();
		}
				
		private void OnProxyClicked (object sender, EventArgs a){
			bool SettingsActive = !SettingsNoProxyRb.Active;		
			SettingsProxyPort.IsEditable = SettingsActive;
			SettingsProxyPort.Sensitive = SettingsActive;
			SettingsProxyAddress.IsEditable = SettingsActive;
			SettingsProxyAddress.Sensitive = SettingsActive;
		}
		
		public void Close(){
			mainWindow.Destroy();
		}
		
		public void Show(){
			mainWindow.ShowAll();
			Invalidate();
		}
		
		private void OnCancelButtonClicked (object sender, EventArgs a){
			config.GConfClient.RemoveNotify(config.GConfPath+"themesDownloadPath",OnThemesPathChangedEventHandler);
			Close();
		}

		//Apply all settings (eventually refresh critical things)
		private void OnOkButtonClicked (object sender, EventArgs a){
			config.ThemesDownloadPath=SettingsLocationFc.CurrentFolder;
			config.XmlRefreshInterval = SettingsXmlCb.Active;
			ProxyAttrStruct proxy = config.Proxy;
			proxy.Ip = SettingsProxyAddress.Text;
			try{
				proxy.Port = Int32.Parse(SettingsProxyPort.Text);
			}catch{
				proxy.Port = 0;
			}
			
			if (SettingsProxyActiveRb.Active)
				config.ProxyKind = CConfiguration.ProxyType.ptGang;
			else if (SettingsProxySystemActiveRb.Active) 
				config.ProxyKind = CConfiguration.ProxyType.ptSystem;
			else 
				config.ProxyKind =CConfiguration.ProxyType.ptNone;
			
			config.DontBotherForUpdates = !(SettingsUpdateCb.Active);
			OnCancelButtonClicked(sender,a);
		}

		private void OnThemesPathChangedEventHandler(object sender, GConf.NotifyEventArgs a){
			string path = (string)config.GConfClient.Get(config.GConfPath+"themesDownloadPath");
			if (Directory.Exists(path))
			    SettingsLocationFc.SetCurrentFolder(path);
			else {
				new CInfoWindow(Catalog.GetString("Error"),Catalog.GetString("The download location has been changed manually! "+
				                                                             "Normally, this is no problem at all! But if you change "+
				                                                             "the location to a place that doesn't exists then it will "+
				                                                             "be a problem!\n\n So this is a \"Location couldn't be "+
				                                                             "found\" message. Please correct the download location!"),Gtk.Stock.DialogError,true);
			}
		}
		
	}
}
