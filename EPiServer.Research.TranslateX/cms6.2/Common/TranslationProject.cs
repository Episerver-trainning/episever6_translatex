using System.Collections.Generic;

namespace EPiServer.Research.Translation4.Common
{
    public delegate List<TranslationPage> PageLoaderHandler(TranslationProject p);
    public delegate List<TranslationFile> FileLoaderHandler(TranslationProject p);
    public delegate void ProjectSaveHandler(TranslationProject p);
    public class TranslationProject
    {
        public PageLoaderHandler PageLoader { get; set; }

        private string _connectorName = string.Empty;

        public string ConnectorName
        {
            get
            {
                return  _connectorName;
            }
            set
            {
                _connectorName = value;
            }
        }
        public FileLoaderHandler FileLoader { get; set; }

        public string Name { get; set; }

        public string LocalID { get; set; }

        public string RemoteID { get; set; }

        public TranslationStatus Status { get; set; }

        public string Email { get; set; }

        public string SourceLanguage { get; set; }

        public int RemoteStatus { get; set; }
        
        private List<TranslationPage> _pages;

        /// <summary>
        /// Pages to be translated
        /// </summary>
        public List<TranslationPage> Pages
        {
            get
            {
                if (_pages == null)
                {
                    if (PageLoader != null)
                    {
                        _pages = PageLoader(this);
                    }
                }
                return _pages;
            }
            set
            {
                _pages = value;
            }
        }
        private List<TranslationFile> _files;
        public List<TranslationFile> Files
        {
            get
            {
                if (_files == null)
                {
                    if (FileLoader != null)
                    {
                        _files = FileLoader(this);
                    }
                }
                return _files;
            }
            set
            {
                _files = value;
            }
        }

        public List<string> TargetLanguages { get; set; }

        public IConnector Connector { get; set; }

        /// <summary>
        /// All properties
        /// </summary>
        public Dictionary<string, string> Properties { get; set; }

        public bool Modified { get; set; }

        public string WorkSessionID { get; set; }

        public TranslationProject()
        {
            FileLoader = null;
            PageLoader = null;
        }

        public ProjectSaveHandler ProjHandler { get; set; }

        public void Save()
        {
            if (ProjHandler != null)
            {
                ProjHandler(this);
            }
        }
    }
}
