using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using System;
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
        #region UI-Sandbox

        class UISandbox
        {
            public void Initialize(Window window, byte Action)
            {
                var frame = window.SpritesBuilder;
                switch (Action)
                {
                    case 0:
                        window.Base = new Rectangle((int)(window.Screen.TextureSize.X / 2 - (128 * window.Scale)), (int)(window.Screen.TextureSize.Y / 2 - (128 * window.Scale)), (int)(256 * window.Scale), (int)(256 * window.Scale));
                        window.MyFrame = window.Base;
                        window.Configs[1] = 1;
                        window.Configs[5] = "UI-Framework Sandbox 0.1   (c) The Order-All rights reserved";
                        break;
                    case 1:
                        if (frame.Count == 0)
                        {
                            window.SpritesBuilder = window.Content();
                            frame = window.SpritesBuilder;
                            float scale = (window.MyFrame.Width / 512f > window.MyFrame.Height / 512f) ? (float)window.MyFrame.Height / 512f : (float)window.MyFrame.Width / 512f;
                            var centerPos = new Vector2I(window.MyFrame.Center.X, window.MyFrame.Center.Y);
                            var TA = TextAlignment.CENTER;
                            {
                                frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(4f, 12f) * scale + centerPos, new Vector2(472f, 410f) * scale, Color.White, null, TA));
                                window.Buttons.Add(new Button(centerPos.X - (int)(40 * scale), centerPos.Y -(int)(20 * scale), (int)(80 * scale), (int)(40 * scale), null, null, null, "Hello World!" , null, scale/2) );
                            }
                            foreach (MySprite Sprite in window.ToolBar()) { frame.Add(Sprite); }
                            foreach (MySprite Sprite in window.Footer()) { frame.Add(Sprite); }
                        }
                        break;
                    case 2:
                        window.Sprites = frame.ToList();
                        window.SpritesBuilder.Clear();
                        break;
                    case 3: break;
                    case 4: break;
                }
            }
        }
        #endregion
    }
}
