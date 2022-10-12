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
        #region WebBrowser

        class WebBrowser
        {
            public WebBrowser(long id, string tag)
            {
                Id = id; Tag = tag;
            }
            long Id;
            string Tag;
            List<MySprite> frame;
            Window Window;
            float Z;
            Vector2I P;
            public void MyBrowserInterface(Window _Window, byte Action)
            {
                Window = _Window;
                frame = Window.SpritesBuilder;
                switch (Action)
                {
                    case 0:
                        Window.Base = new Rectangle((int)(Window.Screen.TextureSize.X / 2 - (128 * Window.Scale)), (int)(Window.Screen.TextureSize.Y / 2 - (128 * Window.Scale)), (int)(256 * Window.Scale), (int)(256 * Window.Scale));
                        Window.MyFrame = Window.Base;
                        Window.Configs[1] = 1;
                        Window.Configs[4] = $"Compass Browser - {Tag} at igc://{Id}";
                        Window.Configs[5] = "Compass Browser Version 0.1                (c) The Order-All rights reserved";
                        if (Window.Configs[10] == null)
                        {
                            Window.Configs[10] = new MyDecoder(Id, Tag);
                        }
                        break;
                    case 1:
                        if (frame.Count == 0)
                        {
                            P = new Vector2I(Window.MyFrame.Center.X, Window.MyFrame.Center.Y);
                            Window.SpritesBuilder = Window.Content();
                            frame = Window.SpritesBuilder;
                            var size = ((MyDecoder)Window.Configs[10]).Frame;
                            frame.Add(new MySprite(SpriteType.CLIP_RECT, "SquareSimple", new Vector2(0f, 0f) * Z + new Vector2Point(Window.MyFrame.Location).Vector, new Vector2(512f, 512f) * Z)); // Box
                            try
                            {
                                Z = (Window.MyFrame.Width / size.Width > Window.MyFrame.Height / size.Height) ? (float)Window.MyFrame.Height / size.Width : (float)Window.MyFrame.Width / size.Height;
                            }
                            catch {
                                Z = (Window.MyFrame.Width / 512f > Window.MyFrame.Height / 512f) ? (float)Window.MyFrame.Height / 512f : (float)Window.MyFrame.Width / 512f;
                            }
                            foreach (MySprite S in ((MyDecoder)Window.Configs[10]).Sprites)
                            {
                                frame.Add(new MySprite(S.Type, S.Data, S.Position * Z + new Vector2Point(Window.MyFrame.Location).Vector, (S.Size != null) ? S.Size  * Z: null, S.Color, S.FontId, S.Alignment, (S.Type == SpriteType.TEXT) ? S.RotationOrScale * Z : S.RotationOrScale));
                            }
                            foreach (MySprite Sprite in Window.ToolBar()) { frame.Add(Sprite); }
                            foreach (MySprite Sprite in Window.Footer()) { frame.Add(Sprite); }
                        }
                        break;
                    case 2:
                        Window.Sprites = frame.ToList();
                        Window.SpritesBuilder.Clear();
                        break;
                }
            }
        }
        #endregion
    }
}
