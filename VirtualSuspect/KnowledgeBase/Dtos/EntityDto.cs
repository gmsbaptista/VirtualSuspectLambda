namespace VirtualSuspect.KnowledgeBase
{
    public class EntityDto
    {

        private string value;

        public string Value
        {
            get
            {
                return value;
            }
        }

        private string speech;

        public string Speech
        {
            get
            {
                return speech;
            }
        }

        private string type;

        public string Type
        {
            get
            {
                return type;
            }
        }

        public EntityDto(string _value, string _speech, string _type)
        {

            speech = _speech;
            value = _value;
            type = _type;

        }

    }
}