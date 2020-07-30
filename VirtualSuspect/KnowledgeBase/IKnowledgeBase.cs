using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.Query;

namespace VirtualSuspect.KnowledgeBase
{
    interface IKnowledgeBase
    {

        /// <summary>
        /// Accessor for the virtual agent's properties
        /// </summary>
        /// <returns></returns>
        Dictionary<string, string> Properties { get; }

        /// <summary>
        /// Creates a new action with the parameters in 'ac'
        /// </summary>
        /// <param name="ac">Contains the parameters to define an action</param>
        /// <returns></returns>
        ActionNode CreateNewAction(ActionDto ac);

        /// <summary>
        /// Creates a new Entity with the paremeters in 'en'
        /// </summary>
        /// <param name="en">Contains the parameters to be used</param>
        /// <returns></returns>
        EntityNode CreateNewEntity(EntityDto en);

        /// <summary>
        /// Creates a new Event using the Entities in 'ev'
        /// </summary>
        /// <param name="ev">Contains the entities used to create an event</param>
        /// <returns></returns>
        EventNode CreateNewEvent(EventDto ev);

        /// <summary>
        /// Adds this Event Node to the story
        /// </summary>
        /// <param name="en">The event Node to be added to the story</param>
        void AddEventToStory(EventNode en);

        /// <summary>
        /// Gets a list of all the entities with the dimension
        /// </summary>
        /// <param name="dimension"></param>
        /// <returns></returns>
        IEnumerable<EntityNode> GetAllEntitiesByDimension(KnowledgeBaseManager.DimentionsEnum dimension);

    }
}
