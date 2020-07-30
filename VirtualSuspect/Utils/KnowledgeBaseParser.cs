using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using VirtualSuspect.KnowledgeBase;

namespace VirtualSuspect.Utils
{
    public static class KnowledgeBaseParser
    {

        public static KnowledgeBaseManager parseFromFile(string filePath)
        {

            XmlDocument xmlFile = new XmlDocument();

            xmlFile.Load(filePath);

            return parseFromXml(xmlFile.DocumentElement);

        }

        public static KnowledgeBaseManager parseFromXml(XmlNode xmlRoot)
        {

            KnowledgeBaseManager kb = new KnowledgeBaseManager();

            Dictionary<uint, EntityNode> entities = new Dictionary<uint, EntityNode>();

            //Extract all entities
            XmlNodeList entityNodesList = xmlRoot.SelectNodes("entity");

            foreach (XmlNode entityNode in entityNodesList)
            {

                uint id = UInt32.Parse(entityNode.SelectSingleNode("id").InnerText);

                string type = entityNode.SelectSingleNode("type").InnerText;

                if (type == "TimeSpan")
                {//Example: dd/MM/yyyyTHH:mm:ss>dd/MM/yyyyTHH:mm:ss

                    string beginTime = entityNode.SelectSingleNode("begin").InnerText;

                    string endTime = entityNode.SelectSingleNode("end").InnerText;

                    EntityDto newEntityDto = new EntityDto(beginTime + ">" + endTime, "", "TimeSpan");

                    entities.Add(id, kb.CreateNewEntity(newEntityDto)); //Source of Polymorphism problems

                }
                else if (type == "TimeInstant")
                {//Example: dd/MM/yyyyTHH:mm:ss

                    string time = entityNode.SelectSingleNode("value").InnerText;

                    EntityDto newEntityDto = new EntityDto(time, "", "TimeInstant");

                    entities.Add(id, kb.CreateNewEntity(newEntityDto)); //Source of Polymorphism problems

                }
                else
                {

                    string value = entityNode.SelectSingleNode("value").InnerText;

                    string speech = entityNode.SelectSingleNode("speech").InnerText;

                    EntityDto newEntityDto = new EntityDto(value, speech, type);

                    entities.Add(id, kb.CreateNewEntity(newEntityDto));

                }

            }

            foreach (XmlNode entityNode in entityNodesList)
            {
                uint id = UInt32.Parse(entityNode.SelectSingleNode("id").InnerText);
                XmlNodeList associationNodesList = entityNode.SelectNodes("association");
                foreach (XmlNode associationNode in associationNodesList)
                {
                    string associationRelation = associationNode.Attributes["relation"].Value;
                    if (associationRelation == "Parent")
                    {
                        uint enID = UInt32.Parse(associationNode.SelectSingleNode("entity").Attributes["id"].Value);
                        entities[id].Parent = entities[enID];
                    }
                    else
                    {
                        string speech = associationNode.SelectSingleNode("speech").InnerText;
                        entities[id].AddAssociation(associationRelation, speech);
                    }
                }
            }

            //Extract all actions from each episode
            XmlNodeList eventsNodesList = xmlRoot.SelectNodes("event");

            foreach (XmlNode eventXmlNode in eventsNodesList)
            {

                //Get Action if it does not exist yet
                ActionNode newActionNode = kb.GetOrCreateAction(eventXmlNode.SelectSingleNode("action").InnerText);

                //Get Time
                XmlNode timeXmlNode = eventXmlNode.SelectSingleNode("association[@relation='Time']");
                EntityNode timeNode = entities[UInt32.Parse(timeXmlNode.SelectSingleNode("entity").Attributes["id"].Value)];

                //Get Location
                XmlNode locationXmlNode = eventXmlNode.SelectSingleNode("association[@relation='Location']");
                EntityNode locationNode = entities[UInt32.Parse(locationXmlNode.SelectSingleNode("entity").Attributes["id"].Value)];

                //Get Subject
                XmlNode subjectXmlNode = eventXmlNode.SelectSingleNode("association[@relation='Subject']");
                EntityNode subjectNode = entities[UInt32.Parse(subjectXmlNode.SelectSingleNode("entity").Attributes["id"].Value)];

                //Get accusatory factor
                int incriminatoryValue = Int32.Parse(eventXmlNode.SelectSingleNode("incriminatory").InnerText);

                //Get real flag
                bool realFlag = eventXmlNode.SelectSingleNode("real").InnerText == "true";

                //Make new EventNode
                EventDto eventDto = new EventDto(incriminatoryValue, newActionNode, timeNode, locationNode, subjectNode, realFlag);

                //Make associations
                XmlNodeList associationNodesList = eventXmlNode.SelectNodes("association");

                foreach (XmlNode associationNode in associationNodesList)
                {

                    string associationRelation = associationNode.Attributes["relation"].Value;

                    uint enID = UInt32.Parse(associationNode.SelectSingleNode("entity").Attributes["id"].Value);

                    if (associationRelation == "Agent")
                    {
                        eventDto.AddAgent(entities[enID]);
                    }
                    else if (associationRelation == "Theme")
                    {
                        eventDto.AddTheme(entities[enID]);
                    }
                    else if (associationRelation == "Manner")
                    {
                        eventDto.AddManner(entities[enID]);
                    }
                    else if (associationRelation == "Reason")
                    {
                        eventDto.AddReason(entities[enID]);
                    }

                }

                EventNode newEventNode = kb.CreateNewEvent(eventDto);

                if (eventXmlNode.SelectSingleNode("real").InnerText == "true")
                {
                    kb.AddEventToStory(newEventNode);
                }

            }

            //Extract porperties
            Dictionary<string, string> properties = kb.Properties;

            XmlNode propertyNode;

            if ((propertyNode = xmlRoot.SelectSingleNode("/story/properties/name")) != null)
            {

                properties.Add("Name", propertyNode.InnerText);

            }

            if ((propertyNode = xmlRoot.SelectSingleNode("/story/properties/gender")) != null)
            {

                properties.Add("Gender", propertyNode.InnerText);

            }

            kb.PropagateIncriminatoryValues();

            return kb;

        }
    }
}
