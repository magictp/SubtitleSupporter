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
        internal ResultModel handler(string filePath, string model, int startTime, int endTime)
        {
            ResultModel result = new ResultModel();
            String audioFilePath = "";
            try
            {
                audioFilePath = VideoUtils.exportAudio(filePath, startTime, endTime);

                if (string.IsNullOrEmpty(audioFilePath) || !CommonUtils.CheckFile(audioFilePath))
                {
                    result.errorMsg = "export audio failed!";
                    LogUtil.GetInstance().Error(result.errorMsg);
                    return result;
                }
                string pyPath = Path.Combine(Program.currentFolderPath, "WhisperLocal.py");
                string command = "\"{0}\" -f \"{1}\" -ms {2}";
                string parameter = string.Format(command, pyPath, audioFilePath, model);
                List<string> resultList = CommonUtils.LaunchCommandLineApp("python", parameter, true);
                if (resultList.Count <= 0)
                {
                    return result;
                }

                //parse json
                result.subtitleResult = CommonUtils.ParseWhisperJson(resultList[0], (double)startTime / 1000.0);
            }
            catch (Exception ex)
            {
                result.errorMsg = "run local whisper failed!";
                LogUtil.GetInstance().Error(ex);
            }
            finally
            {
#if RELEASE
                try
                {
                    if(!string.IsNullOrEmpty(audioFilePath))
                    {
                        File.Delete(audioFilePath);
                    }
                }
                catch (Exception ex)
                {
                    LogUtil.GetInstance().Warn("Delete file failed. " + ex.Message);
                }
#endif
            }
            return result;
        }
    }
}
