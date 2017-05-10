using System;
using System.Collections.Generic;
using System.Text;

namespace EPiServer.Research.Translation4.Common
{
    public interface IConnector
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns>return string.Empty if failed</returns>
         string SendProject(TranslationProject project);
 
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
         string RetrieveProject(TranslationProject project);

         void UpdateProject(TranslationProject project);
         TranslationPage GetPage(byte[] data);

        /// <summary>
        ///  For furture development. 
        /// </summary>
        /// <returns></returns>
         int GetHandlerSupportedVersionNumber();
         //string GetPageRemoteStatus(TranslationProject project, TranslationPage tp,string language);
         //string GetProjectRemoteStatus(TranslationProject tp);
    }
}
