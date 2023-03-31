using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleSupporter.model
{
    internal class ResultModel
    {
        internal string? errorMsg { get; set; }
        internal List<SubtitleModel>? subtitleResult { get; set; }
        internal string? resultString { get; set; }

        internal ResultModel()
        {
            subtitleResult = new List<SubtitleModel>();
        }
    }
}
