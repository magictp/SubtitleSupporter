using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SubtitleSupporter.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubtitleSupporter.utils;

namespace SubtitleSupporter.handler
{
    internal class WhisperLocalHandler
    {
        internal ResultModel handler(string filePath, string model)
        {
            ResultModel result = new ResultModel();

            try
            {
                string pyPath = Path.Combine(Program.currentFolderPath, "WhisperLocal.py");
                string command = "\"{0}\" -f \"{1}\" -ms {2}";
                string parameter = string.Format(command, pyPath, filePath, model);
                List<string> resultList = CommonUtils.LaunchCommandLineApp("python", parameter, true);
                if (resultList.Count <= 0)
                {
                    return result;
                }

                //parse json
                result.subtitleResult = CommonUtils.ParseWhisperJson(resultList[0]);
            }
            catch (Exception ex)
            {
                result.errorMsg = "run local whisper failed!";
                LogUtil.GetInstance().Error(ex);
            }
            return result;
        }
    }
}
