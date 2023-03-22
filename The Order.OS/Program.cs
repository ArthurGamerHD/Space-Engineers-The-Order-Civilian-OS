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
            static public bool VERYDANGEROUS60FPS = false; //Do NOT try that on a Multiplayer world, you will cause a LOT of lag for all clients
        }

        //------------- INSTALED PROGRAMS SHORTCUT HERE -------------\\

        void Shortcuts()
        {
            Programs.Add("DEBUG", new Action<Window, byte>(Debug));
            Programs.Add("MEDIA", new Action<Window, byte>(new MediaPlayer().Initialize));
            Programs.Add("MATH", new Action<Window, byte>(new MathVisualizer().Initialize));
            Programs.Add("DIAG", new Action<Window, byte>(new DiagInterface(Blocks, Me.CubeGrid, VHD, Logger).Initialize));
            Programs.Add("SANDBOX", new Action<Window, byte>(new UISandbox().Initialize));
        }

        //-------------- DO NOT TOUCH BELLOW THIS LINE --------------\\
        //-------- (Or touch, I'm just a comment, Not a cop) --------\\

        #endregion
    }
}
