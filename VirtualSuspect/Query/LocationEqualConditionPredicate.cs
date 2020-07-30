using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.KnowledgeBase;

namespace VirtualSuspect.Query
{
    public class LocationEqualConditionPredicate : IConditionPredicate
    {

        private string location;

        public LocationEqualConditionPredicate(string location)
        {

            this.location = location;

        }

        public Predicate<EventNode> CreatePredicate()
        {
            return
                delegate (EventNode node)
                {
                    return node.Location.Value == location || node.Location.CheckParent(location);
                };
        }

        public KnowledgeBaseManager.DimentionsEnum GetSemanticRole()
        {

            return KnowledgeBaseManager.DimentionsEnum.Location;

        }

        public List<string> GetValues()
        {

            List<string> result = new List<string>();

            result.Add(location);

            return result;

        }
    }
}
