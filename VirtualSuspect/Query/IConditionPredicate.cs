using System;
using System.Collections.Generic;
using VirtualSuspect.KnowledgeBase;

namespace VirtualSuspect.Query
{

    public interface IConditionPredicate
    {

        /// <summary>
        /// Creates the delegate to be used as predicate
        /// </summary>
        /// <returns>a delgate to the predicate</returns>
        Predicate<EventNode> CreatePredicate();

        /// <summary>
        /// Returns the semantic role of the condition
        /// </summary>
        /// <returns>Ex: Theme, Action, Agent, Manner, Reason, Time, Location</returns>
        KnowledgeBaseManager.DimentionsEnum GetSemanticRole();

        /// <summary>
        /// Returns the list of values used inside the condition
        /// </summary>
        /// <returns></returns>
        List<string> GetValues();
    }

}