using System.IO;

namespace EPiServer.Research.Translation4.Common
{
    public class TranslationFile:TranslationItem
    {
        public delegate Stream StreamHandler(string filepath);

        public StreamHandler _streamLoader = null;

        public string PageLink { get; set; }
        public string FilePath { get; set; }

        public Stream FileStream
        {
            get
            {
                if (_streamLoader != null)
                {
                    return _streamLoader(FilePath);
                }
                return null;
            }
        }
    }
}
