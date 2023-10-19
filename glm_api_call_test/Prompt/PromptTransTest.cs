using glm_api_call_test.api;
using glm_api_call_test.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace glm_api_call_test.Prompt
{
    /// <summary>
    /// 带提示词的翻译测试
    /// </summary>
    internal class PromptTransTest
    {
        public PromptTransTest(ILLM api)
        {
            this.api = api;
        }

        ILLM api;

        string GetSourceBasePath()
        {
            var basePath = new DirectoryInfo("./").FullName;
            var ret = Path.Combine(basePath, "Prompt/Source");
            if (Directory.Exists(ret))
            {
                return ret;
            }

            ret = Path.Combine(basePath, "../../../Prompt/Source");
            if (Directory.Exists(ret))
            {
                return ret;
            }

            throw new Exception("not find Prompt/Source");
        }

        string GetOutputBasePath()
        {
            var basePath = new DirectoryInfo("./").FullName;
            var ret = Path.Combine(basePath, "Prompt/Output");
            if (Directory.Exists(ret))
            {
                return ret;
            }

            ret = Path.Combine(basePath, "../../../Prompt/Output");
            if (Directory.Exists(ret))
            {
                return ret;
            }

            throw new Exception("not find Prompt/Output");
        }

        public string[] Prompt = new string[] { 
            "将下面段落翻译成中文，对于代码，指令，不要进行翻译，以下是正文：",
            "翻译下面段落成中文，不要翻译代码，指令。以下是正文： ",
            "Translate the following paragraph into Chinese. Do not translate the code and instructions. Here is the main text:",
        };

        public void Test()
        {
            var baseSourceFolder = GetSourceBasePath();
            var baseOutputFolder = GetOutputBasePath();

            StringBuilder logs = new StringBuilder();
            var outFolder = new DirectoryInfo(Path.Combine(baseOutputFolder, api.Name)).FullName;
            Directory.CreateDirectory(outFolder);
            api.Translation("hello world.");

            foreach (var file in Directory.GetFiles(Path.Combine(baseSourceFolder, "Article"), "*.txt"))
            {
                var name = new FileInfo(file).Name;
                var outTxtFile = Path.Combine(outFolder, name);

                if (File.Exists(outTxtFile))
                {
                    // 输出文件存在，则不再做测试，要重新测试，则需要手动先删除输出文件目录
                    Console.WriteLine($"out file:{outTxtFile} exists.");
                    continue;
                }

                // 第一行是文本的url地址，或者内容的来源，不参与翻译
                var lines = File.ReadAllLines(file).ToList();
                var sb = new StringBuilder();

                foreach (var item in api.GetTestTP().Take(1))
                {
                    api.SetTP(item);

                    var start = DateTime.Now;

                    Stopwatch stopwatch = Stopwatch.StartNew();

                    foreach(var prompt in Prompt)
                    {
                        sb.AppendLine("------ prompt ------");
                        sb.AppendLine(prompt);
                        sb.AppendLine("------ ------");

                        string trans = TxtSpilteAndCall(lines, (int)(api.MaxLength * 0.7),
                            (source) => { return api.PromptChat(prompt, source); });

                        sb.Append(trans);
                        sb.AppendLine();
                        sb.AppendLine("------ ------");
                        sb.AppendLine($"原文长度:{lines.Sum(o => o.Length)}    译文长度:{trans.Length}");
                        sb.AppendLine();

                        File.WriteAllText(outTxtFile, sb.ToString());
                    }

                    stopwatch.Stop();

                    var 翻译耗时 = stopwatch.Elapsed.TotalSeconds;
                    var infos = GPUInfoUtils.GetGpuInfos(start, DateTime.Now);
                    var 平均负债 = infos.Average(o => o.利用率);
                    var 已用内存 = infos.Average(o => o.已使用内存);
                    var 显卡功耗 = infos.Average(o => o.功耗);

                    start = DateTime.Now;
                    stopwatch.Restart();

                    sb.AppendLine("----性能----");
                    sb.AppendLine($"top_p:{item.top_p}  temperature: {item.temperature}");
                    sb.AppendLine($"翻译耗时: {翻译耗时}sec 负载:{平均负债} 已用显存:{已用内存} 显卡功耗:{显卡功耗}");
                    sb.AppendLine("");

                    File.WriteAllText(outTxtFile, sb.ToString());
                }

            }
        }

        private string TxtSpilteAndCall(List<string> lines, int spliteLength, Func<string, string> fun)
        {
            string ret = "";

            // 根据 api.MaxLength 对 lines 的内容做拆分，
            // 拆分后，调用 api.Translation 进行翻译，合并到 trans 里

            List<StringBuilder> transSource = new List<StringBuilder>();
            StringBuilder lastSB = new StringBuilder();
            transSource.Add(lastSB);

            foreach (var line in lines)
            {
                if (lastSB.Length + line.Length > spliteLength)
                {
                    lastSB = new StringBuilder();
                    transSource.Add(lastSB);
                }
                lastSB.AppendLine(line);
            }

            foreach (var tsb in transSource)
            {
                ret += fun(tsb.ToString()) + "\r\n";
            }

            return ret;
        }
    }
}
