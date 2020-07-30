using System;
using System.Collections.Generic;
using System.Linq;

namespace VirtualSuspect.KnowledgeBase
{
    public class EventNode
    {

        private uint id;

        public uint ID
        {
            get
            {
                return id;
            }
        }

        /// <summary>
        /// this event was replaced by the event with ID replacedByID
        /// </summary>
        public uint replacedByID = 0;

        /// <summary>
        /// this event replaced the event with ID replacedID
        /// </summary>
        public uint replacedID = 0;

        private bool originalStory;

        public bool OriginalStory
        {
            get
            {
                return originalStory;
            }
        }

        private int incriminatory;

        public int Incriminatory
        {
            get
            {
                return incriminatory;
            }
        }

        private ActionNode action;

        public ActionNode Action
        {
            get
            {
                return action;
            }

            set
            {
                action = value;
            }
        }

        private EntityNode time;

        public EntityNode Time
        {
            get
            {
                return time;
            }

            set
            {
                time = value;
            }
        }

        private EntityNode location;

        public EntityNode Location
        {
            get
            {
                return location;
            }

            set
            {
                location = value;
            }
        }

        private EntityNode subject;

        public EntityNode Subject
        {
            get
            {
                return subject;
            }

            set
            {
                subject = value;
            }
        }

        private List<EntityNode> agent;

        public List<EntityNode> Agent
        {
            get
            {
                return agent;
            }
        }

        private List<EntityNode> theme;

        public List<EntityNode> Theme
        {
            get
            {
                return theme;
            }
        }

        private List<EntityNode> manner;

        public List<EntityNode> Manner
        {
            get
            {
                return manner;
            }
        }

        private List<EntityNode> reason;

        public List<EntityNode> Reason
        {
            get
            {
                return reason;
            }
        }

        public float Know
        {

            get
            {

                int totalNumEntities = ToMTable.Count;
                int numKnownEntities = ToMTable.Count(x => x.Value == true);

                return 100.0f * numKnownEntities / totalNumEntities;
            }
        }

        internal Dictionary<EntityNode, bool> ToMTable;

        internal EventNode(uint id, int incriminatory, bool originalStory, ActionNode action)
        {

            this.id = id;
            this.incriminatory = incriminatory;
            this.action = action;
            this.originalStory = originalStory;

            agent = new List<EntityNode>();
            theme = new List<EntityNode>();
            manner = new List<EntityNode>();
            reason = new List<EntityNode>();

            ToMTable = new Dictionary<EntityNode, bool>();

        }

        public EventNode(uint id, int incriminatory, bool originalStory, ActionNode action, EntityNode time, EntityNode location, EntityNode subject)
        {

            this.id = id;
            this.incriminatory = incriminatory;
            this.action = action;
            this.time = time;
            this.location = location;
            this.subject = subject;
            this.originalStory = originalStory;

            agent = new List<EntityNode>();
            theme = new List<EntityNode>();
            manner = new List<EntityNode>();
            reason = new List<EntityNode>();

            ToMTable = new Dictionary<EntityNode, bool>();

            ToMTable.Add(time, false);
            ToMTable.Add(location, false);
            ToMTable.Add(subject, false);

        }

        public void AddAgent(EntityNode agent)
        {

            this.agent.Add(agent);
            if (CanAddToToM(agent))
                ToMTable.Add(agent, false);

        }

        public void AddAgent(params EntityNode[] agents)
        {

            this.agent.AddRange(agents);
            foreach (EntityNode agent in agents)
            {
                if (CanAddToToM(agent))
                    ToMTable.Add(agent, false);
            }
        }

        public void AddAgent(List<EntityNode> agents)
        {

            this.agent.AddRange(agents);
            foreach (EntityNode agent in agents)
            {
                if (CanAddToToM(agent))
                    ToMTable.Add(agent, false);
            }
        }

        public void AddTheme(EntityNode theme)
        {

            this.theme.Add(theme);
            if (CanAddToToM(theme))
                ToMTable.Add(theme, false);
        }

        public void AddTheme(params EntityNode[] themes)
        {

            this.theme.AddRange(themes);
            foreach (EntityNode theme in themes)
            {
                if (CanAddToToM(theme))
                    ToMTable.Add(theme, false);
            }
        }

        public void AddTheme(List<EntityNode> themes)
        {

            this.theme.AddRange(themes);
            foreach (EntityNode theme in themes)
            {
                if (CanAddToToM(theme))
                    ToMTable.Add(theme, false);
            }
        }

        public void AddManner(EntityNode manner)
        {

            this.manner.Add(manner);
            if (CanAddToToM(manner))
                ToMTable.Add(manner, false);
        }

        public void AddManner(params EntityNode[] manners)
        {

            this.manner.AddRange(manners);
            foreach (EntityNode manner in manners)
            {
                if (CanAddToToM(manner))
                    ToMTable.Add(manner, false);
            }
        }

        public void AddManner(List<EntityNode> manners)
        {

            this.manner.AddRange(manners);
            foreach (EntityNode manner in manners)
            {
                if (CanAddToToM(manner))
                    ToMTable.Add(manner, false);
            }
        }

        public void AddReason(EntityNode reason)
        {

            this.reason.Add(reason);
            if (CanAddToToM(reason))
                ToMTable.Add(reason, false);
        }

        public void AddReason(params EntityNode[] reasons)
        {

            this.reason.AddRange(reasons);
            foreach (EntityNode reason in reasons)
            {
                if (CanAddToToM(reason))
                    ToMTable.Add(reason, false);
            }
        }

        public void AddReason(List<EntityNode> reasons)
        {

            this.reason.AddRange(reasons);
            foreach (EntityNode reason in reasons)
            {
                if (CanAddToToM(reason))
                    ToMTable.Add(reason, false);
            }
        }

        /// <summary>
        /// Filters all entities in this event by semantic role and value
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns>returns the entity if there is a match or null otherwise</returns>
        /// 
        public EntityNode FindEntity(KnowledgeBaseManager.DimentionsEnum type, string value)
        {

            switch (type)
            {
                case KnowledgeBaseManager.DimentionsEnum.Time:
                    if (Time.Value == value)
                        return time;
                    break;
                case KnowledgeBaseManager.DimentionsEnum.Location:
                    if (Location.Value == value)
                        return location;
                    break;
                case KnowledgeBaseManager.DimentionsEnum.Subject:
                    if (Subject.Value == value)
                        return subject;
                    break;
                case KnowledgeBaseManager.DimentionsEnum.Agent:
                    return Agent.Find(x => x.Value == value);
                case KnowledgeBaseManager.DimentionsEnum.Theme:
                    return Theme.Find(x => x.Value == value);
                case KnowledgeBaseManager.DimentionsEnum.Reason:
                    return Reason.Find(x => x.Value == value);
                case KnowledgeBaseManager.DimentionsEnum.Manner:
                    return Manner.Find(x => x.Value == value);
                default:
                    return null;

            }

            return null;
        }

        public List<EntityNode> FindEntitiesByType(KnowledgeBaseManager.DimentionsEnum type)
        {

            List<EntityNode> nodes = new List<EntityNode>();

            switch (type)
            {
                case KnowledgeBaseManager.DimentionsEnum.Time:
                    nodes.Add(Time);
                    break;
                case KnowledgeBaseManager.DimentionsEnum.Location:
                    nodes.Add(Location);
                    break;
                case KnowledgeBaseManager.DimentionsEnum.Subject:
                    nodes.Add(Subject);
                    break;
                case KnowledgeBaseManager.DimentionsEnum.Agent:
                    nodes.AddRange(Agent);
                    break;
                case KnowledgeBaseManager.DimentionsEnum.Theme:
                    nodes.AddRange(Theme);
                    break;
                case KnowledgeBaseManager.DimentionsEnum.Reason:
                    nodes.AddRange(Reason);
                    break;
                case KnowledgeBaseManager.DimentionsEnum.Manner:
                    nodes.AddRange(Manner);
                    break;
            }

            return nodes;

        }

        /// <summary>
        /// Marks an entitiy Node as known by the user
        /// </summary>
        /// <param name="node"></param>
        public void TagAsKnown(EntityNode node)
        {

            ToMTable[node] = true;
        }

        public bool IsKnown(EntityNode node)
        {

            return ToMTable[node];
        }

        public bool ContainsEntity(EntityNode node)
        {

            return Time == node ||
                    Location == node ||
                    Subject == node ||
                    Theme.Contains(node) ||
                    Agent.Contains(node) ||
                    Reason.Contains(node) ||
                    Manner.Contains(node);

        }

        private bool CanAddToToM(EntityNode node)
        {

            return ToMTable.Keys.Count(x => x == node) == 0;

        }
    }
}