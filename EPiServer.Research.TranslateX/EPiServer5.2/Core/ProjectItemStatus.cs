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
    public enum ProjectItemStatus
    {
        NoSend = 1,
        Send=2,
        RemoteRejected=3,
        RemoteDone=4
    }
}
