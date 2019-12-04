using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.KnowledgeBase;
using VirtualSuspect.Query;

namespace VirtualSuspectNaturalLanguage.Component {
    public static class AgentNaturalLanguageGenerator {

        public static string Generate(QueryResult result, Dictionary<KnowledgeBaseManager.DimentionsEnum, List<QueryResult.Result>> resultsByDimension) {

            string answer = "";

            answer += "Because ";

            Dictionary<EntityNode, int> mergedAgents = MergeAndSumAgentsCardinality(resultsByDimension[KnowledgeBaseManager.DimentionsEnum.Agent]);

            answer += CombineValues("and", mergedAgents.Select(x=>x.Key.Speech));
                
            return answer;
        }

        #region Utility Methods

        private static string CombineValues(string term, IEnumerable<string> values) {

            string combinedValues = "";

            for (int i = 0; i < values.Count(); i++) {

                combinedValues += values.ElementAt(i);

                if (i == values.Count() - 2) {
                    combinedValues += " " + term + " ";
                }
                else if (i < values.Count() - 1) {
                    combinedValues += ", ";
                }
            }

            return combinedValues;

        }

        private static Dictionary<EntityNode, int> MergeAndSumAgentsCardinality(List<QueryResult.Result> agents) {

            Dictionary<EntityNode, int> agentsWithCardinality = new Dictionary<EntityNode, int>();

            foreach (QueryResult.Result agentResult in agents) {

                foreach (EntityNode agentNode in agentResult.values) {

                    if (!agentsWithCardinality.ContainsKey(agentNode)) {

                        agentsWithCardinality.Add(agentNode, agentResult.cardinality);

                    }
                    else {

                        agentsWithCardinality[agentNode] += agentResult.cardinality;
                    }
                }
            }

            return agentsWithCardinality;
        }
        #endregion   
    }
}
