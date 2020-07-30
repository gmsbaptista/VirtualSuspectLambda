using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.Query;

namespace VirtualSuspect.Handler
{
    class FilterAnswerHandler : IPosHandler
    {

        private VirtualSuspectQuestionAnswer virtualSuspect;

        public FilterAnswerHandler(VirtualSuspectQuestionAnswer virtualSuspect)
        {

            this.virtualSuspect = virtualSuspect;

        }

        public QueryResult Modify(QueryResult result)
        {

            //If the Lie Strategy is to Hide the real Answer then its content
            //should be hiden and replaced by an negative answer
            if (virtualSuspect.Strategy == VirtualSuspectQuestionAnswer.LieStrategy.Hide)
            {

                QueryResult newResult = new QueryResult(result.Query);

                //TODO: Select best negative answer type
                Random generator = new Random(DateTime.Now.Millisecond);
                String negativeAnswerType = (generator.NextDouble() < 0.5) ? "non-remember" : "denial";
                newResult.AddMetaData("negative-answer", negativeAnswerType);

                return newResult;

            }

            return result;

        }
    }
}
