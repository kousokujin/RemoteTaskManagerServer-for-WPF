using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RemoteTaskManager_for_WPF
{
    class performance
    {
        int core;   //CPUスレッド数
        ulong maxmem;   //最大メモリ(MB)

        public PerformanceCounter all_cpu; //全体のCPU使用率
        public PerformanceCounter[] thread_cpu = new PerformanceCounter[Environment.ProcessorCount];    //スレッドごとのCPU使用率
        public PerformanceCounter mem; //メモリ使用量

        public performance()
        {
            core = Environment.ProcessorCount;

            Microsoft.VisualBasic.Devices.ComputerInfo info = new Microsoft.VisualBasic.Devices.ComputerInfo();
            maxmem = info.TotalPhysicalMemory / 1000000;

            all_cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            //thread_cpu[core] = new PerformanceCounter();

            for (int i = 0; i < core; i++)
            {
                thread_cpu[i] = new PerformanceCounter();

                thread_cpu[i].CategoryName = "Processor";
                thread_cpu[i].CounterName = "% Processor Time";
                thread_cpu[i].InstanceName = i.ToString();
            }

            mem = new PerformanceCounter("Process", "Working Set", "_Total");
        }

        public ulong max_mem()
        {
            return maxmem;
        }

        public int cpu_count()
        {
            return core;
        }

        public int sisya(float input)
        {
            int output = (int)(input + 0.5);
            return output;
        }

        public int get_all_cpu()
        {
            float useage = all_cpu.NextValue();
            int output = sisya(useage);

            return output;
        }

        public int get_thread_cpu(int n)
        {
            float useage = thread_cpu[n].NextValue();
            int output = sisya(useage);

            return output;
        }

        public int get_use_mem()
        {
            float output = mem.NextValue();
            return (int)((output / 1000000) + 0.5);
        }

        public string get_string()
        {
            string out_str;

            out_str = string.Format("{0},{1}", get_use_mem(), get_all_cpu());

            for (int i = 0; i < core; i++)
            {
                string str = string.Format(",{0}", get_thread_cpu(i));
                out_str += str;
            }

            return out_str;
        }
    }
}
