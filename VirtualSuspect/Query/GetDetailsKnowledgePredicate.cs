using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.KnowledgeBase;

namespace VirtualSuspect.Query
{
    public class GetDetailsKnowledgePredicate : IKnowledgePredicate
    {

        public Func<EntityNode, string> CreateFunction()
        {
            return delegate (EntityNode node)
            {

                if (node.Associations.TryGetValue("Details", out string details))
                {
                    return details;
                }
                else
                {
                    return "";
                }

            };
        }

    }
}
