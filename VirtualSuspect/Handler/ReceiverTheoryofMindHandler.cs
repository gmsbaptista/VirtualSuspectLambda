using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.Query;
using VirtualSuspect.KnowledgeBase;

namespace VirtualSuspect.Handler
{

    internal class ReceiverTheoryOfMindHandler : IPreHandler
    {

        IQuestionAnswerSystem questionAnswer;
        bool yesOrNoTOM;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="questionAnswer">the owner of this module</param>
        /// <param name="YesOrNoTOM">true if the upgrades ToM when asking yes or no question</param>
        internal ReceiverTheoryOfMindHandler(IQuestionAnswerSystem questionAnswer, bool YesOrNoTOM = false)
        {

            this.questionAnswer = questionAnswer;
            this.yesOrNoTOM = YesOrNoTOM;

        }

        public QueryDto Modify(QueryDto query)
        {

            //If it shouldnt update when is a yes or No question
            if (query.QueryType == QueryDto.QueryTypeEnum.YesOrNo && !yesOrNoTOM)
                return query;

            List<EventNode> queryEvents = questionAnswer.KnowledgeBase.Events;

            //Get all the events that match the query conditions
            foreach (IConditionPredicate condition in query.QueryConditions)
            {

                queryEvents = queryEvents.FindAll(condition.CreatePredicate());

            }

            //Tag all the entities used inside the conditions from the events selected
            foreach (EventNode node in queryEvents)
            {

                foreach (IConditionPredicate condition in query.QueryConditions)
                {

                    foreach (string value in condition.GetValues())
                    {

                        if (condition.GetSemanticRole() != KnowledgeBaseManager.DimentionsEnum.Action)
                        {
                            EntityNode entity = node.FindEntity(condition.GetSemanticRole(), value);

                            if (entity != null)
                                node.TagAsKnown(entity);
                        }
                    }
                }
            }

            return query;
        }

    }

}
