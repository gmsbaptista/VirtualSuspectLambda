using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.KnowledgeBase;
using VirtualSuspect.Query;

namespace VirtualSuspect
{

    public interface IQuestionAnswerSystem
    {

        KnowledgeBaseManager KnowledgeBase { get; }

        /// <summary>
        /// Query the knowledge with the condition of the query
        /// </summary>
        /// <param name="query">The dto containing the query conditions</param>
        /// <returns></returns>
        QueryResult Query(QueryDto query);

    }

}
