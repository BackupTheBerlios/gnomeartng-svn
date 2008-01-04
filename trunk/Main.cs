// /home/neo/Projects/Monodevelop-Projects/GnomeArtNG/Main.cs created with MonoDevelop
// User: neo at 16:24 24.11.2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//
// project created on 24.11.2007 at 16:24
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
	private CArtManager man;
	private CConfiguration config;
	
	private ComboBox imageTypeBox;
	private ComboBox imageResolutionsBox;
    private ComboBox imageStyleBox;
	private static int ListStoreCount = 10;
	private ListStore[] stores = new ListStore[ListStoreCount];
	private Gtk.IconView[] IconViews=new Gtk.IconView[ListStoreCount];
	private Gtk.ScrolledWindow[] sWins = new Gtk.ScrolledWindow[ListStoreCount]; 
	
	//[Widget] Gtk.Image AboutImage;	

	//Die Iconviews mit den einzelnen Voransichten werden dynamisch erzeugt 
	
	//Menüs
	[Widget] Gtk.ImageMenuItem QuitMenuItem;
	[Widget] Gtk.ImageMenuItem InfoMenuItem;

	//Buttons
	[Widget] Gtk.Button InstallButton;	
	[Widget] Gtk.Button RevertButton;	
	[Widget] Gtk.Table LowerTable;	
	[Widget] Gtk.Notebook MainNotebook;
	[Widget] Gtk.Button StartButton;	
	
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
		new GnomeArtNgApp (args);
	}

	public GnomeArtNgApp (string[] args) {
		Application.Init();
		//Mehrsprachigkeit initialisieren
		Catalog.Init("i18n","./locale");
		config=new CConfiguration();
		//GreeterWindow!
		//if (config.NeverStartedBefore)
		//	;
		//Glade laden
		Glade.XML gxml = new Glade.XML (null, "gui.glade", "MainWindow", null);
		gxml.Autoconnect (this);
		
		//Events verbinden
		ExtInfoPreviewButton.Clicked += new EventHandler(OnPreviewButtonClicked);
		InstallButton.Clicked  += new EventHandler(OnInstallButtonClicked);
		RevertButton.Clicked  += new EventHandler(OnRevertButtonClicked);
		MainNotebook.SwitchPage += new SwitchPageHandler(OnSwitchPage);
		StartButton.Clicked += new EventHandler(OnStartButtonClicked);
		QuitMenuItem.Activated += new EventHandler(OnQuitItemSelected);
		//InfoMenuItem;
		
		//ArtManager erzeugen
		man = new CArtManager(config);

		//Stores anlegen und IconViews anlegen
		for(int i=0;i<ListStoreCount;i++){
			sWins[i] = (Gtk.ScrolledWindow)(gxml.GetWidget("swin"+i));
			stores[i]= new ListStore (typeof(Pixbuf),typeof (string), typeof (string));
			IconViews[i] = new Gtk.IconView(stores[i]);
			IconViews[i].SelectionChanged += new System.EventHandler(OnSelChanged);
			IconViews[i].ItemActivated += new ItemActivatedHandler(OnItemActivated);
			IconViews[i].PixbufColumn = 0;
			sWins[i].Add(IconViews[i]);
			IconViews[i].Show();
		}
		
		//ComboBoxen anlegen
		imageTypeBox = ComboBox.NewText();
		imageTypeBox.Changed += new EventHandler(OnImageTypeBoxChanged);
		imageResolutionsBox = ComboBox.NewText();
		imageResolutionsBox.Changed += new EventHandler(OnImageResolutionsBoxChanged);
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
		Application.Run ();
	}

	private void OnQuitItemSelected(object sender, EventArgs a){
		Application.Quit();
	}
	
	private void OnImageTypeBoxChanged(object sender, EventArgs a){
		FillComboboxWithStrings(imageResolutionsBox, ((CBackgroundTheme)(man.Theme)).GetAvailableResolutions());
	}
	
	private void OnImageStyleBoxChanged(object sender, EventArgs a){
		((CBackgroundTheme)(man.Theme)).ImgStyle = (CBackgroundTheme.ImageStyle)imageStyleBox.Active;
	}
	
	private void OnImageResolutionsBoxChanged(object sender, EventArgs a){
		((CBackgroundTheme)(man.Theme)).ImageIndex = imageResolutionsBox.Active;
	}
	
	private void OnSwitchPage(object sender, SwitchPageArgs s){
		int pageNum = ((Gtk.Notebook)sender).Page;
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
		IconViews[pageNum].Model.GetIterFirst(out iter);
		//Falls noch kein gefüllter Store (und damit noch kein gültiger Iterator -> Stamp==0
		if (iter.Stamp==0){
			man.GetAllThumbs();
			FillStore(pageNum);
	        IconViews[pageNum].GrabFocus ();
		}
		FillExtendedSection(man.Theme);
	}
	
	private void OnWindowDeleteEvent (object sender, DeleteEventArgs a) {
		Application.Quit ();
		a.RetVal = true;
	}
	
	private void OnPreviewButtonClicked (object sender, EventArgs e){
		/*
			Pango.Layout layout = new Pango.Layout(ExtInfoImage.PangoContext);			
			layout.Wrap = Pango.WrapMode.Word;
			layout.FontDescription = FontDescription.FromString ("Bitstream Vera Sans Mono 10");
			layout.SetMarkup ("Hello Pango.Layout");
			ExtInfoImage.Pixmap.DrawLayout(ExtInfoImage.Style.TextGC(StateType.Normal), 0, 0, layout);
		 */ 
		
		CStatusWindow sw=new CStatusWindow(Catalog.GetString("Downloading the preview file"),1,false,true,true);
		sw.SetProgress("1/1 - " +Catalog.GetString("Connected to art.gnome.org"));
		sw.Mainlabel=Catalog.GetString("<i>Downloading the preview file</i>\n\nYour preview is beeing downloaded. After the download has been finished,"+
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

	private void OnStartButtonClicked (object sender, System.EventArgs a){
		
		man.GetAllThumbs();
		FillStore(MainNotebook.Page);
		IconViews[MainNotebook.Page].GrabFocus ();
	}
	
	private void OnSelChanged(object sender,EventArgs e){
		if (((Gtk.IconView)sender).SelectedItems.Length>0){
			man.ThemeIndex=int.Parse(((Gtk.IconView)sender).SelectedItems[0].ToString());
			FillExtendedSection(man.Theme);
		}
	}
	
	//Test zum einfachen reload oder neuladen von einzelnen Thumbs
	private void OnItemActivated(object sender, ItemActivatedArgs a){
		man.GetThumb();
		FillStore(MainNotebook.Page);
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
		ExtInfoImage.Pixbuf = theme.ThumbnailPic;
		ExtInfoName.Text = theme.Name;
		ExtInfoAuthor.Text = theme.Author;
		ExtInfoLicense.Text = theme.License;
		if (theme.Description.Trim() != ""){
			ExtInfoDescription.Text = theme.Description;
			ExtInfoDescription.UseMarkup=true;
		}
		else
			ExtInfoDescription.Text = Catalog.GetString("No description has been entered by the author");
		ExtInfoDownloads.Text = theme.DownloadCount.ToString();
		ExtInfoRating.Text = theme.VoteSum.ToString();
		if (isImage){
			CBackgroundTheme bgt =(CBackgroundTheme)theme;
			FillComboboxWithStrings(imageTypeBox, bgt.GetAvailableTypes());
			FillComboboxWithStrings(imageResolutionsBox, bgt.GetAvailableResolutions(bgt.GetImageTypeFromString(imageTypeBox.ActiveText)));
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
			stores[StoreIndex].AppendValues (theme.ThumbnailPic,theme.Name, theme.Author);
			
          }
		if (themeCount>=0)
			man.ThemeIndex=0;
     }
}
