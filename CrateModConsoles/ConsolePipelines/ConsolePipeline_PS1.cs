﻿using DiscUtils.Iso9660;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using CrateModLoader.Tools;
using CrateModLoader.Tools.ISO;

namespace CrateModLoader.ModPipelines
{

    public class ConsolePipeline_PS1 : ConsolePipeline
    {

        public string ISO_Label = "";

        public override ConsolePipelineInfo Metadata => new ConsolePipelineInfo()
        {
            Console = ConsoleMode.PS1,
            NeedsDetection = true,
            CanBuildROMfromFolder = false, // original file required for label
        };

        public override string TempPath => ModLoaderGlobals.BaseDirectory + ModLoaderGlobals.TempName + @"\";
        public override string ProcessPath => ModLoaderGlobals.TempName + @"\";

        private string TempBinPath = ModLoaderGlobals.BaseDirectory + "binconvout.iso";
        private BackgroundWorker GlobalWorker;
        public bool isBINimage = false;
        private int ExtractIterator = 0;
        private int ExtractFileCount = 0;
        private bool isExtracting = false;
        private string extractInputPath;
        private string extractOutputPath;

        public ConsolePipeline_PS1()
        {
            ISO_Label = "";
        }

        public override bool DetectROM(string inputPath, out string titleID, out uint regionID)
        {
            bool found = false;
            titleID = null;
            regionID = 0;
            using (FileStream isoStream = File.Open(inputPath, FileMode.Open))
            {
                CDReader cd;
                MemoryStream tempbin = null;

                if (Path.GetExtension(inputPath).ToLower() == ".bin") // PS1 CD image
                {
                    tempbin = new MemoryStream();
                    PSX2ISO.Run(isoStream, tempbin);
                    cd = new CDReader(tempbin, true);
                    isBINimage = true;
                }
                else if (!CDReader.Detect(isoStream))
                {
                    return false;
                }
                else
                {
                    cd = new CDReader(isoStream, true);
                }

                if (cd.FileExists(@"SYSTEM.CNF"))
                {
                    using (StreamReader sr = new StreamReader(cd.OpenFile(@"SYSTEM.CNF", FileMode.Open)))
                    {
                        titleID = sr.ReadLine();
                        found = (titleID.Contains("BOOT ") || titleID.Contains("BOOT="));
                    }
                }
                else
                {
                    found = false;
                }

                cd.Dispose();

                if (tempbin != null)
                {
                    tempbin.Dispose();
                    tempbin.Close();
                }
            }
            if (!found)
            {
                titleID = null;
            }
            return found;
        }

        public override bool DetectFolder(string inputPath, out string titleID, out uint regionID)
        {
            titleID = null;
            regionID = 0;
            bool found = false;
            string filePath = inputPath + @"SYSTEM.CNF";
            if (File.Exists(filePath))
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    titleID = sr.ReadLine();
                    found = (titleID.Contains("BOOT ") || titleID.Contains("BOOT="));
                }
            }
            if (!found)
            {
                titleID = null;
            }
            return found;
        }

        public override void Build(string inputPath, string outputPath, BackgroundWorker worker)
        {
            // Use CDBuilder
            CDBuilder isoBuild = new CDBuilder();
            isoBuild.UseJoliet = true;
            isoBuild.VolumeIdentifier = ISO_Label;

            DirectoryInfo di = new DirectoryInfo(inputPath);
            HashSet<FileStream> files = new HashSet<FileStream>();

            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                ISO_Common.Recursive_AddDirs(isoBuild, dir, dir.Name + @"\", files, true);
            }
            foreach (FileInfo file in di.GetFiles())
            {
                ISO_Common.AddFile(isoBuild, file, string.Empty, files, true);
            }

            using (FileStream output = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            using (Stream input = isoBuild.Build())
            {
                ISO2PSX.Run(input, output);
            }

            //isoBuild.Build(outputPath);

            foreach (FileStream file in files)
            {
                file.Close();
            }
        }

        private void Extractor_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int prog_corrected = (int)((e.ProgressPercentage / 100f) * 25f);
            GlobalWorker.ReportProgress(1 + prog_corrected);
        }
        private void Extractor_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker a = sender as BackgroundWorker;
            a.DoWork -= Extractor_DoWork;
            a.RunWorkerCompleted -= Extractor_RunWorkerCompleted;
            a.ProgressChanged -= Extractor_ProgressChanged;

            AsyncWorker = null;
        }
        private void Extractor_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            Extractor_Work(worker);

            int LastIterator = 0;

            while (isExtracting)
            {
                if (ExtractIterator != LastIterator)
                {
                    int p = (int)(((ExtractIterator / (float)ExtractFileCount) * 100f));
                    worker.ReportProgress(p);
                    LastIterator = ExtractIterator;
                }
                Thread.Sleep(100);
            }

        }
        private async void Extractor_Work(BackgroundWorker a)
        {

            IList<Task> extractTaskList = new List<Task>();
            Dictionary<string, string> Paths = new Dictionary<string, string>();
            FileStream tempbin = null;
            FileStream extract_isoStream = null;
            CDReader extract_reader;
            ExtractFileCount = 0;
            ExtractIterator = 0;

            // Mounting CDReader
            if (isBINimage) // PS1 BIN image
            {
                using (FileStream isoStream = File.Open(extractInputPath, FileMode.Open))
                {
                    FileStream binconvout = new FileStream(TempBinPath, FileMode.Create, FileAccess.Write);
                    PSX2ISO.Run(isoStream, binconvout);
                    binconvout.Close();
                    tempbin = new FileStream(TempBinPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                }
                extract_reader = new CDReader(tempbin, true);
            }
            else
            {
                extract_isoStream = File.Open(extractInputPath, FileMode.Open);
                extract_reader = new CDReader(extract_isoStream, true);
            }
            ISO_Label = extract_reader.VolumeLabel;

            if (!Directory.Exists(extractOutputPath))
            {
                Directory.CreateDirectory(extractOutputPath);
            }

            // Counting the files
            if (extract_reader.GetDirectories("").Length > 0)
            {
                foreach (string directory in extract_reader.GetDirectories(""))
                {
                    if (extract_reader.GetDirectoryInfo(directory).GetFiles().Length > 0)
                        foreach (string file in extract_reader.GetFiles(directory))
                            ExtractFileCount++;
                    if (extract_reader.GetDirectories(directory).Length > 0)
                    {
                        Recursive_CountFiles(extract_reader, directory, ref ExtractFileCount);
                    }
                }
            }
            if (extract_reader.GetDirectoryInfo("").GetFiles().Length > 0)
            {
                foreach (string file in extract_reader.GetFiles(""))
                {
                    ExtractFileCount++;
                }
            }

            // Scanning file paths, creating directories
            if (extract_reader.GetDirectories("").Length > 0)
            {
                foreach (string directory in extract_reader.GetDirectories(""))
                {
                    Directory.CreateDirectory(extractOutputPath + directory);
                    if (extract_reader.GetDirectoryInfo(directory).GetFiles().Length > 0)
                    {
                        foreach (string file in extract_reader.GetFiles(directory))
                        {
                            string filename = extractOutputPath + file;
                            filename = filename.Replace(";1", string.Empty);
                            Paths.Add(file, filename);
                        }
                    }
                    if (extract_reader.GetDirectories(directory).Length > 0)
                    {
                        Recursive_CreateDirs(extract_reader, directory, ref Paths);
                    }
                }
            }
            if (extract_reader.GetDirectoryInfo("").GetFiles().Length > 0)
            {
                foreach (string file in extract_reader.GetFiles(""))
                {
                    string filename = extractOutputPath + "/" + file;
                    filename = filename.Replace(";1", string.Empty);
                    Paths.Add(file, filename);
                }
            }

            extract_reader.Dispose();
            if (!isBINimage)
            {
                extract_isoStream.Close();
            }

            // Extracting files
            foreach (KeyValuePair<string, string> Path in Paths)
            {
                extractTaskList.Add(ISO_ExtractAsync(Path.Key, Path.Value, a));
            }

            await Task.WhenAll(extractTaskList);

            if (tempbin != null)
            {
                tempbin.Dispose();
                File.Delete(TempBinPath);
            }

            isExtracting = false;

        }

        private async Task ISO_ExtractAsync(string file, string path, BackgroundWorker worker)
        {
            Stream fileStreamFrom = null;
            Stream fileStreamTo = null;

            string input = extractInputPath;
            if (isBINimage)
                input = TempBinPath;

            // CDReader doesn't work in async, so this is the workaround
            Stream iso = File.Open(input, FileMode.Open, FileAccess.Read, FileShare.Read);
            CDReader cd = new CDReader(iso, true);

            fileStreamFrom = cd.OpenFile(file, FileMode.Open);
            fileStreamTo = File.Open(path, FileMode.OpenOrCreate);

            await fileStreamFrom.CopyToAsync(fileStreamTo);
            //fileStreamFrom.CopyTo(fileStreamTo);

            fileStreamFrom.Close();
            fileStreamTo.Close();

            iso.Close();
            cd.Dispose();

            ExtractIterator++;
        }

        public override void Extract(string inputPath, string outputPath, BackgroundWorker worker)
        {

            GlobalWorker = worker;
            extractInputPath = inputPath;
            extractOutputPath = outputPath;
            isExtracting = true;

            AsyncWorker = new BackgroundWorker();
            AsyncWorker.WorkerReportsProgress = true;
            AsyncWorker.DoWork += new DoWorkEventHandler(Extractor_DoWork);
            AsyncWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Extractor_RunWorkerCompleted);
            AsyncWorker.ProgressChanged += new ProgressChangedEventHandler(Extractor_ProgressChanged);
            AsyncWorker.RunWorkerAsync();


        }

        private void Recursive_CreateDirs(CDReader cd, string dir, ref Dictionary<string, string> Paths)
        {
            foreach (string directory in cd.GetDirectories(dir))
            {
                Directory.CreateDirectory(TempPath + @"\" + directory);
                if (cd.GetDirectoryInfo(directory).GetFiles().Length > 0)
                {
                    foreach (string file in cd.GetFiles(directory))
                    {
                        string filename = TempPath + @"\" + file;
                        filename = filename.Replace(";1", string.Empty);
                        Paths.Add(file, filename);
                        //fileStreamFrom = cd.OpenFile(file, FileMode.Open);
                        //fileStreamTo = File.Open(TempPath + @"\" + file, FileMode.OpenOrCreate);
                        //UpdateExtractProgress(worker, FileCount, ref FileIterator);
                    }
                }
                if (cd.GetDirectories(directory).Length > 0)
                {
                    Recursive_CreateDirs(cd, directory, ref Paths);
                }
            }
        }

        private void Recursive_CountFiles(CDReader cd, string dir, ref int FileCount)
        {
            foreach (string directory in cd.GetDirectories(dir))
            {
                if (cd.GetDirectoryInfo(directory).GetFiles().Length > 0)
                    foreach (string file in cd.GetFiles(directory))
                        FileCount++;
                if (cd.GetDirectories(directory).Length > 0)
                {
                    Recursive_CountFiles(cd, directory, ref FileCount);
                }
            }
        }

    }
}