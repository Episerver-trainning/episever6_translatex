using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;

namespace EPiServer.Research.Translation4.Common
{
    public class TranslationFile:TranslationItem
    {
        public delegate Stream StreamHandler(string filepath);

        public StreamHandler StreamLoader = null;
        public TranslationFile()
        {
        }

        private string _pageLink;
        public string PageLink
        {
            get
            {
                return _pageLink;
            }
            set
            {
                _pageLink = value;
            }
        }
        private string _filePath;
        public string FilePath
        {
            get
            {
                return _filePath;
            }
            set
            {
                _filePath = value;
            }
        }

        
        public Stream FileStream
        {
            get
            {
                if (StreamLoader != null)
                    return StreamLoader(this.FilePath);
                return null;
            }
        }
    }
}
