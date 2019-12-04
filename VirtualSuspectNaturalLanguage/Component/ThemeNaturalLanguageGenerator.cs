using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.KnowledgeBase;
using VirtualSuspect.Query;

namespace VirtualSuspectNaturalLanguage.Component {
    public static class ThemeNaturalLanguageGenerator {

        public static string Generate(QueryResult result, Dictionary<KnowledgeBaseManager.DimentionsEnum, List<QueryResult.Result>> resultsByDimension) {

            string answer = "";

            answer += " ";

            Dictionary<EntityNode, int> mergedThemes = MergeAndSumThemesCardinality(resultsByDimension[KnowledgeBaseManager.DimentionsEnum.Theme]);

            answer += CombineValues("and", mergedThemes.Select(x=>x.Key.Speech));
                
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

        private static Dictionary<EntityNode, int> MergeAndSumThemesCardinality(List<QueryResult.Result> themes) {

            Dictionary<EntityNode, int> themesWithCardinality = new Dictionary<EntityNode, int>();

            foreach (QueryResult.Result themeResult in themes) {

                foreach (EntityNode themeNode in themeResult.values) {

                    if (!themesWithCardinality.ContainsKey(themeNode)) {

                        themesWithCardinality.Add(themeNode, themeResult.cardinality);

                    }
                    else {

                        themesWithCardinality[themeNode] += themeResult.cardinality;
                    }
                }
            }

            return themesWithCardinality;
        }
        #endregion   
    }
}
