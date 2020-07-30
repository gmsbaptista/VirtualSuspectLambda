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
    public class TimeBetweenConditionPredicate : IConditionPredicate
    {

        private DateTime beginTime;

        private DateTime endTime;

        public TimeBetweenConditionPredicate(string begin, string end)
        {

            beginTime = DateTime.ParseExact(begin, "dd/MM/yyyyTHH:mm:ss", CultureInfo.InvariantCulture);

            endTime = DateTime.ParseExact(end, "dd/MM/yyyyTHH:mm:ss", CultureInfo.InvariantCulture);

        }

        public Predicate<EventNode> CreatePredicate()
        {
            return
                delegate (EventNode node)
                {

                    if (node.Time.Type == "TimeInstant")
                    { //Example: dd/MM/yyyyTHH:mm:ss

                        DateTime value = DateTime.ParseExact(node.Time.Value, "dd/MM/yyyyTHH:mm:ss", CultureInfo.InvariantCulture);

                        return value >= beginTime && value <= endTime;

                    }
                    else if (node.Time.Type == "TimeSpan")
                    {//Example: dd/MM/yyyyTHH:mm:ss>dd/MM/yyyyTHH:mm:ss

                        String b = node.Time.Value.Split('>')[0];

                        DateTime valueBegin = DateTime.ParseExact(node.Time.Value.Split('>')[0], "dd/MM/yyyyTHH:mm:ss", CultureInfo.InvariantCulture);

                        DateTime valueEnd = DateTime.ParseExact(node.Time.Value.Split('>')[1], "dd/MM/yyyyTHH:mm:ss", CultureInfo.InvariantCulture);

                        return valueBegin <= endTime && valueEnd > beginTime;
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

            String date = "" + beginTime.ToString("dd/MM/yyyyTHH:mm:ss");

            date += ">";

            date += endTime.ToString("dd/MM/yyyyTHH:mm:ss");

            entities.Add(date);

            return entities;

        }
    }
}
