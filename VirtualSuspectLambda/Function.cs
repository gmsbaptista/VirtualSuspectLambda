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
                QueryResult queryResult;
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

                        BuildAnswer(ref innerResponse, ref prompt, speechText, true);
                        break;
                    case "GetTimeFocusIntent":
                        log.LogLine($"GetTimeFocusIntent: a GetInformation question with a GetTime focus");

                        query = new QueryDto(QueryDto.QueryTypeEnum.GetInformation);
                        query.AddFocus(new GetTimeFocusPredicate());

                        AddQueryConditions(query, intentRequest, log);

                        queryResult = virtual_suspect.Query(query);
                        speechText = NaturalLanguageGenerator.GenerateAnswer(queryResult);

                        BuildAnswer(ref innerResponse, ref prompt, speechText, true);
                        break;
                    case "GetLocationFocusIntent":
                        log.LogLine($"GetLocationFocusIntent: a GetInformation question with a GetLocation focus");

                        query = new QueryDto(QueryDto.QueryTypeEnum.GetInformation);
                        query.AddFocus(new GetLocationFocusPredicate());

                        AddQueryConditions(query, intentRequest, log);

                        queryResult = virtual_suspect.Query(query);
                        speechText = NaturalLanguageGenerator.GenerateAnswer(queryResult);

                        BuildAnswer(ref innerResponse, ref prompt, speechText, true);
                        break;
                    case "GetAgentFocusIntent":
                        log.LogLine($"GetAgentFocusIntent: a GetInformation question with a GetAgent focus");

                        query = new QueryDto(QueryDto.QueryTypeEnum.GetInformation);
                        query.AddFocus(new GetAgentFocusPredicate());

                        AddQueryConditions(query, intentRequest, log);

                        queryResult = virtual_suspect.Query(query);
                        speechText = NaturalLanguageGenerator.GenerateAnswer(queryResult);

                        BuildAnswer(ref innerResponse, ref prompt, speechText, true);
                        break;
                    case "GetThemeFocusIntent":
                        log.LogLine($"GetThemeFocusIntent: a GetInformation question with a GetTheme focus");

                        query = new QueryDto(QueryDto.QueryTypeEnum.GetInformation);
                        query.AddFocus(new GetThemeFocusPredicate());

                        AddQueryConditions(query, intentRequest, log);

                        queryResult = virtual_suspect.Query(query);
                        speechText = NaturalLanguageGenerator.GenerateAnswer(queryResult);

                        BuildAnswer(ref innerResponse, ref prompt, speechText, true);
                        break;
                    case "GetReasonFocusIntent":
                        log.LogLine($"GetReasonFocusIntent: a GetInformation question with a GetReason focus");

                        query = new QueryDto(QueryDto.QueryTypeEnum.GetInformation);
                        query.AddFocus(new GetReasonFocusPredicate());

                        AddQueryConditions(query, intentRequest, log);

                        queryResult = virtual_suspect.Query(query);
                        speechText = NaturalLanguageGenerator.GenerateAnswer(queryResult);

                        BuildAnswer(ref innerResponse, ref prompt, speechText, true);
                        break;
                    case "ValidationIntent":
                        log.LogLine($"ValidationIntent: a YesOrNo question");

                        query = new QueryDto(QueryDto.QueryTypeEnum.YesOrNo);

                        AddQueryConditions(query, intentRequest, log);

                        queryResult = virtual_suspect.Query(query);
                        speechText = NaturalLanguageGenerator.GenerateAnswer(queryResult);

                        BuildAnswer(ref innerResponse, ref prompt, speechText, true);
                        break;
                    default:
                        log.LogLine($"Unknown intent: " + intentRequest.Intent.Name);

                        speechText = "What you said wasn't recognized by the Virtual Suspect model. Try saying something else.";

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
        ///  Adds the relevant conditions to the query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="intent"></param>
        /// <param name="log"></param>
        /// <returns>string</returns>
        private void AddQueryConditions(QueryDto query, IntentRequest intent, ILambdaLogger log)
        {
            Dictionary<string, Slot> intent_slots = intent.Intent.Slots;

            if (SlotExists(intent_slots, "subject"))
            {
                List<string> agents = new List<string>() { "Peter Barker" };
                query.AddCondition(new AgentEqualConditionPredicate(agents));
            }
            if (SlotExists(intent_slots, "agent"))
            {
                string agent = TrueSlotValue(intent_slots["agent"]);
                log.LogLine($"agent slot: " + agent);
                List<string> agents = new List<string>() { agent };
                query.AddCondition(new AgentEqualConditionPredicate(agents));
            }
            if (SlotExists(intent_slots, "action"))
            {
                string action = TrueSlotValue(intent_slots["action"]);
                log.LogLine($"action slot: " + action);
                query.AddCondition(new ActionEqualConditionPredicate(action));
            }
            if (SlotExists(intent_slots, "location"))
            {
                string location = TrueSlotValue(intent_slots["location"]);
                log.LogLine($"location slot: " + location);
                query.AddCondition(new LocationEqualConditionPredicate(location));
            }
            if (SlotExists(intent_slots, "reason"))
            {
                string reason = TrueSlotValue(intent_slots["reason"]);
                log.LogLine($"reason slot: " + reason);
                List<string> reasons = new List<string>() { reason };
                query.AddCondition(new ReasonEqualConditionPredicate(reasons));
            }
            if (SlotExists(intent_slots, "theme"))
            {
                string theme = TrueSlotValue(intent_slots["theme"]);
                log.LogLine($"theme slot: " + theme);
                List<string> themes = new List<string>() { theme };
                query.AddCondition(new ThemeEqualConditionPredicate(themes));
            }
            if (SlotExists(intent_slots, "date_one") || SlotExists(intent_slots, "date_two") 
                || SlotExists(intent_slots, "time_one") || SlotExists(intent_slots, "time_two")) //Time slots
            {
                if (SlotExists(intent_slots, "date_one") && SlotExists(intent_slots, "date_two")) //Two dates
                {
                    string date1 = TrueSlotValue(intent_slots["date_one"]);
                    log.LogLine($"date_one slot: " + date1);
                    string date2 = TrueSlotValue(intent_slots["date_two"]);
                    log.LogLine($"date_two slot: " + date2);
                    query.AddCondition(new TimeBetweenConditionPredicate(CreateTimeStamp(date1, "00:00:00"), 
                        CreateTimeStamp(date2, "23:59:00")));
                }
                else if (SlotExists(intent_slots, "date_one") && !SlotExists(intent_slots, "time_one")) //One date and no time
                {
                    string date1 = TrueSlotValue(intent_slots["date_one"]);
                    log.LogLine($"date_one slot: " + date1);
                    query.AddCondition(new TimeBetweenConditionPredicate(CreateTimeStamp(date1, "00:00:00"),
                        CreateTimeStamp(date1, "23:59:00")));
                }
                else if (SlotExists(intent_slots, "date_one") && 
                    SlotExists(intent_slots, "time_one") && SlotExists(intent_slots,"time_two")) //One date and two times
                {
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
                    string date1 = TrueSlotValue(intent_slots["date_one"]);
                    log.LogLine($"date_one slot: " + date1);
                    string time1 = TrueSlotValue(intent_slots["time_one"]);
                    log.LogLine($"time_one slot: " + time1);
                    query.AddCondition(new TimeEqualConditionPredicate(CreateTimeStamp(date1, time1)));
                }
                else
                {
                    log.LogLine($"WARNING: unexpected date condition!!");
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
                speechText = "I don't know.";
            }
            if (inCharacter)
            {
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
    }
}
