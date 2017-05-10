using System;
using EPiServer.Research.Translation4.Common;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Text;
using System.Net.Mail;
using System.IO.Packaging;

namespace EPiServer.Research.Connector.Language.XLIFF
{
    public class Connector: IConnector
    {
        private readonly string _tempDir = ApplicationSettings.Instance.Xliffworkpath;  //ConfigurationManager.AppSettings["xliffworkpath"];
       
        public string GetConnectorName()
        {
            return "XLiff connector";
        }

        public int GetHandlerSupportedVersionNumber()
        {
            return 1;
        }

        public string GetValidFilename(string originalFilename)
        {
            string ret;

            ret = originalFilename;

            foreach (char c in Path.GetInvalidFileNameChars())
                ret = ret.Replace(c, '_');

            foreach (char c in Path.GetInvalidPathChars())
                ret = ret.Replace(c, '_');

            return ret;
        }
        public string SendProject(TranslationProject project)
        {
            if (project.RemoteID == "0")
            { 
                //Give a temp id to project
                project.RemoteID = Guid.NewGuid().ToString();
                project.RemoteStatus = (int)TranslationStatus.Created;
                project.Modified = true;
                project.Save();
            }
            // Get / Create Folder Path for temp storage
            string workpath = Path.Combine(_tempDir, project.RemoteID);
            string zippath = Path.Combine(_tempDir, GetValidFilename(project.Name) + ".translation.zip");
            if (!Directory.Exists(workpath))
            {
                Directory.CreateDirectory(workpath);
            }

            foreach (TranslationPage tp in project.Pages)
            {
                string tempfilename = tp.OriginalID;
                string tempfilepath = Path.Combine(workpath, tempfilename+".xlf");

                bool isCompleted = true;
                foreach (string lang in project.TargetLanguages)
                {
                    if (tp.GetStatus(lang) != TranslationStatus.Sent)
                    {
                        isCompleted = false;
                        break;
                    }
                }

                if (!isCompleted)
                {
                    Stream f = new FileStream(tempfilepath, FileMode.Create, FileAccess.Write);
                    XmlTextWriter xwriter = new XmlTextWriter(f, Encoding.UTF8);
                    xwriter.WriteStartDocument(false);
                    xwriter.WriteDocType("xliff", "-//XLIFF//DTD XLIFF//EN", "http://www.oasis-open.org/committees/xliff/documents/xliff.dtd", null);
                    
                    xwriter.WriteStartElement("xliff");
                    xwriter.WriteAttributeString("version", "1.0");

                    foreach (string lang in project.TargetLanguages)
                    {
                        xwriter.WriteStartElement("file");
                        xwriter.WriteAttributeString("original", tp.ItemID.ToString());                        
                        xwriter.WriteAttributeString("source-language", project.SourceLanguage);
                        xwriter.WriteAttributeString("target-language", lang);
                        xwriter.WriteAttributeString("datatype", "plaintext");

                        xwriter.WriteStartElement("header");
                        xwriter.WriteEndElement(); //header

                        xwriter.WriteStartElement("body");
                        

                        foreach (string key in tp.Properties.Keys)
                        {
                            xwriter.WriteStartElement("trans-unit");

                            xwriter.WriteAttributeString("resname", key);
                            //xwriter.WriteAttributeString("extype", "String");
                            xwriter.WriteAttributeString("restype", "String");
                            xwriter.WriteAttributeString("datatype", "text|html");
                            //xwriter.WriteAttributeString("url", tp.Properties["LinkURL"] as string);                            
                            xwriter.WriteAttributeString("id", key);

                            xwriter.WriteStartElement("source");
                            xwriter.WriteString(tp.Properties[key] as string);
                            xwriter.WriteEndElement(); //source

                            xwriter.WriteStartElement("target");
                            xwriter.WriteCData(tp.Properties[key] as string);
                            xwriter.WriteEndElement(); //target

                            xwriter.WriteEndElement(); //trans-unit
                        }
                        xwriter.WriteEndElement(); //body
                        xwriter.WriteEndElement();// file
                    }
                    xwriter.WriteEndElement();// xliff
                    xwriter.WriteEndDocument();
                    xwriter.Flush();
                    f.Close();
                    foreach (string lang in project.TargetLanguages)
                    {
                        tp.SetRemoteID(lang, tempfilename+".xlf");
                        tp.SetStatus(lang, TranslationStatus.Sent);
                    }
                }

            }
            foreach (TranslationFile tf in project.Files)
            {
                foreach (string lang in project.TargetLanguages)
                {
                    if (tf.GetStatus(lang) != TranslationStatus.Sent)
                    {
                        Stream strm = tf.FileStream;
                        byte[] data = new byte[strm.Length];

                        strm.Read(data, 0, data.Length);

                        strm.Close();
                        string tempfilename = Guid.NewGuid().ToString() + "_" + lang.Replace("-","_") + "." + Path.GetExtension(tf.FilePath);
                        
                        string tempfilepath = Path.Combine(workpath, tempfilename );

                        Stream f = new FileStream(tempfilepath, FileMode.Create, FileAccess.Write);
                        f.Write(data, 0, data.Length);
                        f.Close();
                        tf.SetRemoteID(lang, tempfilename);
                        tf.SetStatus(lang, TranslationStatus.Sent);
                    }
                }
            } 
            string[] filenames = Directory.GetFiles(workpath);
            foreach (string filename in filenames)
            {
                AddFileToZip(zippath, filename);
            }

            MailMessage message = new MailMessage(new MailAddress("translation@site.com"), new MailAddress(project.Properties["xliffemail"]));
            message.Attachments.Add(new Attachment(zippath));
            message.Subject = "Translation package is here";
            SmtpClient mailclient = new SmtpClient();
            mailclient.Send(message);
            
            project.Status = TranslationStatus.ReadyForImport;
            project.RemoteStatus = (int)TranslationStatus.ReadyForImport;
            project.Modified = true;

            return String.Empty;
        }

        public string RetrieveProject(TranslationProject project)
        {
            // Get / Create Folder Path for temp storage
            string workpath = Path.Combine(_tempDir, "incoming\\" + project.RemoteID);
            string zippath = Path.Combine(_tempDir, "incoming\\" + project.RemoteID + ".zip");

            if (File.Exists(zippath))
            {
                if (!Directory.Exists(workpath))
                {
                    Directory.CreateDirectory(workpath);
                    unzipfile(zippath, workpath);
                }

                foreach (TranslationPage tp in project.Pages)
                {
                    foreach (string lang in project.TargetLanguages)
                    {
                        string rId = tp.GetRemoteID(lang);
                        if (rId != "0")
                        {          
                            string workfile =  Path.Combine(workpath,rId);
                            if (File.Exists(workfile))
                            {
                                XmlDocument xdoc = new XmlDocument();

                                XmlReaderSettings settings = new XmlReaderSettings
                                    {
                                        XmlResolver = null,
                                        DtdProcessing = DtdProcessing.Prohibit
                                    };
                                XmlReader x = XmlReader.Create(new FileStream(workfile,FileMode.Open),settings);
                                xdoc.Load(x);                                

                                XmlNode node = xdoc.SelectSingleNode("/xliff/file[@target-language=\'"+lang+"\']");

                                byte[] data = Encoding.Default.GetBytes( node.OuterXml) ;
                                
                                tp.SetData(lang, data);
                                tp.SetStatus(lang, TranslationStatus.Received);
                                tp.SetRemoteStatus(lang, TranslationStatus.Received.ToString());
                                x.Close();
                            }
                        } 
                    }
                }
                foreach (TranslationFile tf in project.Files)
                {
                    foreach (string lang in project.TargetLanguages)
                    {
                        string rId = tf.GetRemoteID(lang);
                        if (tf.GetStatus(lang)== TranslationStatus.Sent)
                        {
                            string workfile = Path.Combine(workpath, rId);
                            
                            FileStream f = new FileStream(workfile,FileMode.Open);
                            byte[] data = new byte[f.Length];
                            f.Read(data, 0, (int)f.Length);
                            f.Close();                            
                            tf.SetData(lang, data);
                            tf.SetStatus(lang, TranslationStatus.Received);
                            tf.SetRemoteStatus(lang, TranslationStatus.Received.ToString());
                
                        }
                    }
                }
                project.Status = TranslationStatus.ReadyForImport;
                project.Modified = true;
            }

            return "";
        }

 

        public void UpdateProject(TranslationProject project)
        {
            //throw new NotImplementedException();
        }

        public TranslationPage GetPage(byte[] data)
        {
            TranslationPage ret = new TranslationPage();

            ret.Properties = new System.Collections.Hashtable();
            XmlTextReader reader = new XmlTextReader(new StreamReader(new MemoryStream(data),Encoding.Default));
            
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == "trans-unit")
                        {
                            if (reader.MoveToAttribute("resname"))
                            {
                                string propertyname = reader.Value;
                                string propertyvalue = "";

                                while (reader.Read())
                                {
                                    if (reader.Name == "target")
                                    {
                                        reader.Read();
                                        break;
                                    }
                                }

                                if ( (reader.NodeType == XmlNodeType.CDATA) || (reader.NodeType==XmlNodeType.Text) )
                                    propertyvalue = reader.Value;
                                ret.Properties.Add(propertyname, propertyvalue);
                            }
                        }
                        break;
                }
            }
            reader.Close();
            return ret;
        }

        public void SetProjectProperty(TranslationProject project)
        {
            project.Status = TranslationStatus.ReadyForSend;
        }
        
        void AddFileToZip(string zipFilename, string fileToAdd)
        {
            using (Package zip = Package.Open(zipFilename, FileMode.OpenOrCreate))
            {
                string destFilename = ".\\" + Path.GetFileName(fileToAdd);
                Uri uri = PackUriHelper.CreatePartUri(new Uri(destFilename, UriKind.Relative));
                if (zip.PartExists(uri))
                {
                    zip.DeletePart(uri);
                }
                PackagePart part = zip.CreatePart(uri, "", CompressionOption.Normal);
                using (FileStream fileStream = new FileStream(fileToAdd, FileMode.Open, FileAccess.Read))
                {
                    using (Stream dest = part.GetStream())
                    {
                        CopyStream(fileStream, dest);
                    }
                }
            }
        }
        const int BUFFER_SIZE = 4096;
        private void CopyStream(Stream inputStream, Stream outputStream)
        {
            long bufferSize = inputStream.Length < BUFFER_SIZE ? inputStream.Length : BUFFER_SIZE;
            byte[] buffer = new byte[bufferSize];
            int bytesRead = 0;
            long bytesWritten = 0;
            while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                outputStream.Write(buffer, 0, bytesRead);
                bytesWritten += bufferSize;
            }
        }
        private void unzipfile(string zippath, string workpath)
        {
            using (Package zip = Package.Open(zippath, FileMode.Open))
            {
                foreach (PackagePart pp in zip.GetParts())
                {
                    string toPath = Path.Combine(workpath, pp.Uri.ToString().Replace("/",""));
                    using (Stream fileStream = new FileStream(toPath, FileMode.CreateNew, FileAccess.Write))
                    {
                        using (Stream sourceStream = pp.GetStream())
                        {
                            CopyStream(sourceStream, fileStream);
                        }
                    }
                }
            }
        }
    }
}
