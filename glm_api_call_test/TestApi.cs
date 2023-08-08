using glm_api_call_test.api;
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

                var lines = File.ReadAllLines(file);

                var sb = new StringBuilder();
                // 第一行是文本的url地址，翻译前
                sb.AppendJoin("\r\n", lines.Skip(1));
                Stopwatch stopwatch = Stopwatch.StartNew();
                var source = sb.ToString();

                var trans = api.Translation(source);

                stopwatch.Stop();
                var 翻译耗时 = stopwatch.Elapsed.TotalSeconds;

                stopwatch.Restart();
                var s = api.Summary(trans);
                stopwatch.Stop();
                var 总结耗时 = stopwatch.Elapsed.TotalSeconds;


                sb.Clear();

                sb.Append(trans);
                sb.AppendLine();
                sb.AppendLine("----总结----");
                sb.AppendLine(s);

                sb.AppendLine("----性能----");
                sb.AppendLine($"原文长度:{source.Length}    译文长度:{trans.Length} 总结长度: {s.Length}");
                sb.AppendLine($"翻译耗时: {翻译耗时}sec  总结耗时:{总结耗时}sec");

                File.WriteAllText(outTxtFile, sb.ToString());
            }
        }
    }
}
