﻿using EPiServer.Research.Translation4.Common;

namespace EPiServer.Research.Translation.Core
{
    public class TranslationPageLoader: IItemLoader
    {
        public void Save(TranslationItem item)
        {
            Manager.Current.SavePage((TranslationPage)item);
        }

        public string GetItemLanguageID(int id, string language)
        {
            return Manager.Current.GetRemoteID(id, language);
        }

        public void SetItemLanguageID(int id, string language, string remoteId)
        {
            Manager.Current.SetRemoteID(id, language, remoteId);
        }

        public TranslationStatus GetItemLanguageStatus(int id, string language)
        {
            return Manager.Current.GetItemLanguageStatus(id, language);
        }

        public string GetItemLanguageRemoteStatus(int id, string language)
        {
            return Manager.Current.GetItemLanguageRemoteStatus(id, language);
        }

        public void SetItemLanguageStatus(int id, string language, TranslationStatus status)
        {
            Manager.Current.SetItemLanguageStatus(id, language, status);
        }

        public void SetItemLanguageRemoteStatus(int id, string language, string status)
        {
            Manager.Current.SetItemLanguageRemoteStatus(id, language, status);
        }
        public byte[] GetItemLanguageData(int id, string language)
        {
            return Manager.Current.GetItemLanguageData(id, language);
        }

        public void SetItemLanguageData(int id, string language, byte[] data)
        {
            Manager.Current.SetItemLanguageData(id, language, data);
        }

     }
}
