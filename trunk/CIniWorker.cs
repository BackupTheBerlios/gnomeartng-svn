// /home/neo/Projects/Monodevelop-Projects/GnomeArtNG/CIniWorker.cs created with MonoDevelop
// User: neo at 17:12 19.12.2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers


using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Xml;

namespace GnomeArtNG
{

 
	/// <summary>
	/// A Class to manage Ini files
	/// </summary>
	public class CIniWorker
	{
		/// <summary>
		/// Contents of the file
		/// </summary>
		private List<String> lines = new List<string>();
		
		/// <summary>
		/// Full Path and Name of the file
		/// </summary>
		private String fileName = "";

		/// <summary>
		/// What Char should be treated as a comment? 
		/// It's a list separated by a ;
		/// The first sign is per default taken for new comments 
		/// </summary>
		private String CommentCharacters = "#;";

		/// <summary>
		/// RegEx for a comment in a row
		/// </summary>
		private String regCommentStr = "";

		/// <summary>
		/// RegEx for an entry
		/// </summary>
		private Regex regEntry = null;

		/// <summary>
		/// RegEx for an Headline ( [Head] )
		/// </summary>
		private Regex regCaption = null;

		/// <summary>
		/// Empty standard constructor
		/// </summary>
		public CIniWorker()
		{
			regCommentStr = @"(\s*[" + CommentCharacters + "](?<comment>.*))?";
			regEntry = new Regex(@"^[ \t]*(?<entry>([^=])+)=(?<value>([^=" + CommentCharacters + "])+)" + regCommentStr + "$");
			regCaption = new Regex(@"^[ \t]*(\[(?<caption>([^\]])+)\]){1}" + regCommentStr + "$");
		}

		/// <summary>
		/// constructor that reads a file immediately
		/// </summary>
		/// <param name="filename">Name der einzulesenden Datei</param>
		public CIniWorker(string filename) : this ()
		{
			if (!File.Exists(filename))
					return;
			fileName = filename;
			using (StreamReader sr = new StreamReader(fileName))
				while (!sr.EndOfStream) lines.Add(sr.ReadLine().TrimEnd());
		}

		/// <summary>
		/// Saves the current file
		/// </summary>
		/// <returns>true if save was successfull</returns>
		public Boolean Save(){
			if (fileName == "") return false;
			try
			{
				using (StreamWriter sw = new StreamWriter(fileName))
					foreach (String line in lines)
						sw.WriteLine(line);
			}
			catch (IOException ex)
			{
				throw new IOException("Fehler beim Schreiben der Datei " + fileName, ex);
			}
			catch
			{
				throw new IOException("Fehler beim Schreiben der Datei " + fileName);
			}
			return true;
		}
		
		/// <summary>
		/// Saves a file to a specific place an name. All following operations are done 
		/// on the old fileName, not the one set here!
		/// </summary>
		/// <returns>true if save was successfull</returns>
		public Boolean Save(string FileName)
		{
			string temp=fileName;
			fileName=FileName;
			bool b=Save();
			fileName=temp;
			return b;
		}
		
		/// <summary>
		/// File directory
		/// </summary>
		/// <returns></returns>
		public String getDirectory()
		{
			return Path.GetDirectoryName(fileName);
		}

		/// <summary>
		/// Creates the in the given Inifile all sections in Sections 
		/// Sections must be devided by Divider
		/// </summary>
		/// <param name="Sections">Sections</param>
		/// <param name="Divider">Character that divides the sections</param>
		/// <returns>true if successfull</returns>
		public bool CreateSections(string Sections, char Divider){
			string[] tempArr = Sections.Split(Divider);
			foreach (string caption in tempArr){
				setValue(caption,"","",true);
			}
			return true;
		}
		
		/// <summary>
		/// Sucht die Zeilennummer (nullbasiert) 
		/// eines gewünschten Eintrages
		/// </summary>
		/// <param name="Caption">Name des Bereiches</param>
		/// <param name="CaseSensitive">true = Gross-/Kleinschreibung beachten</param>
		/// <returns>Nummer der Zeile, sonst -1</returns>
		private int SearchCaptionLine(String Caption, Boolean CaseSensitive)
		{
			if (!CaseSensitive) Caption = Caption.ToLower();
			for (int i = 0; i < lines.Count; i++)
			{
				String line = lines[i].Trim();
				if (line == "") continue;
				if (!CaseSensitive) line = line.ToLower();
				// Erst den gewünschten Abschnitt suchen
				if (line == "[" + Caption + "]")
					return i;
			}
			return -1;// Bereich nicht gefunden
		}

		/// <summary>
		/// Sucht die Zeilennummer (nullbasiert) 
		/// eines gewünschten Eintrages
		/// </summary>
		/// <param name="Caption">Name des Bereiches</param>
		/// <param name="Entry">Name des Eintrages</param>
		/// <param name="CaseSensitive">true = Gross-/Kleinschreibung beachten</param>
		/// <returns>Nummer der Zeile, sonst -1</returns>
		private int SearchEntryLine(String Caption, String Entry, Boolean CaseSensitive)
		{
			Caption = Caption.ToLower();
			if (!CaseSensitive) Entry = Entry.ToLower();
			int CaptionStart = SearchCaptionLine(Caption, false);
			if (CaptionStart < 0) return -1;
			for (int i = CaptionStart + 1; i < lines.Count; i++)
			{
				String line = lines[i].Trim();
				if (line == "") continue;
				if (!CaseSensitive) line = line.ToLower();
				if (line.StartsWith("[")) 
					return -1;// Ende, wenn der nächste Abschnitt beginnt
				if (Regex.IsMatch(line, @"^[ \t]*[" + CommentCharacters + "]"))
					continue; // Kommentar
				if (line.StartsWith(Entry)) 
					return i;// Eintrag gefunden
			}
			return -1;// Eintrag nicht gefunden
		}

		/// <summary>
		/// Kommentiert einen Wert aus
		/// </summary>
		/// <param name="Caption">Name des Bereiches</param>
		/// <param name="Entry">Name des Eintrages</param>
		/// <param name="CaseSensitive">true = Gross-/Kleinschreibung beachten</param>
		/// <returns>true = Eintrag gefunden und auskommentiert</returns>
		public Boolean commentValue(String Caption, String Entry, Boolean CaseSensitive)
		{
			int line = SearchEntryLine(Caption, Entry, CaseSensitive);
			if (line < 0) return false;
			lines[line] = CommentCharacters[0] + lines[line];
			return true;
		}

		/// <summary>
		/// Löscht einen Wert
		/// </summary>
		/// <param name="Caption">Name des Bereiches</param>
		/// <param name="Entry">Name des Eintrages</param>
		/// <param name="CaseSensitive">true = Gross-/Kleinschreibung beachten</param>
		/// <returns>true = Eintrag gefunden und gelöscht</returns>
		public Boolean deleteValue(String Caption, String Entry, Boolean CaseSensitive)
		{
			int line = SearchEntryLine(Caption, Entry, CaseSensitive);
			if (line < 0) return false;
			lines.RemoveAt(line);
			return true;
		}

		/// <summary>
		/// Liest den Wert eines Eintrages aus (case sensitive)
		/// </summary>
		/// <param name="Caption">Name des Bereiches</param>
		/// <param name="Entry">Name des Eintrages</param>
		/// <param name="CaseSensitive">true = Gross-/Kleinschreibung beachten</param>
		/// <returns>Wert des Eintrags oder leer</returns>
		public String getValue(String Caption, String Entry, Boolean CaseSensitive)
		{
			int line = SearchEntryLine(Caption, Entry, CaseSensitive);
			if (line < 0) return "";
			int pos = lines[line].IndexOf("=");
			if (pos < 0) return "";
			return lines[line].Substring(pos + 1).Trim();
			// Evtl. noch abschliessende Kommentarbereiche entfernen
		}

		/// <summary>
		/// Setzt einen Wert in einem Bereich. Wenn der Wert
		/// (und der Bereich) noch nicht existiert, werden die
		/// entsprechenden Einträge erstellt.
		/// </summary>
		/// <param name="Caption">Name des Bereichs</param>
		/// <param name="Entry">name des Eintrags</param>
		/// <param name="Value">Wert des Eintrags</param>
		/// <param name="CaseSensitive">true = Gross-/Kleinschreibung beachten</param>
		/// <returns>true = Eintrag erfolgreich gesetzt</returns>
		public Boolean setValue(String Caption, String Entry, String Value, Boolean CaseSensitive)
		{
			Caption = Caption.ToLower();
			if (!CaseSensitive) Entry = Entry.ToLower();
			int lastCommentedFound = -1;
			int CaptionStart = SearchCaptionLine(Caption, false);
			if (CaptionStart < 0)
			{
				lines.Add("[" + Caption + "]");
				if (Entry !="")
					lines.Add(Entry + "=" + Value);
				return true;
			}
			int EntryLine = SearchEntryLine(Caption, Entry, CaseSensitive);
			for (int i = CaptionStart + 1; i < lines.Count; i++)
			{
				String line = lines[i].Trim();
				if (!CaseSensitive) line = line.ToLower();
				if (line == "") continue;
				// Ende, wenn der nächste Abschnitt beginnt
				if (line.StartsWith("["))
				{
					if (Entry !="")
						lines.Insert(i, Entry + "=" + Value);
					return true;
					
				}
				// Suche aukommentierte, aber gesuchte Einträge
				// (evtl. per Parameter bestimmen können?), falls
				// der Eintrag noch nicht existiert.
				if (EntryLine<0)
					if (Regex.IsMatch(line, @"^[ \t]*[" + CommentCharacters + "]"))	{
						String tmpLine = line.Substring(1).Trim();
						if (tmpLine.StartsWith(Entry))
						{
							// Werte vergleichen, wenn gleich,
							// nur Kommentarzeichen löschen
							int pos = tmpLine.IndexOf("=");
							if (pos > 0)
							{
								if (Value == tmpLine.Substring(pos + 1).Trim())
								{
									lines[i] = tmpLine;
									return true;
								}
							}
							lastCommentedFound = i;
						}
						continue;// Kommentar
					}
				if (line.StartsWith(Entry))	{
					if (Entry!="")
						lines[i] = Entry + "=" + Value;
					return true;
					
				}
			}
			if (Entry!=""){
				if (lastCommentedFound > 0)
					lines.Insert(lastCommentedFound + 1, Entry + "=" + Value);
				else
					lines.Insert(CaptionStart + 1, Entry + "=" + Value);
			}
			return true;
		}

		/// <summary>
		/// Liest alle Einträge uns deren Werte eines Bereiches aus
		/// </summary>
		/// <param name="Caption">Name des Bereichs</param>
		/// <returns>Sortierte Liste mit Einträgen und Werten</returns>
		public SortedList<String, String> getCaption(String Caption)
		{
			SortedList<String, String> result = new SortedList<string,string>();
			Boolean CaptionFound = false;
			for (int i = 0; i < lines.Count; i++)
			{
				String line = lines[i].Trim();
				if (line == "") continue;
				// Erst den gewünschten Abschnitt suchen
				if (!CaptionFound)
					if (line.ToLower() != "[" + Caption + "]") continue;
					else
					{
						CaptionFound = true;
						continue;
					}
				// Ende, wenn der nächste Abschnitt beginnt
				if (line.StartsWith("[")) break;
				if (Regex.IsMatch(line, @"^[ \t]*[" + CommentCharacters + "]")) continue; // Kommentar
				int pos = line.IndexOf("=");
				if (pos < 0)
					result.Add(line, "");
				else
					result.Add(line.Substring(0,pos).Trim(),line.Substring(pos + 1).Trim());
			}
			return result;
		}

		/// <summary>
		/// Erstellt eine Liste aller enthaltenen Bereiche
		/// </summary>
		/// <returns>Liste mit gefundenen Bereichen</returns>
		public List<string> getAllCaptions()
		{
			List<string> result = new List<string>();
			for (int i = 0; i < lines.Count; i++)
			{
				//String line = lines[i];
				Match mCaption = regCaption.Match(lines[i]);
				if (mCaption.Success)
					result.Add(mCaption.Groups["caption"].Value.Trim());
			}
			return result;
		}

		/// <summary>
		/// Exportiert sämtliche Bereiche und deren Werte
		/// in ein XML-Dokument
		/// </summary>
		/// <returns>XML-Dokument</returns>
		public XmlDocument exportToXml()
		{
			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement(
				Path.GetFileNameWithoutExtension(this.fileName));
			doc.AppendChild(root);
			XmlElement Caption = null;
			for (int i = 0; i < lines.Count; i++)
			{
				Match mEntry = regEntry.Match(lines[i]);
				Match mCaption = regCaption.Match(lines[i]);
				if (mCaption.Success)
				{
					Caption = doc.CreateElement(mCaption.Groups["caption"].Value.Trim());
					root.AppendChild(Caption);
					continue;
				}
				if (mEntry.Success)
				{
					XmlElement xe = doc.CreateElement(mEntry.Groups["entry"].Value.Trim());
					xe.InnerXml = mEntry.Groups["value"].Value.Trim();
					if (Caption == null)
						root.AppendChild(xe);
					else
						Caption.AppendChild(xe);
				}
			}
			return doc;
		}
	}
}
