using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.KnowledgeBase;

namespace VirtualSuspect.Query
{
    public class GetParentKnowledgePredicate : IKnowledgePredicate
    {

        public Func<EntityNode, string> CreateFunction()
        {
            return delegate (EntityNode node)
            {

                if (node.Parent != null)
                {
                    return node.Parent.Value;
                }
                else
                {
                    return "";
                }

            };
        }

    }
}
