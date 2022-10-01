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
        public class Button
        {
            public Rectangle Hitbox;
            public Action Clicked;
            public Action Hover;
            public Action Click;
            public Button(int x, int y, int w, int h, Action MyAction, Action MyClick = null, Action MyHover = null)
            {
                Hitbox = new Rectangle(x, y, w, h);
                Clicked = MyAction;
                Click = MyClick;
                Hover = MyHover;
            }
            public void OnClick()
            {
                if (Click != null)
                    Click();
            }
            public void OnClicked()
            {
                if (Clicked != null)
                    Clicked();
            }
            public void OnHover()
            {
                if (Hover != null)
                    Hover();
            }
        }
    }

}
