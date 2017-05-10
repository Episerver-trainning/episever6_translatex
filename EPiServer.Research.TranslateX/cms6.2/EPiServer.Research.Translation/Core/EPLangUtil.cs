using System;
using System.Globalization;
using EPiServer.DataAbstraction;

namespace EPiServer.Research.Translation.Core
{
    public class LangPair
    {
        public LangPair()
        {
        }

        public LangPair(string name, string locale)
        {
            Name = name;
            Locale = locale;
        }

        public string Name { get; set; }
        public string Locale { get; set; }
    }

    public class EPLangUtil
    {
        private static readonly CultureInfo[] _availableCultures =  CultureInfo.GetCultures(CultureTypes.InstalledWin32Cultures);
        public static LangPair[] GetLanaguages()
        {
            LanguageBranchCollection lbc = LanguageBranch.ListEnabled();
            LangPair[] langs = new LangPair[lbc.Count];
            int count = 0;
            foreach (LanguageBranch lb in lbc)
            {            
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
            return String.Empty;
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
            return String.Empty;
        }


        public static LangPair[] GetValidLanaguages()
        {
            LanguageBranchCollection lbc = EPiServer.DataAbstraction.LanguageBranch.ListEnabled();
            LangPair[] langs = new LangPair[lbc.Count];
            int count = 0;
            foreach (LanguageBranch lb in lbc)
            {
                //string locale = GetLocaleName(LanguageBranch.Load(lb.LanguageID).ID);
                string localeid = lb.LanguageID;

                if (lb.LanguageID.Length == 2)
                    localeid = Manager.Current.ToValidLocale(lb.LanguageID);

                langs[count] = new LangPair(lb.Name, localeid);
                count++;
            }
            return langs;
        }

        public static string GetValidLanguageString(string locale)
        {
            return Manager.Current.ToValidLocale(locale);
        }
    }
}
