using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleSupporter.model
{
    internal class ParameterModel
    {
        internal string file { get; set; }
        internal string model { get; set; }
        internal string coor { get; set; }
        internal double confidence { get; set; }
        internal string output { get; set; }
        internal string subtitle { get; set; }
        internal bool qsvAccel { get; set; }
        internal bool localOCR { get; set; }
        internal int startTime { get; set; }
        internal int endTime { get; set; }
    }
}
