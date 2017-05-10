using System.Collections;

namespace EPiServer.Research.Translation4.Common
{
    public class TranslationItem
    {
        public IItemLoader ItemLoader { get; set; }

        public int Indent { get; set; }

        public int ItemID { get; set; }

        public int RemoteStatus { get; set; }

        public string RemoteID { get; set; }

        public Hashtable Properties { get; set; }

        public bool Modified { get; set; }

        public bool Sent { get; set; }

        public int Status { get; set; }

        public void Save()
        {
            ItemLoader.Save(this);
        }

        public string GetRemoteID(string lang)
        {
            return ItemLoader.GetItemLanguageID(this.ItemID, lang);
        }

        public void SetRemoteID(string lang, string id)
        {
            ItemLoader.SetItemLanguageID(this.ItemID, lang, id);
        }

        public TranslationStatus GetStatus(string lang)
        {
            return ItemLoader.GetItemLanguageStatus(ItemID, lang);
        }
        public string GetRemoteStatus(string lang)
        {
            return ItemLoader.GetItemLanguageRemoteStatus(ItemID, lang);
        }

        public void SetData(string lang, byte[] data)
        {
            ItemLoader.SetItemLanguageData(ItemID, lang, data);
        }

        public void SetStatus(string lang, TranslationStatus p)
        {
            ItemLoader.SetItemLanguageStatus(ItemID, lang, p);
        }
        public void SetRemoteStatus(string lang, string p)
        {
            ItemLoader.SetItemLanguageRemoteStatus(ItemID, lang, p);
        }

        public byte[] GetData(string lang)
        {
            return ItemLoader.GetItemLanguageData(ItemID, lang);
        }
    }
}
