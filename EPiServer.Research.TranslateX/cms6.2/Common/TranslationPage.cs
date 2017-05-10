namespace EPiServer.Research.Translation4.Common
{
    public class TranslationPage:TranslationItem
    {
        public TranslationPage()
        {
            ItemID = 0;
        }

        public string ParentNode { get; set; }

        /// <summary>
        /// PageID for the source page.
        /// </summary>
        public string OriginalID { get; set; }
    }
}
