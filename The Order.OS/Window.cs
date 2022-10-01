﻿using Sandbox.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        #region Window
        public class Window
        {
            public MultiScreenSpriteSurface Screen;
            public Rectangle MyFrame, Base, BoundingBox;
            public List<MySprite> Sprites;
            public List<MySprite> SpritesBuilder;
            public float Scale;
            public MetaData Meta = new MetaData();
            public List<IMyTerminalBlock> IOs;
            public Action<Window> TaskManager;
            public List<IMySoundBlock> Sound { get; }
            bool Debug;
            public List<Button> Buttons = new List<Button>();
            public IList Configs = new List<object>(new object[16]);
            public Window(Workstation _workstation, string _Title, Action<Window, byte> _Action)
            {
                Debug = new About().Debug;
                Configs[0] = _workstation;
                Configs[1] = 1;
                Configs[2] = _Action;
                Configs[3] = false;
                Configs[4] = _Title;
                Configs[5] = _Title;
                Scale = _workstation.Scale;
                Screen = _workstation.Screen;
                Sound = _workstation.Sound;
                TaskManager = new Action<Window>(_workstation.TaskManager);
                SpritesBuilder = new List<MySprite>();
                Sprites = SpritesBuilder.ToList();
                IOs = new List<IMyTerminalBlock> { _workstation.Controller, _workstation.Tool };
                Start();
            }
            public void Start() { ((Action<Window, byte>)Configs[2])(this, 0); }
            public void Run() { ((Action<Window, byte>)Configs[2])(this, 1); }
            public void Render()
            {
                ((Action<Window, byte>)Configs[2])(this, 2);
                if (Debug) foreach (Button Button in Buttons) { Sprites.Add(new MySprite(SpriteType.TEXTURE, "SquareHollow", new Vector2(Button.Hitbox.Left, Button.Hitbox.Top + (Button.Hitbox.Height / 2)), new Vector2(Button.Hitbox.Width, Button.Hitbox.Height), Color.Red)); }
                Buttons = new List<Button>();
                Sprites.Add(MySprite.CreateClearClipRect());
            }
            public List<MySprite> ToolBar()
            {
                Buttons.Add(new Button(MyFrame.Right - 32, MyFrame.Top - 32, 32, 32, new Action(Close)));
                Buttons.Add(new Button(MyFrame.Right - 64, MyFrame.Top - 32, 32, 32, new Action(Maximize)));
                Buttons.Add(new Button(MyFrame.Right - 96, MyFrame.Top - 32, 32, 32, new Action(Background)));
                BoundingBox = new Rectangle(BoundingBox.X, BoundingBox.Y - 32, BoundingBox.Width, BoundingBox.Height + 32);
                return new List<MySprite> {
                    MySprite.CreateClearClipRect(),
                    new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(MyFrame.Center.X, MyFrame.Top - 16), new Vector2(MyFrame.Width, 32), Meta.Theme, null, TextAlignment.CENTER, 0f),
                    new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(MyFrame.Right -16, MyFrame.Top - 16), new Vector2(16, 4), Color.White, null, TextAlignment.CENTER, .78f),
                    new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(MyFrame.Right -16, MyFrame.Top - 16), new Vector2(16, 4), Color.White, null, TextAlignment.CENTER, -.78f),
                    new MySprite(SpriteType.TEXTURE, "SquareHollow", new Vector2(MyFrame.Right - 46, MyFrame.Top - 18), new Vector2(10, 10), Color.White, null, TextAlignment.CENTER, 0f),
                    new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(MyFrame.Right - 50, MyFrame.Top - 14), new Vector2(10, 10), Color.White, null, TextAlignment.CENTER, 0f),
                    new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(MyFrame.Right - 80, MyFrame.Top - 12), new Vector2(12, 4), Color.White, null, TextAlignment.CENTER, 0f),
                    new MySprite(SpriteType.CLIP_RECT, "SquareSimple", new Vector2(MyFrame.Left, MyFrame.Top - 32), new Vector2(MyFrame.Width-96, 32)),
                    new MySprite(SpriteType.TEXT, Configs[4].ToString(), new Vector2(MyFrame.Left + 3, MyFrame.Top - 32),null, Color.White, "DEBUG", TextAlignment.LEFT, 1f),
                    MySprite.CreateClearClipRect()
            };
            }
            public List<MySprite> Footer()
            {
                BoundingBox = new Rectangle(BoundingBox.X, BoundingBox.Y, BoundingBox.Width, BoundingBox.Height + 20);
                return new List<MySprite> {
                    new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(MyFrame.Left, MyFrame.Bottom + 10), new Vector2(MyFrame.Width, 20), Meta.Theme, null, TextAlignment.LEFT, 0f),
                    new MySprite(SpriteType.CLIP_RECT, "SquareSimple", new Vector2(MyFrame.Left, MyFrame.Bottom), new Vector2(MyFrame.Width, 20)),
                    new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(MyFrame.Left, MyFrame.Bottom), new Vector2(MyFrame.Width, 2), Color.White, null, TextAlignment.LEFT, 0f),
                    new MySprite(SpriteType.TEXT, (string)Configs[5], new Vector2(MyFrame.Center.X, MyFrame.Bottom + 6),null, Color.White, "DEBUG", TextAlignment.CENTER, .4f)};
            }
            public List<MySprite> Content()
            {
                BoundingBox = MyFrame;
                return new List<MySprite> {
                new MySprite(),
                MySprite.CreateClearClipRect(),
                new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2I(MyFrame.Center.X, MyFrame.Center.Y), new Vector2(MyFrame.Width, MyFrame.Height), Meta.Theme, null, TextAlignment.CENTER, 0f),
                new MySprite(SpriteType.CLIP_RECT,"SquareSimple", new Vector2(MyFrame.X+2, MyFrame.Y), new Vector2(MyFrame.Width-4, MyFrame.Height), null, null, TextAlignment.LEFT, 0f),
                new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2I(MyFrame.Center.X, MyFrame.Center.Y), new Vector2(MyFrame.Width, MyFrame.Height), Meta.CanvasColor, null, TextAlignment.CENTER, 0f)};
            }

            public int CursorUpdate(Rectangle CHitbox, Vector2 CLP, int Status, bool Click)
            {
                if (!(bool)Configs[3] && Status >= 1)
                {
                    if ((int)Configs[1] == 2)
                    {
                        if (new Rectangle(MyFrame.Right, MyFrame.Bottom, (int)(32 * (Scale)), (int)(32 * (Scale))).Intersects(new Rectangle(CHitbox.X, CHitbox.Y, 1, 1)) || Status == 4) { Status = (Click) ? 4 : 0; MyFrame = new Rectangle(MyFrame.X, MyFrame.Y, (int)Math.Max(MyFrame.Width - (CLP.X - CHitbox.X), 128), (int)Math.Max(MyFrame.Height - (CLP.Y - CHitbox.Y), 128)); return Status; }
                    }
                    else if ((int)Configs[1] == 1)
                    {
                        var Size = (Math.Max(MyFrame.Width - (CLP.X - CHitbox.X), 128) + Math.Max(MyFrame.Height - (CLP.Y - CHitbox.Y), 128)) / 2;
                        if (new Rectangle(MyFrame.Right, MyFrame.Bottom, (int)(32 * (Scale)), (int)(32 * (Scale))).Intersects(new Rectangle(CHitbox.X, CHitbox.Y, 1, 1)) || Status == 4) { Status = (Click) ? 4 : 0; MyFrame = new Rectangle(MyFrame.X, MyFrame.Y, (int)Size, (int)Size); return Status; }
                    }
                    if (new Rectangle(MyFrame.Left, MyFrame.Top - (int)(32 * Scale), MyFrame.Width - (int)(96 * (Scale)), (int)(32 * (Scale))).Intersects(CHitbox) || Status == 5) { Status = (Click) ? 5 : 0; MyFrame = new Rectangle((int)(MyFrame.X - (CLP.X - CHitbox.X)), (int)(MyFrame.Y - (CLP.Y - CHitbox.Y)), MyFrame.Width, MyFrame.Height); return Status; };
                }
                foreach (Button button in Buttons)
                {
                    if (CHitbox.Intersects(button.Hitbox) && Status == 3)
                        if (Status == 1 || Status == 2)
                            button.OnClick();
                        else if (Status == 3)
                            button.OnClicked();
                        else
                            button.OnHover();
                }
                return Status;

            }

            public void Maximize()
            {
                if ((bool)Configs[3])
                {
                    MyFrame = Base;
                    Configs[3] = false;
                }
                else if ((int)Configs[1] == 1)
                {
                    int Size = (int)Math.Min(Screen.TextureSize.X, Screen.TextureSize.Y) - 80;
                    MyFrame = new Rectangle((int)(Screen.TextureSize.X / 2 - Size / 2), (int)(Screen.TextureSize.Y / 2 - Size / 2) - 8, Size, Size);//(int)window.Screen.TextureSize.X, (int)window.Screen.TextureSize.Y - 64);
                    Configs[3] = true;
                }
                else if ((int)Configs[1] == 2)
                {
                    MyFrame = new Rectangle(0, 32, (int)Screen.TextureSize.X, (int)Screen.TextureSize.Y - 64);
                    Configs[3] = true;
                }
            }
            public void Close()
            {
                TaskManager(this);
            }
            public void Background() { }

        }
        #endregion
    }
}
