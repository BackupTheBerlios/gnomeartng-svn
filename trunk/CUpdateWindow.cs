/*
	This program is free software; you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation; version 3 of the License.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	Created by Thomas Beck at 22:32 11.06.2008
*/


using System;
using Gtk;
using Gdk;
using Glade;
using GnomeArtNG;
using Mono.Unix;
using System.IO;

namespace GnomeArtNG {
	
	public class CUpdateWindow : CWindow
	{
		
		private static string windowName="UpdateWindow";
		private static string windowTitle = Catalog.GetString("Find and install software-updates");
		public bool RestartRequested = false;
		//updateWindow
		[Widget] Gtk.ProgressBar UpdateProgressBar;
		[Widget] Gtk.Button UpdateHeaderButton;		
		[Widget] Gtk.Button UpdateDonateButton;
		[Widget] Gtk.Button UpdateCloseButton;
		[Widget] Gtk.Button UpdateStatusButton;
		[Widget] Gtk.Image UpdateHeaderImage;
		[Widget] Gtk.Image UpdateDonateButtonImage;
		[Widget] Gtk.Label UpdateCurrentVersionLabel;
		[Widget] Gtk.Label UpdateNewVersionLabel;
		[Widget] Gtk.Image UpdateStatusImage;
		[Widget] Gtk.Label UpdateStatusLabel;
		[Widget] Gtk.Label UpdateDonateLabel;
		[Widget] Gtk.Label UpdateHeaderLabel;
		
		public CUpdateWindow(CConfiguration config):base(config, windowName, windowTitle, WindowShowType.wstNo) {
			initializeWidgets();
			UpdateProgressBar.Hide();
			UpdateStatusButton.IsFocus = true;
		}
		
		private void onCloseButtonClicked (object sender, EventArgs a){
			Close();
		}
		
		private void onInstallClicked (object sender, EventArgs a){
			string tmpfile="/tmp/newgang.deb";
			UpdateProgressBar.Show();
			try {
				new CFileDownloader(config).DownloadFile(config.NewestVersionDownloadLocation, tmpfile,UpdateProgressBar);
				if (File.Exists(tmpfile)){
					CUtility.ExecuteSu(config,"gdebi-gtk "+tmpfile);						
				}
				removeClickHandler(onInstallClicked);
				setHeaderLabelText(Catalog.GetString("All operations are done, do you like to restart GANG?"));
				UpdateStatusLabel.Text=Catalog.GetString("Restart app?");
				UpdateStatusImage.Stock = Gtk.Stock.DialogQuestion;
				addClickHandler(onRestartClicked);
			} 			
			catch {
			}
		}
		private void removeClickHandler(EventHandler handler){
			UpdateStatusButton.Clicked-=handler;
			UpdateHeaderButton.Clicked-=handler;
		}
		
		private void addClickHandler(EventHandler handler){
			UpdateStatusButton.Clicked+=handler;
			UpdateHeaderButton.Clicked+=handler;
		}
					
		private void onRestartClicked(object sender, EventArgs a){
			removeClickHandler(onRestartClicked);			
			//restart the application
			CUtility.Execute(System.Windows.Forms.Application.ExecutablePath,"",false);
			RestartRequested = true;
			Close();
		}
		
		private void onOpenLocationButtonClicked(object sender, EventArgs a){
			CUtility.Execute("gnome-www-browser",@"http://gnomeartng.berlios.de");		
		}
		
		private void setHeaderLabelText(string Text){
			UpdateHeaderLabel.Markup = "<span size=\"x-large\" weight=\"bold\">"+Text+"</span>";		
		}	
		public void CheckForUpdate(){
			onStatusButtonClicked(null,new EventArgs());
		}
		
		private void onStatusButtonClicked (object sender, EventArgs a){
			UpdateStatusLabel.Text=Catalog.GetString("Connecting");
			try{
				UpdateStatusButton.Sensitive = false;
				if (config.UpdateAvailable){
					setHeaderLabelText(Catalog.GetString("An online update is available, version ")+config.NewestVersionNumberOnServer);
					UpdateHeaderImage.Pixbuf = CUtility.GetPixbuf("./images/update_available.png",config);
					UpdateStatusImage.Stock = Gtk.Stock.Yes;
					removeClickHandler(onStatusButtonClicked);
						
					if (config.Distribution == CConfiguration.DistriType.dtUbuntu){
						UpdateStatusLabel.Text=Catalog.GetString("Install");
						addClickHandler(onInstallClicked);
					}
					else{
						UpdateStatusLabel.Text=Catalog.GetString("Open download location");
						addClickHandler(onOpenLocationButtonClicked);
					}
				}
				else{
					UpdateStatusImage.Stock = Gtk.Stock.Refresh;
					UpdateStatusLabel.Text=Catalog.GetString("There is no new version of GANG available");
					UpdateHeaderImage.Pixbuf = CUtility.GetPixbuf("./images/update_checking.png",config);
				}
				UpdateCurrentVersionLabel.Text=CConfiguration.Version;
				UpdateNewVersionLabel.Text=config.NewestVersionNumberOnServer;
			}
			finally {
				UpdateStatusButton.Sensitive=true;
			}
		}

		private void initializeWidgets(){
			UpdateCloseButton.Clicked+=new EventHandler(onCloseButtonClicked);
			UpdateDonateButton.Clicked+=new EventHandler(onDonateButtonClicked);
			addClickHandler(onStatusButtonClicked);
			/*		
			UpdateDonateLabel.ModifyFg(Gtk.StateType.Normal,new Gdk.Color(230,230,230));
			UpdateDonateLabel.ModifyFg(Gtk.StateType.Prelight,new Gdk.Color(230,230,230));
			UpdateDonateButton.ModifyBg(Gtk.StateType.Prelight,new Gdk.Color(100,100,100));
			UpdateDonateButton.ModifyBg(Gtk.StateType.Normal,new Gdk.Color(55,57,53));
			
			UpdateHeaderLabel.ModifyFg(Gtk.StateType.Normal,new Gdk.Color(230,230,230));
			UpdateHeaderLabel.ModifyFg(Gtk.StateType.Prelight,new Gdk.Color(230,230,230));				
			UpdateHeaderButton.ModifyBg(Gtk.StateType.Normal,new Gdk.Color(55,57,53));
			UpdateHeaderButton.ModifyBg(Gtk.StateType.Prelight,new Gdk.Color(100,100,100));
			*/
			setHeaderLabelText(Catalog.GetString("Use \"Check\" or click here to search for an online update"));
			UpdateHeaderImage.Pixbuf = CUtility.GetPixbuf("./images/update_checking.png",config);
			
			//UpdateDonateButtonImage.Pixbuf = CUtility.GetPixbuf("./PayPalLogo.gif",config);
			UpdateStatusLabel.Text = Catalog.GetString("Check");
		}
		
		
		private void onDonateButtonClicked (object sender, EventArgs a){
			CUtility.Execute("gnome-www-browser",@"https://www.paypal.com/cgi-bin/webscr"+
			                 "?cmd=_donations&business=software%40plasmasolutions%2ede&item_name=Gnome%2dArt%20Next%20Generation%20donation"+
			                 "&no_shipping=0&no_note=1&tax=0&currency_code=EUR&lc=DE&bn=PP%2dDonationsBF&charset=UTF%2d8",false);
		}
	}
}
