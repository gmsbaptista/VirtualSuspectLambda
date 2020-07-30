using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.KnowledgeBase;

namespace VirtualSuspect.Query
{
    public class ThemeEqualConditionPredicate : IConditionPredicate
    {

        private List<string> themes;

        public ThemeEqualConditionPredicate(List<string> themes)
        {

            this.themes = themes;

        }

        public Predicate<EventNode> CreatePredicate()
        {
            return
                delegate (EventNode node)
                {

                    if (themes.Count == 1)
                    {
                        //if there is only one theme in the list to match we find if any of the node's themes is a match

                        return node.Theme.Any(x => x.Value == themes[0]);

                    }
                    else
                    {
                        //otherwise, we look if every theme in our list to match are in the nodes themes list
                        //TODO: Test this mambo
                        return !themes.Except(node.Theme.Select(x => x.Value)).Any();
                    }

                };
        }

        public KnowledgeBaseManager.DimentionsEnum GetSemanticRole()
        {

            return KnowledgeBaseManager.DimentionsEnum.Theme;

        }

        public List<string> GetValues()
        {

            return themes;

        }

    }
}
