using System;
using System.Collections.Generic;
using VirtualSuspect.KnowledgeBase;

namespace VirtualSuspect.Query
{
    public interface IKnowledgePredicate
    {

        /// <summary>
        /// Creates the delegate to be used as predicate
        /// </summary>
        /// <returns>a delegate to the predicate</returns>
        Func<EntityNode, string> CreateFunction();


    }
}