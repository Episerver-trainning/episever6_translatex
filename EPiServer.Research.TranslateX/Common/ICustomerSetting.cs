using System;
using System.Collections.Generic;
using System.Text;

namespace EPiServer.Research.Translation4.Common
{
    public interface ICustomerStep
    {
       //void Load(TranslationProject project);
       void Save(TranslationProject project);
    }
}
