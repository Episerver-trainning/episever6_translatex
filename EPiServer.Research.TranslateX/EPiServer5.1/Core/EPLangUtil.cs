using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Globalization;
using EPiServer.DataAbstraction;

namespace EPiServer.Research.Translation4.Core
{
    public class LangPair
    {
        public LangPair()
        {
        }

        public LangPair(string name, string locale)
        {
            this.Name = name;
            this.Locale = locale;
        }
        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }
        private string _locale;
        public string Locale
        {
            get
            {
                return _locale;
            }
            set
            {
                _locale = value;
            }
        }
    }

    public class EPLangUtil
    {
        private static CultureInfo[] _availableCultures = _availableCultures = CultureInfo.GetCultures(CultureTypes.InstalledWin32Cultures);
        public static LangPair[] GetLanaguages()
        {
            LanguageBranchCollection lbc = EPiServer.DataAbstraction.LanguageBranch.ListEnabled();
            LangPair[] langs = new LangPair[lbc.Count];
            int count = 0;
            foreach (LanguageBranch lb in lbc)
            {                
                //string locale = GetLocaleName(LanguageBranch.Load(lb.LanguageID).ID);
                langs[count] = new LangPair(lb.Name, lb.LanguageID);
                count++;
            }
            return langs;
        }
        public static string GetLocaleName(int currentLocale)
        {
            foreach (CultureInfo info in _availableCultures)
            {
                if (info.LCID == currentLocale)
                {
                    return info.Name;
                }
            }
            return "";
        }
        public static string FindLangIDFromLocale(string locale)
        {           
            LanguageBranchCollection lc = LanguageBranch.ListEnabled();
            foreach (LanguageBranch l in lc)
            {
                if (locale == l.LanguageID)
                {
                    return l.LanguageID;
                }
            }
            return "";
        }
    }
}
