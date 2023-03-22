using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        public class SystemLogger : ILogger
        {

            private FileHandler _file;
            private IMyTextSurface _surface;

            public SystemLogger(FileHandler file, IMyTextSurface surface = null)
            {
                _file = file;
                _surface = surface;

                if (file.Exist("Logs"))
                    Logs = file.ReadAllText("Logs", false).Split('\n').ToList();
            }

            public List<string> Logs = new List<string>();

            public void Log(LogTier tier, object source, string message)
            {
                Logs.Add($"{DateTime.Now} - {tier} - {message} - {source}");
                _file.WriteAllText("Logs", string.Join("\n", Logs), false);
            }
        }
    }
}
