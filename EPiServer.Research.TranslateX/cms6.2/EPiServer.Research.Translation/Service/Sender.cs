using System;
using System.Data;
using EPiServer.PlugIn;
using EPiServer.Research.Translation.Core;
using EPiServer.Research.Translation4.Common;
using System.Threading;
using log4net;

namespace EPiServer.Research.Translation.Service
{
    [ScheduledPlugIn(DisplayName = "Translation scheduler service", Description = "This service updates translation task")]
    public class Sender
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Sender));

        public static string Execute()
        {
            bool created;
            Mutex mx = new Mutex(true, Configuration.Settings.Instance.SiteUrl.ToString(), out created);

            _log.Debug("Service started at machine: " + Environment.MachineName);
            if (!created)
            {
                return "Service is already running";
            }
            
            Thread.CurrentThread.Priority = ThreadPriority.Lowest;

            string ret = "OK";
            try
            {
                DataSet ds = Manager.Current.GetProjects();
                if (ds == null)
                {
                    _log.Debug("Didn't get any translation projects");
                    return "Failed getting translation projects";
                }

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    TranslationProject project = Manager.Current.GetTranslationProject((int)dr["pkid"]);
                    _log.Debug("Running on project " + project.Name + " ( "  + project.LocalID + " )");
                    try
                    {
                        IConnector connector = Manager.Current.GetConntectorByName(project.ConnectorName);
                        if (connector == null)
                        {
                            _log.Debug("connector is null");
                            return "Connector is null";
                        }
                        if ((project.Status == TranslationStatus.Received) || (project.Status == TranslationStatus.Sent) || (project.Status == TranslationStatus.Created))
                        {
                            connector.UpdateProject(project);
                            if (project.Modified)
                            {
                                project.Save();
                            }
                        }

                        project = Manager.Current.GetTranslationProject((int)dr["pkid"]);
                        if ((project.Status == TranslationStatus.ReadyForSend) || (project.Status == TranslationStatus.Sending))
                        {
                            string RemoteID = connector.SendProject(project);
                            if (project.Modified)
                            {
                                project.Save();
                            }
                        }
                        project = Manager.Current.GetTranslationProject((int)dr["pkid"]);
                        if ((project.Status == TranslationStatus.ReadyForRecieve) || (project.Status == TranslationStatus.Receiving))
                        {
                            string RemoteID = connector.RetrieveProject(project);
                            if (project.Modified)
                            {
                                project.Save();
                            }
                        }                                                          
                    }
                    catch (Exception exception)
                    {
                        if (ret == "OK")
                        {
                            ret = String.Empty;
                        }
                        ret += "<br/>\r\nError on project:" + project.Name + " with error:" + exception.Message + "\r\nStack trace: " + exception.StackTrace + "\n";

                        _log.Error("Error on project:" + project.Name + " with error:" + exception.Message + "\r\nStack trace: " + exception.StackTrace);
                    }
                }
            }
            catch (Exception exp)
            {
                _log.Error(exp);
                ret = exp.Message + "<br/>\r\n" + exp.StackTrace;
            }

            mx.Close();

            return ret;
        }
    }   
}
