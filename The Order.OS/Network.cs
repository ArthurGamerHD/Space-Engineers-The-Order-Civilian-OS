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
                    UpdateBrowser(iGCMessage.Data, iGCMessage.Source, iGCMessage.Tag);
                }
            }
            else if ((UpdateSource & UpdateType.IGC) > 0 & Argument == "RxU")
            {
                while (RxU.HasPendingMessage)
                {
                    MyIGCMessage iGCMessage = RxU.AcceptMessage();
                    UpdateBrowser(iGCMessage.Data, iGCMessage.Source, iGCMessage.Tag);
                }
            }
        }
        void UpdateBrowser(object Data, long Origin = 0, string Tag = "")
        {
            bool Found = false;
            string SData = Data.ToString();
            Me.CustomData = SData;
            foreach (Workstation Workstation in MyWorkstations)
            {
                foreach (Window Window in Workstation.Windows)
                    if (Window.Configs[10] is MyDecoder)
                        if (((MyDecoder)Window.Configs[10]).Origin == Origin && ((MyDecoder)Window.Configs[10]).Tag == Tag) { ((MyDecoder)Window.Configs[10]).Decode(SData); Found = true; break; }
                if (Found) break;
            }
            if (!Found)
            {
                Window Window = new Window(MyWorkstations[0], "Compass", new Action<Window, byte>(new WebBrowser(Origin, Tag).MyBrowserInterface));
                ((MyDecoder)Window.Configs[10]).Decode(SData);
                MyWorkstations[0].Windows.Add(Window);
            }

        }
    }
}
