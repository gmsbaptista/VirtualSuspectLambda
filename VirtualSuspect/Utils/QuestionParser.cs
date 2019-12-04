using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using VirtualSuspect.Query;
using VirtualSuspect.Exception;
using VirtualSuspect.KnowledgeBase;

namespace VirtualSuspect.Utils{

    public static class QuestionParser{
        
        public static QueryDto ExtractFromXml(XmlNode question) {

            QueryDto newQueryDto;

            String questionType = question.SelectSingleNode("type").InnerText;

            if(questionType == "get-information") {

                newQueryDto = new QueryDto(QueryDto.QueryTypeEnum.GetInformation);

            } else if (questionType == "yes-no") {

                newQueryDto = new QueryDto(QueryDto.QueryTypeEnum.YesOrNo);

            } else { //if the type is invalid

                throw new MessageFieldException("Invalid question type: " + questionType);

            }

            //Get Focus Field
            XmlNodeList focusNodeList = question.SelectNodes("focus");

            foreach(XmlNode focusNode in focusNodeList) {

                KnowledgeBaseManager.DimentionsEnum focusDimension = KnowledgeBaseManager.convertToDimentions(focusNode.SelectSingleNode("dimension").InnerText);

                //Parse the focus according to the dimension
                switch (focusDimension) {
                    case KnowledgeBaseManager.DimentionsEnum.Manner:
                        newQueryDto.AddFocus(new GetMannerFocusPredicate());
                        break;
                    case KnowledgeBaseManager.DimentionsEnum.Agent:
                        newQueryDto.AddFocus(new GetAgentFocusPredicate());
                        break;
                    case KnowledgeBaseManager.DimentionsEnum.Location:
                        newQueryDto.AddFocus(new GetLocationFocusPredicate());
                        break;
                    case KnowledgeBaseManager.DimentionsEnum.Time:
                        newQueryDto.AddFocus(new GetTimeFocusPredicate());
                        break;
                    case KnowledgeBaseManager.DimentionsEnum.Reason:
                        newQueryDto.AddFocus(new GetReasonFocusPredicate());
                        break;
                    case KnowledgeBaseManager.DimentionsEnum.Theme:
                        newQueryDto.AddFocus(new GetThemeFocusPredicate());
                        break;
                }

            }

            //Get Conditions Predicate
            XmlNodeList conditionsNodeList = question.SelectNodes("condition");
            foreach(XmlNode conditionNode in conditionsNodeList) {

                KnowledgeBaseManager.DimentionsEnum conditionDimension = KnowledgeBaseManager.convertToDimentions(conditionNode.SelectSingleNode("dimension").InnerText);

                QueryDto.OperatorEnum conditionOperator = parseOperator(conditionDimension, conditionNode.SelectSingleNode("operator").InnerText);

                switch (conditionDimension) {
                    case KnowledgeBaseManager.DimentionsEnum.Action:

                        switch (conditionOperator) {
                            case QueryDto.OperatorEnum.Equal:

                                string action = conditionNode.SelectSingleNode("value").InnerText;

                                newQueryDto.AddCondition(new ActionEqualConditionPredicate(action));

                                break;

                            default:
                                //nothing to do
                                break;
                        }

                        break;

                    case KnowledgeBaseManager.DimentionsEnum.Manner:

                        switch (conditionOperator) {
                            case QueryDto.OperatorEnum.Equal:

                                List<string> manners = new List<string>();

                                foreach(XmlNode mannerNode in conditionNode.SelectNodes("value")) {

                                    manners.Add(mannerNode.InnerText);

                                }
                                
                                newQueryDto.AddCondition(new MannerEqualConditionPredicate(manners));

                                break;

                            default:
                                //nothing to do
                                break;
                        }

                        break;
                    case KnowledgeBaseManager.DimentionsEnum.Agent:

                        switch (conditionOperator) {
                            case QueryDto.OperatorEnum.Equal:

                                List<string> agents = new List<string>();

                                foreach (XmlNode agentNode in conditionNode.SelectNodes("value")) {

                                    agents.Add(agentNode.InnerText);

                                }

                                newQueryDto.AddCondition(new AgentEqualConditionPredicate(agents));

                                break;

                            default:
                                //nothing to do
                                break;
                        }

                        break;

                    case KnowledgeBaseManager.DimentionsEnum.Location:

                        switch (conditionOperator) {

                            case QueryDto.OperatorEnum.Equal:

                                string location = conditionNode.SelectSingleNode("value").InnerText;

                                newQueryDto.AddCondition(new LocationEqualConditionPredicate(location));

                                break;

                            default:
                                //nothing to do
                                break;
                        }

                        break;
                    case KnowledgeBaseManager.DimentionsEnum.Time:

                        switch (conditionOperator) {
                            case QueryDto.OperatorEnum.Equal:

                                string time = conditionNode.SelectSingleNode("value").InnerText;

                                newQueryDto.AddCondition(new TimeEqualConditionPredicate(time));

                                break;

                            case QueryDto.OperatorEnum.Between:

                                string begin = conditionNode.SelectSingleNode("begin").InnerText;

                                string end = conditionNode.SelectSingleNode("end").InnerText;

                                newQueryDto.AddCondition(new TimeBetweenConditionPredicate(begin, end));

                                break;

                            default:

                                //nothing to do
                                break;
                        }
                        break;

                    case KnowledgeBaseManager.DimentionsEnum.Reason:

                        switch (conditionOperator) {
                            case QueryDto.OperatorEnum.Equal:

                                List<string> reasons = new List<string>();

                                foreach (XmlNode reasonNode in conditionNode.SelectNodes("value")) {

                                    reasons.Add(reasonNode.InnerText);

                                }

                                newQueryDto.AddCondition(new ReasonEqualConditionPredicate(reasons));

                                break;

                            default:
                                //nothing to do
                                break;
                        }

                        break;

                    case KnowledgeBaseManager.DimentionsEnum.Theme:

                        switch (conditionOperator) {
                            case QueryDto.OperatorEnum.Equal:

                                List<string> themes = new List<string>();

                                foreach (XmlNode themeNode in conditionNode.SelectNodes("value")) {

                                    themes.Add(themeNode.InnerText);

                                }

                                newQueryDto.AddCondition(new ThemeEqualConditionPredicate(themes));

                                break;

                            default:
                                //nothing to do
                                break;
                        }

                        break;

                    }

                }

            return newQueryDto;
        }

        /// <summary>
        /// Helper method to map string operator to the enum field
        /// Tests if the operator is available for that particular dimension
        /// </summary>
        /// <param name="dimension">test availability of operator in this dimension</param>
        /// <param name="operatorToParse">operator to be parsed</param>
        /// <returns></returns>
        private static QueryDto.OperatorEnum parseOperator(KnowledgeBaseManager.DimentionsEnum dimension, string operatorToParse) {

            switch (dimension) {
                case KnowledgeBaseManager.DimentionsEnum.Action:
                    switch(operatorToParse) {
                        case "equal":
                            return QueryDto.OperatorEnum.Equal;
                        default:
                            throw new MessageFieldException("Invalid operator for dimension Action: " + operatorToParse);
                    }
                case KnowledgeBaseManager.DimentionsEnum.Manner:
                    switch (operatorToParse) {
                        case "equal":
                            return QueryDto.OperatorEnum.Equal;
                        default:
                            throw new MessageFieldException("Invalid operator for dimension Manner: " + operatorToParse);
                    }
                case KnowledgeBaseManager.DimentionsEnum.Agent:
                    switch (operatorToParse) {
                        case "equal":
                            return QueryDto.OperatorEnum.Equal;
                        default:
                            throw new MessageFieldException("Invalid operator for dimension Agent: " + operatorToParse);
                    }
                case KnowledgeBaseManager.DimentionsEnum.Location:
                    switch (operatorToParse) {
                        case "equal":
                            return QueryDto.OperatorEnum.Equal;
                        default:
                            throw new MessageFieldException("Invalid operator for dimension Location: " + operatorToParse);
                    }
                case KnowledgeBaseManager.DimentionsEnum.Time:
                    switch (operatorToParse) {
                        case "equal":
                            return QueryDto.OperatorEnum.Equal;
                        case "between":
                            return QueryDto.OperatorEnum.Between;
                        default:
                            throw new MessageFieldException("Invalid operator for dimension Time: " + operatorToParse);
                    }
                case KnowledgeBaseManager.DimentionsEnum.Reason:
                    switch (operatorToParse) {
                        case "equal":
                            return QueryDto.OperatorEnum.Equal;
                        default:
                            throw new MessageFieldException("Invalid operator for dimension Reason: " + operatorToParse);
                    }
                case KnowledgeBaseManager.DimentionsEnum.Theme:
                    switch (operatorToParse) {
                        case "equal":
                            return QueryDto.OperatorEnum.Equal;
                        default:
                            throw new MessageFieldException("Invalid operator for dimension Theme: " + operatorToParse);
                    }

                default:
                    //Intentionally left empty (will never reach)
                    throw new MessageFieldException("Invalid dimension");
            }
        }
    }
}
