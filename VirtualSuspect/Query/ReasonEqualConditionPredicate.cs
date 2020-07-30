using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.KnowledgeBase;

namespace VirtualSuspect.Query
{
    public class ReasonEqualConditionPredicate : IConditionPredicate
    {

        private List<string> reasons;

        public ReasonEqualConditionPredicate(List<string> reasons)
        {

            this.reasons = reasons;

        }

        public Predicate<EventNode> CreatePredicate()
        {
            return
                delegate (EventNode node)
                {

                    if (reasons.Count == 1)
                    {
                        //if there is only one reason in the list to match we find if any of the node's reasons is a match

                        return node.Reason.Any(x => x.Value == reasons[0]);

                    }
                    else
                    {
                        //otherwise, we look if every reason in our list to match are in the nodes reasons list
                        //TODO: Test this mambo
                        return !reasons.Except(node.Reason.Select(x => x.Value)).Any();
                    }

                };
        }

        public KnowledgeBaseManager.DimentionsEnum GetSemanticRole()
        {

            return KnowledgeBaseManager.DimentionsEnum.Reason;

        }

        public List<string> GetValues()
        {

            return reasons;

        }
    }
}
