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
        void NetworkUpdate(UpdateType UpdateSource, string Argument)
        {
            if ((UpdateSource & UpdateType.IGC) > 0 & Argument == "RxB")
            {
                while (RxB.HasPendingMessage)
                {
                    MyIGCMessage iGCMessage = RxB.AcceptMessage();
                    var data = iGCMessage.Data;
                }
            }
            if ((UpdateSource & UpdateType.IGC) > 0 & Argument == "RxU")
            {
                while (RxB.HasPendingMessage)
                {
                    MyIGCMessage iGCMessage = RxU.AcceptMessage();
                    var data = iGCMessage.Data;
                }
            }
        }
    }
}
