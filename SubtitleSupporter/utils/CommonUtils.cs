using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SubtitleSupporter.model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleSupporter.utils
{
    internal class CommonUtils
    {

        internal static List<string> LaunchCommandLineApp(string exePath, string args, bool redirectError = false)
        {
            List<string> result = new List<string>();

            // Use ProcessStartInfo class
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = exePath;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = args;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = redirectError;
            string error = "";
            LogUtil.GetInstance().Info("run command: " + exePath + " " + args);
            using (Process exeProcess = Process.Start(startInfo))
            {
                while (!exeProcess.StandardOutput.EndOfStream)
                {
                    result.Add(exeProcess.StandardOutput.ReadLine());
                }

                if (redirectError)
                {
                    while (!exeProcess.StandardError.EndOfStream)
                    {
                        error += exeProcess.StandardError.ReadLine() + "\n";
                    }
                }

                exeProcess.WaitForExit();
                if (!string.IsNullOrEmpty(error))
                {
                    throw new Exception(error);
                }
            }
            return result;
        }

        internal static bool CheckProgram(string name, string parameter)
        {
            try
            {
                LaunchCommandLineApp(name, parameter, true);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        internal static void LoadConfig()
        {
            try
            {
                string[] content = File.ReadAllLines(Path.Combine(Program.currentFolderPath, "SubtitleSuppoter.config"));
                foreach (string line in content)
                {
                    string[] parts = line.Split('=');
                    Program.properties.Add(parts[0], parts[1]);
                }
            }
            catch 
            {
                LogUtil.GetInstance().Warn("load config failed");
            }

        }

        internal static string GetConfig(string key)
        {
            if (Program.properties.ContainsKey(key))
            {
                return Program.properties[key];
            } else
            {
                return "";
            }        
        }

        internal static ResultModel SendPostRequest(string url, Dictionary<string, string> header, HttpContent data)
        {
            var client = new HttpClient();
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
            };

            foreach (KeyValuePair<string, string> kvp in header)
            {
                httpRequestMessage.Headers.Add(kvp.Key, kvp.Value);
            }
            httpRequestMessage.Content = data;


            ResultModel result = new ResultModel();
            var response = client.Send(httpRequestMessage);
            if (!response.IsSuccessStatusCode)
            {
                result.errorMsg = response.ReasonPhrase + "\n";
                result.errorMsg += new StreamReader(response.Content.ReadAsStream(), Encoding.UTF8).ReadToEnd();
            }
            else
            {
                result.resultString = new StreamReader(response.Content.ReadAsStream(), Encoding.UTF8).ReadToEnd();
            }

            return result;
        }

        internal static void SelectFile(string filePath)
        {
            string argument = "/select, \"" + filePath + "\"";
            LaunchCommandLineApp("EXPLORER.EXE", argument);
        }

        internal static bool CheckFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }
            FileInfo fInfo = new FileInfo(filePath);
            if (fInfo.Length <= 0)
            {
                return false;
            }
            return true;
        }

        internal static string HandlePathForFFmpegFilter(string filePath)
        {
            return "'" + filePath.Replace("\\", "/").Replace(":", "\\:").Replace("'", "'\\\\\\''") + "'";
        }

        internal static void PrintSubtitle(ResultModel result)
        {
            if (!string.IsNullOrEmpty(result.errorMsg))
            {
                Console.WriteLine("ERROR: " + result.errorMsg);
            }
            else
            {
                if (result.subtitleResult == null || result.subtitleResult.Count == 0)
                {
                    Console.WriteLine("NO RESULT");
                }
                else
                {
                    foreach (SubtitleModel subtitle in result.subtitleResult)
                    {
                        Console.WriteLine("[" + subtitle.startTime + " --> " + subtitle.endTime + "] " + subtitle.text);
                    }

                }
            }
        }

        internal static List<SubtitleModel> ParseWhisperJson(string jsonString)
        {
            List<Dictionary<string, object>> segments = ((JArray)JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString)["segments"]).ToObject<List<Dictionary<string, object>>>();
            List<SubtitleModel> subtitleResult = new List<SubtitleModel>();
            foreach (Dictionary<string, object> segment in segments)
            {
                SubtitleModel subtitle = new SubtitleModel();
                subtitle.startTime = (double)segment["start"];
                subtitle.endTime = (double)segment["end"];
                subtitle.text = (string)segment["text"];
                subtitleResult.Add(subtitle);
            }
            return subtitleResult;
        }
    }
}
