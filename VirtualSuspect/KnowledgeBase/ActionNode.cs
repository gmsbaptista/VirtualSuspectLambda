namespace VirtualSuspect.KnowledgeBase
{
    public class ActionNode{

        private uint id;

        public uint ID
        {
            get
            {
                return id;
            }
        }

        private string action;

        public string Action
        {

            get
            {
                return action;
            }

        }

        public ActionNode(uint _id,  string _action) {

            id = _id;
            action = _action;

        }

    }
}