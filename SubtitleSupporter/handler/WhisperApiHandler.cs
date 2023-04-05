using SubtitleSupporter.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json.Linq;
using System.Reflection;
using SubtitleSupporter.utils;

namespace SubtitleSupporter.handler
{
    internal class WhisperApiHandler
    {
        const string CONFIG_KEY = "WhisperApi.key";
        const long WHISPER_LIMIT_SIZE = 25 * 1024 * 1024;
        const int SPLIT_FILE_SIZE_IN_M = 20;
        internal ResultModel handler(string filePath, string model, int startTime, int endTime)
        {
            ResultModel result = new ResultModel();

            try
            {
                var finfo = new FileInfo(filePath);
                String audioFilePath = VideoUtils.exportAudio(filePath, startTime, endTime);

                if (string.IsNullOrEmpty(audioFilePath) || !CommonUtils.CheckFile(audioFilePath))
                {
                    result.errorMsg = "export audio failed!";
                    LogUtil.GetInstance().Error(result.errorMsg);
                    return result;
                }

                //if audio file still larger than limit size, split it
                if (new FileInfo(audioFilePath).Length >= WHISPER_LIMIT_SIZE)
                {
                    Dictionary<string, double> splittedFiles = VideoUtils.splitFile(audioFilePath, SPLIT_FILE_SIZE_IN_M);

                    foreach (KeyValuePair<string, double> pair in splittedFiles)
                    {
                        //call whisper api by each file with time offset
                        ResultModel tmpResult = callWhisperApi(pair.Key, model, pair.Value + (double)startTime / 1000.0);
                        if (!string.IsNullOrEmpty(tmpResult.errorMsg))
                        {
                            LogUtil.GetInstance().Error("call whisper api failed with file: " + pair.Key);
                            LogUtil.GetInstance().Error(tmpResult.errorMsg);
                            result.errorMsg = tmpResult.errorMsg;
                            break;
                        }
                        result.subtitleResult.AddRange(tmpResult.subtitleResult);
                    }
#if RELEASE
                    foreach (KeyValuePair<string, double> pair in splittedFiles)
                    {
                        try
                        {
                            File.Delete(pair.Key);
                        }
                        catch (Exception ex)
                        {
                            LogUtil.GetInstance().Warn("Delete file failed. " + ex.Message);
                        }

                    }
                    try
                    {
                        File.Delete(audioFilePath);
                    }
                    catch (Exception ex)
                    {
                        LogUtil.GetInstance().Warn("Delete file failed. " + ex.Message);
                    }
#endif
                }
                else
                {
                    result = callWhisperApi(audioFilePath, model, (double)startTime / 1000.0);
#if RELEASE
                    try
                    {
                        File.Delete(audioFilePath);
                    }
                    catch (Exception ex)
                    {
                        LogUtil.GetInstance().Warn("Delete file failed. " + ex.Message);
                    }
#endif
                }
            }
            catch (Exception ex)
            {
                LogUtil.GetInstance().Error("call whisper api failed");
                LogUtil.GetInstance().Error(ex);
            }

            return result;
        }
        private ResultModel callWhisperApi(string filePath, string model, double offset)
        {
            ResultModel result = new ResultModel();
            string key = CommonUtils.GetConfig(CONFIG_KEY);
            if (string.IsNullOrEmpty(key))
            {
                result.errorMsg = "No key! in config path: " + Path.Combine(Program.currentFolderPath, "SubtitleSuppoter.config");
                return result;
            }

            string url = "https://api.openai.com/v1/audio/transcriptions";
            Dictionary<string, string> header = new Dictionary<string, string>();
            header.Add("Authorization", "Bearer " + key);
            ResultModel response = new ResultModel();


            using (var multipart = new MultipartFormDataContent())
            {
                StringContent modelsize = new StringContent(model);
                modelsize.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
                modelsize.Headers.ContentDisposition.Name = "model";
                multipart.Add(modelsize);
                StringContent format = new StringContent("verbose_json");
                format.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
                format.Headers.ContentDisposition.Name = "response_format";
                multipart.Add(format);
                StringContent language = new StringContent("ja");
                language.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
                language.Headers.ContentDisposition.Name = "language";
                multipart.Add(language);



                var finfo = new FileInfo(filePath);
                var fileContent = new StreamContent(File.OpenRead(finfo.FullName));
                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("file")
                {
                    Name = "file",
                    FileName = "file" + finfo.Extension
                };

                multipart.Add(fileContent);
                response = CommonUtils.SendPostRequest(url, header, multipart);
            }

            if (!string.IsNullOrEmpty(response.errorMsg))
            {
                LogUtil.GetInstance().Error(response.errorMsg);
                result.errorMsg = response.errorMsg;
                return result;
            }

            //parse json
            result.subtitleResult = CommonUtils.ParseWhisperJson(response.resultString, offset);
            return result;
        }
    }
}
