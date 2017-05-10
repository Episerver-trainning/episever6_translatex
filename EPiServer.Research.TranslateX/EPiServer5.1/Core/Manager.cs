using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Data.SqlClient;
using EPiServer.Core;
using EPiServer.Research.Translation4.Common;
using System.Collections.Generic;
using EPiServer.FileSystem;
using System.IO;
using EPiServer.Web.Hosting;
using System.Xml;

namespace EPiServer.Research.Translation4.Core
{
    public class Manager
    {
        public static Manager Current
        {
            get
            {
                if (_manager == null)
                    _manager = new Manager();
                return _manager;
            }
        }
        private static Manager _manager;

        private List<ConnectorDefinition> _connectors = null;

        public List<ConnectorDefinition> Connectors
        {
            get
            {
                if (_connectors == null)
                { 
                    XmlDocument xdoc = new XmlDocument();
                    string path = EPiServer.Global.BaseDirectory + "translationconnector.config";
                    using (Stream strm = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        xdoc.Load(strm);

                        _connectors = new List<ConnectorDefinition>();

                        XmlNodeList xnl = xdoc.SelectNodes("/connectors/connector");
                        foreach (XmlNode xn in xnl)
                        {
                            ConnectorDefinition cd = new ConnectorDefinition();
                            cd.AssemblyName = xn.SelectSingleNode("assemblyname").InnerText;
                            cd.Name = xn.SelectSingleNode("name").InnerText;
                            cd.TypeName = xn.SelectSingleNode("typename").InnerText;
                            cd.ControlToCreatingLaststep=xn.SelectSingleNode("usercontroltolastcreatestep").InnerText;
                            cd.ControlToImportStep=xn.SelectSingleNode("usercontroltofirstimportstep").InnerText;
                            cd.ControlToView = xn.SelectSingleNode("usercontroltoview").InnerText;
                            _connectors.Add(cd);
                        }
                        strm.Close();
                    }                    
                }
                return _connectors;
            }
        }

        public IConnector GetConntectorByName(string name)
        {
            for (int i = 0; i < Connectors.Count; i++)
            {
                if (Connectors[i].Name == name)
                {
                    string connectorName = Connectors[i].TypeName + "," + Connectors[i].AssemblyName;
                    Type assem = Type.GetType(connectorName);
                    IConnector ret = (IConnector)assem.InvokeMember("", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.CreateInstance, null, null, new object[] { });
                    return ret;
                }
            }
            return null;
        }
        public ConnectorDefinition GetConntectorDefinitionByName(string name)
        {
            for (int i = 0; i < Connectors.Count; i++)
            {
                if (Connectors[i].Name == name)
                {
                    return Connectors[i];
                }
            }
            return null;
        }
        public IConnector[] GetConnectors()
        {
            IConnector[] connectors = new IConnector[Connectors.Count];

            for (int i=0 ; i< Connectors.Count;i++)                
            {
                string connectorName = Connectors[i].TypeName + "," + Connectors[i].AssemblyName;
                Type assem = Type.GetType(connectorName);
                connectors[i] = (IConnector)assem.InvokeMember("", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.CreateInstance, null, null, new object[] { });
            }
            return connectors;
        }

        public string ToValidLocale(string s)
        {
            string sLanguage = s;
            if (sLanguage.Length != 5)
            {
                sLanguage = EPiServer.Core.LanguageManager.Instance.Translate("/translationlanguages/" + sLanguage, "en");
            }
            return sLanguage;
        }

        private SqlConnection _conn = null;

        private SqlConnection Connection
        {
            get
            {
                if (_conn == null )
                    _conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[EPiServer.Configuration.Settings.Instance.ConnectionStringName].ConnectionString);
                return _conn;
            }
        }
        private void OpenConnection()
        {
            if (Connection.State==ConnectionState.Closed)
                Connection.Open();
        }
        public int CreateProject(string name, string sessionID,string connectorname, string email, string sourceLanguage,TranslationStatus status)
        {
            string sLanguage = ToValidLocale(sourceLanguage);
            OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "set nocount on;insert into tbltranslationproject (projectname,projectstatus,workid,alertemail,sourcelanguage,connectorname) values (@projectname,@status,@workid,@alertemail,@sourcelanguage,@connectorname);set nocount off;select @@identity";
            cmd.Parameters.Add("@projectname", SqlDbType.NVarChar, 255).Value = name;
            cmd.Parameters.Add("@workid", SqlDbType.UniqueIdentifier).Value = new Guid(sessionID);
            cmd.Parameters.Add("@alertemail", SqlDbType.NVarChar, 255).Value = email;
            cmd.Parameters.Add("@status", SqlDbType.Int).Value = status;
            cmd.Parameters.Add("@sourcelanguage", SqlDbType.NVarChar, 10).Value = sLanguage;
            cmd.Parameters.Add("@connectorname", SqlDbType.NVarChar, 255).Value = connectorname;
            cmd.Connection = Connection;

            int ret = int.Parse(cmd.ExecuteScalar().ToString());           
            Connection.Close();
            return ret;
        }

        public void InsertTargetLanguage(int projectid, string language)
        {
            string sLanguage = ToValidLocale(language);
            OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "insert into tbltranslationprojectlanguage (projectid,targetlanguage) values (@projectid,@targetlanguage)";
            cmd.Parameters.Add("@projectid", SqlDbType.Int).Value = projectid;
            cmd.Parameters.Add("@targetlanguage", SqlDbType.NVarChar, 10).Value = sLanguage;
            cmd.Connection = Connection;
            cmd.ExecuteNonQuery();
            Connection.Close();
        }
        public DataSet GetRelatedProjects(int CurrentPageID)
        {
            DataSet ds = new DataSet();
            OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select distinct tbltranslationproject.* from tblTranslationProjectItem inner join tblTranslationProject on (tblTranslationproject.workid = tblTranslationProjectItem.workid and (page_link like @pagelinklike or page_link = @pagelink) and indent=1)";
            cmd.Parameters.Add("@pagelinklike", SqlDbType.NVarChar, 10).Value = CurrentPageID.ToString() + "[_]%";
            cmd.Parameters.Add("@pagelink", SqlDbType.NVarChar, 10).Value = CurrentPageID.ToString();
            cmd.Connection = Connection;

            SqlDataAdapter sda = new SqlDataAdapter();
            sda.SelectCommand = cmd;
            sda.Fill(ds);
            
            Connection.Close();

            return ds;
        }

        public DataSet GetProjects()
        {
            DataSet ds = new DataSet();
            OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select tbltranslationproject.* from tblTranslationProject";

            cmd.Connection = Connection;

            SqlDataAdapter sda = new SqlDataAdapter();
            sda.SelectCommand = cmd;
            sda.Fill(ds);

            Connection.Close();

            return ds;
        }

        public DataSet GetPageList(string sessionID)
        {
            DataSet ds = new DataSet();
            OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select * from tblTranslationProjectItem where workid=@workid and itemtype=@itemtype";
            cmd.Parameters.Add("@workid", SqlDbType.UniqueIdentifier).Value = new Guid(sessionID);
            cmd.Parameters.Add("@itemtype", SqlDbType.Int).Value = (int)ItemType.Page;
            cmd.Connection = Connection;

            SqlDataAdapter sda = new SqlDataAdapter();
            sda.SelectCommand = cmd;
            sda.Fill(ds);

            Connection.Close();

            return ds;
        }

        public TranslationProject GetTranslationProject(int projectid)
        {
            OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select * from tbltranslationproject where pkid=@projectid";
            cmd.Parameters.Add("@projectid", SqlDbType.Int).Value = projectid;
            cmd.Connection = Connection;

            TranslationProject ret = null;
            using (SqlDataReader sdr = cmd.ExecuteReader())
            {
                if (sdr.Read())
                {
                    ret = new TranslationProject();
                    ret.LocalID = ((int)sdr["pkid"]).ToString();
                    ret.Name = (string)sdr["projectname"];
                    ret.RemoteID = sdr["remoteid"] as string ?? "0";
                    ret.Status = (TranslationStatus)((int)sdr["projectstatus"]);
                    ret.Email = (string)(sdr["alertemail"]);
                    ret.SourceLanguage = (string)(sdr["sourcelanguage"]);
                    ret.WorkSessionID = ((Guid)sdr["workid"]).ToString();
                    ret.ConnectorName = (string)(sdr["connectorname"]);
                    //ret.RemoteStatus = ((ProjectStatus)Enum.Parse(typeof(ProjectStatus), ((string)(sdr["remoteStatus"])))).ToString();

                }
                sdr.Close();
                if (ret != null)
                {
                    string langs = GetProjectTargetLanguages(int.Parse(ret.LocalID));
                    if (langs.Length > 0)
                    {
                        ret.TargetLanguages = new List<string>();
                        string[] langarray = langs.Split(new char[] { ',' });
                        foreach (string lang in langarray)
                            ret.TargetLanguages.Add(lang);
                    }
                    else
                    {
                        ret.TargetLanguages = new List<string>();
                    }
                }
            }
            ret.Properties = new Dictionary<string, string>();
            OpenConnection();
            cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.Connection = Connection;
            cmd.CommandText = "select * from tbltranslationprojectproperty where fkprojectid=@projid";
            cmd.Parameters.Add("@projid", SqlDbType.Int).Value = projectid;
            using (SqlDataReader sdr = cmd.ExecuteReader())
            {                              
                while (sdr.Read())
                {
                    ret.Properties.Add((string)sdr["propertyname"], (string)sdr["propertyvalue"]);
                }
                sdr.Close();
            }            
            Connection.Close();
            ret.PageLoader = new PageLoaderHandler(PageLoad);
            ret.ProjHandler = new ProjectSaveHandler(Save);
            ret.FileLoader = new FileLoaderHandler(FileLoad);
            return ret;
        }

        public string GetProjectTargetLanguages(int projectid)
        {
            OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select targetlanguage from tbltranslationprojectlanguage where projectid=@projectid";
            cmd.Parameters.Add("@projectid", SqlDbType.Int).Value = projectid;
            cmd.Connection = Connection;

            string ret = "";
            using (SqlDataReader sdr = cmd.ExecuteReader())
            {
                while (sdr.Read())
                {
                    if (ret.Length > 0)
                    {
                        ret += ",";
                    }
                    ret += sdr.GetString(0);
                }
                sdr.Close();
            }
            Connection.Close();

            return ret;
            
        }

        public void Save(TranslationProject project)
        {
            OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.Connection = Connection;
            cmd.CommandText = @"update tbltranslationproject 
                                set 
                                projectname=@projectname, 
                                remoteid=@remoteid, 
                                projectstatus = @projectstatus,
                                alertemail = @alertemail,
                                sourceLanguage = @sourceLanguage,
                                remoteStatus = @remoteStatus,
                                connectorname = @connectorname  
                                where pkid=@projectid";
            cmd.Parameters.Add("@projectname", SqlDbType.NVarChar, 50).Value = project.Name;
            cmd.Parameters.Add("@remoteid", SqlDbType.NVarChar, 50).Value = project.RemoteID;
            ///ToDo : handle status 
            cmd.Parameters.Add("@projectstatus", SqlDbType.Int).Value = project.Status; 
            cmd.Parameters.Add("@alertemail", SqlDbType.NVarChar, 255).Value = project.Email;
            cmd.Parameters.Add("@sourceLanguage", SqlDbType.NVarChar, 10).Value = project.SourceLanguage;
            cmd.Parameters.Add("@remoteStatus", SqlDbType.Int).Value = project.RemoteStatus;
            cmd.Parameters.Add("@connectorname", SqlDbType.NVarChar, 255).Value = project.ConnectorName;
            cmd.Parameters.Add("@projectid", SqlDbType.Int).Value = project.LocalID;
            cmd.ExecuteNonQuery();

            cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.Connection = Connection;
            cmd.CommandText = "delete from tbltranslationprojectproperty where fkprojectid=@projid";
            cmd.Parameters.Add("@projid", SqlDbType.Int).Value = project.LocalID;
            cmd.ExecuteNonQuery();

            foreach (string key in project.Properties.Keys)
            {
                cmd = new SqlCommand();
                cmd.CommandType = CommandType.Text;
                cmd.Connection = Connection;
                cmd.CommandText = "insert into tbltranslationprojectproperty (fkprojectid,propertyname,propertyvalue) values (@projid,@pname,@pvalue)";
                cmd.Parameters.Add("@projid", SqlDbType.Int).Value = project.LocalID;
                cmd.Parameters.Add("@pname", SqlDbType.NVarChar,50).Value = key;
                cmd.Parameters.Add("@pvalue", SqlDbType.NVarChar,50).Value = project.Properties[key];
                cmd.ExecuteNonQuery();
            }
            Connection.Close();
        }        

        public List<TranslationPage> GetProjectPages(TranslationProject project)
        {
            OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select tblTranslationProjectItem.* from tblTranslationProjectItem inner join tblTranslationProject on tbltranslationProject.workid=tbltranslationProjectItem.workid where tbltranslationproject.pkid=@projectid and tbltranslationprojectitem.itemtype=@itemtype and tbltranslationprojectitem.status=@status";
            cmd.Parameters.Add("@projectid", SqlDbType.Int).Value = project.LocalID;
            cmd.Parameters.Add("@itemtype", SqlDbType.Int).Value = (int)ItemType.Page;
            cmd.Parameters.Add("@status",SqlDbType.Int).Value=(int)(ProjectItemStatus.Send);
            cmd.Connection = Connection;

            List<TranslationPage> pages = new List<TranslationPage>();

            using (SqlDataReader sdr = cmd.ExecuteReader())
            {

                while (sdr.Read())
                {
                    PageReference prCurrent = PageReference.Parse(sdr["page_link"] as string);
                    PageData pdCurrent = DataFactory.Instance.GetPage(prCurrent);

                    TranslationPage tp = new TranslationPage();
                    tp.ItemID = (int)sdr["pkid"];
                    tp.OriginalID = prCurrent.ToString();
                    tp.Properties = new System.Collections.Hashtable();
                    tp.Status = (int)sdr["status"];
                    tp.ItemLoader = new TranslationPageLoader();
                    tp.Indent = (int)sdr["indent"];
                    tp.ParentNode = (string)sdr["parent_link"];
                    foreach (string key in pdCurrent.Property.Keys)
                    {
                        PropertyData prop = pdCurrent.Property[key];
                        if (((prop as PropertyLongString) != null) || ((prop as PropertyString) != null))
                        {
                            tp.Properties.Add(key, prop.Value);
                        }
                    }
                    pages.Add(tp);
                }
                sdr.Close();
            }
            Connection.Close();

            return pages;
        }

        public List<TranslationFile> GetProjectFiles(TranslationProject project)
        {
            OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select tblTranslationProjectItem.* from tblTranslationProjectItem inner join tblTranslationProject on tbltranslationProject.workid=tbltranslationProjectItem.workid where tbltranslationproject.pkid=@projectid and tbltranslationprojectitem.itemtype=@itemtype and tbltranslationprojectitem.status=@status";
            cmd.Parameters.Add("@projectid", SqlDbType.Int).Value = project.LocalID;
            cmd.Parameters.Add("@itemtype", SqlDbType.Int).Value = (int)ItemType.File;
            cmd.Parameters.Add("@status", SqlDbType.Int).Value = (int)(ProjectItemStatus.Send);
            cmd.Connection = Connection;

            List<TranslationFile> files = new List<TranslationFile>();

            using (SqlDataReader sdr = cmd.ExecuteReader())
            {

                while (sdr.Read())
                {


                    TranslationFile file = new TranslationFile();
                    file.PageLink = sdr["page_link"] as string;
                    file.FilePath = sdr["page_name"] as string;
                    file.ItemID = (int)sdr["pkid"];
                    UnifiedFile currentFile = System.Web.Hosting.HostingEnvironment.VirtualPathProvider.GetFile(file.FilePath) as UnifiedFile;

                    file.Properties = new System.Collections.Hashtable();
                    file.Status = (int)sdr["status"];
                    file.ItemLoader = new TranslationFileLoader();
                    file.StreamLoader = new TranslationFile.StreamHandler(GetFileStream);
                    file.Indent = (int)sdr["indent"];
                    //foreach (string key in pdCurrent.Property.Keys)
                    //{
                    //    PropertyData prop = pdCurrent.Property[key];
                    //    if (((prop as PropertyLongString) != null) || ((prop as PropertyString) != null))
                    //    {
                    //        file.Properties.Add(key, prop.Value);
                    //    }
                    //}
                    files.Add(file);
                }
                sdr.Close();
            }
            Connection.Close();

            return files;
        }

        public Stream GetFileStream(string filepath)
        {
            UnifiedFile uf = System.Web.Hosting.HostingEnvironment.VirtualPathProvider.GetFile(filepath) as UnifiedFile;
            if (uf != null)
            {
                return uf.Open(FileMode.Open,FileAccess.Read);
            }
            return null;
        }

        public string GetRemoteID(int itemID, string lang)
        {
            OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.Connection = Connection;
            cmd.CommandText = @"select remoteId from tblTranslationProjectItemData where fkTranslationProjectItemId=@fkTranslationProjectItemId and fkTranslationProjectLanguage=@fkTranslationProjectLanguage";
            cmd.Parameters.Add("@fkTranslationProjectItemId", SqlDbType.Int).Value = itemID;
            cmd.Parameters.Add("@fkTranslationProjectLanguage", SqlDbType.NVarChar,50).Value = lang;
            string remoteid = cmd.ExecuteScalar() as string;
            if (remoteid == null)
                remoteid = "0";
            Connection.Close();
            return remoteid;
        }
        public TranslationStatus GetItemLanguageStatus(int itemID, string language)
        {
            OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.Connection = Connection;
            cmd.CommandText = @"select status from tblTranslationProjectItemData where fkTranslationProjectItemId=@fkTranslationProjectItemId and fkTranslationProjectLanguage=@fkTranslationProjectLanguage";
            cmd.Parameters.Add("@fkTranslationProjectItemId", SqlDbType.Int).Value = itemID;
            cmd.Parameters.Add("@fkTranslationProjectLanguage", SqlDbType.NVarChar, 50).Value = language;
            int status = (int)cmd.ExecuteScalar();
            
            Connection.Close();
            return (TranslationStatus)status;
        }

        public string GetItemLanguageRemoteStatus(int itemID, string language)
        {
            OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.Connection = Connection;
            cmd.CommandText = @"select remotestatus from tblTranslationProjectItemData where fkTranslationProjectItemId=@fkTranslationProjectItemId and fkTranslationProjectLanguage=@fkTranslationProjectLanguage";
            cmd.Parameters.Add("@fkTranslationProjectItemId", SqlDbType.Int).Value = itemID;
            cmd.Parameters.Add("@fkTranslationProjectLanguage", SqlDbType.NVarChar, 50).Value = language;
            string status = cmd.ExecuteScalar() as string;

            Connection.Close();
            return status;
        }
        public byte[] GetItemLanguageData(int itemID, string language)
        {
            OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.Connection = Connection;
            cmd.CommandText = @"select data from tblTranslationProjectItemData where fkTranslationProjectItemId=@fkTranslationProjectItemId and fkTranslationProjectLanguage=@fkTranslationProjectLanguage";
            cmd.Parameters.Add("@fkTranslationProjectItemId", SqlDbType.Int).Value = itemID;
            cmd.Parameters.Add("@fkTranslationProjectLanguage", SqlDbType.NVarChar, 50).Value = language;
            byte[] ret = null;

            using (SqlDataReader sdr = cmd.ExecuteReader())
            {

                if (sdr.Read())
                {
                    if (sdr.HasRows)
                    {
                        if (!sdr.IsDBNull(0))
                        {
                            ret = new byte[Convert.ToInt32(sdr.GetBytes(0, 0, null, 0, Int32.MaxValue))];
                            sdr.GetBytes(0, 0, ret, 0, ret.Length);
                        }
                    }
                }
                sdr.Close();
            }
            Connection.Close();
            return ret;
        }
        public void SetItemLanguageData(int itemID, string lang, byte[] data)
        {
            OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.Connection = Connection;
            cmd.CommandText = @"select pkid from tblTranslationProjectItemData where fkTranslationProjectItemId=@fkTranslationProjectItemId and fkTranslationProjectLanguage=@fkTranslationProjectLanguage";
            cmd.Parameters.Add("@fkTranslationProjectItemId", SqlDbType.Int).Value = itemID;
            cmd.Parameters.Add("@fkTranslationProjectLanguage", SqlDbType.NVarChar, 50).Value = lang;
            object rowid = cmd.ExecuteScalar();
            if (rowid == null)
            {
                cmd = new SqlCommand();
                cmd.CommandType = CommandType.Text;
                cmd.Connection = Connection;
                cmd.CommandText = @"insert into tblTranslationProjectItemData (fkTranslationProjectItemId,data,fkTranslationProjectLanguage) values (@fkTranslationProjectItemId,@data,@fkTranslationProjectLanguage)";
                cmd.Parameters.Add("@fkTranslationProjectItemId", SqlDbType.Int).Value = itemID;
                cmd.Parameters.Add("@data",SqlDbType.Image,data.Length).Value = data;
                cmd.Parameters.Add("@fkTranslationProjectLanguage", SqlDbType.NVarChar, 50).Value = lang;
                cmd.ExecuteNonQuery();
            }
            else
            {
                cmd = new SqlCommand();
                cmd.CommandType = CommandType.Text;
                cmd.Connection = Connection;
                cmd.CommandText = @"update tblTranslationProjectItemData set data=@data where fkTranslationProjectItemId=@fkTranslationProjectItemId and fkTranslationProjectLanguage=@fkTranslationProjectLanguage";
                cmd.Parameters.Add("@data", SqlDbType.Image, data.Length).Value = data;
                cmd.Parameters.Add("@fkTranslationProjectItemId", SqlDbType.Int).Value = itemID;
                cmd.Parameters.Add("@fkTranslationProjectLanguage", SqlDbType.NVarChar, 50).Value = lang;
                cmd.ExecuteNonQuery();
            }

            Connection.Close();

        }
        public void SetRemoteID(int itemID,string lang,string id)
        {
            OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.Connection = Connection;
            cmd.CommandText = @"select pkid from tblTranslationProjectItemData where fkTranslationProjectItemId=@fkTranslationProjectItemId and fkTranslationProjectLanguage=@fkTranslationProjectLanguage";
            cmd.Parameters.Add("@fkTranslationProjectItemId", SqlDbType.Int).Value = itemID;
            cmd.Parameters.Add("@fkTranslationProjectLanguage", SqlDbType.NVarChar, 50).Value = lang;
            object rowid = cmd.ExecuteScalar() ;
            if (rowid == null)
            {
                cmd = new SqlCommand();
                cmd.CommandType = CommandType.Text;
                cmd.Connection = Connection;
                cmd.CommandText = @"insert into tblTranslationProjectItemData (fkTranslationProjectItemId,remoteId,fkTranslationProjectLanguage) values (@fkTranslationProjectItemId,@remoteID,@fkTranslationProjectLanguage)";                
                cmd.Parameters.Add("@fkTranslationProjectItemId", SqlDbType.Int).Value = itemID;
                cmd.Parameters.Add("@remoteID", SqlDbType.NVarChar, 50).Value = id;
                cmd.Parameters.Add("@fkTranslationProjectLanguage", SqlDbType.NVarChar, 50).Value = lang;
                cmd.ExecuteNonQuery();
            }
            else
            {
                cmd = new SqlCommand();
                cmd.CommandType = CommandType.Text;
                cmd.Connection = Connection;
                cmd.CommandText = @"update tblTranslationProjectItemData set remoteId=@remoteID where fkTranslationProjectItemId=@fkTranslationProjectItemId and fkTranslationProjectLanguage=@fkTranslationProjectLanguage";
                cmd.Parameters.Add("@remoteId", SqlDbType.NVarChar, 50).Value = id;
                cmd.Parameters.Add("@fkTranslationProjectItemId", SqlDbType.Int).Value = itemID;
                cmd.Parameters.Add("@fkTranslationProjectLanguage", SqlDbType.NVarChar, 50).Value = lang;
                cmd.ExecuteNonQuery();
            }            

            Connection.Close();
        }
        public void SetItemLanguageStatus(int itemID, string lang, TranslationStatus status)
        {
            OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.Connection = Connection;
            cmd.CommandText = @"select pkid from tblTranslationProjectItemData where fkTranslationProjectItemId=@fkTranslationProjectItemId and fkTranslationProjectLanguage=@fkTranslationProjectLanguage";
            cmd.Parameters.Add("@fkTranslationProjectItemId", SqlDbType.Int).Value = itemID;
            cmd.Parameters.Add("@fkTranslationProjectLanguage", SqlDbType.NVarChar, 50).Value = lang;
            object rowid = cmd.ExecuteScalar();
            if (rowid == null)
            {
                cmd = new SqlCommand();
                cmd.CommandType = CommandType.Text;
                cmd.Connection = Connection;
                cmd.CommandText = @"insert into tblTranslationProjectItemData (fkTranslationProjectItemId,status,fkTranslationProjectLanguage) values (@fkTranslationProjectItemId,@status,@fkTranslationProjectLanguage)";
                cmd.Parameters.Add("@fkTranslationProjectItemId", SqlDbType.Int).Value = itemID;
                cmd.Parameters.Add("@status",SqlDbType.Int).Value = status;
                cmd.Parameters.Add("@fkTranslationProjectLanguage", SqlDbType.NVarChar, 50).Value = lang;
                cmd.ExecuteNonQuery();
            }
            else
            {
                cmd = new SqlCommand();
                cmd.CommandType = CommandType.Text;
                cmd.Connection = Connection;
                cmd.CommandText = @"update tblTranslationProjectItemData set status=@status where fkTranslationProjectItemId=@fkTranslationProjectItemId and fkTranslationProjectLanguage=@fkTranslationProjectLanguage";
                cmd.Parameters.Add("@status", SqlDbType.Int).Value = status;
                cmd.Parameters.Add("@fkTranslationProjectItemId", SqlDbType.Int).Value = itemID;
                cmd.Parameters.Add("@fkTranslationProjectLanguage", SqlDbType.NVarChar, 50).Value = lang;
                cmd.ExecuteNonQuery();
            }

            Connection.Close();
        }
        public void SetItemLanguageRemoteStatus(int itemID, string lang, string status)
        {
            OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.Connection = Connection;
            cmd.CommandText = @"select pkid from tblTranslationProjectItemData where fkTranslationProjectItemId=@fkTranslationProjectItemId and fkTranslationProjectLanguage=@fkTranslationProjectLanguage";
            cmd.Parameters.Add("@fkTranslationProjectItemId", SqlDbType.Int).Value = itemID;
            cmd.Parameters.Add("@fkTranslationProjectLanguage", SqlDbType.NVarChar, 50).Value = lang;
            object rowid = cmd.ExecuteScalar();
            if (rowid == null)
            {
                cmd = new SqlCommand();
                cmd.CommandType = CommandType.Text;
                cmd.Connection = Connection;
                cmd.CommandText = @"insert into tblTranslationProjectItemData (fkTranslationProjectItemId,remotestatus,fkTranslationProjectLanguage) values (@fkTranslationProjectItemId,@remotestatus,@fkTranslationProjectLanguage)";
                cmd.Parameters.Add("@fkTranslationProjectItemId", SqlDbType.Int).Value = itemID;
                cmd.Parameters.Add("@remotestatus", SqlDbType.NChar,10).Value = status;
                cmd.Parameters.Add("@fkTranslationProjectLanguage", SqlDbType.NVarChar, 50).Value = lang;
                cmd.ExecuteNonQuery();
            }
            else
            {
                cmd = new SqlCommand();
                cmd.CommandType = CommandType.Text;
                cmd.Connection = Connection;
                cmd.CommandText = @"update tblTranslationProjectItemData set remotestatus=@remotestatus where fkTranslationProjectItemId=@fkTranslationProjectItemId and fkTranslationProjectLanguage=@fkTranslationProjectLanguage";
                cmd.Parameters.Add("@remotestatus", SqlDbType.NVarChar, 10).Value = status;
                cmd.Parameters.Add("@fkTranslationProjectItemId", SqlDbType.Int).Value = itemID;
                cmd.Parameters.Add("@fkTranslationProjectLanguage", SqlDbType.NVarChar, 50).Value = lang;
                cmd.ExecuteNonQuery();
            }

            Connection.Close();
        }


        public void SavePage(TranslationPage page)
        {
            OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.Connection = Connection;
            cmd.CommandText = @"update tbltranslationprojectitem
                                set 
                                status=@status, 
                                remoteid=@remoteid, 
                                remotestatus = @remotestatus                                
                                where pkid=@projectitemid";
            cmd.Parameters.Add("@status", SqlDbType.Int).Value = page.Status;
            cmd.Parameters.Add("@remoteid", SqlDbType.NVarChar, 50).Value = page.RemoteID??"0";            
            cmd.Parameters.Add("@remotestatus", SqlDbType.Int).Value = page.RemoteStatus;
            cmd.Parameters.Add("@projectitemid", SqlDbType.Int).Value = page.ItemID;
            cmd.ExecuteNonQuery();

            Connection.Close();            
        }

        public void SaveFile(TranslationFile file)
        {
            OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.Connection = Connection;
            cmd.CommandText = @"update tbltranslationprojectitem
                                set 
                                status=@status, 
                                remoteid=@remoteid, 
                                remotestatus = @remotestatus                                
                                where pkid=@projectitemid";
            cmd.Parameters.Add("@status", SqlDbType.Int).Value = file.Status;
            cmd.Parameters.Add("@remoteid", SqlDbType.NVarChar, 50).Value = file.RemoteID ?? "0";
            cmd.Parameters.Add("@remotestatus", SqlDbType.Int).Value = file.RemoteStatus;
            cmd.Parameters.Add("@projectitemid", SqlDbType.Int).Value = file.ItemID;
            cmd.ExecuteNonQuery();

            Connection.Close();
        }

        public List<TranslationPage> PageLoad(TranslationProject project)
        {
            return GetProjectPages(project);            
        }

        public List<TranslationFile> FileLoad(TranslationProject project)
        {
            return GetProjectFiles(project);
        }


        internal void SetPageStatus(string sessionID, string pageID, int status)
        {
            OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "update tblTranslationProjectItem set status = @status where workid=@workid and page_link=@page_link and itemtype=@itemtype";
            cmd.Parameters.Add("@status", SqlDbType.Int).Value = status;
            cmd.Parameters.Add("@workid", SqlDbType.UniqueIdentifier).Value = new Guid(sessionID);
            cmd.Parameters.Add("@page_link", SqlDbType.NVarChar, 50).Value = pageID;
            cmd.Parameters.Add("@itemtype", SqlDbType.Int).Value = (int)ItemType.Page;
            cmd.Connection = Connection;

            cmd.ExecuteNonQuery();
            Connection.Close();
        }

        internal void SetFileStatus(string sessionID, string filepath, ProjectItemStatus status)
        {
            OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "update tblTranslationProjectItem set status = @status where workid=@workid and page_name=@filepath and itemtype=@itemtype";
            cmd.Parameters.Add("@status", SqlDbType.Int).Value = (int)status;
            cmd.Parameters.Add("@workid", SqlDbType.UniqueIdentifier).Value = new Guid(sessionID);
            cmd.Parameters.Add("@filepath", SqlDbType.NVarChar, 50).Value = filepath;
            cmd.Parameters.Add("@itemtype", SqlDbType.Int).Value = (int)ItemType.File;
            cmd.Connection = Connection;

            cmd.ExecuteNonQuery();
            Connection.Close();
        }

        internal ProjectItemStatus GetFileStatus(string sessionID, string filepath)
        {
            OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select status from tblTranslationProjectItem where workid=@workid and page_name=@filepath and itemtype=@itemtype";            
            cmd.Parameters.Add("@workid", SqlDbType.UniqueIdentifier).Value = new Guid(sessionID);
            cmd.Parameters.Add("@filepath", SqlDbType.NVarChar, 50).Value = filepath;
            cmd.Parameters.Add("@itemtype", SqlDbType.Int).Value = (int)ItemType.File;
            cmd.Connection = Connection;

            ProjectItemStatus ret = (ProjectItemStatus)((int)cmd.ExecuteScalar());
            Connection.Close();
            return ret;
        }


        internal void InsertItemIntoDatabase(PageReference pageLink, PageReference parentLink, int indent, string pageName, string sessionID,ProjectItemStatus status,ItemType itemtype)
        {
            OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "insert into tblTranslationProjectItem (indent,page_link,parent_link,page_name,workid,status,itemtype) values (@indent,@page_link,@parent_link,@page_name,@workid,@status,@itemtype)";
            cmd.Parameters.Add("@indent", SqlDbType.Int).Value = indent;
            cmd.Parameters.Add("@page_link", SqlDbType.NVarChar, 50).Value = pageLink.ToString();
            cmd.Parameters.Add("@parent_link", SqlDbType.NVarChar, 50).Value = parentLink.ToString();
            cmd.Parameters.Add("@page_name", SqlDbType.NVarChar, 255).Value = pageName;
            cmd.Parameters.Add("@workid", SqlDbType.UniqueIdentifier).Value = new Guid(sessionID);
            cmd.Parameters.Add("@status", SqlDbType.Int).Value = (int)(status);
            cmd.Parameters.Add("@itemtype", SqlDbType.Int).Value = (int)itemtype;
            cmd.Connection = Connection;
            cmd.ExecuteNonQuery();
            Connection.Close();
        }

        internal int GetPagesCount(string sessionID)
        {
            OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = Connection;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select count(*) from tbltranslationprojectitem where workid=@sessionid and itemtype=0 and status<>1";
            cmd.Parameters.Add("@sessionid", SqlDbType.UniqueIdentifier).Value = new Guid(sessionID);
            int ret = (int)cmd.ExecuteScalar();
            Connection.Close();
            return ret;
        }

        internal int GetFilesCount(string sessionID)
        {
            OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = Connection;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select count(*) from tbltranslationprojectitem where workid=@sessionid and itemtype=1 and status<>1";
            cmd.Parameters.Add("@sessionid", SqlDbType.UniqueIdentifier).Value = new Guid(sessionID);
            int ret = (int)cmd.ExecuteScalar();
            Connection.Close();
            return ret;
        }
    }
}
