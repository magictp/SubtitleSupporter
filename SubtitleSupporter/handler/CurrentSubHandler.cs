using SubtitleSupporter.model;
using SubtitleSupporter.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleSupporter.handler
{
    internal class CurrentSubHandler
    {
        internal ResultModel handler(string filePath, string coor, double confidence)
        {
            string tmpFolder = "";
            ResultModel result = new ResultModel();
            try
            {
                //check coor
                string[] coorStrings = coor.Split(',');
                if (coorStrings.Length != 4)
                {
                    result.errorMsg = "The coordinate should be in format x1,y1,x2,y2 where x1,y1 are for top-left corner and x2,y2 are for bottom-right corner";
                    return result;
                }
                int x1 = 0;
                int y1 = 0;
                int x2 = 0;
                int y2 = 0;
                try
                {
                    x1 = int.Parse(coorStrings[0]);
                    y1 = int.Parse(coorStrings[1]);
                    x2 = int.Parse(coorStrings[2]);
                    y2 = int.Parse(coorStrings[3]);
                }
                catch (Exception)
                {
                    result.errorMsg = "The coordinate should be in format x1,y1,x2,y2 where x1,y1,x2,y2 are all integer";
                    return result;
                }
                if (x1 < 0 || x2 <= 0 || y1 < 0 || y2 < 0)
                {
                    result.errorMsg = "The coordinate should be in format x1,y1,x2,y2 where x1,y1,x2,y2 are all positive";
                    return result;
                }

                if (x1 >= x2 || y1 >= y2)
                {
                    result.errorMsg = "The coordinate should be in format x1,y1,x2,y2 where x1 should be less than x2 and y1 should be less than y2";
                    return result;
                }

                //check confidence
                if (confidence <= 0 || confidence > 1)
                {
                    result.errorMsg = "The confidence should between 0 and 1";
                    return result;
                }

                //prepare file path
                string folder = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileName(filePath);
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                tmpFolder = Path.Combine(folder, fileNameWithoutExt + "_" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString());
                Directory.CreateDirectory(tmpFolder);
                string croppedVideoPath = Path.Combine(tmpFolder, "cropped_" + fileName);
                string timeFilePath = Path.Combine(tmpFolder, "time.txt");

                //crop file
                VideoUtils.cropVideo(filePath, x1, y1, x2, y2, croppedVideoPath);
                if (!CommonUtils.CheckFile(croppedVideoPath))
                {
                    result.errorMsg = "Crop video failed";
                    LogUtil.GetInstance().Error(result.errorMsg);
                    return result;
                }

                //get scene
                VideoUtils.getScene(croppedVideoPath, timeFilePath, confidence);
                if (!CommonUtils.CheckFile(timeFilePath))
                {
                    result.errorMsg = "get scene info failed";
                    LogUtil.GetInstance().Error(result.errorMsg);
                    return result;
                }

                //handle time file
                List<SubtitleModel> subtitleResult = new List<SubtitleModel>();
                String[] lines = File.ReadAllLines(timeFilePath);
                foreach (String line in lines)
                {
                    if (line.StartsWith("frame:"))
                    {
                        String[] info = line.Replace("   ", " ").Split(' ');
                        int id = Int32.Parse(line.Substring(0, line.IndexOf(" ")).Replace("frame:", ""));
                        String startTime = line.Substring(line.IndexOf("pts_time:")).Replace("pts_time:", "");
                        double startPtsTime = Double.Parse(startTime);

                        SubtitleModel subtitle = new SubtitleModel();
                        subtitle.startTime = startPtsTime;
                        subtitleResult.Add(subtitle);
                    }
                }
                for (int i = 0; i < subtitleResult.Count - 1; i++)
                {
                    subtitleResult[i].endTime = subtitleResult[i + 1].startTime;
                }
                if (subtitleResult.Count > 0)
                {
                    try
                    {
                        subtitleResult[subtitleResult.Count - 1].endTime = VideoUtils.getVideoDuration(croppedVideoPath);
                    }
                    catch (Exception ex)
                    {
                        result.errorMsg = "get scene info failed";
                        LogUtil.GetInstance().Error(result.errorMsg);
                        LogUtil.GetInstance().Error(ex);
                        return result;
                    }

                }
                result.subtitleResult = subtitleResult;
                return result;
            }
            catch (Exception ex)
            {
                result.errorMsg = "get current subtitle failed";
                LogUtil.GetInstance().Error(result.errorMsg);
                LogUtil.GetInstance().Error(ex);
                return result;
            }
            finally
            {
#if RELEASE
                if(!string.IsNullOrEmpty(tmpFolder) && Directory.Exists(tmpFolder))
                {
                    Directory.Delete(tmpFolder, true);
                }
#endif
            }
            
        }
    }
}
