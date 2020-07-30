using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.KnowledgeBase;

namespace VirtualSuspect.Query
{
    public class GetValueKnowledgePredicate : IKnowledgePredicate
    {

        public Func<EntityNode, string> CreateFunction()
        {
            return delegate (EntityNode node)
            {

                if (node.Associations.TryGetValue("Value", out string value))
                {
                    return value;
                }
                else
                {
                    return "";
                }

            };
        }

    }
}
