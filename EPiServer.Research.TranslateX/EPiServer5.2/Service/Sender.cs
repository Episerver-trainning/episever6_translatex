using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using EPiServer.PlugIn;
using EPiServer.Research.Translation4.Core;
using EPiServer.Research.Translation4.Common;
using System.Collections.Generic;
using System.Threading;

namespace EPiServer.Research.Translation4.Service
{
    [ScheduledPlugIn(DisplayName = "Translation scheduler service", Description = "This service updates translation task")]
    public class Sender
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(Sender));

        public static string Execute()
        {
            bool created;
            Mutex mx = new Mutex(true, EPiServer.Configuration.Settings.Instance.SiteUrl.ToString(), out created);

            logger.Debug("Service started at machine: " + Environment.MachineName);
            if (!created)
            {
                return "Service is already running";
            }
            
            Thread.CurrentThread.Priority = ThreadPriority.Lowest;

            string ret = "OK";
            try
            {
                DataSet ds = Manager.Current.GetProjects();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    TranslationProject project = Manager.Current.GetTranslationProject((int)dr["pkid"]);
                    logger.Debug("Running on project " + project.Name + " ( "  + project.LocalID + " )");
                    try
                    {
                        IConnector connector = Manager.Current.GetConntectorByName(project.ConnectorName);
                        if ((project.Status == TranslationStatus.Received) || (project.Status == TranslationStatus.Sent) || (project.Status == TranslationStatus.Created))
                        {
                            connector.UpdateProject(project);
                            if (project.Modified)
                                project.Save();
                        }

                        project = Manager.Current.GetTranslationProject((int)dr["pkid"]);
                        if ((project.Status == Translation4.Common.TranslationStatus.ReadyForSend) || (project.Status == Translation4.Common.TranslationStatus.Sending))
                        {
                            string RemoteID = connector.SendProject(project);
                            if (project.Modified)
                                project.Save();
                        }
                        project = Manager.Current.GetTranslationProject((int)dr["pkid"]);
                        if ((project.Status == TranslationStatus.ReadyForRecieve) || (project.Status == TranslationStatus.Receiving))
                        {
                            string RemoteID = connector.RetrieveProject(project);
                            if (project.Modified)
                                project.Save();
                        }                                                          
                    }
                    catch (Exception expone)
                    {
                        if (ret == "OK")
                            ret = "";
                        ret += "<br/>\r\nError on project:" + project.Name + " with error:" + expone.Message + "\n";

                        logger.Error("Error on project:" + project.Name + " with error:" + expone.Message);
                    }
                }
            }
            catch (Exception exp)
            {
                logger.Error(exp);
                ret = exp.Message + "<br/>\r\n" + exp.StackTrace;
            }

            mx.Close();

            return ret;
        }
    }   
}
