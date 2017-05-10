using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using EPiServer.Research.Translation4.Common;
using EPiServer.Research.Connector.Language.LionBridge.FreewayWS;
using System.Xml;
using System.Configuration;
using System.Net.Mail;

namespace EPiServer.Research.Connector.Language.LionBridge
{
    public class Connector : IConnector
    {
        public Connector()
        {

        }

        public int GetHandlerSupportedVersionNumber()
        {
            return 1;
        }

        public Task[] GetTasks()
        {
            string ticket = GetTicket(username, password);
            Vojo v = GetWS();
            TasksList tl = v.GetTasks(ticket);
            return tl.Tasks;
        }

        public SubTask[] GetSubTasks(string taskID)
        {
            string ticket = GetTicket(username, password);
            Vojo v = GetWS();
            SubTasksList stl = v.GetSubTasks(ticket, taskID);
            return stl.SubTasks;
        }

        public UOM[] GetUOM(string taskID)
        {
            try
            {
                string ticket = GetTicket(username, password);
                Vojo v = GetWS();
                UOMsList ul = v.GetUOMs(ticket, taskID);
                return ul.UOMs;
            }
            catch
            {
            }
            return new UOM[]{};
        }

        private FreewayWS.Vojo _vojo = null;
        private FreewayAuth.FreewayAuth _auth = null;
        string wsurl = ConfigurationSettings.AppSettings["translationws"];
        string wsauthurl = ConfigurationSettings.AppSettings["translationauthws"];
        string username = ConfigurationSettings.AppSettings["translationuser"];
        string password = ConfigurationSettings.AppSettings["translationpwd"];
        public Vojo GetWS()
        {
            // only keep one instance of the Service
            if (_vojo == null)
            {
                _vojo = new Vojo();
                _vojo.UseDefaultCredentials = true;
                _vojo.Url = wsurl;                    
            }

            return _vojo;
        }
        public FreewayAuth.FreewayAuth GetAuthWS()
        {
            // only keep one instance of the Service
            if (_auth == null)
            {
                _auth = new FreewayAuth.FreewayAuth();
                
                _auth.Url = wsauthurl;
            }
            return _auth;
        }
        public string GetTicket(string user, string pass)
        {
            return GetAuthWS().Logon(user, pass);
        }
        #region IConnector Members

        public string GetConnectorName()
        {
            return "LionBridge";
        }
        public string SendProject(TranslationProject p)
        {            
            string ticket = GetTicket(username,password);
            Vojo v = GetWS();
            if (p.RemoteID == "0")
            {
                p.RemoteID = v.CreateProject(ticket, p.Name, "", "", "", DateTime.Parse(p.Properties["projectstartdate"] as string), DateTime.Parse(p.Properties["projectdeliverydate"] as string), null);
                p.RemoteStatus = (int)v.GetProjectStatus(ticket, p.RemoteID).ProjectStatusCode;                
                p.Modified = true;
            }
            else
            {
                p.RemoteStatus = (int)v.GetProjectStatus(ticket, p.RemoteID).ProjectStatusCode;
                p.Modified = true;
            }
            p.Status = TranslationStatus.Sending;
            p.Save();

            foreach (TranslationPage tp in p.Pages)
            {
                string tempfilename = tp.OriginalID + ".xml";
                
                MemoryStream m = new MemoryStream();
                XmlTextWriter xwriter = new XmlTextWriter(m,Encoding.UTF8);
                xwriter.WriteStartDocument(true);
                xwriter.WriteStartElement("page");
                xwriter.WriteAttributeString("itemid", tp.ItemID.ToString());
                foreach (string key in tp.Properties.Keys)
                {
                    xwriter.WriteStartElement("property");
                    
                    xwriter.WriteAttributeString("name",key);
                    
                    xwriter.WriteString(tp.Properties[key] as string);
                    xwriter.WriteEndElement();
                }
                xwriter.WriteEndElement();
                xwriter.WriteEndDocument();
                xwriter.Flush();
                byte[] data = m.ToArray();
                foreach (string lang in p.TargetLanguages)
                {                    
                    if (tp.GetStatus(lang)==TranslationStatus.Created)
                    {
                        FileInfoList fil = v.AddFileToProject( ticket, p.RemoteID, tempfilename, data, p.SourceLanguage,new string[]{lang}, new Meta[] { }, false);
                        foreach (EPiServer.Research.Connector.Language.LionBridge.FreewayWS.FileInfo fi in fil.FileInfoItems)
                        {
                            tp.SetRemoteID(fi.TargetLanguageID, fi.FileID);
                            tp.SetStatus(fi.TargetLanguageID, TranslationStatus.Sent);                            
                        }
                    }
                    
                }
                m.Close();
            }
            foreach (TranslationFile tf in p.Files)
            {
                Stream strm = tf.FileStream;
                byte[] data = new byte[strm.Length];

                strm.Read(data, 0, data.Length);

                strm.Close();
                
                foreach (string lang in p.TargetLanguages)
                {
                    if (tf.GetStatus(lang)==TranslationStatus.Created)
                    {
                        FileInfoList fil = v.AddFileToProject( ticket, p.RemoteID, Path.GetFileName(tf.FilePath), data, p.SourceLanguage,new string[]{lang}, new Meta[] { }, false);
                        //fil = v.GetFileStatus(ticket, p.RemoteID, p.SourceLanguage, string.Empty, string.Empty, Path.GetFileName(tf.FilePath));
                        foreach (EPiServer.Research.Connector.Language.LionBridge.FreewayWS.FileInfo fi in fil.FileInfoItems)
                        {
                            tf.SetRemoteID(fi.TargetLanguageID, fi.FileID);
                            tf.SetStatus(fi.TargetLanguageID, TranslationStatus.Sent);                            
                        }
                    }
                }
            }

            ///TODO: add code for add task
            /// this part is not so clear from lionbridge 
            if (v.GetProjectStatus(ticket, p.RemoteID).ProjectStatusCode == ProjectStatusCode.Draft)
            {
                v.AddTaskToProject(ticket, p.RemoteID, p.SourceLanguage, p.TargetLanguages[0], string.Empty, string.Empty, p.Properties["TaskID"], p.Properties["SubtaskID"], 1.0, p.Properties["UomID"]);
                
                v.SubmitProject(ticket, p.RemoteID);
            }
            p.Status = TranslationStatus.Sent;
            p.RemoteStatus = (int)ProjectStatusCode.Completed;
            p.Modified = true;
            
            return "";
        }

        public string AddPage(TranslationProject project, TranslationPage page)
        {
            throw new NotImplementedException();
        }

        public void UpdateProject(TranslationProject project)
        {
            if (project.RemoteID != "0")
            {
                string ticket = GetTicket(username, password);
                Vojo v = GetWS();


                ProjectStatus rStatus = v.GetProjectStatus(ticket, project.RemoteID);

                if (project.RemoteStatus != (int)rStatus.ProjectStatusCode)
                {
                    project.RemoteStatus = (int)rStatus.ProjectStatusCode;
                    project.Modified = true;
                }

                if (project.Status == TranslationStatus.Received)
                {
                    project.Status = TranslationStatus.ReadyForImport;
                    project.Modified = true;
                    MailMessage mailMessage = new MailMessage(new MailAddress("translate@lionx.com"), new MailAddress(project.Properties["notificationmail"]));
                    mailMessage.Subject = "Translation is ready for import";
                    mailMessage.Body = string.Format("Your translation \"{0}\" is ready for import",project.Name);
                    SmtpClient sc = new SmtpClient();
                    sc.Send(mailMessage);

                }
                else
                {
                    if (project.Status == TranslationStatus.Created)
                    {
                        project.Status = TranslationStatus.ReadyForSend;
                        project.Modified = true;
                    }
                    else
                    {
                        if (project.Status == TranslationStatus.Sent)
                        {
                            if ((int)project.RemoteStatus == (int)ProjectStatusCode.Completed)
                            {
                                project.Status = TranslationStatus.ReadyForRecieve;
                                project.Modified = true;
                            }
                        }
                    }
                }
               
            }
        }
        public string RetrieveProject(TranslationProject project)
        {
            if (project.RemoteID != "0")
            {
                string ticket = GetTicket(username,password);
                Vojo v = GetWS();


                ProjectStatus rStatus = v.GetProjectStatus(ticket, project.RemoteID);

                project.RemoteStatus = (int)rStatus.ProjectStatusCode;
                project.Modified = true;

                if ((int)project.RemoteStatus == (int)ProjectStatusCode.Completed)
                {
                    foreach (TranslationPage tp in project.Pages)
                    {
                        string tempfilename = tp.OriginalID + ".xml";
                        foreach (string lang in project.TargetLanguages)
                        {
                            string rId = tp.GetRemoteID(lang);
                            if (rId != "0")
                            {
                                FileStatusList fsl = v.GetFileStatus(ticket, project.RemoteID, project.SourceLanguage, lang, rId, tempfilename);
                                if (fsl.FileStatuses[0].StatusID != tp.GetRemoteStatus(lang))
                                {
                                    byte[] data = v.RetrieveProjectFileByID(ticket, project.RemoteID, rId);
                                    
                                    tp.SetData(lang, data);
                                    tp.SetStatus(lang, TranslationStatus.Received);
                                    tp.SetRemoteStatus(lang, fsl.FileStatuses[0].StatusID);
                                }
                            } 
                        }
                    }
                    foreach (TranslationFile tf in project.Files)
                    {
                        foreach (string lang in project.TargetLanguages)
                        {
                            string rId = tf.GetRemoteID(lang);
                            if (rId != "0")
                            {
                                FileStatusList fsl = v.GetFileStatus(ticket, project.RemoteID, project.SourceLanguage, lang, rId, Path.GetFileName(tf.FilePath));
                                if (fsl.FileStatuses[0].StatusID != tf.GetRemoteStatus(lang))
                                {
                                    byte[] data = v.RetrieveProjectFileByID(ticket, project.RemoteID, rId);

                                    tf.SetData(lang, data);
                                    tf.SetStatus(lang, TranslationStatus.Received);
                                    tf.SetRemoteStatus(lang, fsl.FileStatuses[0].StatusID);
                                }
                            }
                        }
                    }
                    project.Status = TranslationStatus.Received;
                    project.Modified = true;
                }
            }
            return "";
        }
      


        public TranslationPage GetPage(byte[] data)
        {
            TranslationPage ret = new TranslationPage();

            ret.Properties = new System.Collections.Hashtable();
            XmlTextReader reader = new XmlTextReader(new MemoryStream(data));
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == "property")
                        {
                            if (reader.MoveToAttribute("name"))
                            {
                                string propertyname = reader.Value;
                                string propertyvalue = reader.ReadString();
                                //reader.Read();
                                //if (reader.NodeType == XmlNodeType.Text)
                                //    propertyvalue = reader.Value;
                                ret.Properties.Add(propertyname, propertyvalue);
                            }
                        }
                        break;
                }
            }
            reader.Close();
            return ret;
        }

        

        

        public string GetPageRemoteStatus(TranslationProject project, TranslationPage tp,string lang)
        {
            string rId = tp.GetRemoteID(lang);
            if (rId != "0")
            {
                string ticket = GetTicket(username,password);
                Vojo v = GetWS();
                FileStatusList fsl = v.GetFileStatus(ticket, project.RemoteID, project.SourceLanguage, lang, rId, null);            
                tp.SetRemoteStatus(lang, fsl.FileStatuses[0].StatusID);
                return fsl.FileStatuses[0].StatusID;
            }             
            return "";
        }


        public string GetProjectRemoteStatus(TranslationProject tp)
        {
            if (tp.RemoteID != "0")
            {
                string ticket = GetTicket(username,password);
                Vojo v = GetWS();


                ProjectStatus rStatus = v.GetProjectStatus(ticket, tp.RemoteID);
                return rStatus.ProjectStatusCode.ToString();
            }
            return "";
        }
       

        public void SetProjectProperty(TranslationProject project)
        {
            project.Status = TranslationStatus.ReadyForSend;
        }

        #endregion
    }
}
