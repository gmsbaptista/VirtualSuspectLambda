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

                knowledge_base = KnowledgeBaseParser.parseFromFile("RobberyStory.xml");
                virtual_suspect = new VirtualSuspectQuestionAnswer(knowledge_base);
                lastInteraction = new Context();

                log.LogLine($"first entity in kb: " + knowledge_base.Entities[0].Value);
                log.LogLine($"first action in kb: " + knowledge_base.Actions[0].Action);
                log.LogLine($"first event in kb: " + knowledge_base.Events[0].Action.Action);

                string firstText = "Welcome to the Virtual Suspect demo. ";
                string suspectInformation = "Your suspect's name is Peter. ";
                string lastText = "You can ask him simple questions.";
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

                        speechText = "I do not know what you are talking about";
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
                    case "GetTimeContextualIntent":
                        log.LogLine($"GetTimeFocusIntent: a GetInformation question with a GetTime focus");

                        query = new QueryDto(QueryDto.QueryTypeEnum.GetInformation);
                        query.AddFocus(new GetTimeFocusPredicate());

                        QuestionAnswer(ref innerResponse, ref prompt, log, intentRequest, query);

                        break;
                    case "GetLocationFocusIntent":
                    case "GetLocationContextualIntent":
                        log.LogLine($"GetLocationFocusIntent: a GetInformation question with a GetLocation focus");

                        query = new QueryDto(QueryDto.QueryTypeEnum.GetInformation);
                        query.AddFocus(new GetLocationFocusPredicate());

                        QuestionAnswer(ref innerResponse, ref prompt, log, intentRequest, query);

                        break;
                    case "GetAgentFocusIntent":
                    case "GetAgentContextualIntent":
                        log.LogLine($"GetAgentFocusIntent: a GetInformation question with a GetAgent focus");

                        query = new QueryDto(QueryDto.QueryTypeEnum.GetInformation);
                        query.AddFocus(new GetAgentFocusPredicate());

                        QuestionAnswer(ref innerResponse, ref prompt, log, intentRequest, query);

                        break;
                    case "GetThemeFocusIntent":
                    case "GetThemeContextualIntent":
                        log.LogLine($"GetThemeFocusIntent: a GetInformation question with a GetTheme focus");

                        query = new QueryDto(QueryDto.QueryTypeEnum.GetInformation);
                        query.AddFocus(new GetThemeFocusPredicate());

                        QuestionAnswer(ref innerResponse, ref prompt, log, intentRequest, query);

                        break;
                    case "GetReasonFocusIntent":
                    case "GetReasonContextualIntent":
                        log.LogLine($"GetReasonFocusIntent: a GetInformation question with a GetReason focus");

                        query = new QueryDto(QueryDto.QueryTypeEnum.GetInformation);
                        query.AddFocus(new GetReasonFocusPredicate());

                        QuestionAnswer(ref innerResponse, ref prompt, log, intentRequest, query);

                        break;
                    case "ValidationIntent":
                        log.LogLine($"ValidationIntent: a YesOrNo question");

                        query = new QueryDto(QueryDto.QueryTypeEnum.YesOrNo);

                        QuestionAnswer(ref innerResponse, ref prompt, log, intentRequest, query);

                        break;
                    default:
                        log.LogLine($"Unknown intent: " + intentRequest.Intent.Name);

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
            string speechText;
            if (AddQueryConditions(query, intentRequest, log))
            {
                if (query.QueryConditions.Count > 1)
                {
                    QueryResult queryResult = virtual_suspect.Query(query);
                    lastInteraction.UpdateResult(queryResult);
                    log.LogLine($"query results(" + queryResult.Results.Count + "):");
                    foreach (QueryResult.Result result in queryResult.Results)
                    {
                        log.LogLine($"result dimension: " + result.dimension);
                        log.LogLine($"result cardinality: " + result.cardinality);
                        log.LogLine($"result values:");
                        foreach (EntityNode entity in result.values)
                        {
                            log.LogLine($"value: " + entity.Value);
                        }
                    }
                    if (queryResult.Query.QueryType != QueryDto.QueryTypeEnum.YesOrNo && queryResult.Results.Count == 0 && 
                        options["Empty answer generation"])
                    {
                        if (queryResult.Query.QueryFocus.Count == 1)
                        {
                            speechText = EmptyAnswerGeneration(queryResult.Query.QueryFocus.ElementAt(0).GetSemanticRole());
                        }
                        else
                        {
                            log.LogLine($"unexpected number of focuses");
                            speechText = "I'm not sure what to answer";
                            if (options["Detailed feedback"])
                            {
                                speechText += ". No results and too many focuses";
                            }
                        }
                    }
                    else if (queryResult.Query.QueryType != QueryDto.QueryTypeEnum.YesOrNo && queryResult.Results.Count > 1 && 
                        options["Answer filtering"])
                    {
                        speechText = "You'll have to be more specific";
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
                    speechText = "I'm not sure what you expect me to say";
                    if (options["Detailed feedback"])
                    {
                        speechText += ". No conditions in the query";
                    }
                }
            }
            else
            {
                speechText = "I don't quite understand what you said";
                if (options["Detailed feedback"])
                {
                    speechText += ". Unknown slot values";
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
        private bool AddQueryConditions(QueryDto query, IntentRequest intent, ILambdaLogger log)
        {
            Dictionary<string, Slot> intent_slots = intent.Intent.Slots;

            if (CheckContextualIntent(intent.Intent.Name))
            {
                List<IConditionPredicate> prevConditions = lastInteraction.GetConditions(out bool success);
                if (success)
                {
                    foreach (IConditionPredicate condition in prevConditions)
                    {
                        query.AddCondition(condition);
                    }
                }
                else
                {
                    log.LogLine($"something went wrong in a contextual question, exiting");
                    return false;
                }
            }
            else
            {
                if (SlotExists(intent_slots, "subject"))
                {
                    if (KnownSlot(intent_slots["subject"]))
                    {
                        if (TrueSlotValue(intent_slots["subject"]) == "Peter Barker")
                        {
                            log.LogLine($"subject slot: Peter Barker");
                            List<string> agents = new List<string>() { "Peter Barker" };
                            query.AddCondition(new AgentEqualConditionPredicate(agents));
                        }
                        else
                        {
                            log.LogLine($"subject slot: unexpected subject");
                        }
                    }
                    else
                    {
                        log.LogLine($"unknown subject, exiting");
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
                            string prevAgent = lastInteraction.GetAgent(out bool success);
                            if (success)
                            {
                                List<string> agents = new List<string>() { prevAgent };
                                query.AddCondition(new AgentEqualConditionPredicate(agents));
                            }
                            else
                            {
                                log.LogLine($"missing reference");
                                return false;
                            }

                        }
                        else if (CheckIndirectPronoun(agent))
                        {
                            query.AddCondition(new AgentExistsConditionPredicate());
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
                        return false;
                    }
                }
                if (SlotExists(intent_slots, "action"))
                {
                    if (KnownSlot(intent_slots["action"]))
                    {
                        string action = TrueSlotValue(intent_slots["action"]);
                        log.LogLine($"action slot: " + action);
                        query.AddCondition(new ActionEqualConditionPredicate(action));
                    }
                    else
                    {
                        log.LogLine($"unknown action, exiting");
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
                                query.AddCondition(new LocationEqualConditionPredicate(prevLocation));
                            }
                            else
                            {
                                log.LogLine($"missing reference");
                                return false;
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
                                List<string> themes = new List<string>() { prevTheme };
                                query.AddCondition(new ThemeEqualConditionPredicate(themes));
                            }
                            else
                            {
                                log.LogLine($"missing reference");
                                return false;
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
                        return false;
                    }
                }
                if (SlotExists(intent_slots, "date_one") || SlotExists(intent_slots, "date_two")
                    || SlotExists(intent_slots, "time_one") || SlotExists(intent_slots, "time_two")) //Time slots
                {
                    log.LogLine($"time slots");
                    if (SlotExists(intent_slots, "date_one") && SlotExists(intent_slots, "date_two")) //Two dates
                    {
                        log.LogLine($"time: two dates");
                        string date1 = TrueSlotValue(intent_slots["date_one"]);
                        log.LogLine($"date_one slot: " + date1);
                        string date2 = TrueSlotValue(intent_slots["date_two"]);
                        log.LogLine($"date_two slot: " + date2);
                        query.AddCondition(new TimeBetweenConditionPredicate(CreateTimeStamp(date1, "00:00:00"),
                            CreateTimeStamp(date2, "23:59:59")));
                    }
                    else if (SlotExists(intent_slots, "date_one") && !SlotExists(intent_slots, "time_one")) //One date and no time
                    {
                        log.LogLine($"time: one date and no time");
                        string date1 = TrueSlotValue(intent_slots["date_one"]);
                        log.LogLine($"date_one slot: " + date1);
                        query.AddCondition(new TimeBetweenConditionPredicate(CreateTimeStamp(date1, "00:00:00"),
                            CreateTimeStamp(date1, "23:59:59")));
                    }
                    else if (SlotExists(intent_slots, "date_one") &&
                        SlotExists(intent_slots, "time_one") && SlotExists(intent_slots, "time_two")) //One date and two times
                    {
                        log.LogLine($"time: one date and two times");
                        string date1 = TrueSlotValue(intent_slots["date_one"]);
                        log.LogLine($"date_one slot: " + date1);
                        string time1 = TrueSlotValue(intent_slots["time_one"]);
                        log.LogLine($"time_one slot: " + time1);
                        string time2 = TrueSlotValue(intent_slots["time_two"]);
                        log.LogLine($"time_two slot: " + time2);
                        query.AddCondition(new TimeBetweenConditionPredicate(CreateTimeStamp(date1, time1),
                            CreateTimeStamp(date1, time2)));
                    }
                    else if (SlotExists(intent_slots, "date_one") &&
                        SlotExists(intent_slots, "time_one") && !SlotExists(intent_slots, "time_two")) //One date and one time
                    {
                        log.LogLine($"time: one date and one time");
                        string date1 = TrueSlotValue(intent_slots["date_one"]);
                        log.LogLine($"date_one slot: " + date1);
                        string time1 = TrueSlotValue(intent_slots["time_one"]);
                        log.LogLine($"time_one slot: " + time1);
                        if (time1 == "MO" || time1 == "NI" || time1 == "AF" || time1 == "EV")
                        {
                            //TODO: Check if madrugada makes sense here
                            if (time1 == "MO")
                            {
                                query.AddCondition(new TimeBetweenConditionPredicate(CreateTimeStamp(date1, "06:00:00"),
                                    CreateTimeStamp(date1, "11:59:59")));
                            }
                            else if (time1 == "AF")
                            {
                                query.AddCondition(new TimeBetweenConditionPredicate(CreateTimeStamp(date1, "12:00:00"),
                                    CreateTimeStamp(date1, "16:59:59")));
                            }
                            else if (time1 == "EV")
                            {
                                query.AddCondition(new TimeBetweenConditionPredicate(CreateTimeStamp(date1, "17:00:00"),
                                    CreateTimeStamp(date1, "19:59:59")));
                            }
                            else if (time1 == "NI")
                            {
                                query.AddCondition(new TimeBetweenConditionPredicate(CreateTimeStamp(date1, "20:00:00"),
                                    CreateTimeStamp(date1, "23:59:59")));
                            }
                        }
                        else
                        {
                            query.AddCondition(new TimeEqualConditionPredicate(CreateTimeStamp(date1, time1)));
                        }
                    }
                    else
                    {
                        log.LogLine($"WARNING: unexpected date condition!!");
                        return false;
                    }
                }
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
        ///  Creates a timestamp the VirtualSuspect uses based on the given date and time
        /// </summary>
        /// <param name="date"></param>
        /// <param name="time"></param>
        /// <returns>string</returns>
        private string CreateTimeStamp (string date, string time)
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
        ///  Generates a question appropriate answer for empty responses
        /// </summary>
        /// <param name="dimension"></param>
        /// <returns>string</returns>
        private string EmptyAnswerGeneration (KnowledgeBaseManager.DimentionsEnum dimension)
        {
            string answer;

            switch (dimension)
            {
                case KnowledgeBaseManager.DimentionsEnum.Agent:
                    answer = "No one";
                    break;
                case KnowledgeBaseManager.DimentionsEnum.Location:
                    answer = "Nowhere";
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
                    answer = "No idea";
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
        private void BuildAnswer (ref IOutputSpeech innerResponse, ref IOutputSpeech prompt, string speechText, bool inCharacter)
        {
            if (string.IsNullOrEmpty(speechText))
            {
                speechText = "I don't know";
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
            }
            else
            {
                innerResponse = new PlainTextOutputSpeech();
                (innerResponse as PlainTextOutputSpeech).Text = speechText;
            }
            prompt = innerResponse;
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
        private string ToggleOption (IntentRequest intent, ILambdaLogger log)
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
        private string TurnOption (IntentRequest intent, ILambdaLogger log, bool mode)
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
                log.LogLine($"toggled " + option + (mode? " on" : " off"));
            }
            else
            {
                answer += "That's not a valid option.";
                log.LogLine($"invalid option");
            }

            return answer;
        }

        /// <summary>
        ///  Checks whether a word is a direct pronoun
        /// </summary>
        /// <param name="pronoun"></param>
        /// <returns>bool</returns>
        private bool CheckDirectPronoun (string pronoun)
        {
            List<string> directPronouns = new List<string>()
            {
                "there", "him", "it"
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
                "something", "someone", "anything", "anyone"
            };

            return indirectPronouns.Contains(pronoun);
        }

        /// <summary>
        ///  Checks whether an intent is contextual
        /// </summary>
        /// <param name="intentName"></param>
        /// <returns>bool</returns>
        private bool CheckContextualIntent(string intentName)
        {
            List<string> contextualIntents = new List<string>()
            {
                "GetTimeContextualIntent", "GetLocationContextualIntent", "GetAgentContextualIntent", "GetThemeContextualIntent", "GetReasonContextualIntent"
            };

            return contextualIntents.Contains(intentName);
        }

        private class Context
        {
            private QueryResult result;

            public void UpdateResult (QueryResult res)
            {
                this.result = res;
            }

            public string GetAgent (out bool success)
            {
                string agent = "";
                success = false;

                if (this.result.Results.Count == 1 &&
                    this.result.Results.ElementAt(0).dimension == KnowledgeBaseManager.DimentionsEnum.Agent)
                {
                    agent = this.result.Results.ElementAt(0).values.ElementAt(0).Value;
                    success = true;
                }
                else
                {
                    foreach (IConditionPredicate condition in this.result.Query.QueryConditions)
                    {
                        if (condition.GetSemanticRole() == KnowledgeBaseManager.DimentionsEnum.Agent && 
                            !condition.GetValues().Contains("Peter Barker"))
                        {
                            agent = condition.GetValues().ElementAt(0);
                            success = true;
                            break;
                        }
                    }
                }

                return agent;
            }

            public string GetEntity (KnowledgeBaseManager.DimentionsEnum dimension, out bool success)
            {
                string entity = "";
                success = false;

                if (this.result.Results.Count == 1 &&
                    this.result.Results.ElementAt(0).dimension == dimension)
                {
                    entity = this.result.Results.ElementAt(0).values.ElementAt(0).Value;
                    success = true;
                }
                else
                {
                    foreach (IConditionPredicate condition in this.result.Query.QueryConditions)
                    {
                        if (condition.GetSemanticRole() == dimension &&
                            !condition.GetValues().Contains("Peter Barker"))
                        {
                            entity = condition.GetValues().ElementAt(0);
                            success = true;
                            break;
                        }
                    }
                }
                return entity;
            }

            public List<IConditionPredicate> GetConditions (out bool success)
            {
                List<IConditionPredicate> conditions = new List<IConditionPredicate>();
                success = true;

                foreach (IConditionPredicate condition in this.result.Query.QueryConditions)
                {
                    conditions.Add(condition);
                }

                if (this.result.Results.Count == 1)
                {
                    List<string> values = new List<string>();
                    foreach (EntityNode entity in this.result.Results.ElementAt(0).values)
                    {
                        values.Add(entity.Value);
                    }
                    if (this.result.Results.ElementAt(0).dimension == KnowledgeBaseManager.DimentionsEnum.Agent)
                    {
                        conditions.Add(new AgentEqualConditionPredicate(values));
                    }
                    else if (this.result.Results.ElementAt(0).dimension == KnowledgeBaseManager.DimentionsEnum.Location)
                    {
                        conditions.Add(new LocationEqualConditionPredicate(values.ElementAt(0)));
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
                        conditions.Add(new TimeBetweenConditionPredicate(values.ElementAt(0).Split('>')[0], values.ElementAt(0).Split('>')[1]));
                    }
                    else
                    {
                        success = false;
                    }
                }
                else if (this.result.Results.Count > 1)
                {
                    success = false;
                }

                return conditions;
            }
        }
    }
}
