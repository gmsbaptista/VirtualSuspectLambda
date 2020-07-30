namespace VirtualSuspect.KnowledgeBase
{
    public class ActionDto
    {

        private string action;

        public string Action
        {

            get
            {
                return action;
            }

        }

        public ActionDto(string _action)
        {

            action = _action;

        }
    }
}