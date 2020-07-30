using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualSuspect.Exception;
using VirtualSuspect.KnowledgeBase;

namespace VirtualSuspect.Query
{
    public class TimeEqualConditionPredicate : IConditionPredicate
    {

        private DateTime datetime;

        public TimeEqualConditionPredicate(string time)
        {

            datetime = DateTime.ParseExact(time, "dd/MM/yyyyTHH:mm:ss", CultureInfo.InvariantCulture);

        }

        public Predicate<EventNode> CreatePredicate()
        {
            return
                delegate (EventNode node)
                {

                    DateTime value = DateTime.Now;

                    if (node.Time.Type == "TimeInstant")
                    { //Example: dd/MM/yyyyTHH:mm:ss

                        value = DateTime.ParseExact(node.Time.Value, "dd/MM/yyyyTHH:mm:ss", CultureInfo.InvariantCulture);
                        return datetime.Equals(value);

                    }
                    else if (node.Time.Type == "TimeSpan")
                    {

                        DateTime valueBegin = DateTime.ParseExact(node.Time.Value.Split('>')[0], "dd/MM/yyyyTHH:mm:ss", CultureInfo.InvariantCulture);

                        DateTime valueEnd = DateTime.ParseExact(node.Time.Value.Split('>')[1], "dd/MM/yyyyTHH:mm:ss", CultureInfo.InvariantCulture);

                        return datetime >= valueBegin && datetime < valueEnd;
                        //throw new MessageFieldException("Cannot test equality between a Time Instant and a Time Span");

                    }

                    return false;

                };
        }

        public KnowledgeBaseManager.DimentionsEnum GetSemanticRole()
        {

            return KnowledgeBaseManager.DimentionsEnum.Time;

        }

        public List<string> GetValues()
        {

            List<string> entities = new List<string>();

            String date = "" + datetime.ToString("dd/MM/yyyyTHH:mm:ss");

            entities.Add(date);

            return entities;

        }
    }
}
