using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Watchdog.Processes
{
    public abstract class ProcessBase
    {
        /// <summary>
        /// 프로세스
        /// </summary>
        public virtual Process Process { get; protected set; }

        /// <summary>
        /// 프로세스 이름(log에 표시될 이름)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 프로세스 구분
        /// </summary>
        public abstract ProcessMode Mode { get; }

        /// <summary>
        /// 프로세스 버전
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 관련 프로세스
        /// </summary>
        public string[] RelatedProcesses { get; set; }

        /// <summary>
        /// 응답없음 대기시간
        /// </summary>
        public double RespondingTimeout { get; set; }

        /// <summary>
        /// 프로세스 상태
        /// </summary>
        public ProcessStatus Status { get; set; }

        public abstract (bool isStart, string error) Start();

        public abstract bool Stop();

        public abstract bool IsRunning();

        public abstract bool IsResponding();

        public bool IsResponding(int hWnd)
        {
            IntPtr ptr;
            var result = SendMessageTimeout(
                new HandleRef(hWnd, new IntPtr(hWnd)),
                0, // NULL, WM_NULL 메시지는 작동하지 않습니다. 수신자 창에서 무시할 메시지를 게시하려는 경우 응용 프로그램에서 WM_NULL 메시지를 보냅니다.
                IntPtr.Zero,
                IntPtr.Zero,
                (int)SMTOFlag.SMTO_NORMAL, //(int)SMTOFlag.SMTO_ABORTIFHUNG,
                (int)RespondingTimeout,
                out ptr);

            return result != IntPtr.Zero;
        }

        /// <summary>
        /// 파일경로를 통한 프로세스를 가져옵니다.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Process GetProcess(string fileName)
        {
            // 파일이름이 변경되면 안됨(파일명=프로세스명)
            var processName = Path.GetFileNameWithoutExtension(fileName);

            foreach (var process in Process.GetProcessesByName(processName))
            {
                var processPath = GetProcessPath(process);

                if (string.IsNullOrEmpty(processPath) == false)
                {
                    if (string.Compare(processPath, fileName, true) == 0)
                    {
                        return process;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 프로세스를 통한 경로를 가져옵니다.
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        public string GetProcessPath(Process process)
        {
            try
            {

                if (IntPtr.Size == 4) // 현재 프로세스가 32-bit인 경우
                {
                    if (Is64Bit(process))
                    {
                        // 32-bit 환경에서 64-bit 환경의 Process.MainModule 속성 접근시 Exception 발생하여
                        // 아래와 같은 함수를 호출하게 되었다.
                        string query = "SELECT ExecutablePath, ProcessID FROM Win32_Process";
                        ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

                        foreach (ManagementObject item in searcher.Get())
                        {
                            object id = item["ProcessID"];
                            object path = item["ExecutablePath"];

                            if (id != null && path != null)
                            {
                                return path.ToString();
                            }
                        }

                        // 찾지 못한경우
                        return null;
                    }
                }

                return process.MainModule.FileName;
            }
            catch // System Process 인 경우 예외 발생
            {
                return null;
            }
        }

        /// <summary>
        /// see https://msdn.microsoft.com/en-us/library/windows/desktop/ms684139%28v=vs.85%29.aspx 
        /// 
        /// ※ 가져온곳=https://stackoverflow.com/a/33206186
        /// </summary>
        /// <see cref=""/>
        /// <param name="process"></param>
        /// <returns></returns>
        public static bool Is64Bit(Process process)
        {
            if (!Environment.Is64BitOperatingSystem)
                return false;
            // if this method is not available in your version of .NET, use GetNativeSystemInfo via P/Invoke instead

            bool isWow64;
            if (!IsWow64Process(process.Handle, out isWow64))
                throw new Win32Exception();
            return !isWow64;
        }

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);

        [DllImport("user32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        /// <summary>
        /// 윈도우 핸들에 대한 프로세스 ID를 얻는다.
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        private uint GetProcessId(int hwnd)
        {
            uint processId = 0;

            GetWindowThreadProcessId(new IntPtr(hwnd), out processId);

            return processId;
        }

        public delegate bool EnumWindowsCallback(int hwnd, int lParam);

        /// <summary>
        /// 프로세스 ID로 윈도우 핸들을 얻는다.
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        public int GetWindowHandle(uint processId)
        {
            /*
             * 핸들을 각 창에 차례로 전달하여 응용 프로그램 정의 콜백 함수에 전달하여 화면의 모든 최상위 창을 열거합니다. 
             * EnumWindows 는 마지막 최상위 창이 열거되거나 콜백 함수가 FALSE를 반환 할 때까지 계속 됩니다.
             */

            /*
             * EnumWindows 사용방법 참고
             * https://crynut84.tistory.com/49
             */

            int rethWnd = 0;

            EnumWindowsCallback callback = (hwnd, lParam) =>
            {
                uint pid = GetProcessId(hwnd);

                if (pid == processId)
                {
                    rethWnd = hwnd;
                    // return false가 나올때 까지 해당 콜백이 계속 실행된다.
                    return false;
                }

                return true;
            };

            EnumWindows(callback, 0);

            return rethWnd;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumWindows(EnumWindowsCallback callback, int extraData);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageTimeout(
            HandleRef hWnd,
            int message,
            IntPtr wParam,
            IntPtr lParam,
            int flags,
            int timeout,
            out IntPtr pdwResult);

        public enum SMTOFlag : int
        {
            /// <summary>
            /// 수신 스레드가 응답하지 않거나 "멈춤"상태 인 경우 시간 종료 기간이 경과하기를 기다리지 않고 함수가 리턴됩니다.
            /// </summary>
            SMTO_ABORTIFHUNG = 0x0002,

            /// <summary>
            /// 함수가 리턴 할 때까지 호출 스레드가 다른 요청을 처리하지 못하게합니다.
            /// </summary>
            SMTO_BLOCK = 0x0001,

            /// <summary>
            /// 함수가 리턴되기를 기다리는 동안 호출 스레드는 다른 요청을 처리 할 수 ​​없습니다.
            /// </summary>
            SMTO_NORMAL = 0x0000,

            /// <summary>
            /// 수신 스레드가 메시지를 처리하는 한이 기능은 시간 종료 기간을 강제하지 않습니다.
            /// </summary>
            SMTO_NOTIMEOUTIFNOTHUNG = 0x0008,

            /// <summary>
            /// 메시지가 처리되는 동안 수신 창이 손상되거나 소유 스레드가 죽으면 함수는 0을 리턴해야합니다.
            /// </summary>
            SMTO_ERRORONEXIT = 0x0020,
        }
    }
}
