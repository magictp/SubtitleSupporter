using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleSupporter.utils
{
    internal class VideoUtils
    {
#if DEBUG
        static string ffmpegPath = "ffmpeg.exe";
        static string ffprobePath = "ffprobe.exe";
#else
        static string ffmpegPath = Path.Combine(Program.currentFolderPath, "ffmpeg.exe");
        static string ffprobePath = Path.Combine(Program.currentFolderPath, "ffprobe.exe");
#endif

        internal static string exportAudio(string videoPath)
        {
            string folder = Path.GetDirectoryName(videoPath);
            string fileName = Path.GetFileNameWithoutExtension(videoPath);
            string audioFilePath = Path.Combine(folder, fileName + "_" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + ".mp3");

            string command = "-y -i \"{0}\" -acodec libmp3lame  \"{1}\"";
            string parameter = string.Format(command, videoPath, audioFilePath);
            try
            {
                CommonUtils.LaunchCommandLineApp(ffmpegPath, parameter);
            }
            catch (Exception ex)
            {
                LogUtil.GetInstance().Error(ex);
                return "";
            }
            return audioFilePath;
        }


        internal static Dictionary<string, double> splitFile(string filePath, int limitFileSizeInM)
        {
            Dictionary<string, double> result = new Dictionary<string, double>();
            string folder = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);

            int fileCount = 1;
            FileInfo currentFileInfo = new FileInfo(filePath);
            double currentDuration = 0.0;
            while (currentFileInfo.Length > limitFileSizeInM * 1024 * 1024)
            {
                string newFilePath = Path.Combine(folder, fileName + "_" + fileCount + extension);
                fileCount++;
                //spilt file
                string command = "-y -ss {0} -i \"{1}\" -fs {2} -c copy \"{3}\"";
                string parameter = string.Format(command, currentDuration, filePath, limitFileSizeInM * 1024 * 1024, newFilePath);
                CommonUtils.LaunchCommandLineApp(ffmpegPath, parameter);
                if (!File.Exists(newFilePath))
                {
                    throw new Exception("split file failed");
                }
                currentFileInfo = new FileInfo(newFilePath);

                //get new file duration
                double duration = getVideoDuration(newFilePath);
                result.Add(newFilePath, currentDuration);
                currentDuration += duration;
            }

            return result;
        }

        internal static void combineVideoWithAss(string videoFilePath, string assFilePath, string outputFilePath, bool isQsvAccel)
        {

            //prepare ffmpeg parameter
            string command = "";
            if (isQsvAccel)
            {
                command += " -hwaccel_output_format qsv ";
            }
            command += " -i \"{0}\" -vf ass={1} ";
            if (isQsvAccel)
            {
                command += " -c:v h264_qsv ";

            }
            command += " \"{2}\"";
            assFilePath = CommonUtils.HandlePathForFFmpegFilter(assFilePath);
            string parameter = String.Format(command, videoFilePath, assFilePath, outputFilePath);
            CommonUtils.LaunchCommandLineApp(ffmpegPath, parameter);
        }

        internal static double getVideoDuration(string filePath)
        {
            string command = "-i \"{0}\"  -show_format -v quiet";
            string parameter = string.Format(command, filePath);
            List<string> resultString = CommonUtils.LaunchCommandLineApp(ffprobePath, parameter, true);
            string durationString = "";
            foreach (string s in resultString)
            {
                if (s.StartsWith("duration="))
                {
                    durationString = s.Replace("duration=", "");
                }
            }
            return double.Parse(durationString);
        }

        internal static void cropVideo(string videoPath, int startX, int startY, int endX, int endY, string newFilePath)
        {
            string command = "-y -i \"{0}\" -filter:v \"crop = {1}:{2}:{3}:{4}\" \"{5}\"";
            string parameter = string.Format(command, videoPath, endX - startX, endY - startY, startX, startY, newFilePath);
            CommonUtils.LaunchCommandLineApp(ffmpegPath, parameter);
        }

        internal static void getScene(string videoPath, string timeFilePath, double confidence)
        {
            string folder = Path.GetDirectoryName(videoPath);
            string command2 = "-i \"{0}\" -filter_complex \"select = 'gt(scene,{1})',metadata = print:file = {2}\" -vsync vfr \"{3}\"";
            string parameter2 = string.Format(command2, videoPath, confidence, CommonUtils.HandlePathForFFmpegFilter(timeFilePath), Path.Combine(folder, "img%05d.png"));
            CommonUtils.LaunchCommandLineApp(ffmpegPath, parameter2);
        }
    }
}
