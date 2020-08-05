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
    public static class AgentNaturalLanguageGenerator
    {

        public static string Generate(QueryResult result, Dictionary<KnowledgeBaseManager.DimentionsEnum, List<QueryResult.Result>> resultsByDimension)
        {

            string answer = "";

            bool hasAction = result.Query.QueryConditions.Count(x => x.GetSemanticRole() == KnowledgeBaseManager.DimentionsEnum.Action) > 0;
            bool hasSubject = result.Query.QueryConditions.Any(x => x.GetSemanticRole() == KnowledgeBaseManager.DimentionsEnum.Subject && x.GetValues().Contains("Peter Sanders"));

            if (hasAction && hasSubject)
            {
                answer += "I";
                //Add verb from action resource
                NaturalLanguageResourceManager manager = NaturalLanguageResourceManager.Instance;
                ActionResource resource = manager.FindResource<ActionResource>(result.Query.QueryConditions.Find(x => x.GetSemanticRole() == KnowledgeBaseManager.DimentionsEnum.Action).GetValues().ElementAt(0));

                answer += " " + resource.Speech;

                if (result.Query.QueryConditions.Count(x => x.GetSemanticRole() == KnowledgeBaseManager.DimentionsEnum.Theme) > 0)
                {
                    answer += " it";
                }

                //Add preposition for the dimension
                answer += " " + resource.ExtractPreposition(KnowledgeBaseManager.DimentionsEnum.Agent);
            }
            else
            {
                answer += "It was";
            }

            Dictionary<EntityNode, int> mergedAgents = MergeAndSumAgentsCardinality(resultsByDimension[KnowledgeBaseManager.DimentionsEnum.Agent]);

            answer += CombineValues("and", mergedAgents.Select(x => x.Key.Speech));

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

        private static Dictionary<EntityNode, int> MergeAndSumAgentsCardinality(List<QueryResult.Result> agents)
        {

            Dictionary<EntityNode, int> agentsWithCardinality = new Dictionary<EntityNode, int>();

            foreach (QueryResult.Result agentResult in agents)
            {

                foreach (IStoryNode agentNode in agentResult.values)
                {

                    if (!agentsWithCardinality.ContainsKey((EntityNode)agentNode))
                    {

                        agentsWithCardinality.Add((EntityNode)agentNode, agentResult.cardinality);

                    }
                    else
                    {

                        agentsWithCardinality[(EntityNode)agentNode] += agentResult.cardinality;
                    }
                }
            }

            return agentsWithCardinality;
        }
        #endregion   
    }
}
