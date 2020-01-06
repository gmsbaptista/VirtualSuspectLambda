using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualSuspect.KnowledgeBase
{
    public interface IStoryNode
    {

        uint ID { get; }

        string Value { get; }

    }
}
