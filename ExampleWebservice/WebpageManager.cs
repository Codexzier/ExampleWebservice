using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExampleWebservice
{
    /// <summary>
    /// Simple manger to collect html side from directory 'Webpages'.
    /// </summary>
    public class WebpageManager
    {
        private readonly string _current = $"{Environment.CurrentDirectory}\\Webpages\\";
        private readonly IDictionary<string, PageItem> _pages = new Dictionary<string, PageItem>();
        private readonly FileSystemWatcher _fileWatcher;

        public WebpageManager()
        {
            this._pages.Add("/", new PageItem($"index.html"));
            this._pages.Add("/getData", new PageItem($"dataupload.json", true));

            this._fileWatcher = new FileSystemWatcher(this._current);
            this._fileWatcher.NotifyFilter = NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName;
            this._fileWatcher.Created += this._fileWatcher_Created;
            this._fileWatcher.Renamed += this._fileWatcher_Renamed;
            this._fileWatcher.EnableRaisingEvents = true;
        }

        private void _fileWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            if (!this.TryKeyname(e.OldFullPath, out string oldkey))
            {
                return;
            }

            var item = this._pages.FirstOrDefault(f => oldkey.EndsWith(f.Key));

            if(item.Key != null)
            {
                this._pages.Remove(item.Key);
            }

            this.AddPage(e.Name);
        }

        private void _fileWatcher_Created(object sender, FileSystemEventArgs e)
        {
            this.AddPage(e.Name);
        }

        /// <summary>
        /// Add page as new key value. It check exist or file is a html.
        /// </summary>
        /// <param name="name">Set a html file.</param>
        private void AddPage(string name)
        {
            if (!this.TryKeyname(name, out string key))
            {
                return;
            }

            if (this._pages.ContainsKey(key))
            {
                return;
            }

            this._pages.Add(key, new PageItem(name));
        }

        /// <summary>
        /// Create keyname from the filename
        /// </summary>
        /// <param name="filename">Filename to check it is html.</param>
        /// <param name="key">Output a key name for the aktual file.</param>
        /// <returns>Return true, if is an html.</returns>
        private bool TryKeyname(string filename, out string key)
        {
            // only html
            if (!filename.EndsWith(".html"))
            {
                key = string.Empty;
                return false;
            }

            key = $"/{filename.Substring(0, filename.Length - 5)}";
            return true;
        }

        /// <summary>
        /// Load the file content.
        /// </summary>
        /// <param name="method">method name</param>
        /// <param name="funcGetData">for data response.</param>
        /// <returns>Return the page or data in string.</returns>
        public string GetPage(string method, Func<string> funcGetData)
        {
            if (!this._pages.ContainsKey(method))
            {
                return "Missing side!";
            }

            string webpage = this._pages[method].Filename;
            string targetFile = $"{this._current}{webpage}";

            if (this._pages[method].LoadData)
            {
                File.WriteAllText(targetFile, funcGetData.Invoke());
            }

            if (!File.Exists(targetFile))
            {
                return "ERROR 404 - Side not found";
            }

            using var reader = new StreamReader(targetFile);

            return reader.ReadToEnd();
        }
    }
}
