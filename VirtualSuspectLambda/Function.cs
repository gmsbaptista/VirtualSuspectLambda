using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization;

using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;

using VirtualSuspect;
using VirtualSuspect.Query;
using VirtualSuspect.KnowledgeBase;
using VirtualSuspect.Utils;

using VirtualSuspectNaturalLanguage;

using Newtonsoft.Json;
using System.Globalization;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace VirtualSuspectLambda
{
    public class Function
    {
        const string voice = "Matthew";
        private KnowledgeBaseManager knowledge_base;
        private VirtualSuspectQuestionAnswer virtual_suspect;
        private Context lastInteraction;
        //private bool bitchMode = false;
        private Dictionary<string, bool> options = new Dictionary<string, bool>()
        {
            {"Bish mode", false },
            {"Slot filtering", true },
            {"Answer filtering", true },
            {"Empty answer generation", true },
            {"Detailed feedback", false }
        };

        /// <summary>
        /// Application entry point
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            SkillResponse response = new SkillResponse
            {
                Response = new ResponseBody()
            };
            response.Response.ShouldEndSession = false;
            response.Response.Reprompt = new Reprompt();
            response.Version = "1.0";

            IOutputSpeech innerResponse = null;
            IOutputSpeech prompt = null;

            var log = context.Logger;
            log.LogLine($"Skill Request Object:");
            log.LogLine(JsonConvert.SerializeObject(input));

            if (input.GetRequestType() == typeof(LaunchRequest))
            {
                log.LogLine($"LaunchRequest: open Virtual Suspect");

                knowledge_base = KnowledgeBaseParser.parseFromFile("NewRobberyStory.xml");
                virtual_suspect = new VirtualSuspectQuestionAnswer(knowledge_base);
                lastInteraction = new Context();
                lastInteraction.UpdateResult(new QueryResult(new QueryDto(QueryDto.QueryTypeEnum.YesOrNo)));

                ResetOptions();

                log.LogLine($"first entity in kb: " + knowledge_base.Entities[0].Value);
                log.LogLine($"first action in kb: " + knowledge_base.Actions[0].Value);
                log.LogLine($"first event in kb: " + knowledge_base.Events[0].Action.Value);

                string firstText = "Welcome to the Virtual Suspect prototype. ";
                string suspectInformation = "Your suspect is Peter, he is accused of robbing a jewelry shop. ";
                string lastText = "You can ask him questions to find out more about the case.";
                string speechText = firstText + suspectInformation + lastText;

                innerResponse = new PlainTextOutputSpeech();
                (innerResponse as PlainTextOutputSpeech).Text = speechText;
                prompt = new PlainTextOutputSpeech();
                (prompt as PlainTextOutputSpeech).Text = lastText;
            }
            else if (input.GetRequestType() == typeof(IntentRequest))
            {
                var intentRequest = (IntentRequest)input.Request;

                QueryDto query;
                string speechText;

                switch (intentRequest.Intent.Name)
                {
                    case "AMAZON.StopIntent":
                        log.LogLine($"AMAZON.StopIntent: close Virtual Suspect");

                        speechText = "You are now leaving the Virtual Suspect. Thank you for playing!";

                        BuildAnswer(ref innerResponse, ref prompt, speechText, false);
                        response.Response.ShouldEndSession = true;
                        break;
                    case "AMAZON.CancelIntent":
                        log.LogLine($"AMAZON.CancelIntent: close Virtual Suspect");

                        speechText = "You are now leaving the Virtual Suspect. Thank you for playing!";

                        BuildAnswer(ref innerResponse, ref prompt, speechText, false);
                        response.Response.ShouldEndSession = true;
                        break;
                    case "AMAZON.HelpIntent":
                        log.LogLine($"AMAZON.HelpIntent: send help message");

                        speechText = "You can ask the suspect questions about the case.";

                        BuildAnswer(ref innerResponse, ref prompt, speechText, false);
                        break;
                    case "AMAZON.FallbackIntent":
                        log.LogLine($"AMAZON.FallbackIntent: express confusion");
                        //pregen answer
                        speechText = "I don't think that has anything to do with what we're talking about";
                        if (options["Detailed feedback"])
                        {
                            speechText += ". Fallback intent";
                        }

                        BuildAnswer(ref innerResponse, ref prompt, speechText, true);
                        break;
                    case "ToggleOptionIntent":
                        log.LogLine($"ToggleOptionIntent: switch an option");

                        speechText = ToggleOption(intentRequest, log);

                        BuildAnswer(ref innerResponse, ref prompt, speechText, false);
                        break;
                    case "TurnOnOptionIntent":
                        log.LogLine($"TurnOnOptionIntent: turn option on");

                        speechText = TurnOption(intentRequest, log, true);

                        BuildAnswer(ref innerResponse, ref prompt, speechText, false);
                        break;
                    case "TurnOffOptionIntent":
                        log.LogLine($"TurnOffOptionIntent: turn option off");

                        speechText = TurnOption(intentRequest, log, false);

                        BuildAnswer(ref innerResponse, ref prompt, speechText, false);
                        break;
                    case "CheckOptionsIntent":
                        log.LogLine($"CheckOptionsIntent: say all the options");

                        speechText = "";

                        foreach (string option in options.Keys)
                        {
                            speechText += option + " is " + (options[option] ? "on." : "off.") + "\n";
                        }

                        BuildAnswer(ref innerResponse, ref prompt, speechText, false);
                        break;
                    case "GreetingIntent":
                        log.LogLine($"GreetingIntent: say hello");

                        speechText = "Hello";

                        BuildAnswer(ref innerResponse, ref prompt, speechText, true);
                        break;
                    case "IntrospectionIntent":
                        log.LogLine($"IntrospectionIntent: give polite answer");

                        speechText = "I am fine. Ask your questions";

                        BuildAnswer(ref innerResponse, ref prompt, speechText, true);
                        break;
                    case "ThanksIntent":
                        log.LogLine($"GreetingIntent: say hello");

                        speechText = "You're welcome";

                        BuildAnswer(ref innerResponse, ref prompt, speechText, true);
                        break;
                    case "GetTimeFocusIntent":
                        log.LogLine($"GetTimeFocusIntent: a GetInformation question with a GetTime focus");

                        query = new QueryDto(QueryDto.QueryTypeEnum.GetInformation);
                        query.AddFocus(new GetTimeFocusPredicate());

                        QuestionAnswer(ref innerResponse, ref prompt, log, intentRequest, query);

                        break;
                    case "GetLocationFocusIntent":
                        log.LogLine($"GetLocationFocusIntent: a GetInformation question with a GetLocation focus");

                        query = new QueryDto(QueryDto.QueryTypeEnum.GetInformation);
                        query.AddFocus(new GetLocationFocusPredicate());

                        QuestionAnswer(ref innerResponse, ref prompt, log, intentRequest, query);

                        break;
                    case "GetAgentFocusIntent":
                        log.LogLine($"GetAgentFocusIntent: a GetInformation question with a GetAgent focus");

                        query = new QueryDto(QueryDto.QueryTypeEnum.GetInformation);
                        query.AddFocus(new GetAgentFocusPredicate());

                        QuestionAnswer(ref innerResponse, ref prompt, log, intentRequest, query);

                        break;
                    case "GetThemeFocusIntent":
                        log.LogLine($"GetThemeFocusIntent: a GetInformation question with a GetTheme focus");

                        query = new QueryDto(QueryDto.QueryTypeEnum.GetInformation);
                        query.AddFocus(new GetThemeFocusPredicate());

                        QuestionAnswer(ref innerResponse, ref prompt, log, intentRequest, query);

                        break;
                    case "GetMannerFocusIntent":
                        log.LogLine($"GetMannerFocusIntent: a GetInformation question with a GetManner focus");

                        query = new QueryDto(QueryDto.QueryTypeEnum.GetInformation);
                        query.AddFocus(new GetMannerFocusPredicate());

                        QuestionAnswer(ref innerResponse, ref prompt, log, intentRequest, query);

                        break;
                    case "GetReasonFocusIntent":
                        log.LogLine($"GetReasonFocusIntent: a GetInformation question with a GetReason focus");

                        query = new QueryDto(QueryDto.QueryTypeEnum.GetInformation);
                        query.AddFocus(new GetReasonFocusPredicate());

                        QuestionAnswer(ref innerResponse, ref prompt, log, intentRequest, query);

                        break;
                    case "GetActionFocusIntent":
                        log.LogLine($"GetActionFocusIntent: a GetInformation question with a GetAction focus");

                        query = new QueryDto(QueryDto.QueryTypeEnum.GetInformation);
                        query.AddFocus(new GetActionFocusPredicate());

                        QuestionAnswer(ref innerResponse, ref prompt, log, intentRequest, query);

                        break;
                    case "ValidationIntent":
                        log.LogLine($"ValidationIntent: a YesOrNo question");

                        query = new QueryDto(QueryDto.QueryTypeEnum.YesOrNo);

                        QuestionAnswer(ref innerResponse, ref prompt, log, intentRequest, query);

                        break;
                    case "GetDetailsKnowledgeIntent":
                        log.LogLine($"GetDetailsKnowledgeIntent: get Details about an Entity");

                        query = new QueryDto(QueryDto.QueryTypeEnum.GetKnowledge);
                        query.AddKnowledgeFocus(new GetDetailsKnowledgePredicate());

                        QuestionAnswer(ref innerResponse, ref prompt, log, intentRequest, query);

                        break;
                    case "GetRelationshipKnowledgeIntent":
                        log.LogLine($"GetRelationshipKnowledgeIntent: get Relationship about an Entity");

                        query = new QueryDto(QueryDto.QueryTypeEnum.GetKnowledge);
                        query.AddKnowledgeFocus(new GetRelationshipKnowledgePredicate());

                        QuestionAnswer(ref innerResponse, ref prompt, log, intentRequest, query);

                        break;
                    case "GetParentKnowledgeIntent":
                        log.LogLine($"GetParentKnowledgeIntent: get Parent about an Entity");

                        query = new QueryDto(QueryDto.QueryTypeEnum.GetKnowledge);
                        query.AddKnowledgeFocus(new GetParentKnowledgePredicate());

                        QuestionAnswer(ref innerResponse, ref prompt, log, intentRequest, query);

                        break;
                    case "GetValueKnowledgeIntent":
                        log.LogLine($"GetParentKnowledgeIntent: get Value about an Entity");

                        query = new QueryDto(QueryDto.QueryTypeEnum.GetKnowledge);
                        query.AddKnowledgeFocus(new GetValueKnowledgePredicate());

                        QuestionAnswer(ref innerResponse, ref prompt, log, intentRequest, query);

                        break;
                    default:
                        log.LogLine($"Unknown intent: " + intentRequest.Intent.Name);
                        //pregen answer
                        speechText = "What you said wasn't recognized by the Virtual Suspect model. Try saying something else.";
                        if (options["Detailed feedback"])
                        {
                            speechText += " Unknown intent";
                        }

                        BuildAnswer(ref innerResponse, ref prompt, speechText, false);
                        break;
                }
            }

            response.Response.OutputSpeech = innerResponse;
            response.Response.Reprompt.OutputSpeech = prompt;

            log.LogLine($"Skill Response Object...");
            log.LogLine(JsonConvert.SerializeObject(response));

            return response;
        }

        /// <summary>
        ///  Handles the query conditions, the query to the Virtual Suspect and the answer
        /// </summary>
        /// <param name="innerResponse"></param>
        /// <param name="prompt"></param>
        /// <param name="log"></param>
        /// <param name="intentRequest"></param>
        /// <param name="query"></param>
        /// <returns>string</returns>
        private void QuestionAnswer(ref IOutputSpeech innerResponse, ref IOutputSpeech prompt, ILambdaLogger log,
            IntentRequest intentRequest, QueryDto query)
        {
            string speechText = "";
            if (AddQueryConditions(query, intentRequest, log, out string failLog))
            {
                if ((query.QueryType == QueryDto.QueryTypeEnum.GetKnowledge && query.QueryConditions.Count > 0) ||
                    (query.QueryType == QueryDto.QueryTypeEnum.YesOrNo && query.QueryConditions.Count > 1) ||
                    (query.QueryType == QueryDto.QueryTypeEnum.GetInformation && query.QueryConditions.Count >= 1))
                {
                    QueryResult queryResult = virtual_suspect.Query(query);
                    lastInteraction.UpdateResult(queryResult);
                    log.LogLine($"query results(" + queryResult.Results.Count + "):");
                    //log the results
                    foreach (QueryResult.Result result in queryResult.Results)
                    {
                        log.LogLine($"result dimension: " + result.dimension);
                        log.LogLine($"result cardinality: " + result.cardinality);
                        log.LogLine($"result values:");
                        foreach (IStoryNode storyNode in result.values)
                        {
                            log.LogLine($"value: " + storyNode.Value);
                        }
                    }
                    int resultsCount = CountResults(queryResult);   //improvised function to combine adjacent time slots and count them that way
                    if (queryResult.Query.QueryType == QueryDto.QueryTypeEnum.GetInformation && resultsCount == 0 &&
                        options["Empty answer generation"])
                    {
                        if (queryResult.Query.QueryFocus.Count == 1)
                        {
                            speechText = EmptyAnswerGeneration(queryResult.Query.QueryFocus.ElementAt(0).GetSemanticRole());
                        }
                        else
                        {
                            log.LogLine($"unexpected number of focuses");
                            //pregen answer
                            speechText = "Uhh... I'm not sure what to answer";
                            if (options["Detailed feedback"])
                            {
                                speechText += ". No results and too many focuses";
                            }
                        }
                    }
                    else if (queryResult.Query.QueryType == QueryDto.QueryTypeEnum.GetInformation && resultsCount > 4 &&
                        options["Answer filtering"])
                    {
                        //pregen answer
                        if (queryResult.Results.ElementAt(0).dimension == KnowledgeBaseManager.DimentionsEnum.Location)
                        {
                            speechText += "Many places. ";
                        }
                        else if (queryResult.Results.ElementAt(0).dimension == KnowledgeBaseManager.DimentionsEnum.Time)
                        {
                            speechText += "Many times. ";
                        }
                        else if (queryResult.Results.ElementAt(0).dimension == KnowledgeBaseManager.DimentionsEnum.Action)
                        {
                            speechText += "Many things. ";
                        }
                        speechText += "You'll have to be more specific";
                        if (options["Detailed feedback"])
                        {
                            speechText += ". Too many answers";
                        }
                    }
                    else
                    {
                        speechText = NaturalLanguageGenerator.GenerateAnswer(queryResult);
                    }
                }
                else
                {
                    //pregen answer
                    speechText = "There is not enough information in that question for me to be able to give a meaningful answer";
                    if (options["Detailed feedback"])
                    {
                        speechText += ". No conditions in the query";
                    }
                }
            }
            else
            {
                //pregen answer
                speechText = failLog;
                if (options["Detailed feedback"])
                {
                    speechText += ". Query Conditions failed";
                }
            }
            log.LogLine($"speech text: " + speechText);
            BuildAnswer(ref innerResponse, ref prompt, speechText, true);
        }

        /// <summary>
        ///  Adds the relevant conditions to the query, returns false if a slot is not recognized
        /// </summary>
        /// <param name="query"></param>
        /// <param name="intent"></param>
        /// <param name="log"></param>
        /// <returns>string</returns>
        private bool AddQueryConditions(QueryDto query, IntentRequest intent, ILambdaLogger log, out string failLog)
        {
            Dictionary<string, Slot> intent_slots = intent.Intent.Slots;
            failLog = "";

            lastInteraction.NoAccess();

            if (SlotExists(intent_slots, "subject"))
            {
                if (KnownSlot(intent_slots["subject"]))
                {
                    string subject = TrueSlotValue(intent_slots["subject"]);
                    log.LogLine($"subject slot: " + subject);
                    query.AddCondition(new SubjectEqualConditionPredicate(subject));
                }
                else
                {
                    log.LogLine($"unknown subject, exiting");
                    //pregen answer
                    failLog = "I don't know what you mean by: " + intent_slots["subject"].Value;
                    return false;
                }
            }
            if (SlotExists(intent_slots, "agent"))
            {
                if (KnownSlot(intent_slots["agent"]))
                {
                    string agent = TrueSlotValue(intent_slots["agent"]);
                    log.LogLine($"agent slot: " + agent);
                    if (CheckDirectPronoun(agent))
                    {
                        string prevAgent = lastInteraction.GetEntity(KnowledgeBaseManager.DimentionsEnum.Agent, out bool success);
                        if (success)
                        {
                            log.LogLine($"previous agent: " + prevAgent);
                            List<string> agents = new List<string>() { prevAgent };
                            query.AddCondition(new AgentEqualConditionPredicate(agents));
                        }
                        else
                        {
                            string prevTheme = lastInteraction.GetEntity(KnowledgeBaseManager.DimentionsEnum.Theme, out bool themeSuccess);
                            if (themeSuccess && prevTheme == "Delivery Guy")
                            {
                                log.LogLine($"use Delivery Guy as agent");
                                List<string> agents = new List<string>() { prevTheme };
                                query.AddCondition(new AgentEqualConditionPredicate(agents));
                            }
                            else
                            {
                                log.LogLine($"missing reference");
                                //pregen answer
                                failLog = "I don't know who you're referring to in this context when you say: " + agent;
                                return false;
                            }
                        }

                    }
                    else if (CheckIndirectPronoun(agent))
                    {
                        //indirectAgent = true;
                        if (agent == "alone")
                        {
                            query.AddCondition(new AgentAloneConditionPredicate());
                        }
                        else
                        {
                            query.AddCondition(new AgentExistsConditionPredicate());
                        }
                    }
                    else
                    {
                        List<string> agents = new List<string>() { agent };
                        query.AddCondition(new AgentEqualConditionPredicate(agents));
                    }
                }
                else
                {
                    log.LogLine($"unknown agent, exiting");
                    //pregen answer
                    failLog = "I don't know what you mean by: " + intent_slots["agent"].Value;
                    return false;
                }
            }
            if (SlotExists(intent_slots, "action"))
            {
                if (KnownSlot(intent_slots["action"]))
                {
                    string action = TrueSlotValue(intent_slots["action"]);
                    log.LogLine($"action slot: " + action);
                    if (CheckDirectPronoun(action))
                    {
                        string prevAction = lastInteraction.GetEntity(KnowledgeBaseManager.DimentionsEnum.Action, out bool success);
                        if (success)
                        {
                            log.LogLine($"previous action: " + prevAction);
                            query.AddCondition(new ActionEqualConditionPredicate(prevAction));
                            log.LogLine($"checking for corresponding theme...");
                            string prevTheme = lastInteraction.GetEntity(KnowledgeBaseManager.DimentionsEnum.Theme, out bool themeSuccess);
                            if (themeSuccess)
                            {
                                log.LogLine($"this action also had a theme attached: " + prevTheme);
                                List<string> prevThemes = new List<string>() { prevTheme };
                                query.AddCondition(new ThemeEqualConditionPredicate(prevThemes));
                            }
                            else
                            {
                                log.LogLine($"this action didn't have a theme, continue");
                            }
                        }
                        else
                        {
                            log.LogLine($"missing reference");
                            //pregen answer
                            failLog = "I do not know what you're referring to in this context when you say: " + action;
                            return false;
                        }

                    }
                    else if (CheckIndirectPronoun(action))
                    {
                        //do nothing for now
                    }
                    else
                    {
                        query.AddCondition(new ActionEqualConditionPredicate(action));
                    }
                }
                else
                {
                    log.LogLine($"unknown action, exiting");
                    //pregen answer
                    failLog = "I don't know what you mean by: " + intent_slots["action"].Value;
                    return false;
                }
            }
            if (SlotExists(intent_slots, "location"))
            {
                if (KnownSlot(intent_slots["location"]))
                {
                    string location = TrueSlotValue(intent_slots["location"]);
                    log.LogLine($"location slot: " + location);
                    if (CheckDirectPronoun(location))
                    {
                        string prevLocation = lastInteraction.GetEntity(KnowledgeBaseManager.DimentionsEnum.Location, out bool success);
                        if (success)
                        {
                            log.LogLine($"previous location: " + prevLocation);
                            query.AddCondition(new LocationEqualConditionPredicate(prevLocation));
                        }
                        else
                        {
                            string prevTheme = lastInteraction.GetEntity(KnowledgeBaseManager.DimentionsEnum.Theme, out bool themeSuccess);
                            if (themeSuccess && (prevTheme == "Castle Town" || prevTheme == "Silvermoon City"))
                            {
                                log.LogLine($"use " + prevTheme + " as location");
                                query.AddCondition(new LocationEqualConditionPredicate(prevTheme));
                            }
                            else
                            {
                                log.LogLine($"missing reference");
                                //pregen answer
                                failLog = "I do not know what you're referring to in this context when you say: " + location;
                                return false;
                            }
                        }

                    }
                    else if (CheckIndirectPronoun(location))
                    {
                        //do nothing for now
                    }
                    else
                    {
                        query.AddCondition(new LocationEqualConditionPredicate(location));
                    }
                }
                else
                {
                    log.LogLine($"unknown location, exiting");
                    //pregen answer
                    failLog = "I don't know what you mean by: " + intent_slots["location"].Value;
                    return false;
                }
            }
            if (SlotExists(intent_slots, "reason"))
            {
                if (KnownSlot(intent_slots["reason"]))
                {
                    string reason = TrueSlotValue(intent_slots["reason"]);
                    log.LogLine($"reason slot: " + reason);
                    List<string> reasons = new List<string>() { reason };
                    query.AddCondition(new ReasonEqualConditionPredicate(reasons));
                }
                else
                {
                    log.LogLine($"unknown reason, exiting");
                    //pregen answer
                    failLog = "I don't know what you mean by: " + intent_slots["reason"].Value;
                    return false;
                }
            }
            if (SlotExists(intent_slots, "manner"))
            {
                if (KnownSlot(intent_slots["manner"]))
                {
                    string manner = TrueSlotValue(intent_slots["manner"]);
                    log.LogLine($"manner slot: " + manner);
                    if (CheckDirectPronoun(manner))
                    {
                        string prevManner = lastInteraction.GetEntity(KnowledgeBaseManager.DimentionsEnum.Manner, out bool success);
                        if (success)
                        {
                            log.LogLine($"previous manner: " + prevManner);
                            List<string> manners = new List<string>() { prevManner };
                            query.AddCondition(new MannerEqualConditionPredicate(manners));
                        }
                        else
                        {
                            string prevTheme = lastInteraction.GetEntity(KnowledgeBaseManager.DimentionsEnum.Theme, out bool themeSuccess);
                            if (themeSuccess && prevTheme == "Gun")
                            {
                                log.LogLine($"use Gun as manner");
                                List<string> manners = new List<string>() { prevTheme };
                                query.AddCondition(new MannerEqualConditionPredicate(manners));
                            }
                            else
                            {
                                log.LogLine($"missing reference");
                                //pregen answer
                                failLog = "I do not know what you're referring to in this context when you say: " + manner;
                                return false;
                            }
                        }
                    }
                    else if (CheckIndirectPronoun(manner))
                    {
                        query.AddCondition(new MannerExistsConditionPredicate());
                    }
                    else
                    {
                        List<string> manners = new List<string>() { manner };
                        query.AddCondition(new MannerEqualConditionPredicate(manners));
                    }
                }
                else
                {
                    log.LogLine($"unknown manner, exiting");
                    //pregen answer
                    failLog = "I don't know what you mean by: " + intent_slots["manner"].Value;
                    return false;
                }
            }
            if (SlotExists(intent_slots, "theme"))
            {
                if (KnownSlot(intent_slots["theme"]))
                {
                    string theme = TrueSlotValue(intent_slots["theme"]);
                    log.LogLine($"theme slot: " + theme);
                    if (CheckDirectPronoun(theme))
                    {
                        string prevTheme = lastInteraction.GetEntity(KnowledgeBaseManager.DimentionsEnum.Theme, out bool success);
                        if (success)
                        {
                            log.LogLine($"previous theme: " + prevTheme);
                            List<string> themes = new List<string>() { prevTheme };
                            query.AddCondition(new ThemeEqualConditionPredicate(themes));
                        }
                        else
                        {
                            string prevAgent = lastInteraction.GetEntity(KnowledgeBaseManager.DimentionsEnum.Agent, out bool agentSuccess);
                            string prevLocation = lastInteraction.GetEntity(KnowledgeBaseManager.DimentionsEnum.Location, out bool locationSuccess);
                            string prevManner = lastInteraction.GetEntity(KnowledgeBaseManager.DimentionsEnum.Manner, out bool mannerSuccess);
                            if (agentSuccess || locationSuccess || mannerSuccess)
                            {
                                if (agentSuccess && prevAgent == "Delivery Guy")
                                {
                                    log.LogLine($"use " + prevAgent + "as theme");
                                    List<string> themes = new List<string>() { prevAgent };
                                    query.AddCondition(new ThemeEqualConditionPredicate(themes));
                                }
                                if (locationSuccess && (prevLocation == "Castle Town" || prevLocation == "Silvermoon City"))
                                {
                                    log.LogLine($"use " + prevLocation + "as theme");
                                    List<string> themes = new List<string>() { prevLocation };
                                    query.AddCondition(new ThemeEqualConditionPredicate(themes));
                                }
                                if (mannerSuccess && prevManner == "Gun")
                                {
                                    log.LogLine($"use " + prevManner + "as theme");
                                    List<string> themes = new List<string>() { prevManner };
                                    query.AddCondition(new ThemeEqualConditionPredicate(themes));
                                }
                            }
                            else
                            {
                                log.LogLine($"missing reference");
                                //pregen answer
                                failLog = "I do not know what you're referring to in this context when you say: " + theme;
                                return false;
                            }
                        }
                    }
                    else if (CheckIndirectPronoun(theme))
                    {
                        query.AddCondition(new ThemeExistsConditionPredicate());
                    }
                    else
                    {
                        List<string> themes = new List<string>() { theme };
                        query.AddCondition(new ThemeEqualConditionPredicate(themes));
                    }
                }
                else
                {
                    log.LogLine($"unknown theme, exiting");
                    //pregen answer
                    failLog = "I don't know what you mean by: " + intent_slots["theme"].Value;
                    return false;
                }
            }
            if (CheckTimeConditions(intent_slots, out string timeFormat, out string true_date, out string true_time))
            {
                log.LogLine($"new time slots format");
                string date, date1, date2, time1, time2, time_pronoun, prevTime;
                bool prevAccess, success;
                switch (timeFormat)
                {
                    case "2d2t":
                        log.LogLine($"time: two dates and two times");
                        date1 = TrueSlotValue(intent_slots["date_one"]);
                        log.LogLine($"date_one slot: " + date1);
                        date2 = TrueSlotValue(intent_slots["date_two"]);
                        log.LogLine($"date_two slot: " + date2);
                        time1 = TrueSlotValue(intent_slots["time_one"]);
                        log.LogLine($"time_one slot: " + time1);
                        time2 = TrueSlotValue(intent_slots["time_two"]);
                        log.LogLine($"time_two slot: " + time2);
                        query.AddCondition(new TimeBetweenConditionPredicate(CreateTimeStamp(date1, time1), CreateTimeStamp(date2, time2)));
                        break;
                    case "2d0t":
                        log.LogLine($"time: two dates and no times");
                        date1 = TrueSlotValue(intent_slots["date_one"]);
                        log.LogLine($"date_one slot: " + date1);
                        date2 = TrueSlotValue(intent_slots["date_two"]);
                        log.LogLine($"date_two slot: " + date2);
                        query.AddCondition(new TimeBetweenConditionPredicate(CreateTimeStamp(date1, "00:00:00"), CreateTimeStamp(date2, "23:59:59")));
                        break;
                    case "1d2t":
                        log.LogLine($"time: one date and two times");
                        date1 = TrueSlotValue(intent_slots[true_date]);
                        log.LogLine($"date_one slot: " + date1);
                        time1 = TrueSlotValue(intent_slots["time_one"]);
                        log.LogLine($"time_one slot: " + time1);
                        time2 = TrueSlotValue(intent_slots["time_two"]);
                        log.LogLine($"time_two slot: " + time2);
                        if (time2 == "00:00")
                        {
                            log.LogLine($"second time is midnight, changing 23:59:59 to preserve normal time flow");
                            time2 = "23:59:59";
                        }
                        query.AddCondition(new TimeBetweenConditionPredicate(CreateTimeStamp(date1, time1),
                            CreateTimeStamp(date1, time2)));
                        break;
                    case "1d1t":
                        log.LogLine($"time: one date and one time");
                        date1 = TrueSlotValue(intent_slots[true_date]);
                        log.LogLine($"date_one slot: " + date1);
                        time1 = TrueSlotValue(intent_slots[true_time]);
                        log.LogLine($"time_one slot: " + time1);
                        switch (time1)
                        {
                            //TODO: Check if madrugada makes sense here
                            case "MO":
                                query.AddCondition(new TimeBetweenConditionPredicate(CreateTimeStamp(date1, "06:00:00"), CreateTimeStamp(date1, "11:59:59")));
                                break;
                            case "AF":
                                query.AddCondition(new TimeBetweenConditionPredicate(CreateTimeStamp(date1, "12:00:00"), CreateTimeStamp(date1, "16:59:59")));
                                break;
                            case "EV":
                                query.AddCondition(new TimeBetweenConditionPredicate(CreateTimeStamp(date1, "17:00:00"), CreateTimeStamp(date1, "19:59:59")));
                                break;
                            case "NI":
                                query.AddCondition(new TimeBetweenConditionPredicate(CreateTimeStamp(date1, "20:00:00"), CreateTimeStamp(date1, "23:59:59")));
                                break;
                            default:
                                query.AddCondition(new TimeEqualConditionPredicate(CreateTimeStamp(date1, time1)));
                                break;
                        }
                        break;
                    case "1d0t":
                        log.LogLine($"time: one date and no time");
                        date1 = TrueSlotValue(intent_slots[true_date]);
                        log.LogLine($"date_one slot: " + date1);
                        query.AddCondition(new TimeBetweenConditionPredicate(CreateTimeStamp(date1, "00:00:00"), CreateTimeStamp(date1, "23:59:59")));
                        break;
                    case "0d2t":
                        log.LogLine($"time: no date and two times");
                        time1 = TrueSlotValue(intent_slots["time_one"]);
                        log.LogLine($"time_one slot: " + time1);
                        time2 = TrueSlotValue(intent_slots["time_two"]);
                        log.LogLine($"time_two slot: " + time2);
                        if (time2 == "00:00")
                        {
                            log.LogLine($"second time is midnight, changing 23:59:59 to preserve normal time flow");
                            time2 = "23:59:59";
                        }
                        prevAccess = lastInteraction.CheckAccess();
                        prevTime = lastInteraction.GetEntity(KnowledgeBaseManager.DimentionsEnum.Time, out success);
                        if (success)
                        {
                            lastInteraction.RestoreAccess(prevAccess);
                            string[] split = prevTime.Split('>');
                            if (split.Length == 1 || split[0].Split('T')[0] == split[1].Split('T')[0])
                            {
                                if (split.Length == 1) { date = prevTime.Split('T')[0]; } else { date = split[0].Split('T')[0]; }
                                log.LogLine($"previous date: " + date);
                                query.AddCondition(new TimeBetweenConditionPredicate(date + "T" + CreateTimeStamp(time1), date + "T" + CreateTimeStamp(time2)));
                            }
                            else
                            {
                                log.LogLine($"no single date to fill 'that day' pronoun, exiting");
                                //pregen answer
                                failLog = "I don't know which day you're referring to in this context";
                                return false;
                            }
                        }
                        else
                        {
                            log.LogLine($"missing reference");
                            //pregen answer
                            failLog = "I don't know what day you're referring to in this context";
                            return false;
                        }
                        break;
                    case "0d1t":
                        log.LogLine($"time: no date and one time");
                        time1 = TrueSlotValue(intent_slots[true_time]);
                        log.LogLine($"time_one slot: " + time1);
                        prevAccess = lastInteraction.CheckAccess();
                        prevTime = lastInteraction.GetEntity(KnowledgeBaseManager.DimentionsEnum.Time, out success);
                        if (success)
                        {
                            lastInteraction.RestoreAccess(prevAccess);
                            string[] split = prevTime.Split('>');
                            if (split.Length == 1 || split[0].Split('T')[0] == split[1].Split('T')[0])
                            {
                                if (split.Length == 1) { date1 = prevTime.Split('T')[0]; } else { date1 = split[0].Split('T')[0]; }
                                log.LogLine($"previous date: " + date1);
                                switch (time1)
                                {
                                    case "MO":
                                        query.AddCondition(new TimeBetweenConditionPredicate(date1 + "T" + "06:00:00", date1 + "T" + "11:59:59"));
                                        break;
                                    case "AF":
                                        query.AddCondition(new TimeBetweenConditionPredicate(date1 + "T" + "12:00:00", date1 + "T" + "16:59:59"));
                                        break;
                                    case "EV":
                                        query.AddCondition(new TimeBetweenConditionPredicate(date1 + "T" + "17:00:00", date1 + "T" + "19:59:59"));
                                        break;
                                    case "NI":
                                        query.AddCondition(new TimeBetweenConditionPredicate(date1 + "T" + "20:00:00", date1 + "T" + "23:59:59"));
                                        break;
                                    default:
                                        query.AddCondition(new TimeEqualConditionPredicate(date1 + "T" + CreateTimeStamp(time1)));
                                        break;
                                }
                            }
                            else
                            {
                                log.LogLine($"no single date to fill 'that day' pronoun, exiting");
                                //pregen answer
                                failLog = "I don't know which day you're referring to in this context";
                                return false;
                            }
                        }
                        else
                        {
                            log.LogLine($"missing reference");
                            //pregen answer
                            failLog = "I don't know what day you're referring to in this context";
                            return false;
                        }
                        break;
                    case "p":
                        log.LogLine($"time pronoun");
                        if (KnownSlot(intent_slots["time_pronoun"]))
                        {
                            time_pronoun = TrueSlotValue(intent_slots["time_pronoun"]);
                            log.LogLine($"time_pronoun slot: " + time_pronoun);
                            if (CheckDirectPronoun(time_pronoun))
                            {
                                prevAccess = lastInteraction.CheckAccess();
                                prevTime = lastInteraction.GetEntity(KnowledgeBaseManager.DimentionsEnum.Time, out success);
                                lastInteraction.RestoreAccess(prevAccess);
                                if (success)
                                {
                                    log.LogLine($"previous time: " + prevTime);
                                    if (time_pronoun == "that day")
                                    {
                                        string[] split = prevTime.Split('>');
                                        if (split.Length == 1 || split[0].Split('T')[0] == split[1].Split('T')[0])
                                        {
                                            if (split.Length == 1) { date = prevTime.Split('T')[0]; } else { date = split[0].Split('T')[0]; }
                                            query.AddCondition(new TimeBetweenConditionPredicate(date + "T" + "00:00:00", date + "T" + "23:59:59"));
                                        }
                                        else
                                        {
                                            log.LogLine($"no single date to fill 'that day' pronoun, exiting");
                                            //pregen answer
                                            failLog = "I don't know which day you're referring to in this context";
                                            return false;
                                        }
                                    }
                                    else if (time_pronoun == "that time" || time_pronoun == "then")
                                    {
                                        string[] split = prevTime.Split('>');
                                        if (split.Length == 1)
                                        {
                                            query.AddCondition(new TimeEqualConditionPredicate(prevTime));
                                        }
                                        else
                                        {
                                            query.AddCondition(new TimeBetweenConditionPredicate(split[0], split[1]));
                                        }
                                    }
                                    else
                                    {
                                        log.LogLine($"unknown time pronoun, exiting");
                                        //pregen answer
                                        failLog = "This is embarassing, but I don't know what mean by: " + time_pronoun;
                                        return false;
                                    }
                                }
                                else
                                {
                                    log.LogLine($"missing reference");
                                    //pregen answer
                                    failLog = "I don't know what time you're referring to in this context when you say: " + time_pronoun;
                                    return false;
                                }

                            }
                            else if (CheckIndirectPronoun(time_pronoun))
                            {
                                //do nothing for now
                            }
                            else
                            {
                                log.LogLine($"there is no time pronoun, exiting");
                                //pregen answer
                                failLog = "Uhh... I don't know what mean by: " + time_pronoun;
                                return false;
                            }
                        }
                        else
                        {
                            log.LogLine($"unknown time pronoun, exiting");
                            //pregen answer
                            failLog = "I don't know what mean by: " + intent_slots["time_pronoun"];
                            return false;
                        }
                        break;
                    default:
                        log.LogLine($"WARNING: unexpected date condition!!");
                        //pregen answer
                        failLog = "This is quite unusual, I don't know how to process that date and/or time";
                        return false;
                }
            }

            if (query.QueryType == QueryDto.QueryTypeEnum.GetInformation && 
                (query.QueryConditions.Count == 0 || 
                (query.QueryConditions.Count == 1 && query.QueryConditions.ElementAt(0).GetSemanticRole() == KnowledgeBaseManager.DimentionsEnum.Subject) ||
                (query.QueryConditions.Count <= 2 && lastInteraction.CheckAccess())))
            {
                log.LogLine($"trying out the new contextual functionality, adding previous conditions");
                List<IConditionPredicate> prevConditions = lastInteraction.GetConditions(out bool success, out string contextFailLog, out int contextFailCode);
                if (success)
                {
                    foreach (IConditionPredicate condition in prevConditions)
                    {
                        bool conditionExists = false;
                        foreach (IConditionPredicate queryCondition in query.QueryConditions)
                        {
                            if (condition.GetSemanticRole() == queryCondition.GetSemanticRole() ||
                                condition.GetSemanticRole() == query.QueryFocus.ElementAt(0).GetSemanticRole())
                            {
                                conditionExists = true;
                                break;
                            }
                        }
                        if (!conditionExists)
                        {
                            query.AddCondition(condition);
                        }
                    }
                }
                else
                {
                    log.LogLine($"something went wrong in a contextual question, exiting");
                    log.LogLine($"what went wrong was: " + contextFailLog);
                    //pregen answer
                    failLog = "I don't know what you're referring to in this context";
                    if (options["Detailed feedback"])
                    {
                        switch (contextFailCode)
                        {
                            case 1:
                                failLog += ". I cannot make sense of my own answer";
                                break;
                            case 2:
                                failLog += ". There are multiple things you could be referring to";
                                break;
                            default:
                                failLog += ". Attention, this part of the code should not be reached and something is very wrong";
                                break;
                        }
                    }
                    return false;
                }
            }


            if (query.QueryFocus.ElementAt(0).GetSemanticRole() == KnowledgeBaseManager.DimentionsEnum.Reason &&
                !query.QueryConditions.Any(x => x.GetSemanticRole() == KnowledgeBaseManager.DimentionsEnum.Action))
            {
                query.QueryFocus.Clear();
                query.AddFocus(new GetActionFocusPredicate());
            }

            //Debug
            log.LogLine($"QueryDto type: " + query.QueryType);
            log.LogLine($"QueryDto conditions:");
            foreach (IConditionPredicate condition in query.QueryConditions)
            {
                log.LogLine($"Condition role: " + condition.GetSemanticRole());
                foreach (string value in condition.GetValues())
                {
                    log.LogLine($"value: " + value);
                }
            }
            lastInteraction.NoAccess();
            return true;
        }

        /// <summary>
        ///  A simpler way of verifying the existence of the slot
        /// </summary>
        /// <param name="intent_slots"></param>
        /// <param name="slot_type"></param>
        /// <returns>string</returns>
        private bool SlotExists(Dictionary<string, Slot> intent_slots, string slot_type)
        {
            if (!intent_slots.TryGetValue(slot_type, out Slot value))
            {
                return false;
            }
            else
            {
                return !string.IsNullOrEmpty(value.Value);
            }
        }

        /// <summary>
        ///  Checks if the slot value is recognized
        /// </summary>
        /// <param name="slot"></param>
        /// <returns>string</returns>
        private bool KnownSlot(Slot slot)
        {
            if (options["Slot filtering"])
            {
                if (slot.Resolution != null)
                {
                    return slot.Resolution.Authorities[0].Status.Code == "ER_SUCCESS_MATCH";
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        ///  Returns the original slot value if the word is a recognized synonym, if not, returns the slot value
        /// </summary>
        /// <param name="slot"></param>
        /// <returns>string</returns>
        private string TrueSlotValue(Slot slot)
        {
            if (slot.Resolution != null)
            {
                if (slot.Resolution.Authorities[0].Status.Code == "ER_SUCCESS_MATCH")
                {
                    return slot.Resolution.Authorities[0].Values[0].Value.Name;
                }
                else
                {
                    return slot.Value;
                }
            }
            else
            {
                return slot.Value;
            }
        }

        /// <summary>
        ///  Handles all the different time conditions checks
        /// </summary>
        /// <param name="intentSlots"></param>
        /// <param name="format"></param>
        /// <param name="trueDate"></param>
        /// <param name="trueTime"></param>
        /// <returns>string</returns>
        private bool CheckTimeConditions(Dictionary<string, Slot> intentSlots, out string format, out string trueDate, out string trueTime)
        {
            List<string> slotNames = new List<string>() { "date_one", "date_two", "time_one", "time_two", "time_pronoun" };
            Dictionary<string, bool> slotExistence = new Dictionary<string, bool>()
            {
                {"date_one", false },
                {"date_two", false },
                {"time_one", false },
                {"time_two", false },
                {"time_pronoun", false }
            };
            format = "";
            trueDate = "";
            trueTime = "";
            int dateN = 0;
            int timeN = 0;

            foreach (string slot_name in slotNames)
            {
                slotExistence[slot_name] = SlotExists(intentSlots, slot_name);
            }

            if (slotExistence["date_one"] && slotExistence["date_two"])
            {
                dateN = 2;
            }
            else if (slotExistence["date_one"] || slotExistence["date_two"])
            {
                dateN = 1;
                if (slotExistence["date_one"]) { trueDate = "date_one"; } else { trueDate = "date_two"; }
            }
            if (slotExistence["time_one"] && slotExistence["time_two"])
            {
                timeN = 2;
            }
            else if (slotExistence["time_one"] || slotExistence["time_two"])
            {
                timeN = 1;
                if (slotExistence["time_one"]) { trueTime = "time_one"; } else { trueTime = "time_two"; }
            }

            if (dateN != 0 || timeN != 0)
            {
                format = "" + dateN + "d" + timeN + "t";
                return true;
            }
            else if (slotExistence["time_pronoun"])
            {
                format = "p";
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///  Creates a timestamp the VirtualSuspect uses based on the given date and time
        /// </summary>
        /// <param name="date"></param>
        /// <param name="time"></param>
        /// <returns>string</returns>
        private string CreateTimeStamp(string date, string time)
        {
            string[] date_elements = date.Split('-');

            if (time.Length == 5)
            {
                time += ":00";
            }
            else if (time.Length == 2)
            {
                time += ":00:00";
            }

            return date_elements[2] + "/" + date_elements[1] + "/" + "2016" + "T" + time;
        }

        /// <summary>
        ///  Creates a timestamp the VirtualSuspect uses based on the given date and time
        /// </summary>
        /// <param name="time"></param>
        /// <returns>string</returns>
        private string CreateTimeStamp(string time)
        {
            if (time.Length == 5)
            {
                time += ":00";
            }
            else if (time.Length == 2)
            {
                time += ":00:00";
            }

            return time;
        }


        private int CountResults(QueryResult result)
        {
            //very martelado
            //TODO: revisit and do this properly
            Dictionary<KnowledgeBaseManager.DimentionsEnum, List<QueryResult.Result>> resultsByDimension = new Dictionary<KnowledgeBaseManager.DimentionsEnum, List<QueryResult.Result>>();
            foreach (QueryResult.Result queryResult in result.Results)
            {
                if (!resultsByDimension.ContainsKey(queryResult.dimension))
                {
                    resultsByDimension[queryResult.dimension] = new List<QueryResult.Result>();
                }
                resultsByDimension[queryResult.dimension].Add(queryResult);
            }
            if (resultsByDimension.ContainsKey(KnowledgeBaseManager.DimentionsEnum.Time))
            {
                List<KeyValuePair<DateTime, DateTime>> dateTimeList = new List<KeyValuePair<DateTime, DateTime>>();
                foreach (QueryResult.Result value in resultsByDimension[KnowledgeBaseManager.DimentionsEnum.Time])
                {
                    if (value.values.ElementAt(0).Value.Split('>').Length > 1)
                    {
                        DateTime firstDate = DateTime.ParseExact(value.values.ElementAt(0).Value.Split('>')[0], "dd/MM/yyyyTHH:mm:ss", CultureInfo.InvariantCulture);
                        DateTime secondDate = DateTime.ParseExact(value.values.ElementAt(0).Value.Split('>')[1], "dd/MM/yyyyTHH:mm:ss", CultureInfo.InvariantCulture);

                        dateTimeList.Add(new KeyValuePair<DateTime, DateTime>(firstDate, secondDate));
                    }
                    else
                    {
                        DateTime date = DateTime.ParseExact(value.values.ElementAt(0).Value, "dd/MM/yyyyTHH:mm:ss", CultureInfo.InvariantCulture);

                        dateTimeList.Add(new KeyValuePair<DateTime, DateTime>(date, date));
                    }
                }
                dateTimeList = NaturalLanguageGenerator.SortAndMergeSequenceDateTime(dateTimeList);
                return dateTimeList.Count;
            }
            else
            {
                return result.Results.Count;
            }
        }

        /// <summary>
        ///  Generates a question appropriate answer for empty responses
        /// </summary>
        /// <param name="dimension"></param>
        /// <returns>string</returns>
        private string EmptyAnswerGeneration(KnowledgeBaseManager.DimentionsEnum dimension)
        {
            string answer;
            //pregen answer

            switch (dimension)
            {
                case KnowledgeBaseManager.DimentionsEnum.Action:
                    answer = "Nothing";
                    if (options["Detailed feedback"])
                    {
                        answer = "That didn't happen";
                    }
                    break;
                case KnowledgeBaseManager.DimentionsEnum.Agent:
                    answer = "No one";
                    break;
                case KnowledgeBaseManager.DimentionsEnum.Location:
                    answer = "Nowhere";
                    break;
                case KnowledgeBaseManager.DimentionsEnum.Manner:
                    answer = "In no particular way";
                    break;
                case KnowledgeBaseManager.DimentionsEnum.Reason:
                    answer = "No reason";
                    break;
                case KnowledgeBaseManager.DimentionsEnum.Theme:
                    answer = "Nothing";
                    break;
                case KnowledgeBaseManager.DimentionsEnum.Time:
                    answer = "Never";
                    break;
                default:
                    answer = "Uhh... No idea";
                    break;
            }

            return answer;
        }

        /// <summary>
        ///  Builds the inner response of the answer object with the speechText
        /// </summary>
        /// <param name="innerResponse"></param>
        /// <param name="prompt"></param>
        /// <param name="speechText"></param>
        /// <param name="inCharacter"></param>
        /// <returns>string</returns>
        private void BuildAnswer(ref IOutputSpeech innerResponse, ref IOutputSpeech prompt, string speechText, bool inCharacter)
        {
            if (string.IsNullOrEmpty(speechText))
            {
                //pregen answer
                speechText = "Uhh... I have no idea how to answer that";
                if (options["Detailed feedback"])
                {
                    speechText += ". Empty answer";
                }
            }
            if (inCharacter)
            {
                if (options["Bish mode"])
                {
                    if (speechText == "Yes")
                    {
                        speechText = "Yaaaaas";
                    }
                    speechText += " bish";
                }
                innerResponse = new SsmlOutputSpeech();
                (innerResponse as SsmlOutputSpeech).Ssml = SsmlDecorate(VoiceDecorate(voice, speechText));
                //pregen answer
                string promptText = "There was a problem processing your input, so I have to repeat my previous answer: " + speechText;
                prompt = new SsmlOutputSpeech();
                (prompt as SsmlOutputSpeech).Ssml = SsmlDecorate(VoiceDecorate(voice, promptText));
            }
            else
            {
                innerResponse = new PlainTextOutputSpeech();
                (innerResponse as PlainTextOutputSpeech).Text = speechText;
                prompt = innerResponse;
            }
        }

        /// <summary>
        ///  Wraps the response with the <speak> tag, for SSML responses
        /// </summary>
        /// <param name="speech"></param>
        /// <returns>string</returns>
        private string SsmlDecorate(string speech)
        {
            return "<speak>" + speech + "</speak>";
        }

        /// <summary>
        ///  Wraps the response with the SSML voice of a character, corresponding to the name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="speech"></param>
        /// <returns>string</returns>
        private string VoiceDecorate(string name, string speech)
        {
            return "<voice name='" + name + "'>" + speech + "</voice>";
        }

        /// <summary>
        ///  Toggles an option on or off
        /// </summary>
        /// <param name="intent"></param>
        /// <param name="log"></param>
        /// <returns>string</returns>
        private string ToggleOption(IntentRequest intent, ILambdaLogger log)
        {
            Dictionary<string, Slot> intent_slots = intent.Intent.Slots;
            string answer = "";

            if (SlotExists(intent_slots, "option") && KnownSlot(intent_slots["option"]))
            {
                string option = TrueSlotValue(intent_slots["option"]);
                answer += option + " was " + (options[option] ? "on." : "off.");
                options[option] = !options[option];
                answer += " It is now " + (options[option] ? "on." : "off.");
                log.LogLine($"toggled " + option + " option");
            }
            else
            {
                answer += "That's not a valid option.";
                log.LogLine($"invalid option");
            }

            return answer;
        }

        /// <summary>
        ///  Turns an option on or off
        /// </summary>
        /// <param name="intent"></param>
        /// <param name="log"></param>
        /// <param name="mode"></param>
        /// <returns>string</returns>
        private string TurnOption(IntentRequest intent, ILambdaLogger log, bool mode)
        {
            Dictionary<string, Slot> intent_slots = intent.Intent.Slots;
            string answer = "";

            if (SlotExists(intent_slots, "option") && KnownSlot(intent_slots["option"]))
            {
                string option = TrueSlotValue(intent_slots["option"]);
                if (options[option] == mode)
                {
                    answer += option + " was already " + (mode ? "on." : "off.");
                }
                else
                {
                    options[option] = mode;
                    answer += option + " is now " + (mode ? "on." : "off.");
                }
                log.LogLine($"toggled " + option + (mode ? " on" : " off"));
            }
            else
            {
                answer += "That's not a valid option.";
                log.LogLine($"invalid option");
            }

            return answer;
        }

        /// <summary>
        ///  Resets the options
        /// </summary>
        /// <returns>void</returns>
        private void ResetOptions()
        {
            options["Bish mode"] = false;
            options["Slot filtering"] = true;
            options["Answer filtering"] = true;
            options["Empty answer generation"] = true;
            options["Detailed feedback"] = false;
        }

        /// <summary>
        ///  Checks whether a word is a direct pronoun
        /// </summary>
        /// <param name="pronoun"></param>
        /// <returns>bool</returns>
        private bool CheckDirectPronoun(string pronoun)
        {
            List<string> directPronouns = new List<string>()
            {
                "there", "him", "it", "that day", "that time", "then", "that place", "that", "its", "he"
            };

            return directPronouns.Contains(pronoun);
        }

        /// <summary>
        ///  Checks whether a word is a indirect pronoun
        /// </summary>
        /// <param name="pronoun"></param>
        /// <returns>bool</returns>
        private bool CheckIndirectPronoun(string pronoun)
        {
            List<string> indirectPronouns = new List<string>()
            {
                "something", "someone", "anything", "anyone", "alone"
            };

            return indirectPronouns.Contains(pronoun);
        }

        private class Context
        {
            private QueryResult result;

            private bool accessed = false;

            public bool CheckAccess()
            {
                return accessed;
            }

            public void NoAccess()
            {
                accessed = false;
            }

            public void RestoreAccess(bool prevAccess)
            {
                accessed = prevAccess;
            }

            public void UpdateResult(QueryResult res)
            {
                this.result = res;
            }

            public string GetEntity(KnowledgeBaseManager.DimentionsEnum dimension, out bool success)
            {
                string entity = "";
                success = false;

                if (this.result.Results.Count == 1 &&
                    this.result.Results.ElementAt(0).dimension == dimension)
                {
                    entity = this.result.Results.ElementAt(0).values.ElementAt(0).Value;
                    success = true;
                }
                else if (this.result.Results.Count == 1 &&
                    this.result.Results.ElementAt(0).dimension == KnowledgeBaseManager.DimentionsEnum.Manner &&
                    dimension == KnowledgeBaseManager.DimentionsEnum.Theme &&
                    this.result.Results.ElementAt(0).values.ElementAt(0).Value == "Gun")
                {
                    entity = this.result.Results.ElementAt(0).values.ElementAt(0).Value;
                    success = true;
                }
                else if (this.result.Results.Count == 1 &&
                    this.result.Results.ElementAt(0).dimension == KnowledgeBaseManager.DimentionsEnum.Action &&
                    dimension == KnowledgeBaseManager.DimentionsEnum.Theme &&
                    this.result.Results.ElementAt(0).values.Count == 2)
                {
                    entity = this.result.Results.ElementAt(0).values.ElementAt(1).Value;
                    success = true;
                }
                else
                {
                    foreach (IConditionPredicate condition in this.result.Query.QueryConditions)
                    {
                        if (condition.GetSemanticRole() == dimension)
                        {
                            entity = condition.GetValues().ElementAt(0);
                            success = true;
                            break;
                        }
                    }
                }
                accessed = true;
                return entity;
            }

            public List<IConditionPredicate> GetConditions(out bool success, out string failLog, out int failCode)
            {
                List<IConditionPredicate> conditions = new List<IConditionPredicate>();
                success = true;
                failLog = "";
                failCode = 0;

                foreach (IConditionPredicate condition in this.result.Query.QueryConditions)
                {
                    conditions.Add(condition);
                }

                if (this.result.Results.Count == 1)
                {
                    List<string> values = new List<string>();
                    foreach (IStoryNode entity in this.result.Results.ElementAt(0).values)
                    {
                        values.Add(entity.Value);
                    }
                    if (this.result.Results.ElementAt(0).dimension == KnowledgeBaseManager.DimentionsEnum.Action)
                    {
                        conditions.Add(new ActionEqualConditionPredicate(values.ElementAt(0)));
                    }
                    else if (this.result.Results.ElementAt(0).dimension == KnowledgeBaseManager.DimentionsEnum.Agent)
                    {
                        conditions.Add(new AgentEqualConditionPredicate(values));
                    }
                    else if (this.result.Results.ElementAt(0).dimension == KnowledgeBaseManager.DimentionsEnum.Location)
                    {
                        conditions.Add(new LocationEqualConditionPredicate(values.ElementAt(0)));
                    }
                    else if (this.result.Results.ElementAt(0).dimension == KnowledgeBaseManager.DimentionsEnum.Manner)
                    {
                        conditions.Add(new MannerEqualConditionPredicate(values));
                    }
                    else if (this.result.Results.ElementAt(0).dimension == KnowledgeBaseManager.DimentionsEnum.Reason)
                    {
                        conditions.Add(new ReasonEqualConditionPredicate(values));
                    }
                    else if (this.result.Results.ElementAt(0).dimension == KnowledgeBaseManager.DimentionsEnum.Theme)
                    {
                        conditions.Add(new ThemeEqualConditionPredicate(values));
                    }
                    else if (this.result.Results.ElementAt(0).dimension == KnowledgeBaseManager.DimentionsEnum.Time)
                    {
                        if (values.ElementAt(0).Split('>').Length > 1)
                        {
                            conditions.Add(new TimeBetweenConditionPredicate(values.ElementAt(0).Split('>')[0], values.ElementAt(0).Split('>')[1]));
                        }
                        else
                        {
                            conditions.Add(new TimeEqualConditionPredicate(values.ElementAt(0)));
                        }
                    }
                    else
                    {
                        success = false;
                        failLog = "There was only one result but it could not be mapped to any of the known dimensions";
                        failCode = 1;
                    }
                }
                else if (this.result.Results.Count > 1)
                {
                    success = false;
                    failLog = "There was more than one result and therefore it cannot be mapped to one specific context";
                    failCode = 2;
                }

                accessed = true;
                return conditions;
            }
        }
    }
}
