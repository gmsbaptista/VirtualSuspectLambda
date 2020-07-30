using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.KnowledgeBase;
using VirtualSuspect.Query;

namespace VirtualSuspectNaturalLanguage.Component
{
    public static class ThemeNaturalLanguageGenerator
    {

        public static string Generate(QueryResult result, Dictionary<KnowledgeBaseManager.DimentionsEnum, List<QueryResult.Result>> resultsByDimension)
        {

            string answer = "";

            bool hasAction = result.Query.QueryConditions.Count(x => x.GetSemanticRole() == KnowledgeBaseManager.DimentionsEnum.Action) > 0;

            if (hasAction)
            {
                answer += "I";
                //Add verb from action resource
                NaturalLanguageResourceManager manager = NaturalLanguageResourceManager.Instance;
                ActionResource resource = manager.FindResource<ActionResource>(result.Query.QueryConditions.Find(x => x.GetSemanticRole() == KnowledgeBaseManager.DimentionsEnum.Action).GetValues().ElementAt(0));

                answer += " " + resource.Speech;

                //Add preposition for the dimension
                if (resource.ExtractPreposition(KnowledgeBaseManager.DimentionsEnum.Theme) != "")
                {
                    answer += " " + resource.ExtractPreposition(KnowledgeBaseManager.DimentionsEnum.Theme);
                }
            }
            else
            {
                answer += "It was";
            }

            Dictionary<EntityNode, int> mergedThemes = MergeAndSumThemesCardinality(resultsByDimension[KnowledgeBaseManager.DimentionsEnum.Theme]);

            answer += CombineValues("and", mergedThemes.Select(x => x.Key.Speech));

            return answer;
        }

        #region Utility Methods

        private static string CombineValues(string term, IEnumerable<string> values)
        {

            string combinedValues = "";

            for (int i = 0; i < values.Count(); i++)
            {

                combinedValues += " " + values.ElementAt(i);

                if (i == values.Count() - 2)
                {
                    combinedValues += " " + term;
                }
                else if (i < values.Count() - 1)
                {
                    combinedValues += ",";
                }
            }

            return combinedValues;

        }

        private static Dictionary<EntityNode, int> MergeAndSumThemesCardinality(List<QueryResult.Result> themes)
        {

            Dictionary<EntityNode, int> themesWithCardinality = new Dictionary<EntityNode, int>();

            foreach (QueryResult.Result themeResult in themes)
            {

                foreach (IStoryNode themeNode in themeResult.values)
                {

                    if (!themesWithCardinality.ContainsKey((EntityNode)themeNode))
                    {

                        themesWithCardinality.Add((EntityNode)themeNode, themeResult.cardinality);

                    }
                    else
                    {

                        themesWithCardinality[(EntityNode)themeNode] += themeResult.cardinality;
                    }
                }
            }

            return themesWithCardinality;
        }
        #endregion   
    }
}
