using Sandbox.ModAPI.Ingame;
using System.Linq;
using VRage.Game.GUI.TextPanel;
using VRageMath;
using System;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        #region Debug
        public void Debug(Window Window, byte Action)
        {
            var frame = Window.SpritesBuilder;
            switch (Action)
            {
                case 0:
                    Window.Base = new Rectangle((int)(Window.Screen.TextureSize.X / 2 - (128 * Window.Scale)), (int)(Window.Screen.TextureSize.Y / 2 - (128 * Window.Scale)), (int)(256 * Window.Scale), (int)(256 * Window.Scale));
                    Window.MyFrame = Window.Base;
                    Window.Configs[1] = 1;
                    Window.Configs[5] = "Debug info Version 2.0.3   (c) The Order-All rights reserved";
                    break;
                case 1:
                    if (frame.Count == 0)
                    {
                        Window.SpritesBuilder = Window.Content();
                        frame = Window.SpritesBuilder;
                        float Z = (Window.MyFrame.Width / 512f > Window.MyFrame.Height / 512f) ? (float)Window.MyFrame.Height / 512f : (float)Window.MyFrame.Width / 512f;
                        var P = new Vector2I(Window.MyFrame.Center.X, Window.MyFrame.Center.Y);
                        var C = MetaData.CanvasColor;
                        var TA = TextAlignment.CENTER;
                        var ST = SpriteType.TEXTURE;
                        var SS = "SquareSimple";
                        string DebugText = $"Time: {DateTime.Now} \n" +
                            $"Instalation Date: {InternalStorage.Get("TOS", "Instalation")}\n"+
                            $"Screen Resolution: {((Workstation)Window.Configs[0]).ViewPort.Size}\n" +
                            $"Mouse Pos: {((Workstation)Window.Configs[0]).Cursor}\n" +
                            $"Window Size: {{X:{Window.MyFrame.Height} Y:{Window.MyFrame.Height}}}\n" +
                            $"Window Posi: {{X:{Window.MyFrame.X} Y:{Window.MyFrame.Y}}}\n" +
                            $"Frame:{clock / 10}\nScale:{Z}\n" +
                            $"Sprites: {Window.Sprites.Count()}/{((Workstation)Window.Configs[0]).SpriteCount}/{Telemetry.TotalFrameCount}\n" +
                            $"{HWText}";
                        {
                            frame.Add(new MySprite(ST, SS, new Vector2(4f, 12f) * Z + P, new Vector2(472f, 410f) * Z, new Color(9, 98, 166, 255), null, TA)); // InfoDisplayPanel
                            frame.Add(new MySprite(SpriteType.CLIP_RECT, SS, new Vector2(4f, 12f) * Z + (P - (new Vector2(472f, 410f) * Z / 2)), new Vector2(472f, 410f) * Z, new Color(9, 98, 166, 255), null, TextAlignment.LEFT)); // InfoDisplayPanelCanvas
                            frame.Add(new MySprite(SpriteType.TEXT, DebugText, new Vector2(-220f, -147f) * Z + P, null,Color.White, "DEBUG", TextAlignment.LEFT, .9f * Z));
                            frame.Add(new MySprite(SpriteType.TEXT, "      Debug Info:", new Vector2(-128f, -193f) * Z + P, null,Color.White, "DEBUG", TextAlignment.LEFT, 1.3f * Z));
                            frame.Add(MySprite.CreateClearClipRect());
                            frame.Add(new MySprite(SpriteType.CLIP_RECT, SS, new Vector2(Window.MyFrame.X, Window.MyFrame.Y), new Vector2(Window.MyFrame.Width, Window.MyFrame.Height), null, null, TextAlignment.LEFT));
                            frame.Add(new MySprite(SpriteType.TEXT, "Debug Info Display V2.0.1", new Vector2(-88f, -237f) * Z + P, null,Color.White, "DEBUG", TextAlignment.LEFT, 1f * Z)); // Title
                            frame.Add(new MySprite(ST, SS, new Vector2(50f, -200f) * Z + P, new Vector2(380f, 5f) * Z,Color.White, null, TA)); // WhiteLine
                            frame.Add(new MySprite(ST, "Circle", new Vector2(-183f, -200f) * Z + P, new Vector2(100f, 100f) * Z, new Color(9, 98, 166, 255), null, TA)); // ThemeCircle
                            frame.Add(new MySprite(ST, "Circle", new Vector2(-183f, -200f) * Z + P, new Vector2(90f, 90f) * Z, C, null, TA)); // CircleMask
                            frame.Add(new MySprite(ST, SS, new Vector2(210f, -200f) * Z + P, new Vector2(10f, 7f) * Z, C, null, TA)); // MaskLine6
                            frame.Add(new MySprite(ST, SS, new Vector2(150f, -200f) * Z + P, new Vector2(10f, 7f) * Z, C, null, TA)); // MaskLine5
                            frame.Add(new MySprite(ST, SS, new Vector2(90f, -200f) * Z + P, new Vector2(10f, 7f) * Z, C, null, TA)); // MaskLine4
                            frame.Add(new MySprite(ST, SS, new Vector2(30f, -200f) * Z + P, new Vector2(10f, 7f) * Z, C, null, TA)); // MaskLine3
                            frame.Add(new MySprite(ST, SS, new Vector2(-30f, -200f) * Z + P, new Vector2(10f, 7f) * Z, C, null, TA)); // MaskLine2
                            frame.Add(new MySprite(ST, SS, new Vector2(-90f, -200f) * Z + P, new Vector2(10f, 7f) * Z, C, null, TA)); // MaskLine1
                            frame.Add(new MySprite(ST, "Circle", new Vector2(-183f, -200f) * Z + P, new Vector2(80f, 80f) * Z,Color.White, null, TA)); // WhiteCircle
                            frame.Add(new MySprite(ST, MetaData.FactionIcon, new Vector2(-183f, -200f) * Z + P, new Vector2(80f, 80f) * Z, MetaData.FactionColor, null, TA)); // WhiteCircle
                            foreach (MySprite Sprite in Window.ToolBar()) { frame.Add(Sprite); }
                            foreach (MySprite Sprite in Window.Footer()) { frame.Add(Sprite); }
                        }

                    }
                    break;
                case 2:
                    Window.Sprites = frame.ToList();
                    Window.SpritesBuilder.Clear();
                    break;
                case 3: break;
                case 4: break;
            }
        }
        #endregion
    }
}
