using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace EPiServer.Research.Translation4.UI
{
    public class CustomerWizardStep : WizardStepBase
    {
       
        protected override void CreateChildControls()
        {        
            if (UserControlString!=string.Empty)
            {               
                Controls.Add(InnerControl);
                
            }
        }
        private string _userControlString = string.Empty;
        public string UserControlString
        {
            get{
                return _userControlString;
            }
            set
            {
                _userControlString = value;
                
                _innerControl = (UserControl)Page.LoadControl(_userControlString);
                _innerControl.ID = "EXTRA";
                
          
                EnsureChildControls();
            }
        }

        private UserControl _innerControl = null;
        public UserControl InnerControl
        {
            get
            {
                return _innerControl;
            }
        }
        
    }
}
