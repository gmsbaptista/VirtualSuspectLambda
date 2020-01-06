using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.KnowledgeBase;

namespace VirtualSuspect.Query
{
    public class GetActionFocusPredicate : IFocusPredicate
    {
        public Func<EventNode, QueryResult.Result> CreateFunction()
        {
            return delegate (EventNode node) {

                return new QueryResult.Result(node.Action, 1, KnowledgeBaseManager.DimentionsEnum.Action);

            };
        }

        public KnowledgeBaseManager.DimentionsEnum GetSemanticRole()
        {
            return KnowledgeBaseManager.DimentionsEnum.Action;
        }
    }
}
