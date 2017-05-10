using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace EPiServer.Research.Translation4.Core
{
    public class Constants
    {
        public static readonly string USERPROPUSERACCOUNT = "translationAccount";
        public static readonly string USERPROPUSERPASSWORD = "translationPassword";
        public static readonly string VIEWSTATSESSIONID = "translationsessionid";
        public static readonly string VIEWSTATISPAGELOADED = "translationpageloaded";
        public static readonly string VIEWSTATISFILELOADED = "translationfileloaded";
    }

}
