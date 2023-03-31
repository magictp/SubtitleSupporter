using SubtitleSupporter.model;
using SubtitleSupporter.utils;

namespace SubtitleSupporter.handler
{
    internal class CombineHandler
    {
        internal ResultModel handler(string videoFilePath, string assFilePath, bool isQsvAccel)
        {
            ResultModel result = new ResultModel();
            try
            {
                //get path info
                string fileName = Path.GetFileNameWithoutExtension(videoFilePath);
                string folder = Path.GetDirectoryName(videoFilePath);
                string outputFileName = fileName + "_output_" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + ".mp4";
                string outputFilePath = Path.Combine(folder, outputFileName);

                //combine 
                VideoUtils.combineVideoWithAss(videoFilePath, assFilePath, outputFilePath, isQsvAccel);

                //check if combine succeed
                if (CommonUtils.CheckFile(outputFilePath))
                {
                    result.errorMsg = "combine video with subtitle failed!";
                    LogUtil.GetInstance().Error(result.errorMsg);
                    return result;
                }
                try
                {
                    CommonUtils.SelectFile(outputFilePath);
                }
                catch (Exception)
                {
                    LogUtil.GetInstance().Warn("Select to file failed");
                }
            }
            catch (Exception ex)
            {
                result.errorMsg = "combine video with subtitle failed!";
                LogUtil.GetInstance().Error(ex);
            }

            return result;
        }
    }
}
