using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.KnowledgeBase;

namespace VirtualSuspect.Query
{
    public class AgentEqualConditionPredicate : IConditionPredicate
    {

        private List<string> agents;

        public AgentEqualConditionPredicate(List<string> agents)
        {

            this.agents = agents;

        }

        public Predicate<EventNode> CreatePredicate()
        {
            return
                delegate (EventNode node)
                {

                    if (agents.Count == 1)
                    {
                        //if there is only one agent in the list to match we find if any of the node's agents is a match

                        return node.Agent.Any(x => x.Value == agents[0]);

                    }
                    else
                    {
                        //otherwise, we look if every agent in our list to match are in the nodes agents list
                        //TODO: Test this mambo
                        return !agents.Except(node.Agent.Select(x => x.Value)).Any();
                    }

                };
        }

        public KnowledgeBaseManager.DimentionsEnum GetSemanticRole()
        {

            return KnowledgeBaseManager.DimentionsEnum.Agent;

        }

        public List<string> GetValues()
        {

            return agents;

        }
    }
}
