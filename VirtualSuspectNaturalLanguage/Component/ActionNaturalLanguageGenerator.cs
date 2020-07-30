using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.KnowledgeBase;
using VirtualSuspect.Query;

namespace VirtualSuspectNaturalLanguage.Component
{
    public static class ActionNaturalLanguageGenerator
    {

        public static string Generate(QueryResult result, Dictionary<KnowledgeBaseManager.DimentionsEnum, List<QueryResult.Result>> resultsByDimension)
        {
            string answer = "";

            answer += "I";

            Dictionary<ActionNode, int> mergedActions = MergeAndSumActionsCardinality(
                resultsByDimension[KnowledgeBaseManager.DimentionsEnum.Action],
                out Dictionary<ActionNode, List<EntityNode>> actionsWithThemes);
            NaturalLanguageResourceManager manager = NaturalLanguageResourceManager.Instance;
            for (int i = 0; i < mergedActions.Keys.Count; i++)
            {
                ActionNode actionNode = mergedActions.Keys.ElementAt(i);
                ActionResource resource = manager.FindResource<ActionResource>(actionNode.Value);
                answer += " " + resource.Speech;
                if (resource.ExtractPreposition(KnowledgeBaseManager.DimentionsEnum.Theme) != "")
                {
                    answer += " " + resource.ExtractPreposition(KnowledgeBaseManager.DimentionsEnum.Theme);
                }
                answer += CombineValues("and", actionsWithThemes[actionNode].Select(x => x.Speech));
                if (i == mergedActions.Keys.Count - 2)
                {
                    answer += " and";
                }
                else if (i < mergedActions.Keys.Count - 1)
                {
                    answer += ",";
                }
            }

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

        private static Dictionary<ActionNode, int> MergeAndSumActionsCardinality(List<QueryResult.Result> actions, out Dictionary<ActionNode, List<EntityNode>> actionsWithThemes)
        {
            Dictionary<ActionNode, int> actionsWithCardinality = new Dictionary<ActionNode, int>();
            actionsWithThemes = new Dictionary<ActionNode, List<EntityNode>>();

            foreach (QueryResult.Result actionResult in actions)
            {
                if (!actionsWithCardinality.ContainsKey((ActionNode)actionResult.values[0]))
                {
                    actionsWithCardinality.Add((ActionNode)actionResult.values[0], actionResult.cardinality);
                }
                else
                {
                    actionsWithCardinality[(ActionNode)actionResult.values[0]] += actionResult.cardinality;
                }
                if (!actionsWithThemes.ContainsKey((ActionNode)actionResult.values[0]))
                {
                    actionsWithThemes.Add((ActionNode)actionResult.values[0], new List<EntityNode>());
                    if (actionResult.values.Count > 1)
                    {
                        for (int i = 1; i < actionResult.values.Count; i++)
                        {
                            actionsWithThemes[(ActionNode)actionResult.values[0]].Add((EntityNode)actionResult.values[i]);
                        }
                    }
                }
                else
                {
                    if (actionResult.values.Count > 1)
                    {
                        for (int i = 1; i < actionResult.values.Count; i++)
                        {
                            actionsWithThemes[(ActionNode)actionResult.values[0]].Add((EntityNode)actionResult.values[i]);
                        }
                    }
                }
            }
            return actionsWithCardinality;
        }
        #endregion 

    }
}
