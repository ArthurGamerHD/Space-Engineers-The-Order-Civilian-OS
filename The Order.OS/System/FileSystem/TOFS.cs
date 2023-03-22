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
        public class FileHandler
        {
            private Program _program;
            public ILogger logger;
            string Storage
            {
                get
                {
                    return _program.Storage;
                }
                set
                {
                    _program.Storage = value;
                }
            }

            public FileHandler(Program program)
            {
                _program = program;
            }

            public string ReadAllText(string Path, bool allowLog = true)
            {
                var list = Storage.Split('¶').ToList();
                var index = list.FindIndex(A => A.Split('\n')[0] == Path);
                if (index == -1)
                {
                    if (allowLog)
                        logger?.Log(LogTier.critical, this, $"File path {Path} doesn't exist");
                    throw new Exception($"File path {Path} doesn't exist");
                }
                string[] lines = list[index].Split('\n');
                if (allowLog)
                    logger?.Log(LogTier.info, this, $"loaded file {Path}");
                return string.Join("\n", lines.Skip(1));
            }
            public void WriteAllText(string Path, string content, bool allowLog = true)
            {

                var list = Storage.Split('¶').ToList();

                var index = list.FindIndex(A => A.Split('\n')[0] == Path);
                if (index != -1)
                {
                    list[index] = $"{Path}\n{content}";
                    if (allowLog)
                        logger?.Log(LogTier.info, this, $"file {Path} overrided");
                }
                else
                {
                    list.Add($"{Path}\n{content}");
                    if (allowLog)
                        logger?.Log(LogTier.info, this, $"file {Path} created");
                }

                Storage = string.Join("¶", list);
            }

            public bool Exist(string FileName)
            {
                var list = Storage.Split('¶').ToList();
                var index = list.FindIndex(A => A.Split('\n')[0] == FileName);
                return index != -1;
            }

        }
    }
}
