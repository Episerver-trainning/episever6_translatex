using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace EPiServer.Research.Translation4.Common
{
    public class TranslationItem
    {
        public TranslationItem()
        { 
        }
        private IItemLoader _itemLoader;
        public IItemLoader ItemLoader
        {
            get
            {
                return _itemLoader;
            }
            set
            {
                _itemLoader = value;
            }
        }

        private int _indent;
        public int Indent
        {
            get
            {
                return _indent;
            }
            set
            {
                _indent = value;
            }

        }

        private int _itemID;
        public int ItemID
        {
            get
            {
                return _itemID;
            }
            set
            {
                _itemID = value;
            }
        }
        private int _remoteStatus;
        public int RemoteStatus
        {
            get
            {
                return _remoteStatus;
            }
            set
            {
                _remoteStatus = value;
            }
        }

        private string _remoteID;
        public string RemoteID
        {
            get
            {
                return _remoteID;
            }
            set
            {
                _remoteID = value;
            }
        }

        private Hashtable _properties;
        public Hashtable Properties
        {
            get
            {
                return _properties;
            }
            set
            {
                _properties = value;
            }
        }

        private bool _modified;
        public bool Modified
        {
            get
            {
                return _modified;
            }
            set
            {
                _modified = value;
            }
        }

        private bool _sent;
        public bool Sent
        {
            get
            {
                return _sent;
            }
            set
            {
                _sent = value;
            }
        }

        private int _status;
        public int Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
            }
        }
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
