using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using VirtualSuspect.Query;
using VirtualSuspect.KnowledgeBase;

namespace VirtualSuspect.Utils
{
    public static class AnswerGenerator{

        public static XmlDocument GenerateAnswer(QueryResult queryResult) {

            XmlDocument newAnswer = new XmlDocument();
            newAnswer.AppendChild(newAnswer.CreateElement("answer"));

            if(queryResult.Query.QueryType == QueryDto.QueryTypeEnum.YesOrNo) {

                XmlElement newBooleanResponse = newAnswer.CreateElement("YesOrNoResult");
                newBooleanResponse.InnerText = "" + queryResult.YesNoResult;

                XmlNode refElem = newAnswer.DocumentElement.LastChild;

                newAnswer.DocumentElement.InsertAfter(newBooleanResponse, refElem);

            }
            else if (queryResult.Query.QueryType == QueryDto.QueryTypeEnum.GetInformation) {

                foreach (KeyValuePair<string,string> pair in queryResult.MetaData) {

                    XmlElement newMetaData = newAnswer.CreateElement(pair.Key);
                    newMetaData.InnerText = pair.Value;
                    XmlNode refElem = newAnswer.DocumentElement.LastChild;
                    newAnswer.DocumentElement.InsertAfter(newMetaData, refElem);

                }
                foreach (QueryResult.Result result in queryResult.Results) {

                    XmlElement newResponseXml = newAnswer.CreateElement("response");

                    XmlElement newDimensionNode = newAnswer.CreateElement("dimension");
                    newDimensionNode.InnerText = KnowledgeBaseManager.convertToString(result.dimension);

                    XmlElement newCardinalityNode = newAnswer.CreateElement("cardinality");
                    newCardinalityNode.InnerText = "" + result.cardinality;

                    newResponseXml.AppendChild(newDimensionNode);
                    newResponseXml.AppendChild(newCardinalityNode);

                    foreach (string value in result.values.Select(x => x.Value)) {

                        XmlElement newValueNode = newAnswer.CreateElement("value");
                        newValueNode.InnerText = value;

                        newResponseXml.AppendChild(newValueNode);
                    }

                    XmlNode refElem = newAnswer.DocumentElement.LastChild;

                    newAnswer.DocumentElement.InsertAfter(newResponseXml, refElem);

                }

            }

            return newAnswer;
        }

    }
}
