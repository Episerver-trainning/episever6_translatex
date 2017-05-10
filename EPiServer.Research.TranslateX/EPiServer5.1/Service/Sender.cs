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

        private static bool isRunning = false;

        private static object lockObject = new object();
        public static string Execute()
        {
            Thread.CurrentThread.Priority = ThreadPriority.Lowest;
            if (isRunning)
                return "Service is already running";

            string ret = "OK";
            lock (lockObject)
            {
                isRunning = true;

                // big try and catch to make sure release the running state.
                try
                {
                    DataSet ds = Manager.Current.GetProjects();
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        TranslationProject project = Manager.Current.GetTranslationProject((int)dr["pkid"]);
                        try
                        {
                            

                            IConnector connector = Manager.Current.GetConntectorByName(project.ConnectorName);
                            if ((project.Status == Translation4.Common.TranslationStatus.ReadyForSend) || (project.Status == Translation4.Common.TranslationStatus.Sending))
                            {
                                string RemoteID = connector.SendProject(project);
                                if (project.Modified)
                                    project.Save();
                            }
                            else
                            {
                                if ((project.Status == TranslationStatus.ReadyForRecieve) || (project.Status == TranslationStatus.Receiving))
                                {
                                    //project.Status = (int)TranslationStatus.Receiving;
                                    //project.Save();

                                    string RemoteID = connector.RetrieveProject(project);
                                    if (project.Modified)
                                        project.Save();
                                }
                                else
                                {
                                    if ((project.Status == TranslationStatus.Received) || (project.Status == TranslationStatus.Sent) || (project.Status == TranslationStatus.Created))
                                    {
                                        connector.UpdateProject(project);
                                        if (project.Modified)
                                            project.Save();
                                    }
                                }
                            }
                        }
                        catch (Exception expone)
                        {
                            ret += "Error on project:" + project.Name + " with error:" + expone.Message + "\n";
                        }
                    }
                }
                catch (Exception exp)
                {
                    logger.Error(exp);
                    ret = exp.Message + "\r\n" + exp.StackTrace;
                }

                isRunning = false;
            }
            return ret;
        }
    }   
}
