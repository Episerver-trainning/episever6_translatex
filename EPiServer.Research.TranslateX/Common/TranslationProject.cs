using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;

namespace EPiServer.Research.Translation4.Common
{
    public delegate List<TranslationPage> PageLoaderHandler(TranslationProject p);
    public delegate List<TranslationFile> FileLoaderHandler(TranslationProject p);
    public delegate void ProjectSaveHandler(TranslationProject p);
    public class TranslationProject
    {

        public TranslationProject()
        {
        }
        private PageLoaderHandler _pageLoader = null;
        public PageLoaderHandler PageLoader
        {
            get
            {
                return _pageLoader;
            }
            set
            {
                _pageLoader = value;
            }
        }

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

        private FileLoaderHandler _fileLoader = null;


        public FileLoaderHandler FileLoader
        {
            get
            {
                return _fileLoader;
            }
            set
            {
                _fileLoader = value;
            }
        }
        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        private string _localID;
        public string LocalID
        {
            get
            {
                return _localID;
            }
            set
            {
                _localID = value;
            }
        }

        private string _remoteID;
        public string RemoteID
        {
            get
            {
                return _remoteID;
            }
            set
            {
                _remoteID = value;
            }
        }
        private TranslationStatus _status;
        public TranslationStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
            }
        }
        private string _email;
        public string Email
        {
            get
            {
                return _email;
            }
            set
            {
                _email = value;
            }
        }
        private string _sourceLanguage;
        public string SourceLanguage
        {
            get
            {
                return _sourceLanguage;
            }
            set
            {
                _sourceLanguage = value;
            }
        }
        private int _remoteStatus;
        public int RemoteStatus
        {
            get
            {
                return _remoteStatus;
            }
            set
            {
                _remoteStatus = value;
            }
        }



        private List<TranslationPage> _pages = null;

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
        private List<TranslationFile> _files = null;
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

        private List<string> _targetLanguages;
        public List<string> TargetLanguages
        {
            get
            {
                return _targetLanguages;
            }
            set
            {
                _targetLanguages = value;
            }
        }
        private IConnector _connector;
        public IConnector Connector
        {
            get
            {
                return _connector;
            }
            set
            {
                _connector = value;
            }
        }
        private Dictionary<string, string> _properties;

        /// <summary>
        /// All properties
        /// </summary>
        public Dictionary<string, string> Properties
        {
            get
            {
                return _properties;
            }
            set
            {
                _properties = value;
            }
        }

        private bool _modified;
        public bool Modified
        {
            get
            {
                return _modified;
            }
            set
            {
                _modified = value;
            }
        }
        private string _workSessionID;
        public string WorkSessionID
        {
            get
            {
                return _workSessionID;
            }
            set
            {
                _workSessionID = value;
            }
        }

        private ProjectSaveHandler _projHandler = null;
        public ProjectSaveHandler ProjHandler
        {
            set
            {
                _projHandler = value;
            }
            get
            {
                return _projHandler;
            }
        }

        public void Save()
        {
            if (ProjHandler != null)
            {
                ProjHandler(this);
            }
        }
    }
}
