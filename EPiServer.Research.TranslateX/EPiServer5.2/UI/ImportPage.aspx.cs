using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using EPiServer.Research.Translation4.Core;
using EPiServer.Research.Translation4.Common;
using EPiServer.Core;
using System.Collections.Generic;
using EPiServer.FileSystem;
using System.IO;
using EPiServer.Web.Hosting;

namespace EPiServer.Research.Translation4.UI
{
    public partial class ImportPage : EPiServer.SimplePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ConnectorDefinition cd = Manager.Current.GetConntectorDefinitionByName(CurrentProject.ConnectorName);
            if (cd.ControlToImportStep != string.Empty)
            {
                customerStep.UserControlString = cd.ControlToImportStep;
            }
            else
            {
                Wizard1.WizardSteps.RemoveAt(0);
            }
        }

        TranslationProject CurrentProject
        {
            get
            {
                if (_currentProject == null)
                {
                    int projectid = int.Parse(Request.QueryString["projectid"]);
                    _currentProject = Manager.Current.GetTranslationProject(projectid);
                    _currentProject.Connector = Manager.Current.GetConntectorByName(_currentProject.ConnectorName);
                }
                return _currentProject;
            }
        }
        TranslationProject _currentProject = null;
        
        protected void WizardFinished(object sender, EventArgs e)
        {
            if (ViewState["pageselected"] != null)
                createAt.PageLink = (PageReference)ViewState["pageselected"];
            if (ViewState["importas"]!=null)
                importas.SelectedIndex = (int)(ViewState["importas"]);
            int projectid = int.Parse(Request.QueryString["projectid"]);

            int importtype = importas.Value == "0" ? 0 : 1;

            EPiServer.DataAccess.SaveAction saveAction = publishtype.Value=="0"? EPiServer.DataAccess.SaveAction.Publish:EPiServer.DataAccess.SaveAction.CheckIn;

            TranslationProject tp = CurrentProject;
            
            if (importtype == 0)
            {
                // new version 
                List<TranslationPage> pages =  tp.Pages;
                foreach (TranslationPage page in pages)
                {
                    foreach (string lang in tp.TargetLanguages)
                    {
                        if (page.GetStatus(lang) == TranslationStatus.Imported)
                            continue;

                        TranslationPage tpdata = tp.Connector.GetPage(page.GetData(lang));
                        PageData epPage = DataFactory.Instance.GetPage(PageReference.Parse(page.OriginalID));
                        string locale  = EPLangUtil.FindLangIDFromLocale(lang);
                        if (locale == "")
                            locale = Manager.Current.ToInvalidLocale(lang);
                        ILanguageSelector langSel = new LanguageSelector(locale);

                        PageData newPage = DataFactory.Instance.GetPage( new PageReference(epPage.PageLink.ID), langSel);
                        if (newPage == null)
                            newPage = DataFactory.Instance.CreateLanguageBranch(epPage.PageLink, langSel);
                        else
                        {
                            newPage = newPage.CreateWritableClone();
                        }
                        foreach (string key in tpdata.Properties.Keys)
                        {
                            if ( ( newPage.Property[key]!=null) &&
                                (!newPage.Property[key].IsDynamicProperty)
                                && (newPage.Property[key].IsLanguageSpecific)
                                && ((!newPage.Property[key].IsMetaData) || (key.ToLower() == "pagename"))
                                )
                            {
                                if ((key.ToLower() == "pagename") && ( string.IsNullOrEmpty( tpdata.Properties[key] as string) ))
                                    continue;

                                 newPage[key] = tpdata.Properties[key];
                            }
                        }
                        
                        PageReference newPr = DataFactory.Instance.Save(newPage, saveAction);
                        page.SetStatus(lang, TranslationStatus.Imported);
                    }
                }
                foreach (TranslationFile file in tp.Files)
                {
                    UnifiedDirectory upd = DataFactory.Instance.GetPage(PageReference.Parse(file.PageLink)).GetPageDirectory(true);
                    string orginalfilename = Path.GetFileName(file.FilePath);
                    string newfilename = "";

                    foreach (string lang in tp.TargetLanguages)
                    {
                        newfilename = Path.GetFileNameWithoutExtension(orginalfilename) + suffixtext.Text + lang.Replace("-","_") + Path.GetExtension(orginalfilename);
                        UnifiedFile uf = upd.CreateFile(newfilename);
                        BinaryWriter write = new BinaryWriter( uf.Open(FileMode.OpenOrCreate));
                        write.Write(file.GetData(lang));
                        write.Close();
                        file.SetStatus(lang, TranslationStatus.Imported);                     
                    }
                }
            }
            else
            {
                // for each language we do:
                // 1. create a new node
                // 2. add children according to indent pages list in db.
                foreach (string lang in tp.TargetLanguages)
                {
                    List<TranslationPage> pages =  tp.Pages;
                    int indent = 1;
                    PageReference startNode = createAt.PageLink;
                    string parentNode = "0";
                    createNewPage(lang, startNode, indent,parentNode, saveAction,ref pages, ref tp);
                }
            }
            tp.Status = TranslationStatus.Imported;
            tp.Save();
            closeWindowPanel.Visible = true;
        }

        
        private List<string> GetRequiredAndLanguageNeutralPropertyKeys(PageData page)
        {
            List<string> properties = new List<string>();
            foreach (PropertyData prop in page.Property)
            {
                if (prop.IsDynamicProperty)
                {
                    continue;
                }
                if (prop.IsNull)
                {
                    continue;
                }
                if (!page.IsMasterLanguageBranch )
                {
                    continue;
                }
                if ((prop.IsMetaData) && ( prop.Name.ToLower() !="pagename" ))
                {
                    continue;
                }
                if ((prop.IsLanguageSpecific) &&(!prop.IsRequired))
                {
                    continue;
                }
                if (prop.Name == "PageLink" || prop.Name == "PageParentLink")
                {
                    continue;
                }
                properties.Add(prop.Name);
            }

            return properties;
        }


        private void createNewPage(string lang, PageReference startNode, int indent, string parentNode, EPiServer.DataAccess.SaveAction saveAction, ref List<TranslationPage> pages, ref TranslationProject tp)
        {
            foreach (TranslationPage page in pages)
            {
                if ( (page.Indent == indent) && ( indent==1 || page.ParentNode == parentNode ) )
                {

                    TranslationPage tpdata = tp.Connector.GetPage(page.GetData(lang));
                    PageData epPage = DataFactory.Instance.GetPage(PageReference.Parse(page.OriginalID));

                    string locale = EPLangUtil.FindLangIDFromLocale(lang);
                    if (locale == "")
                        locale = EPLangUtil.FindLangIDFromLocale(lang.Substring(0, 2));

                    if (locale == "")
                        locale = EPLangUtil.FindLangIDFromLocale(lang.Substring(3, 2));
                    ILanguageSelector langSel = new LanguageSelector(locale);

                    PageData newPage = DataFactory.Instance.GetDefaultPageData(startNode, epPage.PageTypeID, langSel);

                    // we need do it twice
                    // one for all properties defined in master language
                    // then override all properties defined in original language
                    
                    // get from master language 
                    PageData masterPage = DataFactory.Instance.GetPage(epPage.PageLink, new LanguageSelector(epPage.MasterLanguageBranch));
                    List<string> props = GetRequiredAndLanguageNeutralPropertyKeys(masterPage);
                    foreach (string key in props)
                    {
                        newPage[key] = masterPage.Property[key].Value;
                    }

                    if (masterPage.PageLink != epPage.PageLink)
                    {
                        // override from original language
                        props = GetRequiredAndLanguageNeutralPropertyKeys(epPage);
                        foreach (string key in props)
                        {
                            newPage[key] = epPage.Property[key].Value;
                        }
                    }
                                      
                    // override with translated language
                    foreach (string key in tpdata.Properties.Keys)
                    {
                        // new page we take all data in.
                        if ((newPage.Property[key] != null) && (!newPage.Property[key].IsDynamicProperty)
                            && (newPage.Property[key].IsLanguageSpecific)
                            && ((!newPage.Property[key].IsMetaData) || (key.ToLower() == "pagename"))
                            )
                        {                           
                            newPage[key] = tpdata.Properties[key];
                        }
                    }
                    PageReference newNode = DataFactory.Instance.Save(newPage, saveAction);

                    foreach (TranslationFile file in tp.Files)
                    {
                        if (file.PageLink == page.OriginalID)
                        {                            
                            UnifiedDirectory upd = DataFactory.Instance.GetPage(newNode).GetPageDirectory(true);
                           
                            string orginalfilename = Path.GetFileName(file.FilePath);
                            string newfilename = "";

                            newfilename = orginalfilename;
                            UnifiedFile uf = upd.CreateFile(newfilename);
                            BinaryWriter write = new BinaryWriter(uf.Open(FileMode.OpenOrCreate));
                            write.Write(file.GetData(lang));
                            write.Close();
                        }
                    }


                    createNewPage(lang, newNode, indent + 1, page.OriginalID.Split(new char[]{'_'})[0],saveAction, ref pages,ref tp);
                }
            }
        }

        protected void StepChanged(object sender, EventArgs e)
        {
            if (ViewState["pageselected"] != null)
                createAt.PageLink = (PageReference) ViewState["pageselected"];

            if (ViewState["importas"] != null)
                importas.SelectedIndex = (int)(ViewState["importas"]);
            if (Wizard1.ActiveStep == WizardStep1)
            {
                if (customerStep != null)
                {
                    ICustomerStep iccs = customerStep.InnerControl as ICustomerStep;
                    iccs.Save(CurrentProject);
                }
            }
        }
        protected void NextStep(object sender, EventArgs e)
        {
            
            ViewState["importas"] = importas.SelectedIndex;

            if (createAt.PageLink != null)
            {
                ViewState["pageselected"] = createAt.PageLink;
            }           
        }
        protected void PrevStep(object sender, EventArgs e)
        {
            ViewState["importas"] = importas.SelectedIndex;

            if (createAt.PageLink != null)
            {
                ViewState["pageselected"] = createAt.PageLink;
            }            
        }
    }
}
