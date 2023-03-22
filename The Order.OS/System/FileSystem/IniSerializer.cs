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
        public class IniPseudoSerializer
        {
            public static T Deserialize<T>(string FileName, string name, MyIni ini)
            {
                List<string> names = new List<string>();
                ini.GetSections(names);
                if (names.Contains(FileName) && ini.ContainsKey(FileName, name))
                {
                    MyIniValue value = ini.Get(FileName, name);
                    switch (typeof(T).ToString())
                    {
                        case "System.Boolean": return (T)Convert.ChangeType(value.ToBoolean(), typeof(T));
                        case "System.Byte": return (T)Convert.ChangeType(value.ToByte(), typeof(T));
                        case "System.Char": return (T)Convert.ChangeType(value.ToChar(), typeof(T));
                        case "System.Decimal": return (T)Convert.ChangeType(value.ToDecimal(), typeof(T));
                        case "System.Double": return (T)Convert.ChangeType(value.ToDouble(), typeof(T));
                        case "System.Int16": return (T)Convert.ChangeType(value.ToInt16(), typeof(T));
                        case "System.Int32": return (T)Convert.ChangeType(value.ToInt32(), typeof(T));
                        case "System.Int64": return (T)Convert.ChangeType(value.ToInt64(), typeof(T));
                        case "System.SByte": return (T)Convert.ChangeType(value.ToSByte(), typeof(T));
                        case "System.Single": return (T)Convert.ChangeType(value.ToSingle(), typeof(T));
                        case "System.UInt16": return (T)Convert.ChangeType(value.ToUInt16(), typeof(T));
                        case "System.UInt32": return (T)Convert.ChangeType(value.ToUInt32(), typeof(T));
                        case "System.UInt64": return (T)Convert.ChangeType(value.ToUInt64(), typeof(T));
                        case "System.String": return (T)Convert.ChangeType(value.ToString(), typeof(T));
                    }
                    throw new Exception($"Type {typeof(T)} is not allowed");
                }
                return default(T);
            }

            public static void Serialize<T>(string FileName, string name, ref T value, ref MyIni ini)
            {
                ini.AddSection(FileName);
                ini.Set(FileName, name, $"{value}");
            }
        }

        public interface ISettingsPseudoSerializable
        {
            void Serialize(ref MyIni ini);
            void Deserialize(MyIni ini);
        }

    }
}
