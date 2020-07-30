using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.KnowledgeBase;

namespace VirtualSuspect.Query
{
    public class ActionEqualConditionPredicate : IConditionPredicate
    {

        private string action;

        public ActionEqualConditionPredicate(string action)
        {

            this.action = action;

        }

        public Predicate<EventNode> CreatePredicate()
        {
            return
                delegate (EventNode node)
                {
                    return node.Action.Value == action;
                };
        }

        public KnowledgeBaseManager.DimentionsEnum GetSemanticRole()
        {

            return KnowledgeBaseManager.DimentionsEnum.Action;

        }

        public List<string> GetValues()
        {

            List<string> result = new List<string>();

            result.Add(action);

            return result;

        }
    }
}
