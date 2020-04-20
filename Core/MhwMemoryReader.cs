using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MhwOverlay.Config;
using MhwOverlay.Core.Helpers;
using MhwOverlay.Log;
using MhwOverlay.UI;

namespace MhwOverlay.Core
{
    public class MhwMemoryReader
    {
        public MainWindowModel mainModel { get; set; }
        private enum State
        {
            WaitingForProcess,
            PatternScanning,
            UpdatingMonsters
        }
        public MhwMemoryReader(MainWindowModel model)
        {
            processName = AppConfig.MemoryData.ProcessName;
            currentState = State.WaitingForProcess;
            mainModel = model;
            Start();
        }

        public void Start()
        {
            DoTask(GetMhwProcess, 0);
        }

        private string processName;
        private Process mhwProcess;
        private State currentState;
        private ulong monstersAddress;

        private void DoTask(Action task, int waitTime)
        {
            Thread.Sleep(waitTime);
            var workTask = Task.Run(task);
            workTask.ContinueWith(ProcessNextTask);
        }

        private void ProcessNextTask(Task task)
        {
            if (mhwProcess != null && mhwProcess.HasExited)
            {
                currentState = State.WaitingForProcess;
                Logger.Log(LogLevel.Info, $"MHW Exited");
            }
            switch (currentState)
            {
                case State.WaitingForProcess:
                    //Logger.Log(LogLevel.Info, $"Finding process.");
                    DoTask(GetMhwProcess, 1000);
                    break;
                case State.PatternScanning:
                    //Logger.Log(LogLevel.Info, "Finding monster pattern");
                    DoTask(FindMonsterPattern, 1000);
                    break;
                case State.UpdatingMonsters:
                    // Logger.Log(LogLevel.Info, "UpdateMonsters");
                    DoTask(UpdateMonsters, 1500);
                    break;
            }
        }

        private void GetMhwProcess()
        {
            var process = Process.GetProcessesByName(processName).FirstOrDefault();
            if (process != null && !process.HasExited)
            {
                mhwProcess = process;
                Logger.Log(LogLevel.Info, $"MHW process found. ID={mhwProcess.Id}");
                currentState = State.PatternScanning;
                return;
            }
        }

        private void FindMonsterPattern()
        {
            var bytePattern = new BytePattern(AppConfig.MemoryData.MonsterPattern);
            var result = MemoryHelper.FindPatternAddresses(mhwProcess, bytePattern);
            if (result != null)
            {
                Logger.Log(LogLevel.Info, "Monster memory pattern found.");
                monstersAddress = result.Value;
                currentState = State.UpdatingMonsters;
            }
        }

        private void UpdateMonsters()
        {
            var monsterRootPtr = MemoryHelper.LoadEffectiveAddressRelative(mhwProcess, monstersAddress) - 0x36CE0;

            var monsterBaseList = MemoryHelper.ReadMultiLevelPointer(false, mhwProcess, monsterRootPtr, 0x128, 0x8, 0x0);

            var list = MhwHelper.UpdateMonsters(mhwProcess, monsterBaseList);
            mainModel.CleanMonsterList(list);
            foreach (var monster in list)
                mainModel.AddOrUpdateMonster(monster);
        }
    }
}