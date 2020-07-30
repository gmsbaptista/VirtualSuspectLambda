using System.Collections.Generic;

namespace VirtualSuspect.KnowledgeBase
{
    public class EventDto
    {

        private bool real;

        public bool Real
        {
            get
            {
                return real;
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
        }

        private EntityNode time;

        public EntityNode Time
        {
            get
            {
                return time;
            }
        }

        private EntityNode location;

        public EntityNode Location
        {
            get
            {
                return location;
            }
        }

        private EntityNode subject;

        public EntityNode Subject
        {
            get
            {
                return subject;
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

        public EventDto(int incriminatory, ActionNode action, EntityNode time, EntityNode location, EntityNode subject, bool real)
        {

            this.incriminatory = incriminatory;
            this.action = action;
            this.time = time;
            this.location = location;
            this.subject = subject;
            this.real = real;

            agent = new List<EntityNode>();
            theme = new List<EntityNode>();
            manner = new List<EntityNode>();
            reason = new List<EntityNode>();

        }

        public void AddAgent(EntityNode agent)
        {

            this.agent.Add(agent);

        }

        public void AddAgent(params EntityNode[] agents)
        {

            this.agent.AddRange(agents);

        }

        public void AddTheme(EntityNode theme)
        {

            this.theme.Add(theme);

        }

        public void AddTheme(params EntityNode[] themes)
        {

            this.theme.AddRange(themes);

        }

        public void AddManner(EntityNode manner)
        {

            this.manner.Add(manner);

        }

        public void AddManner(params EntityNode[] manners)
        {

            this.manner.AddRange(manners);

        }

        public void AddReason(EntityNode reason)
        {

            this.reason.Add(reason);

        }

        public void AddReason(params EntityNode[] reasons)
        {

            this.reason.AddRange(reasons);

        }

    }
}