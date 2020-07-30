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
    public static class MannerNaturalLanguageGenerator
    {

        public static string Generate(QueryResult result, Dictionary<KnowledgeBaseManager.DimentionsEnum, List<QueryResult.Result>> resultsByDimension)
        {

            string answer = "";

            answer += "It was";

            bool hasAction = result.Query.QueryConditions.Count(x => x.GetSemanticRole() == KnowledgeBaseManager.DimentionsEnum.Action) > 0;

            if (hasAction)
            {
                NaturalLanguageResourceManager manager = NaturalLanguageResourceManager.Instance;
                ActionResource resource = manager.FindResource<ActionResource>(result.Query.QueryConditions.Find(x => x.GetSemanticRole() == KnowledgeBaseManager.DimentionsEnum.Action).GetValues().ElementAt(0));

                answer += " " + resource.ExtractPreposition(KnowledgeBaseManager.DimentionsEnum.Manner);
            }
            else
            {
                answer += " with";
            }

            Dictionary<EntityNode, int> mergedManners = MergeAndSumMannersCardinality(resultsByDimension[KnowledgeBaseManager.DimentionsEnum.Manner]);

            answer += CombineValues("and", mergedManners.Select(x => x.Key.Speech));

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

        private static Dictionary<EntityNode, int> MergeAndSumMannersCardinality(List<QueryResult.Result> manners)
        {

            Dictionary<EntityNode, int> mannersWithCardinality = new Dictionary<EntityNode, int>();

            foreach (QueryResult.Result mannerResult in manners)
            {

                foreach (IStoryNode mannerNode in mannerResult.values)
                {

                    if (!mannersWithCardinality.ContainsKey((EntityNode)mannerNode))
                    {

                        mannersWithCardinality.Add((EntityNode)mannerNode, mannerResult.cardinality);

                    }
                    else
                    {

                        mannersWithCardinality[(EntityNode)mannerNode] += mannerResult.cardinality;
                    }
                }
            }

            return mannersWithCardinality;
        }
        #endregion   
    }
}
