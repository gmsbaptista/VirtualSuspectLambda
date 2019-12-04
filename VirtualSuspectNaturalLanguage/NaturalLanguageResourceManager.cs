using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using VirtualSuspect.KnowledgeBase;

namespace VirtualSuspectNaturalLanguage {
    public class NaturalLanguageResourceManager {

        #region Singleton

        private static NaturalLanguageResourceManager instance;

        public static NaturalLanguageResourceManager Instance {

            get {
                if (instance == null) {
                    instance = new NaturalLanguageResourceManager();
                }
                return instance;
            }
        }

        #endregion

        private List<Resource> resources;

        private NaturalLanguageResourceManager() {

            XmlDocument actionResourcesDocument = new XmlDocument();

            actionResourcesDocument.Load(@"ActionResources.xml");

            resources = new List<Resource>();

            ParseResourceXml(actionResourcesDocument);

        }

        private void ParseResourceXml(XmlDocument resourceFile) {

            foreach(XmlNode resourceNode in resourceFile.DocumentElement.SelectNodes("resource")) {

                string resourceType = resourceNode.SelectSingleNode("type").InnerText;

                switch(resourceType) {
                    case "action":

                        ActionResource newResource = new ActionResource();
                        newResource.Action = resourceNode.SelectSingleNode("name").InnerText;
                        newResource.Speech = resourceNode.SelectSingleNode("speech").InnerText;
                        newResource.TimePrep = resourceNode.SelectSingleNode("timePreposition").InnerText;
                        newResource.LocationPrep = resourceNode.SelectSingleNode("locationPreposition").InnerText;
                        newResource.ThemePrep = resourceNode.SelectSingleNode("themePreposition").InnerText;
                        newResource.AgentPrep = resourceNode.SelectSingleNode("agentPreposition").InnerText;
                        newResource.ReasonPrep = resourceNode.SelectSingleNode("reasonPreposition").InnerText;
                        newResource.MannerPrep = resourceNode.SelectSingleNode("mannerPreposition").InnerText;
                        resources.Add(newResource);

                        break;
                    default:
                        continue;
                }
            }
            
        }

        public T FindResource<T>(string identifier) {

            return resources.OfType<T>().Where(x => ((Resource)x).Identifier == identifier).FirstOrDefault();

        }
    }

    interface Resource { 
    
        string Identifier { get; }
    
    }

    public class ActionResource : Resource {

        public string Identifier { get { return Action; }}

        public string Action { get; set; }

        public string TimePrep { get; set; }

        public string LocationPrep { get; set; }

        public string ThemePrep { get; set; }

        public string AgentPrep { get; set; }

        public string ReasonPrep { get; set; }

        public string MannerPrep { get; set; }

        public string Speech { get; set; }

        public string ExtractPreposition(KnowledgeBaseManager.DimentionsEnum dimension) {

            switch (dimension) {
                case KnowledgeBaseManager.DimentionsEnum.Time:
                    return TimePrep;
                case KnowledgeBaseManager.DimentionsEnum.Location:
                    return LocationPrep;
                case KnowledgeBaseManager.DimentionsEnum.Agent:
                    return AgentPrep;
                case KnowledgeBaseManager.DimentionsEnum.Theme:
                    return ThemePrep;
                case KnowledgeBaseManager.DimentionsEnum.Reason:
                    return ReasonPrep;
                case KnowledgeBaseManager.DimentionsEnum.Manner:
                    return MannerPrep;
                default:
                    return "";
            }

        }
    }

}
