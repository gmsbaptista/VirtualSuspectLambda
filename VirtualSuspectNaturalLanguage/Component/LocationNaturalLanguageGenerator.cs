using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.KnowledgeBase;
using VirtualSuspect.Query;

namespace VirtualSuspectNaturalLanguage.Component {

    public static class LocationNaturalLanguageGenerator {

        public static string Generate(QueryResult result, Dictionary<KnowledgeBaseManager.DimentionsEnum, List<QueryResult.Result>> resultsByDimension) {

            string answer = "";

            bool hasAction = result.Query.QueryConditions.Count(x => x.GetSemanticRole() == KnowledgeBaseManager.DimentionsEnum.Action) > 0;

            if (hasAction || GetNumberAgents(result.Query) > 0)
            {
                answer += "It was";
            }
            else
            {
                answer += "I was";
            }

            //Merge all entities and sum cardinality
            Dictionary<EntityNode, int> mergedLocations = MergeAndSumLocationsCardinality(resultsByDimension[KnowledgeBaseManager.DimentionsEnum.Location]);

            //Group by Type
            //Dictionary<string, List<EntityNode>> locationGroupByType = GroupLocationByType(mergedLocations);
            
            for (int i = 0; i < mergedLocations.Count; i++){

                //TODO: test some types to use "the"

                answer += " " + mergedLocations.ElementAt(i).Key.Speech;

                if(mergedLocations.Count > 1)
                {
                    answer += " " + NaturalLanguageGetFrequency(mergedLocations.ElementAt(i).Value);
                }

                if (i != mergedLocations.Count - 1) {
                    answer += ",";

                    if (i == mergedLocations.Count - 2) {
                        answer += " and";
                    }

                }
                /*else {
                    answer += ".";
                }*/

            }

            return answer;
        }

        #region Utility Methods

        private static string NaturalLanguageGetFrequency(int number) {

            string frequencyWord = "";

            if (number == 0) {
                frequencyWord = "never";
            }
            else if (number == 1) {
                frequencyWord = "once";
            }
            else if (number == 2) {
                frequencyWord = "twice";
            }
            else if (number >= 3 && number <= 6) {
                frequencyWord = number + " times";
            }
            else {
                Random rng = new Random();
                int randomNumber = rng.Next(2);
                if (randomNumber == 0)
                    frequencyWord = "many times";
                else if (randomNumber == 1)
                    frequencyWord = "several times";
            }

            return frequencyWord;
        }

        private static int GetNumberAgents(QueryDto query) {

            return query.QueryConditions.Count(x => x.GetSemanticRole() == KnowledgeBaseManager.DimentionsEnum.Agent);
        }

        private static Dictionary<EntityNode, int> MergeAndSumLocationsCardinality(List<QueryResult.Result> locations) {

            Dictionary<EntityNode, int> locationsWithCardinality = new Dictionary<EntityNode, int>();

            foreach (QueryResult.Result locationResult in locations) {

                foreach (IStoryNode locationNode in locationResult.values) {

                    if (!locationsWithCardinality.ContainsKey((EntityNode)locationNode)) {

                        locationsWithCardinality.Add((EntityNode)locationNode, locationResult.cardinality);

                    }
                    else {

                        locationsWithCardinality[(EntityNode)locationNode] += locationResult.cardinality;
                    }
                }
            }

            return locationsWithCardinality;
        }
        #endregion
    }

}
