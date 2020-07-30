using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.KnowledgeBase;

namespace VirtualSuspect.Query
{
    public class GetResidenceKnowledgePredicate : IKnowledgePredicate
    {

        public Func<EntityNode, string> CreateFunction()
        {
            return delegate (EntityNode node)
            {

                if (node.Associations.TryGetValue("Residence", out string residence))
                {
                    return residence;
                }
                else
                {
                    return "";
                }

            };
        }

    }
}
