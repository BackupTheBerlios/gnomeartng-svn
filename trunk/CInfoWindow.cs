/*
This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; version 2 of the License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

Thomas Beck at 17:50 13.12.2007
*/

using System;
using Gtk;
using Gdk;
using Glade;
using GnomeArtNG;

namespace GnomeArtNG
{
	
	
	public class CInfoWindow
	{
		
		private Gtk.Window mainWindow;
		public Gtk.Window MainWindow {
			get {return mainWindow;}
		}
		
		//InfoWindow
		[Widget] Gtk.Label InfoHeadLabel;
		//[Widget] Gtk.Label InfoDescriptionLabel;
		[Widget] Gtk.TextView InfoDescription;
		[Widget] Gtk.Button InfoOkButton;
		[Widget] Gtk.Image InfoImage;
		
		
		public string Headline{
			get{
				return InfoHeadLabel.Text;
			}
			set{
				InfoHeadLabel.Text = "<b>"+value+"</b>";
				InfoHeadLabel.UseMarkup=true;
			}
		}
		
		public string Description{
			get{return InfoDescription.Buffer.Text;}
			set{InfoDescription.Buffer.Text = InfoDescription.Buffer.Text+value;}
		}
		
		public void ClearDescription(){
			InfoDescription.Buffer.Clear();
		}
		
		public CInfoWindow(string Headline,string Description, string StockIcon, bool ShowWindow)	{
			string infoW="InfoWindow";
			Glade.XML infoXml= new Glade.XML (null, "gui.glade", infoW, null);
			infoXml.Autoconnect (this);
			mainWindow = (Gtk.Window) infoXml.GetWidget (infoW);
			this.Headline = Headline;
			this.Description = Description;
			InfoOkButton.Clicked+=new EventHandler(OnOkButtonClicked);
			InfoImage.Stock= StockIcon;
			if(ShowWindow)
				Show();
		}
				
		public void Close(){
			mainWindow.Destroy();
		}
		
		public void Invalidate(){
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration ();
		}
		
		public void Show(){
			mainWindow.ShowAll();
			Invalidate();
		}
		
		private void OnOkButtonClicked (object sender, EventArgs b){
			Close();
		}
	}
}
