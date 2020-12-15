using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Watchdog.Processes
{
    public class ExeProcess : ProcessBase
    {
        public override ProcessMode Mode => ProcessMode.Exe;

        public string Arguments { get; set; }

        private string _ProcessPath;

        public string ProcessPath
        {
            get { return _ProcessPath; }
            set
            {
                if (_ProcessPath != value)
                {
                    _ProcessPath = value;
                }

                if (IsCheckFullPath == false)
                {
                    var fullPath = Path.GetFullPath(_ProcessPath);
                    if (fullPath != _ProcessPath)
                    {
                        //logger.Info($"[{Name}] Process path changed from \"{_ProcessPath}\" to full path \"{fullPath}\"");

                        _ProcessPath = fullPath;
                    }

                    IsCheckFullPath = true;
                }
            }
        }

        private bool IsCheckFullPath;

        public override (bool isStart, string error) Start()
        {
            // Running 상태이면 굳히 실행 할 필요가 없다.
            if (IsRunning())
            {
                return (true, string.Empty);
            }

            bool isStart = true;
            string error = string.Empty;

            try
            {
                if (string.IsNullOrWhiteSpace(Arguments))
                {
                    Process = System.Diagnostics.Process.Start(ProcessPath);
                }
                else
                {
                    Process = System.Diagnostics.Process.Start(ProcessPath, Arguments);
                }
            }
            catch (Exception ex)
            {
                //logger.Error(ex);

                isStart = false;
                error = ex.Message;
            }

            return (isStart, error);
        }

        public override bool Stop()
        {
            if (IsRunning())
            {
                Process.Kill();
            }

            return true;
        }

        public override bool IsRunning()
        {
            Process = GetProcess(ProcessPath);

            return Process != null;
        }

        public override bool IsResponding()
        {
            if (IsRunning() == false)
            {
                // 해당 함수를 호출하는 시점에 프로세스를 종료할 수 도 있다.
                //logger.Info($"[{Name}] Process is not running");
                return false;
            }

            var hwnd = GetWindowHandle((uint)Process.Id);

            return IsResponding(hwnd);
        }
    }
}
