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

namespace EPiServer.Research.Translation4.Service
{
    //[ScheduledPlugIn(DisplayName = "Translation receiver service", Description = "This service receive translation task")]
    public class Receiver
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(Receiver));

        private static bool isRunning = false;        
        
        private static object lockObject = new object();
        public static string Execute()
        {

            if (isRunning)
                return "Service already running";
            lock (lockObject)
            {
                isRunning = true;

                // big try and catch to make sure release the running state.
                try
                {

                    DataSet ds = Manager.Current.GetProjects();
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        try
                        {
                            TranslationProject project = Manager.Current.GetTranslationProject((int)dr["pkid"]);
                            if (project.Status == TranslationStatus.ReadyForRecieve)
                            {
                                //project.Status = (int)TranslationStatus.Receiving;
                                //project.Save();

                                string RemoteID = Manager.Current.GetConnectors()[0].RetrieveProject(project);
                                if (project.Modified)
                                    project.Save();
                            }
                        }
                        catch (Exception exp)
                        {
                            logger.Error(exp);
                        }
                    }
                }
                catch (Exception exp)
                {
                    logger.Error(exp);
                }

                isRunning = false;
            }
            return "OK";
        }
    }
}
