using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.KnowledgeBase;
using VirtualSuspect.Query;

namespace VirtualSuspectNaturalLanguage.Component {
    public static class MannerNaturalLanguageGenerator {

        public static string Generate(QueryResult result, Dictionary<KnowledgeBaseManager.DimentionsEnum, List<QueryResult.Result>> resultsByDimension) {

            string answer = "";

            answer += " ";

            Dictionary<EntityNode, int> mergedManners = MergeAndSumMannersCardinality(resultsByDimension[KnowledgeBaseManager.DimentionsEnum.Manner]);

            answer += CombineValues("and", mergedManners.Select(x=>x.Key.Speech));
                
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

        private static Dictionary<EntityNode, int> MergeAndSumMannersCardinality(List<QueryResult.Result> manners) {

            Dictionary<EntityNode, int> mannersWithCardinality = new Dictionary<EntityNode, int>();

            foreach (QueryResult.Result mannerResult in manners) {

                foreach (EntityNode mannerNode in mannerResult.values) {

                    if (!mannersWithCardinality.ContainsKey(mannerNode)) {

                        mannersWithCardinality.Add(mannerNode, mannerResult.cardinality);

                    }
                    else {

                        mannersWithCardinality[mannerNode] += mannerResult.cardinality;
                    }
                }
            }

            return mannersWithCardinality;
        }
        #endregion   
    }
}
