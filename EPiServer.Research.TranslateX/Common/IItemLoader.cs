using System;
using System.Collections.Generic;
using System.Text;

namespace EPiServer.Research.Translation4.Common
{
    public interface IItemLoader
    {
        void Save(TranslationItem item);
        string GetItemLanguageID(int id, string language);
        void SetItemLanguageID(int id, string language, string remoteId);
        TranslationStatus GetItemLanguageStatus(int id, string language);
        string GetItemLanguageRemoteStatus(int id, string language);
        void SetItemLanguageStatus(int id, string language, TranslationStatus status);
        void SetItemLanguageRemoteStatus(int id, string language, string status);
        byte[] GetItemLanguageData(int id, string language);
        void SetItemLanguageData(int id, string language, byte[] data);
    }
}
