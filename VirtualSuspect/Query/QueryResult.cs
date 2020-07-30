using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VirtualSuspect.KnowledgeBase;

namespace VirtualSuspect.Query
{
    public class QueryResult
    {

        /// <summary>
        /// Dictionary that holds meta data that describes the answer
        /// </summary>
        private Dictionary<string, string> metaData;

        public Dictionary<string, string> MetaData
        {
            get
            {
                return metaData;
            }
        }

        private QueryDto query;

        public QueryDto Query
        {
            get
            {
                return query;
            }
        }

        private bool yesNoResult;

        public bool YesNoResult
        {
            get
            {
                return yesNoResult;
            }
        }

        private List<string> knowledgeResults;

        public List<string> KnowledgeResults
        {
            get
            {
                return knowledgeResults;
            }
        }

        private List<Result> results;

        public List<Result> Results
        {
            get
            {
                return results;
            }
        }

        public QueryResult(QueryDto query)
        {

            metaData = new Dictionary<string, string>();

            this.query = query;
            results = new List<Result>();
            knowledgeResults = new List<string>();
        }

        internal void AddResults(IEnumerable<Result> results)
        {

            this.results.AddRange(results);

        }

        internal void AddBooleanResult(bool result)
        {

            yesNoResult = result;

        }

        internal void AddKnowledgeResult(string knowledge)
        {
            this.knowledgeResults.Add(knowledge);
        }

        internal void AddKnowledgeResult(IEnumerable<string> knowledge)
        {
            this.knowledgeResults.AddRange(knowledge);
        }

        public class Result
        {

            public List<IStoryNode> values;

            public int cardinality;

            public KnowledgeBaseManager.DimentionsEnum dimension;

            public Result(IEnumerable<IStoryNode> entityValues, int cardinality, KnowledgeBaseManager.DimentionsEnum dimension)
            {
                values = entityValues.ToList();
                this.cardinality = cardinality;
                this.dimension = dimension;
            }

            public Result(IStoryNode entityValue, int cardinality, KnowledgeBaseManager.DimentionsEnum dimension)
            {

                List<IStoryNode> values = new List<IStoryNode>();
                values.Add(entityValue);

                this.values = values;
                this.cardinality = cardinality;
                this.dimension = dimension;
            }
        }

        internal void CountResult()
        {

            List<Result> newUniqueResults = new List<Result>();

            IEqualityComparer<Result> comparer = new GroupByComparer();
            newUniqueResults.AddRange(results.GroupBy(x => x, comparer).Select(x => x.First()));

            foreach (Result result in newUniqueResults)
            {

                result.cardinality = results.Count(x => comparer.Equals(x, result));

            }

            results = newUniqueResults;

            //hammered by palhas
            List<Result> newNewUniqueResults = new List<Result>();
            foreach (Result result1 in newUniqueResults)
            {
                if (result1.values.Count > 0)
                {
                    newNewUniqueResults.Add(result1);
                }
            }
            results = newNewUniqueResults;
        }

        private class GroupByComparer : IEqualityComparer<Result>
        {
            public bool Equals(Result x, Result y)
            {

                return x.dimension == y.dimension &&
                    Enumerable.SequenceEqual(x.values.OrderBy(t => t.Value), y.values.OrderBy(t => t.Value));

            }

            public int GetHashCode(Result obj)
            {
                int test1 = obj.dimension.GetHashCode();

                int test2 = 0;

                foreach (string value in obj.values.Select(x => x.Value))
                {

                    test2 += value.GetHashCode();
                }

                return test1 + test2;
            }
        }

        public void AddMetaData(string tag, string value)
        {

            metaData.Add(tag, value);

        }

    }
}