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
        #region Debug
        class MediaPlayer
        {
            public MediaPlayer()
            { }

            Window Window;
            float Z, volume;
            Vector2I P;
            TextAlignment Ta = TextAlignment.CENTER;
            Color W = Color.White, C, F;
            List<string> Sounds = new List<string>();
            List<MySprite> frame = new List<MySprite>();
            string selected = "";

            IMySoundBlock MainSound;

            public void MyMediaPlayer(Window _Window, byte Action)
            {
                Window = _Window;
                frame = Window.SpritesBuilder;
                switch (Action)
                {
                    case 0:
                        Window.Base = new Rectangle((int)(Window.Screen.TextureSize.X / 2 - (128 * Window.Scale)), (int)(Window.Screen.TextureSize.Y / 2 - (128 * Window.Scale)), (int)(256 * Window.Scale), (int)(256 * Window.Scale));
                        Window.MyFrame = Window.Base;
                        C = Window.Meta.CanvasColor; F = Window.Meta.FactionColor;
                        Window.Configs[1] = 1;
                        Window.Configs[5] = "Media Player Version 0.1.4 (c) The Order-All rights reserved";
                        Window.Configs[14] = false;
                        Window.Configs[15] = 103;
                        if (Window.Sound.Count != 0)
                        {
                            Window.Sound.First().GetSounds(Sounds);
                            MainSound = Window.Sound.First();
                            foreach (IMySoundBlock Jukebox in Window.Sound)
                            {
                                if (Jukebox.BlockDefinition.TypeIdString == "MyObjectBuilder_Jukebox") MainSound = Jukebox;
                            }
                            selected = MainSound.SelectedSound;
                            volume = MainSound.Volume;
                        }

                        break;
                    case 1:
                        if (frame.Count == 0)
                        {
                            Z = (Window.MyFrame.Width / 512f > Window.MyFrame.Height / 512f) ? (float)Window.MyFrame.Height / 512f : (float)Window.MyFrame.Width / 512f;
                            P = new Vector2I(Window.MyFrame.Center.X, Window.MyFrame.Center.Y);
                            Window.SpritesBuilder = Window.Content();
                            frame = Window.SpritesBuilder;
                            var canvas = C;
                            var TA = TextAlignment.CENTER;
                            {
                                frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(4f, 12f) * Z + P, new Vector2(472f, 410f) * Z, new Color(9, 98, 166, 255), null, TA));
                                frame.Add(new MySprite(SpriteType.CLIP_RECT, "SquareSimple", new Vector2(4f, 12f) * Z + (P - (new Vector2(472f, 410f) * Z / 2)), new Vector2(472f, 410f) * Z));
                                frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", new Vector2(-180f, 170f) * Z + P, new Vector2(50f, 50f) * Z, F, null, Ta, 0f));
                                frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", new Vector2(180f, 170f) * Z + P, new Vector2(50f, 50f) * Z, F, null, Ta, 0f));
                                frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(0f, 170f) * Z + P, new Vector2(355f, 50f) * Z, F, null, Ta, 0f));
                                frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(90f, 170f) * Z + P, new Vector2(10f, 30f) * Z, W, null, Ta, 0f));
                                frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(-90f, 170f) * Z + P, new Vector2(10f, 30f) * Z, W, null, Ta, 0f));
                                frame.Add(new MySprite(SpriteType.TEXTURE, "Triangle", new Vector2(-60f, 170f) * Z + P, new Vector2(30f, 30f) * Z, W, null, Ta, -1.5708f));
                                frame.Add(new MySprite(SpriteType.TEXTURE, "Triangle", new Vector2(-80f, 170f) * Z + P, new Vector2(30f, 30f) * Z, W, null, Ta, -1.5708f));
                                frame.Add(new MySprite(SpriteType.TEXTURE, "Triangle", new Vector2(80f, 170f) * Z + P, new Vector2(30f, 30f) * Z, W, null, Ta, 1.5708f));
                                frame.Add(new MySprite(SpriteType.TEXTURE, "Triangle", new Vector2(60f, 170f) * Z + P, new Vector2(30f, 30f) * Z, W, null, Ta, 1.5708f));
                                frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", new Vector2(0f, 170f) * Z + P, new Vector2(80f, 80f) * Z, C, null, Ta, 0f));
                                frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", new Vector2(0f, 170f) * Z + P, new Vector2(70f, 70f) * Z, F, null, Ta, 0f));
                                if (!(bool)Window.Configs[14])
                                    frame.Add(new MySprite(SpriteType.TEXTURE, "Triangle", new Vector2(5f, 170f) * Z + P, new Vector2(30f, 30f) * Z, new Color(255, 255, 255, 255), null, Ta, 1.5708f));
                                else {
                                    frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(10f, 170f) * Z + P, new Vector2(12f, 30f) * Z, new Color(255, 255, 255, 255), null, Ta, 0));
                                    frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(-10f, 170f) * Z + P, new Vector2(12f, 30f) * Z, new Color(255, 255, 255, 255), null, Ta, 0));
                                }
                                if (Window.Sound.Count == 0)
                                    frame.Add(new MySprite(SpriteType.TEXT, "Warning, no Sound Device Detected", new Vector2(0, 0) * Z + P, null, new Color(255, 255, 255, 255), "DEBUG", Ta, 1f * Z)); // Title
                                else
                                {
                                    if (MainSound.IsSoundSelected)
                                    {
                                        if (selected != MainSound.SelectedSound) {
                                            selected = MainSound.SelectedSound;
                                            Next();
                                        }
                                        frame.Add(new MySprite(SpriteType.TEXT, $"Now Playing: {selected}.xwm", new Vector2(-131f, -190f) * Z + P, null, new Color(255, 255, 255, 255), "DEBUG", TextAlignment.LEFT, 0.8f * Z)); // Now Playing:
                                    }
                                    frame.Add(new MySprite(SpriteType.TEXTURE, "Triangle", new Vector2(108f, 170f) * Z + P, new Vector2(20f, 10f) * Z, new Color(255, 255, 255, 255), null, TextAlignment.LEFT, -1.5708f)); // sprite1Copy
                                    frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(108f, 170f) * Z + P, new Vector2(4f, 4f) * Z, new Color(255, 255, 255, 255), null, TextAlignment.LEFT, 0f)); // sprite1
                                    frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(130f, 170f) * Z + P, new Vector2(50f, 4f) * Z, Window.Meta.CanvasColor, null, TextAlignment.LEFT, 0f)); // sprite2
                                    frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(130f, 170f) * Z + P, new Vector2(50f * volume, 4f) * Z, W, null, TextAlignment.LEFT, 0f)); // sprite2

                                    Window.Buttons = new List<Button>
                                    {
                                        new Button((int)(120f * Z + P.X), (int)(168f * Z + P.Y), (int)(70f * Z), (int)(15 * Z), Slider, Slider)
                                    };

                                    frame.Add(new MySprite(SpriteType.TEXT, ((int)Window.Configs[15] + 1).ToString() + "/" + Sounds.Count().ToString(), new Vector2(-180, 160f) * Z + P, null, new Color(255, 255, 255, 255), "DEBUG", TextAlignment.LEFT, .7f * Z)); // Title
                                    NewButton(45f, 150f, B1);  //Next
                                    NewButton(-95f, 150f, B2);//Prev
                                    NewButton(0f, 150f, B3); //PlayPause
                                }

                                frame.Add(MySprite.CreateClearClipRect());
                                frame.Add(new MySprite(SpriteType.CLIP_RECT, "SquareSimple", new Vector2(Window.MyFrame.X, Window.MyFrame.Y), new Vector2(Window.MyFrame.Width, Window.MyFrame.Height), null, null, TextAlignment.LEFT));
                                frame.Add(new MySprite(SpriteType.TEXT, "Media Player Version 0.1.4", new Vector2(-88f, -237f) * Z + P, null, new Color(255, 255, 255, 255), "DEBUG", TextAlignment.LEFT, 1f * Z)); // Title
                                frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(50f, -200f) * Z + P, new Vector2(380f, 5f) * Z, new Color(255, 255, 255, 255), null, TA)); // WhiteLine
                                frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", new Vector2(-183f, -200f) * Z + P, new Vector2(100f, 100f) * Z, new Color(9, 98, 166, 255), null, TA)); // ThemeCircle
                                frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", new Vector2(-183f, -200f) * Z + P, new Vector2(90f, 90f) * Z, C, null, TA)); // CircleMask
                                frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(210f, -200f) * Z + P, new Vector2(10f, 7f) * Z, C, null, TA)); // MaskLine6
                                frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(150f, -200f) * Z + P, new Vector2(10f, 7f) * Z, C, null, TA)); // MaskLine5
                                frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(90f, -200f) * Z + P, new Vector2(10f, 7f) * Z, C, null, TA)); // MaskLine4
                                frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(30f, -200f) * Z + P, new Vector2(10f, 7f) * Z, C, null, TA)); // MaskLine3
                                frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(-30f, -200f) * Z + P, new Vector2(10f, 7f) * Z, C, null, TA)); // MaskLine2
                                frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(-90f, -200f) * Z + P, new Vector2(10f, 7f) * Z, C, null, TA)); // MaskLine1
                                frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", new Vector2(-183f, -200f) * Z + P, new Vector2(80f, 80f) * Z, new Color(255, 255, 255, 255), null, TA)); // WhiteCircle
                                frame.Add(new MySprite(SpriteType.TEXTURE, Window.Meta.FactionIcon, new Vector2(-183f, -200f) * Z + P, new Vector2(80f, 80f) * Z, Window.Meta.FactionColor, null, TA)); // WhiteCircle
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

            public void B1() { Window.Configs[15] = Sounds.FindIndex(A => A == selected) + 1; if ((int)Window.Configs[15] > Sounds.Count - 1) Window.Configs[15] = 0; MainSound.SelectedSound = Sounds[(int)Window.Configs[15]]; Next(); }
            public void B2() { Window.Configs[15] = Sounds.FindIndex(A => A == selected) - 1; if ((int)Window.Configs[15] < 0) Window.Configs[15] = Sounds.Count - 1; MainSound.SelectedSound = Sounds[(int)Window.Configs[15]]; Next(); }
            public void B3()
            {
                foreach (IMySoundBlock Sound in Window.Sound)
                {
                    Sound.SelectedSound = MainSound.SelectedSound;
                    if (!(bool)Window.Configs[14])
                        Sound.Play();
                    else
                        Sound.Stop();
                }
                Window.Configs[14] = !(bool)Window.Configs[14];
            }
            public void Slider() {
              var X =  Window.Cursor.X - (135f * Z + P.X);
              var Xp = 40*Z;
                volume = Math.Min(X / Xp, 1);
                foreach (IMySoundBlock Sound in Window.Sound)
                {
                    Sound.Volume = volume;
                }
            }
            public void Next()
            {
                foreach (IMySoundBlock Sound in Window.Sound)
                {
                    Sound.SelectedSound = MainSound.SelectedSound;
                        Sound.Play();
                }
                Window.Configs[14] = true;
            }
            public void NewButton(float X, float Y, Action ButtonAction)
            {
                Vector2 ButtonPos = (new Vector2(X, Y) * Z) + P - new Vector2(25, 12.5f) * Z;
                Window.Buttons.Add(new Button((int)ButtonPos.X, (int)ButtonPos.Y, (int)(80f * Z), (int)(50f * Z), ButtonAction));
            }

        }
        #endregion
    }
}
