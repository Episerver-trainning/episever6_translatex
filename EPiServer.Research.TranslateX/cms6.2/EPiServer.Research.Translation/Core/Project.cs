using System;
using System.Data;
using EPiServer.Research.Translation4.Common;

namespace EPiServer.Research.Translation.Core
{
    internal class Project
    {
        public string Name { get; set; }

        public int ID { get; set; }

        public string RemoteID { get; set; }

        public TranslationStatus Status { get; set; }

        public Guid SessionID { get; set; }

        public string Email { get; set; }

        public string SourceLanguage { get; set; }

        public string[] TargetLanguage { get; set; }

        public TranslationStatus RemoteStatus { get; set; }

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
