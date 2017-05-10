using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using EPiServer.Research.Translation4.Common;

namespace EPiServer.Research.Translation4.Core
{
    internal class Project
    {
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
        private int _id;
        public int ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
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
        private Guid _sessionID;
        public Guid SessionID
        {
            get
            {
                return _sessionID;
            }
            set
            {
                _sessionID = value;
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
        private string[] _targetLanguage;
        public string[] TargetLanguage
        {
            get
            {
                return _targetLanguage;
            }
            set
            {
                _targetLanguage = value;
            }
        }
        private TranslationStatus _remoteStatus;
        public TranslationStatus RemoteStatus
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

        public static Project Init(DataRow dr)
        {
            Project p = new Project();
            p.ID = (int)dr["pkid"];
            p.Name = (string)dr["projectname"];
            p.RemoteID = (string)dr["remoteid"];
            p.Status = (TranslationStatus)((int)dr["projectstatus"]);
            p.SessionID = (Guid)(dr["workid"]);
            p.Email = (string)(dr["alertemail"]);
            p.SourceLanguage = (string)(dr["sourcelanguage"]);
            p.RemoteStatus = (TranslationStatus)Enum.Parse(typeof(TranslationStatus), ((string)(dr["remoteStatus"])));

            string langs = Manager.Current.GetProjectTargetLanguages(p.ID);
            if (langs.Length > 0)
            {
                string[] langarray = langs.Split(new char[] { ',' });
                p.TargetLanguage = langarray;
            }
            else
            {
                p.TargetLanguage = new string[] { };
            }
            return p;
        }
    }
}
