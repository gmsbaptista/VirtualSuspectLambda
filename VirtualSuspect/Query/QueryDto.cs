using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualSuspect.Query
{
    public class QueryDto
    {

        #region enumerates

        public enum QueryTypeEnum { YesOrNo, GetInformation, GetKnowledge };

        public enum OperatorEnum { Equal, Between };
        #endregion

        /// <summary>
        /// Type of query
        /// </summary>
        private QueryTypeEnum queryType;

        public QueryTypeEnum QueryType
        {

            get
            {
                return queryType;
            }

        }

        /// <summary>
        /// List with the conditions to be satisfied
        /// </summary>
        private List<IConditionPredicate> queryConditions;

        public List<IConditionPredicate> QueryConditions
        {

            get
            {
                return queryConditions;
            }

        }

        /// <summary>
        /// List with the focus to be selected
        /// </summary>
        private List<IFocusPredicate> queryFocus;

        public List<IFocusPredicate> QueryFocus
        {

            get
            {
                return queryFocus;
            }

        }

        /// <summary>
        /// List with the focus to be selected
        /// </summary>
        private List<IKnowledgePredicate> knowledgeFocus;

        public List<IKnowledgePredicate> KnowledgeFocus
        {

            get
            {
                return knowledgeFocus;
            }

        }

        public QueryDto(QueryTypeEnum type)
        {

            this.queryType = type;

            queryConditions = new List<IConditionPredicate>();

            queryFocus = new List<IFocusPredicate>();

            knowledgeFocus = new List<IKnowledgePredicate>();
        }

        public void AddCondition(IConditionPredicate cond)
        {

            queryConditions.Add(cond);

        }

        public void AddFocus(IFocusPredicate focus)
        {

            queryFocus.Add(focus);
        }

        public void AddKnowledgeFocus(IKnowledgePredicate focus)
        {

            knowledgeFocus.Add(focus);
        }
    }
}
