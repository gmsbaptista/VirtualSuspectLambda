using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.Query;
using VirtualSuspect.KnowledgeBase;

namespace VirtualSuspect.Handler
{

    class SenderTheoryOfMindHandler : IPosHandler
    {

        private IQuestionAnswerSystem questionAnswer;

        private bool AnswerToM;

        internal SenderTheoryOfMindHandler(IQuestionAnswerSystem questionAnswer, bool AnswerToM = false)
        {

            this.questionAnswer = questionAnswer;

            this.AnswerToM = AnswerToM;

        }

        public QueryResult Modify(QueryResult result)
        {

            if (!AnswerToM)
                return result;

            List<EventNode> queryEvents = questionAnswer.KnowledgeBase.Events;

            //Get all the events that match the query conditions
            foreach (IConditionPredicate condition in result.Query.QueryConditions)
            {

                queryEvents = queryEvents.FindAll(condition.CreatePredicate());

            }

            //Tag all the entities used inside the conditions from the events selected
            foreach (EventNode node in queryEvents)
            {

                foreach (IFocusPredicate predicate in result.Query.QueryFocus)
                {

                    KnowledgeBaseManager.DimentionsEnum semanticRole = predicate.GetSemanticRole();

                    if (semanticRole != KnowledgeBaseManager.DimentionsEnum.Action)
                    {

                        List<EntityNode> nodes = node.FindEntitiesByType(semanticRole);

                        foreach (EntityNode entity in nodes)
                        {

                            node.TagAsKnown(entity);

                        }

                    }
                    else
                    {
                        List<EntityNode> nodes = node.FindEntitiesByType(KnowledgeBaseManager.DimentionsEnum.Theme);
                        foreach (EntityNode entity in nodes)
                        {
                            node.TagAsKnown(entity);
                        }
                    }
                }
            }


            return result;

        }

    }

}
