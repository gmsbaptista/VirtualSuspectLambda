using System;
using System.Collections.Generic;
using VirtualSuspect.KnowledgeBase;

namespace VirtualSuspect.Query
{
    public interface IFocusPredicate
    {

        /// <summary>
        /// Creates the delegate to be used as predicate
        /// </summary>
        /// <returns>a delgate to the predicate</returns>
        Func<EventNode, QueryResult.Result> CreateFunction();

        /// <summary>
        /// Returns the semantic role of the condition
        /// </summary>
        /// <returns>Ex: Theme, Action, Agent, Manner, Reason, Time, Location</returns>
        KnowledgeBaseManager.DimentionsEnum GetSemanticRole();


    }
}