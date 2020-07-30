using System.Collections.Generic;

namespace VirtualSuspect.KnowledgeBase
{
    public class EntityNode : IStoryNode
    {

        private uint id;

        public uint ID
        {
            get
            {
                return id;
            }
        }

        private float incriminatory;

        public float Incriminatory
        {

            get
            {
                return incriminatory;
            }

            set
            {
                incriminatory = value;
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

        private EntityNode parent;

        public EntityNode Parent
        {
            set
            {
                parent = value;
            }
            get
            {
                return parent;
            }
        }

        private Dictionary<string, string> associations;

        public Dictionary<string, string> Associations
        {
            get
            {
                return associations;
            }
        }

        public EntityNode(uint _id, string _value, string _speech, string _type)
        {

            id = _id;
            value = _value;
            speech = _speech;
            type = _type;

            associations = new Dictionary<string, string>();

        }

        public bool CheckParent(string value)
        {
            if (parent == null)
            {
                return false;
            }
            return parent.Value == value || parent.CheckParent(value);
        }

        public void AddAssociation(string relation, string speech)
        {
            associations.Add(relation, speech);
        }

        public float EvaluateKnowledge(KnowledgeBaseManager kb)
        {

            float total = 0;
            float known = 0;

            foreach (EventNode node in kb.Story)
            {

                if (node.ContainsEntity(this))
                {

                    total++;
                    if (node.IsKnown(this))
                        known++;

                }

            }

            return known / total * 100;

        }

    }
}