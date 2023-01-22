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
        #region mdk preserve

        //----------- EDIT YOUR SYSTEM COLOR SCHEME HERE -----------\\

        public class MetaData
        {
            static public Color Theme = new Color(0, 35, 145, 255);
            static public Color BackgroundColor = new Color(16, 16, 16, 255);
            static public Color CanvasColor = new Color(32, 32, 32, 255);
            static public Color FactionColor = new Color(21, 0, 25);
            static public string FactionIcon = "Textures\\FactionLogo\\Others\\OtherIcon_18.dds";
        }

        //------------- INSTALED PROGRAMS SHORTCUT HERE -------------\\

        void Shortcuts()
        {
            Programs.Add("DEBUG", new Action<Window, byte>(Debug));
            Programs.Add("MEDIA", new Action<Window, byte>(new MediaPlayer().Initialize));
            Programs.Add("MATH", new Action<Window, byte>(new MathVisualizer().Initialize));
            Programs.Add("DIAG", new Action<Window, byte>(new DiagInterface(Blocks, Me.CubeGrid).Initialize));
            Programs.Add("SANDBOX", new Action<Window, byte>(new UISandbox().Initialize));
        }

        //-------------- DO NOT TOUCH BELLOW THIS LINE --------------\\
        //-------- (Or touch, I'm just a comment, Not a cop) --------\\

        #endregion
        #region LocalStorage

        List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock> { };
        int BlockNumber;
        List<IMyShipController> Helms = new List<IMyShipController> { };
        List<IMyTextPanel> Panels = new List<IMyTextPanel> { };
        List<IMyShipToolBase> Mouse = new List<IMyShipToolBase> { };
        List<Workstation> MyWorkstations = new List<Workstation> { };
        public List<IMyMessageProvider> CommunicationSystems = new List<IMyMessageProvider>();
        int clock;
        MyIni InternalStorage;
        Dictionary<string, Action<Window, byte>> Programs = new Dictionary<string, Action<Window, byte>>();

        #endregion
        #region Ultilities

        public class Vector2Point
        {
            public Vector2 Vector { get; }
            public Point Point { get; }
            public Vector2Point(Point P)
            {
                Vector = new Vector2(P.X, P.Y);
            }
            public Vector2Point(Vector2 P)
            {
                Point = new Point((int)P.X, (int)P.Y);
            }
        }

        #endregion

        public Program()
        {
            InternalStorage = new MyIni();
            if (Storage == "" || !InternalStorage.TryParse(Storage))
            {
                Storage = $"[TOS]\nInstalation={DateTime.Now}\n[TOS-SystemRegister]\n[TOS-Hardware]\n[TOS-Network]\n[TOS-Storage]";
                InternalStorage.TryParse(Storage);
            };
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            ReloadGridTerminalSystem();
            IMyBroadcastListener RxB = IGC.RegisterBroadcastListener(Me.CubeGrid.CustomName);
            IMyUnicastListener RxU = IGC.UnicastListener;
            RxB.SetMessageCallback("RxB");
            RxU.SetMessageCallback("RxU");
            CommunicationSystems.Add(RxB);
            CommunicationSystems.Add(RxU);
            Shortcuts();
        }
        void ReloadGridTerminalSystem()
        {
            Blocks.Clear();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(Blocks);
            if (BlockNumber != Blocks.Count())
            {
                BlockNumber = Blocks.Count();
                foreach (IMyTerminalBlock block in Blocks)
                {
                    if (block is IMyShipController) Helms.Add((IMyShipController)block);
                    if (block is IMyShipToolBase) Mouse.Add((IMyShipToolBase)block);
                    if (block is IMyTextPanel) { Panels.Add((IMyTextPanel)block); }
                }
                Helms.Sort((A, B) => A.CustomName.CompareTo(B.CustomName));
                foreach (IMyShipController Helm in Helms)
                    if (Helm.CustomName.Contains("[TOS:"))
                    {
                        string[] Str = Helm.CustomName.Split('[');
                        foreach (IMyTextPanel Panel in Panels)
                            if (Panel.CustomName.Contains(Str[1]))
                            {
                                Workstation Station = new Workstation(new MultiScreenSpriteSurface(Panel), Helm);
                                List<IMyTerminalBlock> ExtraBlocks = new List<IMyTerminalBlock>();
                                IMyBlockGroup BlockGroup = GridTerminalSystem.GetBlockGroupWithName("[" + Str[1]);

                                if (BlockGroup != null) BlockGroup.GetBlocks(ExtraBlocks);
                                foreach (IMyTerminalBlock Block in ExtraBlocks)
                                {
                                    if (Block is IMySoundBlock) { Station.Sound.Add((IMySoundBlock)Block); }
                                    if (Block is IMyShipToolBase) { Station.Tool = (IMyShipToolBase)Block; }
                                }
                                GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(ExtraBlocks, a => a.CustomName.Contains(Str[1]));
                                foreach (IMyTerminalBlock Block in ExtraBlocks)
                                {
                                    if (Block is IMySoundBlock) { Station.Sound.Add((IMySoundBlock)Block); }
                                    if (Block is IMyShipToolBase) { Station.Tool = (IMyShipToolBase)Block; }
                                }
                                MyWorkstations.Add(Station);
                            }
                    }
            }
            SystemMetrics();
            Me.GetSurface(0).ContentType = ContentType.SCRIPT;
        }
        public void Save()
        {
            Storage = InternalStorage.ToString();
        }

        public void Main(string argument, UpdateType updateSource)
        {

            clock++;

            NetworkUpdate(updateSource, argument);


            var UpdateWorkstation = MyWorkstations.ToList();
            foreach (Workstation MyWorkstation in UpdateWorkstation)
            {
                try
                {
                    MyWorkstation.Update();
                }
                catch (Exception Error)
                {
                    new BSoD(MyWorkstation, Error);
                    MyWorkstation.Controller.CustomData = Error.ToString();
                    MyWorkstations.Remove(MyWorkstation);
                }
            }
            if (argument != null)
            {
                string[] Args = argument.Split(':');
                Args[0] = Args[0].ToUpper();

                try
                {
                    switch (Args[0])
                    {
                        case "FORMAT": Storage = ""; InternalStorage.Clear(); break;
                        case "NEW":
                            {
                                Args[1] = Args[1].ToUpper();
                                MyWorkstations[int.Parse(Args[2])].Windows.Add(new Window(MyWorkstations[0], Args[1], Programs.GetValueOrDefault(Args[1]))); break;
                            }
                    }
                    if (argument.Contains("RESTART ")) foreach (IMyShipController Helm in Helms)
                            if (Helm.CustomName.Contains(argument.Split(' ')[1]))
                            {
                                string[] Str = Helm.CustomName.Split('[');
                                foreach (IMyTextPanel Panel in Panels)
                                    if (Panel.CustomName.Contains(Str[1]))
                                    {
                                        Workstation Station = new Workstation(new MultiScreenSpriteSurface(Panel), Helm);
                                        List<IMyTerminalBlock> ExtraBlocks = new List<IMyTerminalBlock>();
                                        IMyBlockGroup BlockGroup = GridTerminalSystem.GetBlockGroupWithName("[" + Str[1]);
                                        if (BlockGroup != null) BlockGroup.GetBlocks(ExtraBlocks);
                                        foreach (IMyTerminalBlock Block in ExtraBlocks)
                                        {
                                            if (Block is IMySoundBlock) { Station.Sound.Add((IMySoundBlock)Block); }
                                            if (Block is IMyShipToolBase) { Station.Tool = (IMyShipToolBase)Block; }
                                        }
                                        GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(ExtraBlocks, a => a.CustomName.Contains(Str[1]));
                                        foreach (IMyTerminalBlock Block in ExtraBlocks)
                                        {
                                            if (Block is IMySoundBlock) { Station.Sound.Add((IMySoundBlock)Block); }
                                            if (Block is IMyShipToolBase) { Station.Tool = (IMyShipToolBase)Block; }
                                        }
                                        MyWorkstations.Add(Station);
                                    }
                            }
                }
                catch (Exception Error) { Echo($"Argument Parse Error:{Error}"); }
            }
            SystemMetrics();
        }

        string HWText;
        public class TelemetryData
        {
            public List<int> Instructions { get; set; }
            public List<double> Runtime { get; set; }
            public int TotalFrameCount { get; set; }
            public TelemetryData() { Runtime = new List<double> { }; Instructions = new List<int> { }; }
        }
        TelemetryData Telemetry = new TelemetryData();


        void SystemMetrics()
        {
            int TotalFrameCount = 0;
            Telemetry.Runtime.Add(Runtime.LastRunTimeMs);
            if (Telemetry.Runtime.Count > 100) Telemetry.Runtime.RemoveAt(0);
            Telemetry.Instructions.Add(Runtime.CurrentInstructionCount);
            double Max = Telemetry.Instructions.Max(a => a);
            foreach (Workstation WS in MyWorkstations) { TotalFrameCount += WS.Screen.SpriteCount; }
            if (TotalFrameCount > 0) { Telemetry.TotalFrameCount = TotalFrameCount; }
            if (Telemetry.Instructions.Count > 100) Telemetry.Instructions.RemoveAt(0);
            HWText = "CPU Usage:" + (Max / Runtime.MaxInstructionCount).ToString("P") + "\n"
                   + "Max Instruction: " + Max + " / " + Runtime.MaxInstructionCount + "\n"
                   + "Average Instruction:" + Telemetry.Instructions.Average().ToString("0") + "\n"
                   + "Last runtime: " + Telemetry.Runtime.Last() + "\n"
                   + "Max runtime: " + Telemetry.Runtime.Max(a => a);
        }
        public class About
        {
            public string Version = "Alpha 0.3.0";
            public string Edition = "ShipWide Edition";
            public bool Activated = false;
            public bool Debug = true;
        }
    }
}
