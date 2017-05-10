using System;
using System.Web;
using EPiServer.Web;

namespace EPiServer.Research.Translation.Core
{
    public class UntouchPathModule:IHttpModule
    {
        private static bool _isInited = false;

        private const string Locker = "1";

        public void  Dispose()
        { 	        
        }

        public void  Init(HttpApplication context)
        {
 	        lock (Locker)
            {
                if (!_isInited)
                {                   
                    // this does not work with out url
                    UrlRewriteProvider.AddExcludedPath("/EPiServer.Research.Translation/UI/EditProject.aspx");
                    UrlRewriteModule.HttpRewriteInit += new EventHandler<UrlRewriteEventArgs>(UrlRewriteModule_HttpRewriteInit);
                    _isInited = true;
                }
            }
        }

        
        static void UrlRewriteModule_HttpRewriteInit(object sender, UrlRewriteEventArgs e)
        {

            UrlRewriteModule urm = (UrlRewriteModule)sender;

            urm.HttpRewritingToInternal += urm_HttpRewritingToInternal;
            urm.HttpRewritingToExternal += urm_HttpRewritingToExternal;

        }

        static void urm_HttpRewritingToExternal(object sender, UrlRewriteEventArgs e)
        {
            if ((e.Url.Path.ToLower().EndsWith("/episerver.research.translation/ui/editproject.aspx")))
            {

                e.Cancel = true;

            }
        }

        static void urm_HttpRewritingToInternal(object sender, UrlRewriteEventArgs e)
        {

            if ((e.Url.Path.ToLower().EndsWith("/episerver.research.translation/ui/editproject.aspx")))
            {
                e.Cancel = true;
            }

        }
    }
}
