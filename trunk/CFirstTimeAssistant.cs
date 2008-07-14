/*
This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; version 3 of the License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
*/

// Created by Thomas Beck at 13:04 07.07.2008

using System;
using Gtk;
using Gdk;
using Glade;
using GnomeArtNG;
using Mono.Unix;
using System.IO;

namespace GnomeArtNG
{
	
	public class CFirstTimeAssistant : CWindow
	{		
		private static string windowName="FtaWindow";
		private static string windowTitle = Catalog.GetString("First time assistant")+", "+ Catalog.GetString("welcome to")+' '+"Gnome-Art NG";

		//FirstTimeAssistant
		[Widget] Gtk.ProgressBar FtaProgressBar;
		[Widget] Gtk.Button FtaApplyButton;		
		[Widget] Gtk.Button FtaCloseButton;		
		[Widget] Gtk.Image UpdateHeaderImage;
		[Widget] Gtk.Label FtaProgressLabel;
		[Widget] Gtk.Label FtaHeadlineLabel;
		[Widget] Gtk.Label FtaDescriptionLabel;
		
		public CFirstTimeAssistant(CConfiguration config): base(config, windowName, windowTitle, WindowShowType.wstNo) {
			FtaCloseButton.Clicked += new EventHandler(onCancelClicked);
			FtaApplyButton.Clicked += new EventHandler(onApplyClicked);
			FtaHeadlineLabel.Text = "<span size=\"x-large\" weight=\"bold\">"+FtaHeadlineLabel.Text+"</span>";
			FtaHeadlineLabel.UseMarkup = true;
			ShowModal();
		}
		

		private void onApplyClicked(object sender, EventArgs b){
			try{
				string dfile="/tmp/thumbs.tar.gz";
				FtaApplyButton.Sensitive = false;
				FtaCloseButton.Sensitive = false;
				new CFileDownloader(config).DownloadFile(CConfiguration.ThemeBulkUrl,dfile,FtaProgressBar);
				CUtility.UncompressFile(dfile,config.ProgramSettingsPath+config.DirectorySeperator,true,FtaProgressBar);
				Close();				
			}
			catch{
				FtaCloseButton.Sensitive = true;	
				FtaApplyButton.Sensitive = false;
			}
		}

		private void onCancelClicked(object sender, EventArgs b){	
			Close();
		}

	}
}
