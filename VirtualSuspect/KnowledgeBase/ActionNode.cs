namespace VirtualSuspect.KnowledgeBase
{
    public class ActionNode : IStoryNode
    {

        private uint id;

        public uint ID
        {
            get
            {
                return id;
            }
        }

        private string value;

        public string Value
        {

            get
            {
                return value;
            }

        }

        public ActionNode(uint _id, string _action)
        {

            id = _id;
            value = _action;

        }

    }
}