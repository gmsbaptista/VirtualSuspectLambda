using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.KnowledgeBase;

namespace VirtualSuspect.Query
{
    public class MannerEqualConditionPredicate : IConditionPredicate
    {

        private List<string> manners;

        public MannerEqualConditionPredicate(List<string> manners)
        {

            this.manners = manners;

        }

        public Predicate<EventNode> CreatePredicate()
        {
            return
                delegate (EventNode node)
                {

                    if (manners.Count == 1)
                    {
                        //if there is only one manner in the list to match we find if any of the node's manners is a match

                        return node.Manner.Any(x => x.Value == manners[0]);

                    }
                    else
                    {
                        //otherwise, we look if every manner in our list to match are in the nodes manners list
                        //TODO: Test this mambo
                        return !manners.Except(node.Manner.Select(x => x.Value)).Any();
                    }

                };
        }

        public KnowledgeBaseManager.DimentionsEnum GetSemanticRole()
        {

            return KnowledgeBaseManager.DimentionsEnum.Manner;

        }

        public List<string> GetValues()
        {

            return manners;

        }
    }
}
