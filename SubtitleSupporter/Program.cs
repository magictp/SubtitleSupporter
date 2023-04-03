using SubtitleSupporter.handler;
using SubtitleSupporter.model;
using SubtitleSupporter.utils;
using System.CommandLine;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text;

namespace SubtitleSupporter;
class Program
{
    internal static Dictionary<string, string> properties = new Dictionary<string, string>();

    internal static string currentFolderPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

    static async Task Main(string[] args)
    {
        CommonUtils.LoadConfig();
        Console.OutputEncoding = Encoding.UTF8;

        var rootCommand = new RootCommand();
        var handlerOption = new Option<string>("--handler", "The hanlder that need to run[ WhisperLocal | WhisperApi | N46Whisper | CurrentSub | easyOCR |...]");
        handlerOption.AddAlias("-h");

        var fileOption = new Option<string>("--file","The media filepath that need to handle");
        fileOption.AddAlias("-f");

        var modelOption = new Option<string>("--model", ()=> "medium", "The model used in whisper handlers");
        modelOption.AddAlias("-m");

        var coorOption = new Option<string>("--coor", "The coordinate of current subtitle in format x1,y1,x2,y2 where x1,y1 are for top-left corner and x2,y2 are for bottom-right corner");
        coorOption.AddAlias("-coor");

        var confidenceOption = new Option<double>("--confidence", () => 0.1, "The confidence used in CurrentSub handler");
        confidenceOption.AddAlias("-conf");

        var outputFilePathOption = new Option<string>("--output", "The output filepath. If null will show result in commandline");
        outputFilePathOption.AddAlias("-o");

        var subtitleOption = new Option<string>("--subtitle", "The subtitle filepath that need to combine");
        subtitleOption.AddAlias("-ass");

        var qsvAccelOption = new Option<bool>("--qsvAccel", () => false, "If qsv accel will be used, default is false");
        qsvAccelOption.AddAlias("-qsv");

        var localOCROption = new Option<bool>("--localOCR", () => false, "If run local ocr, default is false");
        localOCROption.AddAlias("-locr");

        rootCommand.Add(handlerOption);
        rootCommand.Add(fileOption);
        rootCommand.Add(modelOption);
        rootCommand.Add(confidenceOption);
        rootCommand.Add(outputFilePathOption);
        rootCommand.Add(subtitleOption);
        rootCommand.Add(qsvAccelOption);
        rootCommand.Add(coorOption);
        rootCommand.Add(localOCROption);

        rootCommand.SetHandler((handlerOptionValue, parameter) =>
        {
            ResultModel result = new ResultModel();
            switch(handlerOptionValue)
            {
                case "WhisperLocal":
                    result = new WhisperLocalHandler().handler(parameter.file, parameter.model);
                    CommonUtils.PrintSubtitle(result);
                    break;
                case "WhisperApi":
                    result = new WhisperApiHandler().handler(parameter.file, parameter.model);
                    CommonUtils.PrintSubtitle(result);
                    break;
                case "N46Whisper":
                    break;
                case "CurrentSub":
                    result = new CurrentSubHandler().handler(parameter.file, parameter.coor, parameter.confidence, parameter.localOCR);
                    CommonUtils.PrintSubtitle(result);
                    break;
                case "EasyOCR":
                    break;
                case "Combine":
                    result = new CombineHandler().handler(parameter.file, parameter.subtitle, parameter.qsvAccel);
                    if (!string.IsNullOrEmpty(result.errorMsg))
                    {
                        Console.WriteLine("ERROR: " + result.errorMsg);
                    } else
                    {
                        Console.WriteLine(result.resultString);
                    }
                    break;
                default:
                    break;
            }

            
#if DEBUG
            Console.ReadKey();
#endif

        }, handlerOption, new ParameterBinder(fileOption, modelOption, coorOption, confidenceOption, outputFilePathOption, subtitleOption, qsvAccelOption,localOCROption));

        await rootCommand.InvokeAsync(args);
        
    }


}
