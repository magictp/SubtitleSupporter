using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleSupporter.model
{
    internal class SubtitleModel
    {
        internal double startTime { get; set; }
        internal double endTime { get; set; }
        internal string? text { get; set; }
    }
}
