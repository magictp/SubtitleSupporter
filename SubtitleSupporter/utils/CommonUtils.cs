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

        internal static bool isNewSubtitle(String oldString, String newString, double confidence)
        {
            if (newString.Contains(oldString))
            {
                if (oldString == "")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (CommonUtils.GetDamerauLevenshteinDistance(oldString, newString) > confidence)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        private static double GetDamerauLevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
            {
                return 0;
            }

            if (string.IsNullOrEmpty(t))
            {
                return 0;
            }

            int n = s.Length; // length of s
            int m = t.Length; // length of t

            int[] p = new int[n + 1]; //'previous' cost array, horizontally
            int[] d = new int[n + 1]; // cost array, horizontally

            // indexes into strings s and t
            int i; // iterates through s
            int j; // iterates through t

            for (i = 0; i <= n; i++)
            {
                p[i] = i;
            }

            for (j = 1; j <= m; j++)
            {
                char tJ = t[j - 1]; // jth character of t
                d[0] = j;

                for (i = 1; i <= n; i++)
                {
                    int cost = s[i - 1] == tJ ? 0 : 1; // cost
                                                       // minimum of cell to the left+1, to the top+1, diagonally left and up +cost                
                    d[i] = Math.Min(Math.Min(d[i - 1] + 1, p[i] + 1), p[i - 1] + cost);
                }

                // copy current distance counts to 'previous row' distance counts
                int[] dPlaceholder = p; //placeholder to assist in swapping p and d
                p = d;
                d = dPlaceholder;
            }

            // our last action in the above loop was to switch d and p, so p now 
            // actually has the most recent cost counts
            return (double)(Math.Max(m, n) - p[n]) / (double)Math.Max(m, n);
        }
    }
}
