using glm_api_call_test.api;
using glm_api_call_test.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace glm_api_call_test
{
    /// <summary>
    /// 
    /// </summary>
    public class TestApi
    {
        public TestApi(ILLM api)
        {
            this.api = api;
        }

        ILLM api;

        string GetBasePath()
        {
            var basePath = new DirectoryInfo("./").FullName;
            var ret = Path.Combine(basePath, "txt/chinadaily");
            if (Directory.Exists(ret))
            {
                return ret;
            }

            ret = Path.Combine(basePath, "../../../txt/chinadaily");
            if (Directory.Exists(ret))
            {
                return ret;
            }

            throw new Exception("not find txt/chinadaily");
        }
        public void Test()
        {
            var basePath = GetBasePath();
            StringBuilder logs = new StringBuilder();
            var outFolder = new DirectoryInfo(Path.Combine(basePath,"../" + api.Name)).FullName;
            Directory.CreateDirectory(outFolder);
            api.Translation("hello world.");
            

            foreach (var file in Directory.GetFiles(basePath, "*.txt"))
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
                var lines = File.ReadAllLines(file).Skip(1).ToList();
                var sb = new StringBuilder();
                Console.WriteLine($"file: {file}");
                var tpCount = api.GetTestTP().Length;
                var index = 0;

                foreach (var item in api.GetTestTP())
                {
                    api.SetTP(item);
                    
                    Console.WriteLine($"{index++}/{tpCount}");

                    var start = DateTime.Now;

                    Stopwatch stopwatch = Stopwatch.StartNew();

                    string trans = TxtSpilteAndCall(lines, (int)(api.MaxLength * 0.7),
                        api.Translation);

                    stopwatch.Stop();

                    var 翻译耗时 = stopwatch.Elapsed.TotalSeconds;
                    var infos = GPUInfoUtils.GetGpuInfos(start, DateTime.Now);

                    var 平均负债 = infos.Average(o => o.利用率);
                    var 已用内存 = infos.Average(o => o.已使用内存);
                    var 显卡功耗 = infos.Average(o => o.功耗);

                    start = DateTime.Now;
                    stopwatch.Restart();

                    string summary = TxtSpilteAndCall(trans.Split("\r\n").ToList(), (int)(api.MaxLength * 0.7), api.Summary);

                    stopwatch.Stop();
                    var 总结耗时 = stopwatch.Elapsed.TotalSeconds;

                    infos = GPUInfoUtils.GetGpuInfos(start, DateTime.Now);
                    var 总结平均负债 = infos.Average(o => o.利用率);
                    var 总结已用内存 = infos.Average(o => o.已使用内存);
                    var 总结显卡功耗 = infos.Average(o => o.功耗);
                    
                    sb.Append(trans);
                    sb.AppendLine();
                    sb.AppendLine("----总结----");
                    sb.AppendLine(summary);

                    sb.AppendLine("----性能----");
                    sb.AppendLine($"top_p:{item.top_p}  temperature: {item.temperature}");
                    sb.AppendLine($"原文长度:{lines.Sum(o => o.Length)}    译文长度:{trans.Length} 总结长度: {summary.Length}");
                    sb.AppendLine($"翻译耗时: {翻译耗时}sec 负载:{平均负债} 已用显存:{已用内存} 显卡功耗:{显卡功耗}");
                    sb.AppendLine($"总结耗时:{总结耗时}sec 负载:{总结平均负债} 已用显存:{总结已用内存} 显卡功耗:{总结显卡功耗}");
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
