using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.KnowledgeBase;

namespace VirtualSuspect.Query
{
    public class GetMannerFocusPredicate : IFocusPredicate
    {

        public Func<EventNode, QueryResult.Result> CreateFunction()
        {
            return delegate (EventNode node)
            {

                return new QueryResult.Result(node.Manner, node.Manner.Count, KnowledgeBaseManager.DimentionsEnum.Manner);

            };
        }
        public KnowledgeBaseManager.DimentionsEnum GetSemanticRole()
        {
            return KnowledgeBaseManager.DimentionsEnum.Manner;
        }
    }
}
