using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.Query;
using VirtualSuspect.KnowledgeBase;

namespace VirtualSuspect.Handler
{

    class ModifyKnowledgeBaseHandler : IPreHandler
    {

        private VirtualSuspectQuestionAnswer virtualSuspect;

        public ModifyKnowledgeBaseHandler(VirtualSuspectQuestionAnswer virtualSuspect)
        {

            this.virtualSuspect = virtualSuspect;

        }

        public QueryDto Modify(QueryDto query)
        {

            switch (virtualSuspect.Strategy)
            {
                case VirtualSuspectQuestionAnswer.LieStrategy.AdjustEntity:
                    //TODO:
                    break;

                case VirtualSuspectQuestionAnswer.LieStrategy.AdjustEvent:
                    //TODO:
                    break;

                case VirtualSuspectQuestionAnswer.LieStrategy.Improvise:
                    bool backupToOriginal;
                    //Get a list of incriminatory and active events
                    IEnumerable<EventNode> nodes = virtualSuspect.FilterEvents(query.QueryConditions, out backupToOriginal).Where(x => x.Incriminatory > 0);

                    foreach (EventNode eventNode in nodes)
                    {
                        Dictionary<KnowledgeBaseManager.DimentionsEnum, List<EntityNode>> NonAddedEntities;

                        EventNode duplicateNode = DuplicateEvent(eventNode, virtualSuspect.KnowledgeBase.getNextNodeId("event"), out NonAddedEntities, true);

                        //Keep the same time is mandatory
                        if (NonAddedEntities[KnowledgeBaseManager.DimentionsEnum.Time].Count > 0)
                        {
                            duplicateNode.Time = eventNode.Time;
                            duplicateNode.ToMTable.Add(duplicateNode.Time, false);
                        }

                        //TODO by Palhas: Unsure if this method is best for Subject, but gonna use this for now
                        //Keep the same subject is mandatory
                        if (NonAddedEntities[KnowledgeBaseManager.DimentionsEnum.Subject].Count > 0)
                        {
                            duplicateNode.Subject = eventNode.Subject;
                            duplicateNode.ToMTable.Add(duplicateNode.Subject, false);
                        }

                        //Add a random Location if needed
                        if (NonAddedEntities[KnowledgeBaseManager.DimentionsEnum.Location].Count > 0)
                        {
                            Dictionary<EntityNode, float> SimilarLocationEntities = virtualSuspect.KnowledgeBase.ExtractSimilarEntities(eventNode.Location, true);
                            //Iterate all the similar location and retrieve the best one
                            foreach (EntityNode similarLocation in SimilarLocationEntities.OrderByDescending(x => x.Value).Select(x => x.Key))
                            {
                                if (duplicateNode.ToMTable.ContainsKey(similarLocation))
                                {
                                    continue;
                                }
                                //TODO: Perform Test to check if location is possible(Setting to the first available)
                                //TODO: Improve Space-Time Coherence
                                duplicateNode.Location = similarLocation;
                                duplicateNode.ToMTable.Add(duplicateNode.Location, false);
                                break;
                            }
                        }
                        /*
                        //Get Virtual Agent EntityNode
                        EntityNode SelfAgentNode = virtualSuspect.KnowledgeBase.Entities.Find(x => x.Value == virtualSuspect.KnowledgeBase.Properties["Name"]);
                        bool ContainsSelfAgentNode = eventNode.Agent.Contains(SelfAgentNode);
                        //Add Self Agent if needed
                        if(ContainsSelfAgentNode && NonAddedEntities[KnowledgeBaseManager.DimentionsEnum.Agent].Count > 0){
                            duplicateNode.AddAgent(SelfAgentNode);
                            NonAddedEntities[KnowledgeBaseManager.DimentionsEnum.Agent].Remove(SelfAgentNode);
                        }
                        */
                        //Fill the rest of agents
                        foreach (EntityNode OldAgentNode in NonAddedEntities[KnowledgeBaseManager.DimentionsEnum.Agent])
                        {
                            Dictionary<EntityNode, float> SimilarAgentsEntities = virtualSuspect.KnowledgeBase.ExtractSimilarEntities(OldAgentNode, true);
                            foreach (EntityNode similarAgent in SimilarAgentsEntities.OrderByDescending(x => x.Value).Select(x => x.Key))
                            {
                                //If Agent Already Exists
                                if (!duplicateNode.Agent.Contains(similarAgent))
                                {
                                    duplicateNode.AddAgent(similarAgent);
                                    break;
                                }
                            }
                        }

                        //Fill the rest of theme
                        foreach (EntityNode OldThemeNode in NonAddedEntities[KnowledgeBaseManager.DimentionsEnum.Theme])
                        {
                            Dictionary<EntityNode, float> SimilarThemesEntities = virtualSuspect.KnowledgeBase.ExtractSimilarEntities(OldThemeNode, true);
                            foreach (EntityNode similarTheme in SimilarThemesEntities.OrderByDescending(x => x.Value).Select(x => x.Key))
                            {
                                if (!duplicateNode.Theme.Contains(similarTheme))
                                {
                                    duplicateNode.AddTheme(similarTheme);
                                    break;
                                }
                            }
                        }
                        //Fill the rest of reasons
                        foreach (EntityNode OldReasonNode in NonAddedEntities[KnowledgeBaseManager.DimentionsEnum.Reason])
                        {
                            Dictionary<EntityNode, float> SimilarReasonsEntities = virtualSuspect.KnowledgeBase.ExtractSimilarEntities(OldReasonNode, true);
                            foreach (EntityNode similarReason in SimilarReasonsEntities.OrderByDescending(x => x.Value).Select(x => x.Key))
                            {
                                if (!duplicateNode.Reason.Contains(similarReason))
                                {
                                    duplicateNode.AddReason(similarReason);
                                    break;
                                }
                            }
                        }

                        //Fill the rest of manners
                        foreach (EntityNode OldMannerNode in NonAddedEntities[KnowledgeBaseManager.DimentionsEnum.Manner])
                        {
                            Dictionary<EntityNode, float> SimilarMannersEntities = virtualSuspect.KnowledgeBase.ExtractSimilarEntities(OldMannerNode, true);
                            foreach (EntityNode similarManner in SimilarMannersEntities.OrderByDescending(x => x.Value).Select(x => x.Key))
                            {
                                if (!duplicateNode.Manner.Contains(similarManner))
                                {
                                    duplicateNode.AddManner(similarManner);
                                    break;
                                }
                            }
                        }

                        //Add event to the events list
                        virtualSuspect.KnowledgeBase.Events.Add(duplicateNode);
                        virtualSuspect.KnowledgeBase.ReplaceEvent(eventNode, duplicateNode);
                    }
                    break;
            }

            return query;

        }

        internal EventNode DuplicateEvent(EventNode old, uint newID, out Dictionary<KnowledgeBaseManager.DimentionsEnum, List<EntityNode>> ToAdd, bool keepKnown = true)
        {

            EventNode eventCopy = new EventNode(newID, 0, false, old.Action);

            ToAdd = new Dictionary<KnowledgeBaseManager.DimentionsEnum, List<EntityNode>>();

            ToAdd.Add(KnowledgeBaseManager.DimentionsEnum.Agent, new List<EntityNode>());
            ToAdd.Add(KnowledgeBaseManager.DimentionsEnum.Theme, new List<EntityNode>());
            ToAdd.Add(KnowledgeBaseManager.DimentionsEnum.Location, new List<EntityNode>());
            ToAdd.Add(KnowledgeBaseManager.DimentionsEnum.Manner, new List<EntityNode>());
            ToAdd.Add(KnowledgeBaseManager.DimentionsEnum.Reason, new List<EntityNode>());
            ToAdd.Add(KnowledgeBaseManager.DimentionsEnum.Time, new List<EntityNode>());
            ToAdd.Add(KnowledgeBaseManager.DimentionsEnum.Subject, new List<EntityNode>());

            //Copy each dimension if they are known
            if (keepKnown && old.IsKnown(old.Time))
            {
                eventCopy.Time = old.Time;
                eventCopy.ToMTable.Add(eventCopy.Time, true);
            }
            else
            {
                ToAdd[KnowledgeBaseManager.DimentionsEnum.Time].Add(old.Time);
            }

            if (keepKnown && old.IsKnown(old.Location))
            {
                eventCopy.Location = old.Location;
                eventCopy.ToMTable.Add(old.Location, true);
            }
            else
            {
                ToAdd[KnowledgeBaseManager.DimentionsEnum.Location].Add(old.Location);
            }

            if (keepKnown && old.IsKnown(old.Subject))
            {
                eventCopy.Subject = old.Subject;
                eventCopy.ToMTable.Add(old.Subject, true);
            }
            else
            {
                ToAdd[KnowledgeBaseManager.DimentionsEnum.Subject].Add(old.Subject);
            }

            foreach (EntityNode AgentNode in old.Agent)
            {
                if (keepKnown && old.IsKnown(AgentNode))
                {
                    eventCopy.AddAgent(AgentNode);
                    eventCopy.TagAsKnown(AgentNode);
                }
                else
                {
                    ToAdd[KnowledgeBaseManager.DimentionsEnum.Agent].Add(AgentNode);
                }
            }

            foreach (EntityNode ThemeNode in old.Theme)
            {
                if (keepKnown && old.IsKnown(ThemeNode))
                {
                    eventCopy.AddTheme(ThemeNode);
                    eventCopy.TagAsKnown(ThemeNode);
                }
                else
                {
                    ToAdd[KnowledgeBaseManager.DimentionsEnum.Theme].Add(ThemeNode);
                }
            }

            foreach (EntityNode ReasonNode in old.Reason)
            {
                if (keepKnown && old.IsKnown(ReasonNode))
                {
                    eventCopy.AddReason(ReasonNode);
                    eventCopy.TagAsKnown(ReasonNode);
                }
                else
                {
                    ToAdd[KnowledgeBaseManager.DimentionsEnum.Reason].Add(ReasonNode);
                }
            }

            foreach (EntityNode MannerNode in old.Manner)
            {
                if (keepKnown && old.IsKnown(MannerNode))
                {
                    eventCopy.AddManner(MannerNode);
                    eventCopy.TagAsKnown(MannerNode);
                }
                else
                {
                    ToAdd[KnowledgeBaseManager.DimentionsEnum.Manner].Add(MannerNode);
                }
            }

            return eventCopy;

        }

    }
}
