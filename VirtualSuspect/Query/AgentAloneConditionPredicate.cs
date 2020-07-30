using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.KnowledgeBase;

namespace VirtualSuspect.Query
{
    public class AgentAloneConditionPredicate : IConditionPredicate
    {
        public Predicate<EventNode> CreatePredicate()
        {
            return
                delegate (EventNode node)
                {
                    return node.Agent.Count == 0;
                };
        }

        public KnowledgeBaseManager.DimentionsEnum GetSemanticRole()
        {
            return KnowledgeBaseManager.DimentionsEnum.Agent;
        }

        public List<string> GetValues()
        {
            return new List<string>();
        }
    }
}
