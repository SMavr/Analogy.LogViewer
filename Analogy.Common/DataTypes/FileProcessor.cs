﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Analogy.Common.Interfaces;
using Analogy.Common.Managers;
using Analogy.Interfaces;

namespace Analogy.Common.DataTypes
{
    public class FileProcessor
    {
        public event EventHandler<string> OnFileReadingFinished;
        public DateTime lastNewestMessage;
        private IUserSettingsManager Settings { get; }
        private string FileName { get; set; }
        private ILogMessageCreatedHandler DataWindow { get; }
        public IAnalogyLogger Logger { get; }

        public FileProcessor(IUserSettingsManager settingsManager, ILogMessageCreatedHandler dataWindow, IAnalogyLogger logger)
        {
            DataWindow = dataWindow;
            Logger = logger;
            Settings = settingsManager;
         }

        public async Task<IEnumerable<AnalogyLogMessage>> Process(IAnalogyOfflineDataProvider fileDataProvider,
            string filename, CancellationToken token, bool isReload = false)
        {
            //TODO in case of zip recursive call on all extracted files


            FileName = filename;
            if (string.IsNullOrEmpty(FileName))
            {
                return new List<AnalogyLogMessage>();
            }

            if (!isReload && !DataWindow.ForceNoFileCaching &&
                FileProcessingManager.Instance.AlreadyProcessed(FileName) &&
                Settings.EnableFileCaching) //get it from the cache
            {
                var cachedMessages = FileProcessingManager.Instance.GetMessages(FileName);
                DataWindow.AppendMessages(cachedMessages, GetFileNameAsDataSource(FileName));
                OnFileReadingFinished?.Invoke(this, filename);
                return cachedMessages;

            }

            if (FileProcessingManager.Instance.IsFileCurrentlyBeingProcessed(FileName))
            {
                while (FileProcessingManager.Instance.IsFileCurrentlyBeingProcessed(FileName))
                {
                    await Task.Delay(1000);
                }

                var cachedMessages = FileProcessingManager.Instance.GetMessages(FileName);
                DataWindow.AppendMessages(cachedMessages, GetFileNameAsDataSource(FileName));
                OnFileReadingFinished?.Invoke(this, filename);
                return cachedMessages;

            }

            //otherwise read file:
            try
            {

                if (fileDataProvider.CanOpenFile(filename)) //if can open natively: add to processing and process
                {
                    FileProcessingManager.Instance.AddProcessingFile(FileName);

                    if (!DataWindow.DoNotAddToRecentHistory)
                    {
                        Settings.AddToRecentFiles(fileDataProvider.Id, FileName);
                    }

                    var messages = (await fileDataProvider.Process(filename, token, DataWindow).ConfigureAwait(false))
                        .ToList();
                    FileProcessingManager.Instance.DoneProcessingFile(messages.ToList(), FileName);
                    if (messages.Any())
                    {
                        lastNewestMessage = messages.Select(m => m.Date).Max();
                    }

                    OnFileReadingFinished?.Invoke(this, filename);
                    return messages;
                }
                else //cannot open natively. is it compressed file?
                {
                    if (Settings.EnableCompressedArchives && IsCompressedArchive(filename))
                    {
                        var compressedMessages = new List<AnalogyLogMessage>();
                        string extractedPath = UnzipFilesIntoTempFolder(filename, fileDataProvider);
                        if (string.IsNullOrEmpty(extractedPath))
                        {
                            string msg = $"File is not supported by {fileDataProvider}. File: {filename}";
                            Logger.LogCritical("Analogy", msg);
                            AnalogyLogMessage error = new AnalogyErrorMessage(msg, "Analogy");
                            error.Source = nameof(FileProcessor);
                            error.Module = "Analogy";
                            DataWindow.AppendMessage(error, fileDataProvider.GetType().FullName);
                            return new List<AnalogyLogMessage> { error };
                        }
                        CleanupManager.Instance.AddFolder(extractedPath);
                        var files = Directory.GetFiles(extractedPath);
                        foreach (string file in files)
                        {
                            var messages = await Process(fileDataProvider, file, token, isReload);
                            compressedMessages.AddRange(messages);
                        }
                        return compressedMessages;

                    }
                    else
                    {
                        string msg = $"File is not supported by {fileDataProvider}. File: {filename}";
                        Logger.LogCritical("Analogy", msg);
                        AnalogyLogMessage error = new AnalogyErrorMessage(msg, "Analogy");
                        error.Source = nameof(FileProcessor);
                        error.Module = "Analogy";
                        DataWindow.AppendMessage(error, fileDataProvider.GetType().FullName);
                        return new List<AnalogyLogMessage> { error };
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogCritical("Analogy", $"Error parsing file: {e}");
                var error = new AnalogyErrorMessage($"Error reading file {filename}: Error: {e.Message}", "Analogy");
                error.Source = nameof(FileProcessor);
                error.Module = "Analogy";
                DataWindow.AppendMessage(error, fileDataProvider.GetType().FullName);
                return new List<AnalogyLogMessage> { error };
            }
        }


        private string UnzipFilesIntoTempFolder(string zipPath, IAnalogyOfflineDataProvider fileDataProvider)
        {
            string extractPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(extractPath);
            if (zipPath.EndsWith("zip", StringComparison.InvariantCultureIgnoreCase))
            {
                UnzipZipFileIntoTempFolder(zipPath, extractPath, fileDataProvider);
            }
            else if (zipPath.EndsWith("gz", StringComparison.InvariantCultureIgnoreCase))
            {
                UnzipGzFileIntoTempFolder(zipPath, extractPath);
            }
            else
            {
                Logger.LogError(nameof(UnzipFilesIntoTempFolder), $"Unsupported file: {zipPath}.");
                return string.Empty;
            }

            return extractPath;
        }

        private void UnzipGzFileIntoTempFolder(string zipPath, string extractPath)
        {
            FileInfo fileToDecompress = new FileInfo(zipPath);
            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                string currentFileName = fileToDecompress.Name;
                string newFileName = Path.Combine(extractPath, currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length));

                using (FileStream decompressedFileStream = File.Create(newFileName))
                {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                    }
                }
            }
        }

        private void UnzipZipFileIntoTempFolder(string zipPath, string extractPath, IAnalogyOfflineDataProvider fileDataProvider)
        {

            using (FileStream zipToOpen = new FileStream(zipPath, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                {

                    //build a list of files to be extracted
                    var entries = archive.Entries.Where(entry => !entry.FullName.EndsWith("/") && fileDataProvider.CanOpenFile(entry.FullName));
                    foreach (ZipArchiveEntry entry in entries)
                    {
                        entry.ExtractToFile(Path.Combine(extractPath, entry.Name));
                    }

                    if (!Directory.GetFiles(extractPath).Any())
                    {
                        Logger.LogError(nameof(UnzipFilesIntoTempFolder),
                            "Zip file does not contain any supported files");
                    }

                }
            }
        }
        private static string GetFileNameAsDataSource(string fileName)
        {
            string file = Path.GetFileName(fileName);
            return fileName != null && fileName.Equals(file) ? fileName : $"{file} ({fileName})";
        }
        private static bool IsCompressedArchive(string filename)
        {
            return filename.EndsWith(".gz", StringComparison.InvariantCultureIgnoreCase) ||
                   filename.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase);
        }

    }
}
