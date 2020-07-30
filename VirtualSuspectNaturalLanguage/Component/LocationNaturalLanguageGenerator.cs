using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.KnowledgeBase;
using VirtualSuspect.Query;

namespace VirtualSuspectNaturalLanguage.Component
{

    public static class LocationNaturalLanguageGenerator
    {

        public static string Generate(QueryResult result, Dictionary<KnowledgeBaseManager.DimentionsEnum, List<QueryResult.Result>> resultsByDimension)
        {

            string answer = "";

            bool hasAction = result.Query.QueryConditions.Count(x => x.GetSemanticRole() == KnowledgeBaseManager.DimentionsEnum.Action) > 0;
            bool inTrain = true;

            //hard coded value
            if (hasAction && result.Query.QueryConditions.Find(x => x.GetSemanticRole() == KnowledgeBaseManager.DimentionsEnum.Action).GetValues().ElementAt(0) == "Travel")
            {
                answer += "I travelled from";
                inTrain = false;
            }
            else if (hasAction || GetNumberAgents(result.Query) > 0)
            {
                answer += "It was";
            }
            else
            {
                answer += "I was";
            }

            //Merge all entities and sum cardinality
            Dictionary<EntityNode, int> mergedLocations = MergeAndSumLocationsCardinality(resultsByDimension[KnowledgeBaseManager.DimentionsEnum.Location], inTrain);

            //Group by Type
            //Dictionary<string, List<EntityNode>> locationGroupByType = GroupLocationByType(mergedLocations);

            for (int i = 0; i < mergedLocations.Count; i++)
            {

                //TODO: test some types to use "the"
                EntityNode node = mergedLocations.ElementAt(i).Key;

                answer += " " + GetPreposition(node, inTrain) + " " + node.Speech;

                /*if(mergedLocations.Count > 1 && inTrain)
                {
                    answer += " " + NaturalLanguageGetFrequency(mergedLocations.ElementAt(i).Value);
                }*/
                if (i == mergedLocations.Count - 2)
                {
                    answer += " and";
                }
                else if (i < mergedLocations.Count - 1)
                {
                    answer += ",";
                }
            }

            return answer;
        }

        #region Utility Methods

        private static string NaturalLanguageGetFrequency(int number)
        {

            string frequencyWord = "";

            if (number == 0)
            {
                frequencyWord = "never";
            }
            else if (number == 1)
            {
                frequencyWord = "once";
            }
            else if (number == 2)
            {
                frequencyWord = "twice";
            }
            else if (number >= 3 && number <= 6)
            {
                frequencyWord = number + " times";
            }
            else
            {
                Random rng = new Random();
                int randomNumber = rng.Next(2);
                if (randomNumber == 0)
                    frequencyWord = "many times";
                else if (randomNumber == 1)
                    frequencyWord = "several times";
            }

            return frequencyWord;
        }

        private static int GetNumberAgents(QueryDto query)
        {

            return query.QueryConditions.Count(x => x.GetSemanticRole() == KnowledgeBaseManager.DimentionsEnum.Agent);
        }

        private static Dictionary<EntityNode, int> MergeAndSumLocationsCardinality(List<QueryResult.Result> locations, bool inTrain)
        {

            Dictionary<EntityNode, int> locationsWithCardinality = new Dictionary<EntityNode, int>();

            foreach (QueryResult.Result locationResult in locations)
            {

                foreach (IStoryNode locationNode in locationResult.values)
                {

                    //hard coded value
                    if (inTrain && locationResult.values.Count > 1 && locationNode.Value != "Train")
                    {
                        continue;
                    }
                    else if (!inTrain && locationResult.values.Count > 1 && locationNode.Value == "Train")
                    {
                        continue;
                    }

                    if (!locationsWithCardinality.ContainsKey((EntityNode)locationNode))
                    {

                        locationsWithCardinality.Add((EntityNode)locationNode, locationResult.cardinality);

                    }
                    else
                    {

                        locationsWithCardinality[(EntityNode)locationNode] += locationResult.cardinality;
                    }
                }
            }

            return locationsWithCardinality;
        }

        public static string GetPreposition(EntityNode entity, bool inTrain)
        {
            string preposition = "";

            //hard coded value
            if (entity.Type == "Street" || entity.Type == "Train")
            {
                preposition = "in";
            }
            else if (entity.Type == "City")
            {
                if (!inTrain)
                {
                    preposition = "";
                }
                else
                {
                    preposition = "in";
                }
            }
            else
            {
                preposition = "at";
            }

            return preposition;
        }
        #endregion
    }

}
