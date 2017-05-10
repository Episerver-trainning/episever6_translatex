using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.Research.Translation4.Core
{
    public class ConnectorDefinition
    {
        public ConnectorDefinition()
        {
        }

        public string Name { get; set; }

        public string TypeName { get; set; }

        public string AssemblyName { get; set; }

        public string ControlToCreatingLaststep { get; set; }

        public string ControlToView { get; set; }

        public string ControlToImportStep { get; set; }

    }
}
