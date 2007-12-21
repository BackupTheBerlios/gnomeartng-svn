// /home/neo/Projects/GnomeArtNG/GnomeArtNG/CGdmTheme.cs created with MonoDevelop
// User: neo at 12:16 24.11.2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.IO;
using System.Text;

namespace GnomeArtNG
{
	//Systemweiter Ordner: /usr/share/gdm/themes/<theme_name>
	
	public class CGdmTheme: CTheme
	{
		
		private string previousTheme="";
		private string gdmconfig=@"/etc/gdm/gdm.conf";
		private StringBuilder sb;
		private string tarParams="";
		private bool randomThemeActive =false;
		private bool gdmConfAvailable=false;
		private bool customGdmConfAvailable=false;
		private CIniWorker iworker;
		
		
		override protected void PreInstallation(CStatusWindow sw){}
		override protected void PostInstallation(CStatusWindow sw){}
		/*
		 * 2 Dateien sind wichtig:
		 * /etc/gdm/gdm.conf um herauszufinden ob u.U Multiselect gewählt ist
		 * /etc/gdm/gdm.conf-custom um die gewählten Themes herauszulesen und zu setzen
		 */
		override protected void Installation(CStatusWindow sw){
			LocalThemeFile=config.ThemesPath+Path.GetFileName(DownloadUrl);
			gdmConfAvailable=File.Exists(gdmconfig);
			customGdmConfAvailable=File.Exists(gdmconfig+"-custom");
			if (!installationIsPossible) 
				throw new Exception("Installation is not possible! Please check your environment");
			if (gdmConfAvailable){
				sb = config.Execute("grep","GraphicalThemeRand= "+gdmconfig);
				if (sb.Length<1) 
					throw new Exception("Installation is not possible! grep returned no GraphicalThemeRand-Entry\"");
				else
					randomThemeActive=bool.Parse(sb.ToString().Split('=')[1]);
				
				//Falls nicht vorhanden wird sie erzeugt
				if (!File.Exists(gdmconfig+"-custom")){
					try{File.Create(gdmconfig+"-custom");}
					catch {throw new Exception("Gdm.conf-custom couldn't be created, aborting!");}
				}
				//Datei einlesen
				iworker = new CIniWorker(gdmconfig+"-custom");
				//Entpackparameter
				tarParams = "tar "+config.GetTarParams(DownloadUrl);
				//Herunterladen
				if (!File.Exists(LocalThemeFile))
					DownloadFile(DownloadUrl, LocalThemeFile);
				//Entpacken
				sb = config.Execute("gksu","'"+tarParams+LocalThemeFile+" -C "+config.GdmInstallPath+"'");
				string[] FolderName=sb.ToString().Split('/');
				//Console.WriteLine(FolderName[0]);

				//Eintrag "GraphicalTheme=Themename" 
				if (!randomThemeActive){
					previousTheme = iworker.getValue("greeter","GraphicalTheme",true);
					//Console.WriteLine("PreviousTheme: "+previousTheme);
					iworker.setValue("greeter","GraphicalTheme",FolderName[0],true);
					//Kopieren an einen Ort an dem Schreibberechtigung vorhanden ist 
					iworker.Save("/tmp/gdm.conf-custom");
					//Per gdsudo den Benutzer für diese Aktion zum Superuser werden lassen
					config.Execute("gksudo","'mv /tmp/gdm.conf-custom /etc/gdm/'");
				} else{

				}

				//GDM Die Änderungen mitteilen
				config.Execute("gdmflexiserver", "--command=\"UPDATE_CONFIG greeter/GraphicalTheme\"");
				revertIsAvailable=true;
				
			} else
				throw new Exception("Installation is not possible! no gdm.conf found\"");
		}

		override public void Revert(){
			if (revertIsAvailable){
				//Eintrag " GraphicalTheme=Themename" 
				if (!randomThemeActive){
					//IWorker wurde bei der Installation schon erzeugt
					iworker.setValue("greeter","GraphicalTheme",previousTheme,true);
					//Kopieren an einen Ort an dem Schreibberechtigung vorhanden ist 
					iworker.Save("/tmp/gdm.conf-custom");
					config.Execute("gksudo","'mv /tmp/gdm.conf-custom /etc/gdm/'");
				} else{
					//Random ist aktiv :/
				}
				config.Execute("gdmflexiserver", "--command=\"UPDATE_CONFIG greeter/GraphicalTheme\"");
				revertIsAvailable=false;
			}
		}
	
		
		public CGdmTheme(CConfiguration config):base(config) {
			installationIsPossible=config.GrepIsAvailable && config.TarIsAvailable && config.SedIsAvailable;
		}

	}
}
