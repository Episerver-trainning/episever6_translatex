using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.Web;

namespace EPiServer.Research.Translation4.Core
{
    public class UntouchPathModule:IHttpModule
    {
        private static bool _isInited = false;

        private static string locker = "1";
        
    
        #region IHttpModule Members

        public void  Dispose()
        { 	        
        }

        public void  Init(HttpApplication context)
        {
 	        lock (locker)
            {
                if (!_isInited)
                {                   
                    // this does not work with out url
                    FriendlyUrlRewriteProvider.UnTouchedPaths.Add("/EPiServer.Research.Translation4/UI/EditProject.aspx");

                    UrlRewriteModule.HttpRewriteInit += new EventHandler<UrlRewriteEventArgs>(UrlRewriteModule_HttpRewriteInit);
                    _isInited = true;
                }
            }
        }

        
        static void UrlRewriteModule_HttpRewriteInit(object sender, UrlRewriteEventArgs e)
        {

            UrlRewriteModule urm = (UrlRewriteModule)sender;

            urm.HttpRewritingToInternal += new EventHandler<UrlRewriteEventArgs>(urm_HttpRewritingToInternal);
            urm.HttpRewritingToExternal += new EventHandler<UrlRewriteEventArgs>(urm_HttpRewritingToExternal);

        }

        static void urm_HttpRewritingToExternal(object sender, UrlRewriteEventArgs e)
        {
            if ((e.Url.Path.ToLower().EndsWith("/episerver.research.translation4/ui/editproject.aspx")))
            {

                e.Cancel = true;

            }
        }

        static void urm_HttpRewritingToInternal(object sender, UrlRewriteEventArgs e)
        {

            if ((e.Url.Path.ToLower().EndsWith("/episerver.research.translation4/ui/editproject.aspx")))
            {
                e.Cancel = true;
            }

        }

        #endregion
    }
}
