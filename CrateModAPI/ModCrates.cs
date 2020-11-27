﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Globalization;

namespace CrateModLoader
{
    public class ModCrate
    {
        public Dictionary<string, string> Meta = new Dictionary<string, string>();
        public Dictionary<string, string> Settings = new Dictionary<string, string>();
        public string Path;
        public string Name = "Unnamed Mod";
        public string Desc = "(No Description)";
        public string Author = "(Not credited)";
        public string Version = "v1.0";
        public string CML_Version = ModLoaderGlobals.ProgramVersionSimple.ToString();
        public string TargetGame = ModCrates.AllGamesShortName;
        public string Plugin;
        public bool IsActivated = false;
        public bool HasSettings = false;
        public bool IsFolder = false;
        public bool HasIcon = false;
        // for importing icons in ModCrateMaker
        public string IconPath = "";
        // A workaround for nested folders
        public string NestedPath = "";

        public bool[] LayersModded = new bool[1] { false };
    }

    public static class ModCrates
    {

        /* Plan for how Mod Crates are supposed to work:
         * They're .zip files (or unzipped folders):
         * with folders called "layer0", "layer1", "layer2" etc. 
         * Each layer corresponds to a data archive type that the files inside replace (or add?) (game-specific, except for layer0)
         * modcrateinfo.txt file with the mod's metadata
         * (optional) modcratesettings.txt file with the game specfic settings that can't be altered with mods
         * (optional) modcrateicon.png icon of the mod
         * 
         * Layer 0 is where the base extracted files from a ROM are, so every game is supposed to support it
         */


        public const char Separator = '=';
        public const char CommentSymbol = '!';
        public const string LayerFolderName = "layer";
        public const string InfoFileName = "modcrateinfo.txt";
        public const string SettingsFileName = "modcratesettings.txt";
        public const string IconFileName = "modcrateicon.png";
        public const string UnsupportedGameShortName = "NoGame";
        public const string AllGamesShortName = "All";
        public const string Prop_Name = "Name";
        public const string Prop_Desc = "Description";
        public const string Prop_Author = "Author";
        public const string Prop_Version = "Version";
        public const string Prop_CML_Version = "ModLoaderVersion";
        public const string Prop_Plugin = "Plugin";
        public const string Prop_Game = "Game";

        public static List<ModCrate> SupportedMods = new List<ModCrate>();
        public static int ModsActiveAmount
        {
            get
            {
                int amount = 0;
                foreach (var mod in SupportedMods)
                {
                    if (mod.IsActivated)
                        ++amount;
                }
                return amount;
            }
        }

        public static void PopulateModList(bool IsSupportedGame, string ShortName)
        {

            bool SupportAll = false;
            if (!IsSupportedGame)
            {
                SupportAll = true;
            }
            else if (string.IsNullOrEmpty(ShortName))
            {
                Console.WriteLine("WARN: Target game is missing short name!");
                SupportAll = true;
            }

            List<ModCrate> ModList = new List<ModCrate>();

            DirectoryInfo di = new DirectoryInfo(ModLoaderGlobals.ModDirectory);
            foreach (FileInfo file in di.EnumerateFiles())
            {
                if (file.Extension.ToLower() == ".zip")
                {
                    try
                    {
                        ModCrate Crate = LoadMetadata(file);
                        if (Crate != null)
                        {
                            if (SupportAll)
                            {
                                if (Crate.TargetGame == UnsupportedGameShortName || Crate.TargetGame == AllGamesShortName)
                                {
                                    ModList.Add(Crate);
                                }
                            }
                            else
                            {
                                if (Crate.TargetGame == ShortName || Crate.TargetGame == AllGamesShortName)
                                {
                                    ModList.Add(Crate);
                                }
                            }
                        }
                    }
                    catch
                    {
                        //MessageBox.Show(ModLoaderText.ModCrateErrorPopup + " " + file.FullName);
                    }
                }
            }
            if (di.GetDirectories().Length > 0)
            {
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    try
                    {
                        ModCrate Crate = LoadMetadata(dir);
                        if (Crate != null)
                        {
                            if (SupportAll)
                            {
                                if (Crate.TargetGame == UnsupportedGameShortName || Crate.TargetGame == AllGamesShortName)
                                {
                                    ModList.Add(Crate);
                                }
                            }
                            else
                            {
                                if (Crate.TargetGame == ShortName || Crate.TargetGame == AllGamesShortName)
                                {
                                    ModList.Add(Crate);
                                }
                            }
                        }
                    }
                    catch
                    {
                        //MessageBox.Show(ModLoaderText.ModCrateErrorPopup + " " + dir.FullName);
                    }
                }
            }

            if (ModList.Count <= 0)
            {
                // 404: Mods Not Found
                return;
            }

            for (int mod = 0; mod < ModList.Count; mod++)
            {
                bool wasAdded = false;
                for (int i = 0; i < SupportedMods.Count; i++)
                {
                    if (SupportedMods[i].Path == ModList[mod].Path)
                    {
                        wasAdded = true;
                    }
                }

                if (!wasAdded)
                {
                    SupportedMods.Add(ModList[mod]);
                }
            }
            // todo: if a mod has been removed externally, the list won't update that


        }

        public static void UpdateModSelection(int index, bool value)
        {
            SupportedMods[index].IsActivated = value;
        }

        // Load metadata from a .zip file
        public static ModCrate LoadMetadata(FileInfo file)
        {
            ModCrate NewCrate = new ModCrate();
            bool HasInfo = false;
            bool HasSettings = false;
            int MaxLayer = 0;
            List<int> ModdedLayers = new List<int>();

            using (ZipArchive archive = ZipFile.OpenRead(file.FullName))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                    {
                        if (entry.Name.ToLower() == InfoFileName)
                        {
                            if (entry.Name != entry.FullName)
                            {
                                NewCrate.NestedPath = entry.FullName.Substring(0, entry.FullName.Length - entry.Name.Length);
                            }
                            using (StreamReader fileStream = new StreamReader(entry.Open(), true))
                            {
                                string line;
                                while ((line = fileStream.ReadLine()) != null)
                                {
                                    if (line[0] != CommentSymbol) //reserved for comments
                                    {
                                        string[] setting = line.Split(Separator);
                                        NewCrate.Meta[setting[0]] = setting[1];
                                        HasInfo = true;
                                    }
                                }
                            }
                        }
                        else if (entry.Name.ToLower() == SettingsFileName)
                        {
                            using (StreamReader fileStream = new StreamReader(entry.Open(), true))
                            {
                                string line;
                                while ((line = fileStream.ReadLine()) != null)
                                {
                                    if (line[0] != CommentSymbol) //reserved for comments
                                    {
                                        string[] setting = line.Split(Separator);
                                        NewCrate.Settings[setting[0]] = setting[1];
                                        HasSettings = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (entry.Name.ToLower() == IconFileName)
                    {
                        //NewCrate.Icon = Image.FromStream(entry.Open());
                        NewCrate.HasIcon = true;
                    }
                }
                if (HasInfo)
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (NewCrate.NestedPath != "")
                        {
                            if (entry.FullName.Length > NewCrate.NestedPath.Length && entry.FullName.Substring(NewCrate.NestedPath.Length, LayerFolderName.Length).ToLower() == LayerFolderName)
                            {
                                string NestedNumber = entry.FullName.Substring(NewCrate.NestedPath.Length + LayerFolderName.Length);
                                int Layer = int.Parse(NestedNumber.Split('/')[0]);
                                if (!ModdedLayers.Contains(Layer))
                                {
                                    MaxLayer = Math.Max(MaxLayer, Layer);
                                    ModdedLayers.Add(Layer);
                                }
                            }
                        }
                        else
                        {
                            if (entry.FullName.Split('/')[0].Substring(0, LayerFolderName.Length).ToLower() == LayerFolderName)
                            {
                                int Layer = int.Parse(entry.FullName.Split('/')[0].Substring(LayerFolderName.Length));
                                if (!ModdedLayers.Contains(Layer))
                                {
                                    MaxLayer = Math.Max(MaxLayer, Layer);
                                    ModdedLayers.Add(Layer);
                                }
                            }
                        }
                    }
                }
            }

            if (!HasInfo)
            {
                Console.WriteLine("WARN: Mod Crate has no info.txt file! Ignored.");
                return null;
            }

            if (ModdedLayers.Count > 0)
            {
                NewCrate.LayersModded = new bool[MaxLayer + 1];
                for (int i = 0; i < ModdedLayers.Count; i++)
                {
                    NewCrate.LayersModded[ModdedLayers[i]] = true;
                }
            }
            else
            {
                NewCrate.LayersModded = new bool[1] { false };
            }

            if (HasSettings)
            {
                NewCrate.HasSettings = true;
            }

            if (NewCrate.Meta.ContainsKey(Prop_Name))
                NewCrate.Name = NewCrate.Meta[Prop_Name];
            if (NewCrate.Meta.ContainsKey(Prop_Desc))
                NewCrate.Desc = NewCrate.Meta[Prop_Desc];
            if (NewCrate.Meta.ContainsKey(Prop_Author))
                NewCrate.Author = NewCrate.Meta[Prop_Author];
            if (NewCrate.Meta.ContainsKey(Prop_Version))
                NewCrate.Version = NewCrate.Meta[Prop_Version];
            if (NewCrate.Meta.ContainsKey(Prop_CML_Version))
                NewCrate.CML_Version = NewCrate.Meta[Prop_CML_Version];
            if (NewCrate.Meta.ContainsKey(Prop_Plugin))
                NewCrate.Plugin = NewCrate.Meta[Prop_Plugin];
            if (NewCrate.Meta.ContainsKey(Prop_Game))
                NewCrate.TargetGame = NewCrate.Meta[Prop_Game];
            if (NewCrate.Meta.ContainsKey(Prop_Name + "-" + CultureInfo.CurrentCulture.Name))
                NewCrate.Name = NewCrate.Meta[Prop_Name + "-" + CultureInfo.CurrentCulture.Name];
            if (NewCrate.Meta.ContainsKey(Prop_Desc + "-" + CultureInfo.CurrentCulture.Name))
                NewCrate.Desc = NewCrate.Meta[Prop_Desc + "-" + CultureInfo.CurrentCulture.Name];

            NewCrate.Path = file.FullName;

            //ModList.Add(NewCrate);
            return NewCrate;
        }
        public static ModCrate LoadMetadata(DirectoryInfo dir)
        {
            ModCrate NewCrate = new ModCrate();
            bool HasInfo = false;
            bool HasSettings = false;
            int MaxLayer = 0;
            List<int> ModdedLayers = new List<int>();

            foreach (FileInfo file in dir.EnumerateFiles())
            {
                if (file.Name.ToLower() == InfoFileName)
                {
                    using (StreamReader fileStream = new StreamReader(file.Open(FileMode.Open), true))
                    {
                        string line;
                        while ((line = fileStream.ReadLine()) != null)
                        {
                            if (line[0] != CommentSymbol) //reserved for comments
                            {
                                string[] setting = line.Split(Separator);
                                NewCrate.Meta[setting[0]] = setting[1];
                                HasInfo = true;
                            }
                        }
                    }
                }
                else if (file.Name.ToLower() == SettingsFileName)
                {
                    using (StreamReader fileStream = new StreamReader(file.Open(FileMode.Open), true))
                    {
                        string line;
                        while ((line = fileStream.ReadLine()) != null)
                        {
                            if (line[0] != CommentSymbol) //reserved for comments
                            {
                                string[] setting = line.Split(Separator);
                                NewCrate.Settings[setting[0]] = setting[1];
                                HasSettings = true;
                            }
                        }
                    }
                }
                else if (file.Name.ToLower() == IconFileName)
                {
                    //NewCrate.Icon = Image.FromFile(file.FullName);
                    NewCrate.HasIcon = true;
                }
            }
            if (dir.GetDirectories().Length > 0)
            {
                foreach (DirectoryInfo di in dir.GetDirectories())
                {
                    if (di.Name.Substring(0, LayerFolderName.Length).ToLower() == LayerFolderName)
                    {
                        int Layer = int.Parse(di.Name.Substring(LayerFolderName.Length));
                        if (!ModdedLayers.Contains(Layer))
                        {
                            MaxLayer = Math.Max(MaxLayer, Layer);
                            ModdedLayers.Add(Layer);
                        }
                    }
                }
            }

            if (!HasInfo)
            {
                Console.WriteLine("WARN: Mod Crate has no info.txt file! Ignored.");
                return null;
            }

            NewCrate.IsFolder = true;

            if (ModdedLayers.Count > 0)
            {
                NewCrate.LayersModded = new bool[MaxLayer + 1];
                for (int i = 0; i < ModdedLayers.Count; i++)
                {
                    NewCrate.LayersModded[ModdedLayers[i]] = true;
                }
            }
            else
            {
                NewCrate.LayersModded = new bool[1] { false };
            }

            if (HasSettings)
            {
                NewCrate.HasSettings = true;
            }

            if (NewCrate.Meta.ContainsKey(Prop_Name))
                NewCrate.Name = NewCrate.Meta[Prop_Name];
            if (NewCrate.Meta.ContainsKey(Prop_Desc))
                NewCrate.Desc = NewCrate.Meta[Prop_Desc];
            if (NewCrate.Meta.ContainsKey(Prop_Author))
                NewCrate.Author = NewCrate.Meta[Prop_Author];
            if (NewCrate.Meta.ContainsKey(Prop_Version))
                NewCrate.Version = NewCrate.Meta[Prop_Version];
            if (NewCrate.Meta.ContainsKey(Prop_CML_Version))
                NewCrate.CML_Version = NewCrate.Meta[Prop_CML_Version];
            if (NewCrate.Meta.ContainsKey(Prop_Plugin))
                NewCrate.Plugin = NewCrate.Meta[Prop_Plugin];
            if (NewCrate.Meta.ContainsKey(Prop_Game))
                NewCrate.TargetGame = NewCrate.Meta[Prop_Game];
            if (NewCrate.Meta.ContainsKey(Prop_Name + "-" + CultureInfo.CurrentCulture.Name))
                NewCrate.Name = NewCrate.Meta[Prop_Name + "-" + CultureInfo.CurrentCulture.Name];
            if (NewCrate.Meta.ContainsKey(Prop_Desc + "-" + CultureInfo.CurrentCulture.Name))
                NewCrate.Desc = NewCrate.Meta[Prop_Desc + "-" + CultureInfo.CurrentCulture.Name];

            NewCrate.Path = dir.FullName;

            //ModList.Add(NewCrate);
            return NewCrate;
        }

        public static void ClearModLists()
        {
            //ModList = new List<ModCrate>();
            SupportedMods = new List<ModCrate>();
        }

        /// <summary>
        /// Installs all active mods of the specified layer in the specified path
        /// </summary>
        public static void InstallLayerMods(string basePath, int layer)
        {
            for (int i = 0; i < SupportedMods.Count; i++)
            {
                if (SupportedMods[i].IsActivated && SupportedMods[i].LayersModded.Length > layer && SupportedMods[i].LayersModded[layer])
                {
                    if (!SupportedMods[i].IsFolder)
                    {
                        InstallLayerMod(SupportedMods[i], basePath, layer);
                    }
                    else
                    {
                        InstallLayerModFolder(SupportedMods[i], basePath, layer);
                    }
                }
            }
        }
        public static void InstallLayerMod(ModCrate Crate, string basePath, int layer)
        {
            using (ZipArchive archive = ZipFile.OpenRead(Crate.Path))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.Length > Crate.NestedPath.Length + LayerFolderName.Length + layer.ToString().Length + 1 && entry.FullName[entry.FullName.Length - 1] != '/')
                    {
                        string AdjustedName = entry.FullName.Substring(Crate.NestedPath.Length);
                        if (AdjustedName.Split('/').Length > 1 && AdjustedName.Split('/')[1].Length > 0)
                        {
                            if (AdjustedName.Split('/')[0].Substring(0, LayerFolderName.Length + layer.ToString().Length).ToLower() == LayerFolderName + layer)
                            {
                                int PathLen = Crate.NestedPath.Length + LayerFolderName.Length + layer.ToString().Length + 1;
                                string NewDir = entry.FullName.Substring(PathLen, entry.FullName.Length - PathLen - entry.Name.Length);
                                string extrPath = entry.FullName.Substring(Crate.NestedPath.Length + LayerFolderName.Length + layer.ToString().Length);
                                Directory.CreateDirectory(basePath + NewDir);
                                entry.ExtractToFile(basePath + extrPath, true);
                            }
                        }
                    }
                }
            }
        }
        public static void InstallLayerModFolder(ModCrate Crate, string basePath, int layer)
        {
            DirectoryInfo dest = new DirectoryInfo(basePath);
            DirectoryInfo source = new DirectoryInfo(Crate.Path + @"\" + LayerFolderName + layer);
            Recursive_CopyFiles(basePath, source, dest, "");
        }
        static void Recursive_CopyFiles(string basePath, DirectoryInfo di, DirectoryInfo dest, string buffer)
        {
            string mainbuffer = buffer + @"\";
            foreach (DirectoryInfo dir in di.EnumerateDirectories())
            {
                buffer = Path.Combine(mainbuffer, dir.Name);
                string tempFolder = dest.FullName + buffer + @"\";
                if (!Directory.Exists(tempFolder))
                {
                    Directory.CreateDirectory(tempFolder);
                }
                Recursive_CopyFiles(basePath, dir, dest, buffer);
            }
            foreach (FileInfo file in di.EnumerateFiles())
            {
                string relativePath = Path.Combine(dest.FullName, mainbuffer + @"\" + file.Name);
                File.Copy(file.FullName, basePath + relativePath, true);
            }
        }

        /// <summary>
        /// De-serializes mod properties in activated Mod Crates into the given Modder 
        /// </summary>
        public static void InstallCrateSettings(Modder modder)
        {
            for (int mod = 0; mod < SupportedMods.Count; mod++)
            {
                if (SupportedMods[mod].IsActivated && SupportedMods[mod].HasSettings)
                {
                    foreach (ModPropertyBase prop in modder.Props)
                    {
                        if (SupportedMods[mod].Settings.ContainsKey(prop.CodeName) && !prop.HasChanged) // Manual mod menu changes override mod crates
                        {
                            prop.DeSerialize(SupportedMods[mod].Settings[prop.CodeName], SupportedMods[mod]);
                            prop.HasChanged = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Takes the given Modder's mod properties and saves them into a valid settings text file at the given path. (if fullSettings is false - only saves altered properties)
        /// </summary>
        public static void SaveSettingsToFile(Modder mod, string path, bool fullSettings)
        {

            List<string> LineList = new List<string>();

            LineList.Add(string.Format("{0} {1} {2}", CommentSymbol, ModLoaderGlobals.ProgramVersion, "Auto-Generated Settings File"));

            foreach (ModPropertyBase prop in mod.Props)
            {
                if (fullSettings || (!fullSettings && prop.HasChanged))
                {
                    string text = "";
                    prop.Serialize(ref text);
                    LineList.Add(text);
                }
            }

            File.WriteAllLines(path, LineList);
        }

        /// <summary>
        /// Saves the given Mod Crate using the given Modder to given path.
        /// </summary>
        public static void SaveSimpleCrateToFile(Modder mod, string path, ModCrate crate)
        {
            List<string> LineList_Info = new List<string>();

            LineList_Info.Add(string.Format("{0}{1}{2}", Prop_Name, Separator, crate.Name));
            LineList_Info.Add(string.Format("{0}{1}{2}", Prop_Desc, Separator, crate.Desc));
            LineList_Info.Add(string.Format("{0}{1}{2}", Prop_Author, Separator, crate.Author));
            LineList_Info.Add(string.Format("{0}{1}{2}", Prop_Version, Separator, crate.Version));
            LineList_Info.Add(string.Format("{0}{1}{2}", Prop_CML_Version, Separator, crate.CML_Version));
            LineList_Info.Add(string.Format("{0}{1}{2}", Prop_Game, Separator, crate.TargetGame));

            File.WriteAllLines(Path.Combine(ModLoaderGlobals.BaseDirectory, InfoFileName), LineList_Info);

            SaveSettingsToFile(mod, Path.Combine(ModLoaderGlobals.BaseDirectory, SettingsFileName), false);

            if (crate.HasIcon)
            {
                File.Copy(crate.IconPath, Path.Combine(ModLoaderGlobals.BaseDirectory, IconFileName));
                //crate.Icon.Save(Path.Combine(ModLoaderGlobals.BaseDirectory, IconFileName));
            }

            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            {
                using (ZipArchive zip = new ZipArchive(fileStream, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(Path.Combine(ModLoaderGlobals.BaseDirectory, InfoFileName), InfoFileName);
                    zip.CreateEntryFromFile(Path.Combine(ModLoaderGlobals.BaseDirectory, SettingsFileName), SettingsFileName);
                    if (crate.HasIcon)
                    {
                        zip.CreateEntryFromFile(Path.Combine(ModLoaderGlobals.BaseDirectory, IconFileName), IconFileName);
                    }
                }
            }

            //cleanup
            File.Delete(Path.Combine(ModLoaderGlobals.BaseDirectory, InfoFileName));
            File.Delete(Path.Combine(ModLoaderGlobals.BaseDirectory, SettingsFileName));
            if (crate.HasIcon)
            {
                File.Delete(Path.Combine(ModLoaderGlobals.BaseDirectory, IconFileName));
            }

        }

        /// <summary>
        /// Loads mod properties from given Settings file or Mod Crate at given path into the given Modder
        /// </summary>
        public static void LoadSettingsFromFile(Modder mod, string path)
        {
            FileInfo file = new FileInfo(path);

            bool isModCrate = file.Extension.ToLower() == ".zip";

            Dictionary<string, string> Settings = new Dictionary<string, string>();

            //zip handling
            if (isModCrate)
            {
                using (ZipArchive archive = ZipFile.OpenRead(file.FullName))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                        {
                            if (entry.Name.ToLower() == SettingsFileName)
                            {
                                using (StreamReader fileStream = new StreamReader(entry.Open(), true))
                                {
                                    string line;
                                    while ((line = fileStream.ReadLine()) != null)
                                    {
                                        if (line[0] != CommentSymbol)
                                        {
                                            string[] setting = line.Split(Separator);
                                            Settings[setting[0]] = setting[1];
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                using (StreamReader fileStream = new StreamReader(path, true))
                {
                    string line;
                    while ((line = fileStream.ReadLine()) != null)
                    {
                        if (line[0] != CommentSymbol)
                        {
                            string[] setting = line.Split(Separator);
                            if (setting.Length > 1)
                            {
                                Settings[setting[0]] = setting[1];
                            }
                        }
                    }
                }
            }

            if (Settings.Count == 0)
            {
                //MessageBox.Show(ModLoaderText.ModMenuLoad_Error);
                return;
            }

            foreach (ModPropertyBase prop in mod.Props)
            {
                if (Settings.ContainsKey(prop.CodeName))
                {
                    prop.DeSerialize(Settings[prop.CodeName], null);
                    prop.HasChanged = true;
                }
            }

        }

        /// <summary>
        /// For checking if a layer is modded
        /// </summary>
        public static bool HasLayerModsActive(int layer)
        {
            for (int i = 0; i < SupportedMods.Count; i++)
            {
                if (SupportedMods[i].IsActivated && SupportedMods[i].LayersModded.Length > layer && SupportedMods[i].LayersModded[layer])
                {
                    return true;
                }
            }
            return false;
        }

    }
}
