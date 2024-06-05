using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace glm_api_call_test.utils
{
    class GPUInfo
    {
        public int 索引 { get; set; }
        public string 名称 { get; set; }
        public float 温度 { get; set; }
        public float 利用率 { get; set; }
        public float 内存利用率 { get; set; }
        
        /// <summary>
        /// 单位 M
        /// </summary>
        public float 总内存 { get; set; }

        /// <summary>
        /// 单位 M 
        /// </summary>
        public float 已使用内存 { get; set; }
        /// <summary>
        /// 单位 M 
        /// </summary>
        public float 可用内存 { get; set; }
        public float 功耗 { get; set; }
        public float 功耗限制 { get; set; }

        /// <summary>
        /// 创建时间，基本上等同于采集时间
        /// </summary>
        public DateTime Time { get; private set; } = DateTime.Now;
    }

    internal class GPUInfoUtils
    {
        static GPUInfo[] GetGPUInfos()
        {
            string query = "index,gpu_name,temperature.gpu,utilization.gpu,utilization.memory,memory.total,memory.used,memory.free,power.draw,power.limit";
            ProcessStartInfo psi = new ProcessStartInfo("nvidia-smi", "--query-gpu=" + query + " --format=csv,noheader,nounits");
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            Process process = Process.Start(psi);
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            string[] lines = output.Trim().Split('\n');
            GPUInfo[] gpuInfos = new GPUInfo[lines.Length];
            for (int i = 0; i < lines.Length; ++i)
            {
                string[] values = lines[i].Trim().Split(',');
                gpuInfos[i] = new GPUInfo();
                gpuInfos[i].索引 = int.Parse(values[0]);
                gpuInfos[i].名称 = values[1];
                gpuInfos[i].温度 = float.Parse(values[2]);
                gpuInfos[i].利用率 = float.Parse(values[3]);
                gpuInfos[i].内存利用率 = float.Parse(values[4]);
                gpuInfos[i].总内存 = float.Parse(values[5]);
                gpuInfos[i].已使用内存 = float.Parse(values[6]);
                gpuInfos[i].可用内存 = float.Parse(values[7]);
                gpuInfos[i].功耗 = float.Parse(values[8]);
                gpuInfos[i].功耗限制 = float.Parse(values[9]);
            }
            return gpuInfos;
        }


        static Thread recordThread;
        
        static bool recordThreadRnnStatus = false;

        static List<GPUInfo[]> gpuInfoQueue = new List<GPUInfo[]>();

        public static void StartRecord()
        {
            // 通过线程，间隔1s，使用 GetGPUInfos() 获取 GPU 信息，写入要给队列里

            if(recordThread  == null)
            {
                recordThread = new Thread(() =>
                {
                    recordThreadRnnStatus = true;
                    string query = "index,gpu_name,temperature.gpu,utilization.gpu,utilization.memory,memory.total,memory.used,memory.free,power.draw,power.limit";
                    ProcessStartInfo psi = new ProcessStartInfo("nvidia-smi", "-l 1 --query-gpu=" + query + " --format=csv,noheader,nounits");
                    psi.RedirectStandardOutput = true;
                    psi.UseShellExecute = false;
                    Process process = Process.Start(psi);
                    while (!process.StandardOutput.EndOfStream)
                    {
                        if (!recordThreadRnnStatus)
                            break;

                        string output = process.StandardOutput.ReadLine();
                        string[] values = output.Trim().Split(',');
                        GPUInfo gpuInfo = new GPUInfo();
                        gpuInfo.索引 = int.Parse(values[0]);
                        gpuInfo.名称 = values[1];
                        gpuInfo.温度 = float.Parse(values[2]);
                        gpuInfo.利用率 = float.Parse(values[3]);
                        gpuInfo.内存利用率 = float.Parse(values[4]);
                        gpuInfo.总内存 = float.Parse(values[5]);
                        gpuInfo.已使用内存 = float.Parse(values[6]);
                        gpuInfo.可用内存 = float.Parse(values[7]);
                        gpuInfo.功耗 = float.Parse(values[8]);
                        gpuInfo.功耗限制 = float.Parse(values[9]);
                        lock(gpuInfoQueue)
                        {
                            gpuInfoQueue.Add(new GPUInfo[] { gpuInfo });
                        }
                    }

                    process.Kill();
                    process.Dispose();
                    
                    Console.WriteLine("record thread is exit.");
                });
                recordThread.IsBackground = true;
                recordThread.Start();
            }
            else
            {
                Console.WriteLine("记录显存已启动");
            }
        }

        public static void StopRecord()
        {
            if (recordThread != null)
            {
                recordThreadRnnStatus = false;
                Thread.Sleep(1100);
                recordThread = null;
                GC.Collect();
            }
        }

        public static List<GPUInfo> GetGpuInfos(DateTime start, DateTime end)
        {
            // 根据开始时间和结束时间，获得当前监控记录的gpu信息

            // 1. 从队列里，找到开始时间和结束时间的记录
            // 2. 将这些记录合并，返回

            List<GPUInfo> gpuInfos;
            lock (gpuInfoQueue)
            {
                gpuInfos = gpuInfoQueue.SelectMany(x => x).Where(x => x.Time >= start && x.Time <= end).ToList();
            }
            if (gpuInfos.Count == 0)
                gpuInfos.Add(new GPUInfo() {
                });
            return gpuInfos;
        }
    }

}
