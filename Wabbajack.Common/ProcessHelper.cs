using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using AlphaPath = Alphaleonis.Win32.Filesystem.Path;

namespace Wabbajack.Common
{
    public class ProcessHelper
    {
        public enum StreamType
        {
            Output, 
            Error,
        }

        public AbsolutePath Path { get; set; }
        public AbsolutePath? WorkingDirectory { get; set; } = default;
        public IEnumerable<object> Arguments { get; set; } = Enumerable.Empty<object>();

        /// <summary>
        /// Starts the process in an enhanced compatability mode for applications like ModOrganizer2 that may be doing
        /// wonky things with Windows APIs and permissions. MO2 (being started via this code) can't launch Skyrim without
        /// this being set to true
        /// </summary>
        public bool WorkaroundMode { get; set; } = false;
        public bool LogError { get; set; } = true;
        
        public readonly Subject<(StreamType Type, string Line)> Output = new Subject<(StreamType Type, string)>();

        public bool ThrowOnNonZeroExitCode { get; set; } = false;


        public ProcessHelper()
        {
        }

        public async Task<int> Start()
        {
            var args = Arguments.Select(arg =>
            {
                return arg switch
                {
                    AbsolutePath abs => $"\"{abs}\"",
                    RelativePath rel => $"\"{rel}\"",
                    _ => arg.ToString()
                };
            });
            var info = new ProcessStartInfo
            {
                FileName = (string)Path,
                Arguments = string.Join(" ", args),
                RedirectStandardError = !WorkaroundMode,
                RedirectStandardInput = !WorkaroundMode,
                RedirectStandardOutput = !WorkaroundMode,
                UseShellExecute = WorkaroundMode,
                CreateNoWindow = !WorkaroundMode,
                LoadUserProfile = true,
            };

            if (WorkaroundMode)
                info.WindowStyle = ProcessWindowStyle.Normal;

            if (WorkingDirectory != default)
                info.WorkingDirectory = (string)WorkingDirectory!.Value;
            
            var finished = new TaskCompletionSource<int>();

            var p = new Process
            {
                StartInfo = info,
                EnableRaisingEvents = true,
            };
            EventHandler Exited = (sender, args) =>
            {
                finished.SetResult(p.ExitCode);
            };
            p.Exited += Exited;

            DataReceivedEventHandler OutputDataReceived = (sender, data) =>
            {
                if (string.IsNullOrEmpty(data.Data)) return;
                Output.OnNext((StreamType.Output, data.Data));
            };
            p.OutputDataReceived += OutputDataReceived;

            DataReceivedEventHandler ErrorEventHandler = (sender, data) =>
            {
                if (string.IsNullOrEmpty(data.Data)) return;
                Output.OnNext((StreamType.Error, data.Data));
                if (LogError)
                    Utils.Log($"{Path.FileName} ({p.Id}) StdErr: {data.Data}");
            };
            p.ErrorDataReceived += ErrorEventHandler;

            p.Start();
            if (!WorkaroundMode)
            {
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
            }

            ChildProcessTracker.AddProcess(p);

            try
            {
                p.PriorityClass = ProcessPriorityClass.BelowNormal;
            }
            catch (Exception)
            {
                // ignored
            }


            var result =  await finished.Task;
            p.CancelErrorRead();
            p.CancelOutputRead();
            p.OutputDataReceived -= OutputDataReceived;
            p.ErrorDataReceived -= ErrorEventHandler;
            p.Exited -= Exited;
            
            Output.OnCompleted();

            if (result != 0 && ThrowOnNonZeroExitCode)
                throw new Exception($"Error executing {Path} - Exit Code {result} - Check the log for more information - {string.Join(" ", args.Select(a => a!.ToString()))}");
            return result;
        }
        
    }
}
