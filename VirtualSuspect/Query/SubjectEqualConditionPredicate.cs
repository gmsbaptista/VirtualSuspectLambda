using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.KnowledgeBase;

namespace VirtualSuspect.Query
{
    public class SubjectEqualConditionPredicate : IConditionPredicate
    {

        private string subject;

        public SubjectEqualConditionPredicate(string subject)
        {

            this.subject = subject;

        }

        public Predicate<EventNode> CreatePredicate()
        {
            return
                delegate (EventNode node)
                {
                    return node.Subject.Value == subject;
                };
        }

        public KnowledgeBaseManager.DimentionsEnum GetSemanticRole()
        {

            return KnowledgeBaseManager.DimentionsEnum.Subject;

        }

        public List<string> GetValues()
        {

            List<string> result = new List<string>();

            result.Add(subject);

            return result;

        }
    }
}
