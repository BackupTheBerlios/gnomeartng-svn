/*
This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; version 3 of the License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
*/

using System;
using Gtk;
using Gdk;
using Glade;
using GnomeArtNG;
using System.Collections;
using System.IO;
using Mono.Unix;
using Pango;
using System.Web;
using System.Net;

public class GnomeArtNgApp
{
	private Gtk.Window mainWindow;
	private CArtManager man;
	private CConfiguration config;
	
	private ComboBox imageTypeBox;
	private ComboBox imageResolutionsBox;
    private ComboBox imageStyleBox;
	private static int ListStoreCount = 9;
	private ListStore[] stores = new ListStore[ListStoreCount];
	private Gtk.IconView[] IconViews=new Gtk.IconView[ListStoreCount];
	private Gtk.IconView CurrentIconView;
	private Gtk.ScrolledWindow[] sWins = new Gtk.ScrolledWindow[ListStoreCount]; 
	
	//[Widget] Gtk.Image AboutImage;	

	//Die Iconviews mit den einzelnen Voransichten werden dynamisch erzeugt 

	//HBox
	[Widget] Gtk.HBox FilterBar;
	//[Widget] Gtk.HBox SortBar;

	//Combobox
	//[Widget] Gtk.ComboBox SortKindCb;
	//[Widget] Gtk.ComboBox SortDirectionCb;

	
	//Entries
	[Widget] Gtk.Entry FilterEntry;
		
	//Menüs
	[Widget] Gtk.ImageMenuItem QuitMenuItem;
	[Widget] Gtk.ImageMenuItem FilterMenuItem;
	//[Widget] Gtk.ImageMenuItem SortMenuItem;
	[Widget] Gtk.ImageMenuItem InfoMenuItem;
	[Widget] Gtk.ImageMenuItem PreferencesMenuItem;
	[Widget] Gtk.ImageMenuItem UpdateMenuItem;
	[Widget] Gtk.ImageMenuItem DonateMenuItem;
	[Widget] Gtk.MenuItem FTAItem;
	
	//Buttons
	[Widget] Gtk.Button FilterCloseButton;
	//[Widget] Gtk.Button SortCloseButton;
	[Widget] Gtk.Button InstallButton;	
	[Widget] Gtk.Button RevertButton;
	[Widget] Gtk.Button RefreshButton;	
	[Widget] Gtk.Button SaveButton;
	
	[Widget] Gtk.Table LowerTable;	
	[Widget] Gtk.Notebook MainNotebook;
	
	//Erweiterte Infos Beschriftungen
	[Widget] Gtk.Label ExtInfoResolutionsLabel;	
	[Widget] Gtk.Label ExtInfoImageTypeLabel;
	[Widget] Gtk.Label ExtInfoImageStyleLabel;
	
	//Erweiterte Infos Inhalte
	[Widget] Gtk.Button ExtInfoPreviewButton;
	[Widget] Gtk.Image ExtInfoImage;
	[Widget] Gtk.Label ExtInfoName;
	[Widget] Gtk.Label ExtInfoAuthor;
	[Widget] Gtk.Label ExtInfoLicense;
	[Widget] Gtk.Label ExtInfoDescription;
	[Widget] Gtk.Label ExtInfoDownloads;
	[Widget] Gtk.Label ExtInfoRating;

	public static void Main (string[] args) {
		//Hauptformular und GTKMain-Schleife
		Console.WriteLine("Gnome-Art Next Gen is starting");
		new GnomeArtNgApp (args);
		Console.WriteLine("Bye bye, have a nice day!");
	}

	public GnomeArtNgApp (string[] args) {
		Application.Init();
		//i18n
		Catalog.Init("gnomeartng","./locale");
		config=new CConfiguration();
		//initialize Glade
		string mainW = "MainWindow";
		Glade.XML gxml = new Glade.XML (null, "gui.glade", mainW, null);
		mainWindow = (Gtk.Window) gxml.GetWidget (mainW);
		gxml.Autoconnect (this);
		
		//Connect all events
		mainWindow.DeleteEvent+=new DeleteEventHandler(OnWindowDeleteEvent);
		ExtInfoPreviewButton.Clicked += new EventHandler(OnPreviewButtonClicked);
		InstallButton.Clicked  += new EventHandler(OnInstallButtonClicked);
		RevertButton.Clicked  += new EventHandler(OnRevertButtonClicked);
		RefreshButton.Clicked += new EventHandler(OnRefreshButtonClicked);
		SaveButton.Clicked  += new EventHandler(OnSaveButtonClicked);
		MainNotebook.SwitchPage += new SwitchPageHandler(OnSwitchPage);
		FilterEntry.Changed += new EventHandler(OnFilterEntriesChanged);
//		SortKindCb.Changed += new EventHandler(OnSortKindEntryChanged);
//		SortDirectionCb.Changed += new EventHandler(OnSortDirectionEntryChanged);
//		SortCloseButton.Clicked += new EventHandler(OnSortCloseClicked);
		FilterEntry.KeyReleaseEvent += new KeyReleaseEventHandler(OnFilterbarKeyReleased);
		FilterCloseButton.Clicked += new EventHandler(OnFilterbarCloseClicked);
		
		//Menuitems
		QuitMenuItem.Activated += new EventHandler(OnQuitItemSelected);
		UpdateMenuItem.Activated += new EventHandler(OnUpdateItemSelected);
		DonateMenuItem.Activated += new EventHandler(CUpdateWindow.onDonateButtonClicked);
		FilterMenuItem.Activated += new EventHandler(OnFilterItemSelected);
//		SortMenuItem.Activated += new EventHandler(OnSortItemSelected);
		InfoMenuItem.Activated += new EventHandler(OnInfoItemSelected);
		PreferencesMenuItem.Activated += new EventHandler(OnPreferencesItemSelected);
		FTAItem.Activated += new EventHandler(onFtaItemSelected);
		
		//First, download all thumbs...but don't bother the user with the update message, even if there is one
		bool RestartApp= false;		
		if (config.NeverStartedBefore)
			new CFirstTimeAssistant(config);
		else if (config.DontBotherForUpdates==false) {
			if (config.UpdateAvailable) { 
				Console.WriteLine("An update is available, newest version is: "+config.NewestVersionNumberOnServer);			
				RestartApp = ShowUpdateWindow();
			}
		}	

		if (!RestartApp) { 
			//Application placement - doesn't work properly with compiz (is it the window placement plugin?)
			if (config.SettingsLoadOk) {
				mainWindow.Resize(config.Window.Width, config.Window.Height);
				mainWindow.Move(config.Window.X, config.Window.Y);
				//Console.WriteLine(config.Window.X+" "+ config.Window.Y);
			}

			//ArtManager erzeugen
			man = new CArtManager(config);

			//Stores anlegen und IconViews anlegen
			for(int i=0;i<ListStoreCount;i++){
				sWins[i] = (Gtk.ScrolledWindow)(gxml.GetWidget("swin"+i));
				stores[i]= new ListStore (typeof(Pixbuf),typeof (string), typeof (string), typeof(int));
				IconViews[i] = new Gtk.IconView(stores[i]);
				IconViews[i].SelectionChanged += new System.EventHandler(OnSelChanged);
				IconViews[i].ItemActivated += new ItemActivatedHandler(OnItemActivated);
				IconViews[i].PixbufColumn = 0;
				CurrentIconView = IconViews[0];
				
				sWins[i].Add(IconViews[i]);
				IconViews[i].Show();
			}
			
			//Create the comboboxes
			imageTypeBox = ComboBox.NewText();
			imageResolutionsBox = ComboBox.NewText();
			imageStyleBox = ComboBox.NewText();
			
			//Verschiedene Styles hinzufügen
			imageStyleBox.AppendText(Catalog.GetString("Centered"));
			imageStyleBox.AppendText(Catalog.GetString("Filled"));
			imageStyleBox.AppendText(Catalog.GetString("Scaled"));
			imageStyleBox.AppendText(Catalog.GetString("Zoomed"));
			imageStyleBox.AppendText(Catalog.GetString("Tiled"));
			imageStyleBox.Active=0;
			imageStyleBox.Changed += new EventHandler(OnImageStyleBoxChanged);
			
			LowerTable.Attach(imageTypeBox,1,2,2,3);
			LowerTable.Attach(imageResolutionsBox,1,2,3,4);
			LowerTable.Attach(imageStyleBox,1,2,4,5);
			
			OnSwitchPage(MainNotebook,new SwitchPageArgs());
			Gtk.Application.Run ();
			
		}
	}

	private void OnQuitItemSelected(object sender, EventArgs a){
		Application.Quit();
	}

	private void OnInfoItemSelected(object sender, EventArgs a){
		new CAboutWindow(config,true);
	}

	private bool ShowUpdateWindow(){
		CUpdateWindow updW = new CUpdateWindow(config);
		updW.CheckForUpdate();
		updW.ShowModal();
		return updW.RestartRequested;
	}
	
	//Methode zum manuellen filtern der Ergebnisse
	private bool onFilter (Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		string name = model.GetValue(iter, 1).ToString();
		string entry = FilterEntry.Text;
		if (FilterEntry.Text.Length>2) {
			return name.ToLower().Contains(entry.ToLower());
		} else {
			return true;
		}
	}	

	private void OnFilterItemSelected(object sender, EventArgs a){
		if (!FilterBar.Visible) {
			FilterBar.Show();
		}
		FilterEntry.GrabFocus();
	}

/*	private void OnSortItemSelected(object sender, EventArgs a){
		if (!SortBar.Visible) {
			SortBar.Show();
		}
	}
*/	
	private void OnFilterEntriesChanged(object sender, EventArgs a){
		if (CurrentIconView.Model is Gtk.TreeModelFilter){
			((Gtk.TreeModelFilter) CurrentIconView.Model).Refilter();
		}
	}
/*	
	private void OnSortKindEntryChanged(object sender, EventArgs a){
		System.Console.WriteLine(CurrentIconView.Model.ToString());
		TreeModelFilter store = (TreeModelFilter)(CurrentIconView.Model);
		System.Console.WriteLine("hups");
		int activeItem = ((Gtk.ComboBox)sender).Active;
		if (activeItem == 0) {
			//store.SetSortColumnId(-1, SortType.Ascending);
		} else {
			//store.SetSortColumnId (activeItem, SortType.Ascending);
		}
	}
	private void OnSortDirectionEntryChanged(object sender, EventArgs a){

	}
*/

	private void OnFilterbarKeyReleased(object sender, KeyReleaseEventArgs e){
		if (e.Event.Key == Gdk.Key.Escape){
			OnFilterbarCloseClicked(sender,e);
		}
	}
	
	private void OnFilterbarCloseClicked(object sender, EventArgs a){
		FilterEntry.Text = "";
		FilterBar.Hide();
	}
/*	
	private void OnSortCloseClicked(object sender, EventArgs a){
		SortBar.Hide();
	}
*/		
	private void OnUpdateItemSelected(object sender, EventArgs a){

		if (ShowUpdateWindow())
		    Application.Quit();
	}

	private void OnPreferencesItemSelected(object sender, EventArgs a){
		new CSettingsWindow(config,true);
	}
	private void onFtaItemSelected(object sender, EventArgs a){
		new CFirstTimeAssistant(config);
	}
	
	private void OnImageTypeBoxChanged(object sender, EventArgs a){
		CBackgroundTheme bgTheme = (CBackgroundTheme)(man.Theme);
		bgTheme.BgType = CUtility.StrToImageType(imageTypeBox.ActiveText);
		FillComboboxWithStrings(imageResolutionsBox, bgTheme.GetAvailableResolutions());
	}
	
	private void OnImageStyleBoxChanged(object sender, EventArgs a){
		((CBackgroundTheme)(man.Theme)).ImgStyle = (CBackgroundTheme.ImageStyle)imageStyleBox.Active;
	}
	
	private void OnImageResolutionsBoxChanged(object sender, EventArgs a){
		((CBackgroundTheme)(man.Theme)).ImageIndex = imageResolutionsBox.Active;
	}
	
	private void OnSwitchPage(object sender, SwitchPageArgs s){
		OnFilterbarCloseClicked(sender, s);
		int pageNum = ((Gtk.Notebook)sender).Page;
		CurrentIconView = IconViews[pageNum];
		FilterEntry.Text = "";
		switch(pageNum){
			case 0: man.ArtType=CConfiguration.ArtType.atBackground_gnome; break;
			case 1: man.ArtType=CConfiguration.ArtType.atBackground_other; break;
			case 2: man.ArtType=CConfiguration.ArtType.atBackground_nature; break;
			case 3: man.ArtType=CConfiguration.ArtType.atBackground_abstract; break;
			case 4: man.ArtType=CConfiguration.ArtType.atApplication; break;
			case 5: man.ArtType=CConfiguration.ArtType.atWindowDecoration; break;
			case 6: man.ArtType=CConfiguration.ArtType.atIcon; break;
			case 7: man.ArtType=CConfiguration.ArtType.atGdmGreeter; break;
			case 8: man.ArtType=CConfiguration.ArtType.atSplash; break;
			case 9: man.ArtType=CConfiguration.ArtType.atGtkEngine;	break;
		}

		Gtk.TreeIter iter;
		CurrentIconView.Model.GetIterFirst(out iter);
		//Falls noch kein gefüllter Store (und damit noch kein gültiger Iterator -> Stamp==0
		if (iter.Stamp==0){
			man.GetAllThumbs();
			FillStore(pageNum);
			Gtk.TreeModelFilter filter = new Gtk.TreeModelFilter (CurrentIconView.Model, null);
			filter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (onFilter);
			CurrentIconView.Model = filter;
	        CurrentIconView.GrabFocus ();
		} 
		if (CurrentIconView.SelectedItems.Length > 0)
			man.ThemeIndex=int.Parse(CurrentIconView.SelectedItems[0].ToString());
		FillExtendedSection(man.Theme);
	}
	private void SaveAllProgramSettings(){
		WindowAttrStruct windowAttr;
		mainWindow.GetSize(out(windowAttr.Width), out(windowAttr.Height));		
		mainWindow.GetPosition(out(windowAttr.X), out(windowAttr.Y));
		config.Window = windowAttr;
		config.SaveProgramSettings();
	}

	private void OnWindowDeleteEvent (object sender, DeleteEventArgs a) {
		SaveAllProgramSettings();
		Application.Quit ();
		a.RetVal = true;
	}
	
	private void OnSaveButtonClicked(object sender, EventArgs e){
		CStatusWindow sw=new CStatusWindow(Catalog.GetString("Downloading the theme"),1,false,true,true);
		sw.ExtInfoLabel = Catalog.GetString("Download location")+": "+config.ThemesDownloadPath;
		sw.ButtonSensitive=false;
		try{
			man.Theme.GetThemeFile(sw,config.ThemesDownloadPath);
			sw.Close();
		}
		catch (Exception ex) {
			sw.Close();
			new CInfoWindow(Catalog.GetString("Error")+":"+Catalog.GetString("Download failed!"),ex.Message,Gtk.Stock.DialogError,true);
			
		}
	}
	
	private void OnPreviewButtonClicked (object sender, EventArgs e){
		CStatusWindow sw=new CStatusWindow(Catalog.GetString("Downloading the preview file"),1,false,true,true);
		sw.SetProgress("1/1 - " +Catalog.GetString("Connected to art.gnome.org"));
		sw.Mainlabel=Catalog.GetString("<i>Downloading the preview file</i>\n\nYour preview is being downloaded. After the download has been finished,"+
		                               " the preview will be rescaled if it's not fitting the preview window. See the lower bar to follow the progress.");
		sw.ButtonSensitive=false;
		try{
			man.Theme.GetPreviewImage(sw.DetailProgressBar);
			sw.Close();
			new CPreviewWindow(man.Theme,true);
		}
		catch (Exception ex) {
			sw.Close();
			new CInfoWindow(Catalog.GetString("Error: the preview image could not be loaded!"),ex.Message,Gtk.Stock.DialogError,true);
			
		}
	}
	
	private void OnInstallButtonClicked (object sender, EventArgs e){
		man.Theme.StartInstallation();
	}
	
	private void OnRevertButtonClicked (object sender, EventArgs e){
		man.Theme.Revert();
	}

	private void OnRefreshButtonClicked (object sender, System.EventArgs a){
		man.GetAllThumbs();
		FillStore(MainNotebook.Page);
		IconViews[MainNotebook.Page].GrabFocus ();
	}
	
	private void OnSelChanged(object sender,EventArgs e){
		if (((Gtk.IconView)sender).SelectedItems.Length>0){
			//man.ThemeIndex=int.Parse(((Gtk.IconView)sender).SelectedItems[0].ToString());
			Gtk.TreeIter iter;
			CurrentIconView.Model.GetIter(out iter, CurrentIconView.SelectedItems[0]);
			string themename = CurrentIconView.Model.GetValue(iter, 1).ToString ();
			CTheme theme = man.GetTheme(themename);
			FillExtendedSection(theme);
		}
	}
	
	//Doubleclick
	private void OnItemActivated(object sender, ItemActivatedArgs a){
		OnInstallButtonClicked(sender, a);
	}
	
	private void FillComboboxWithStrings(Gtk.ComboBox box, string[] strings){
		((Gtk.ListStore)(box.Model)).Clear();
		for (int i=0;i<strings.Length;i++){
			box.AppendText(strings[i]);
		} 
		box.Active=0;
		box.Sensitive=(strings.Length>1);
	}
	
	private void FillExtendedSection(CTheme theme){
		bool isImage = config.BackgroundChoosen;
		string themeRating = theme.VoteSum.ToString();
		ExtInfoImage.Pixbuf = theme.ThumbnailPic;
		ExtInfoName.Text = theme.Name;
		ExtInfoAuthor.Text = theme.Author;
		ExtInfoLicense.Text = theme.License;
		if (theme.Description.Trim() != "")
			ExtInfoDescription.Text = theme.Description;
		else
			ExtInfoDescription.Text = Catalog.GetString("No description has been entered by the author");
		ExtInfoDownloads.Text = theme.DownloadCount.ToString();
		
		if (themeRating=="0")
			themeRating=Catalog.GetString("not rated");
			
		ExtInfoRating.Text = themeRating;
		if (isImage){
			CBackgroundTheme bgt =(CBackgroundTheme)theme;
			imageTypeBox.Changed -= (EventHandler)OnImageTypeBoxChanged;
			imageResolutionsBox.Changed -= (EventHandler)OnImageResolutionsBoxChanged;
			FillComboboxWithStrings(imageTypeBox, bgt.GetAvailableTypes());
			FillComboboxWithStrings(imageResolutionsBox, bgt.GetAvailableResolutions(CUtility.StrToImageType(imageTypeBox.ActiveText)));
			imageTypeBox.Changed += new EventHandler(OnImageTypeBoxChanged);
			imageResolutionsBox.Changed += new EventHandler(OnImageResolutionsBoxChanged);
		}
		imageTypeBox.Visible=isImage;
		imageResolutionsBox.Visible = isImage;
		imageStyleBox.Visible = isImage;
		ExtInfoImageTypeLabel.Visible = isImage;
		ExtInfoResolutionsLabel.Visible = isImage;
		ExtInfoImageStyleLabel.Visible = isImage;
	}

	void FillStore (int StoreIndex)  {
		int themeCount = (int)(man.ThemeCount);
		stores[StoreIndex].Clear();
		for(int i=0; i<themeCount;i++) {
			CTheme theme = man.GetTheme(i);
			stores[StoreIndex].AppendValues (theme.ThumbnailPic,theme.Name, theme.Author, theme.DownloadCount);
          }
		if (themeCount>=0)
			man.ThemeIndex=0;
     }
}
