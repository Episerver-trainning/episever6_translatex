using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace EPiServer.Research.Translation4.Common
{
    public class TranslationPage:TranslationItem
    {
        public TranslationPage()
        {
            ItemID = 0;
        }

        

        private string _parentNode;
        public string ParentNode
        {
            get
            {
                return _parentNode;
            }
            set
            {
                _parentNode = value;
            }
        }

        private string _originalID;
        /// <summary>
        /// PageID for the source page.
        /// </summary>
        public string OriginalID
        {
            get
            {
                return _originalID;
            }
            set
            {
                _originalID = value;
            }
        }




    }
}
