using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Game.GUI.TextPanel;
using VRageMath;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        public class Workstation
        {
            public MultiScreenSpriteSurface Screen { get; set; }
            public IMyShipController Controller { get; }
            public IMyShipToolBase Tool { get; set; }
            public List<IMySoundBlock> Sound { get; }
            public Vector2I Cursor, CursorSprite;
            public float Scale = 2;
            public string Wallpaper = "";
            public int CursorStatus, S32;
            public double Standby;
            bool ECursor, ETaskBar, EBackground;
            public List<Window> Windows;
            Vector2I offset, CenterPos;
            public RectangleF ViewPort { get; private set; }
            readonly MetaData MetaData = new MetaData();
            public int SpriteCount { get; set; }
            int clock;

            float X, Y, Xp = 0, Yp = 0; Color MyColor = Color.White; Random rnd = new Random();

            public Workstation(MultiScreenSpriteSurface _Screen, IMyShipController _Controller)
            {
                EBackground = true; ETaskBar = true; ECursor = true;
                Controller = _Controller;
                Screen = _Screen;
                Cursor = (Vector2I)_Screen.SurfaceSize / 2;
                X = Cursor.X; Y = Cursor.Y;
                RefreshDisplay();
                Sound = new List<IMySoundBlock>();
                Windows = new List<Window>();
            }
            public void Update()
            {
                if (Tool != null)
                    GetCursor();
                Standby = Controller.IsUnderControl ? DateTime.Now.Ticks : Standby;
                foreach (Window Window in Windows)
                    Window.Run();
                clock++;
                if (clock == 10)
                {
                    if (Standby + 3600000000 > DateTime.Now.Ticks)
                    {
                        if (ViewPort.Width != Screen.SurfaceSize.X || ViewPort.Height != Screen.SurfaceSize.Y)
                        {
                            RefreshDisplay();
                        }
                        SpriteCount = Screen.SpriteCount;
                        if (EBackground) Backgroud();
                        if (Windows.Count > 0)
                            for (int i = 0; i < Windows.Count; ++i)
                            {
                                Windows[i].Render();
                                if (i == Windows.Count - 1) Screen.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(Windows.Last().BoundingBox.X - 4, Windows.Last().BoundingBox.Center.Y), new Vector2(Windows.Last().BoundingBox.Width + 8, Windows.Last().BoundingBox.Height + 8), new Color(0, 0, 0, 255), null, TextAlignment.LEFT));
                                if (Windows[i].Sprites != null) { foreach (MySprite sprite in Windows[i].Sprites) { Screen.Add(sprite); } }
                            }
                        Screen.Add(MySprite.CreateClearClipRect());
                        if (ETaskBar) TaskBar(); if (ECursor) DrawCursor();
                    }
                    else
                    {
                        var S70 = 70 * Scale;
                        var S50 = 50 * Scale;
                        var S7 = 7 * Scale;
                        var S5 = 5 * Scale;
                        if (Xp == 0) Xp = S7;
                        if (Yp == 0) Yp = S5;
                        X += Xp;
                        if (X + S50 > ViewPort.Width | X - S50 < ViewPort.X | Y + S70 > ViewPort.Height | Y - S70 < ViewPort.Y)
                            MyColor = new Color(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255));
                        Xp = (X + S50 > ViewPort.Width) ? -S7 : (X - S50 < ViewPort.X) ? S7 : Xp;
                        Y += Yp;
                        Yp = (Y + S70 > ViewPort.Height) ? -S5 : (Y - S70 < ViewPort.Y) ? S5 : Yp;
                        var centerPos = new Vector2(X, Y);
                        Screen.Add(new MySprite(SpriteType.TEXTURE, MetaData.FactionIcon, new Vector2(0f, -34f) * Scale + centerPos, new Vector2(100, 100), MyColor, "DEBUG", TextAlignment.CENTER, 2f * Scale));
                        Screen.Add(new MySprite(SpriteType.TEXT, "The Order OS", new Vector2(0f, 09f) * Scale + centerPos, null, MyColor, "DEBUG", TextAlignment.CENTER, 0.8f * Scale));

                    }
                    Screen.Draw();

                    clock = 0;

                }
            }
            public void Backgroud()
            {
                About Me = new About { };
                if (Wallpaper == "")
                {
                    StringBuilder Buffer = new StringBuilder();
                    Screen._anchor.ReadText(Buffer);
                    if (Buffer.ToString().Contains("WIC"))
                        Wallpaper = Buffer.ToString();
                    else Wallpaper = "-";
                }
                else if (Wallpaper != "-")
                {
                    Screen.Add(new MySprite(SpriteType.TEXT, Wallpaper.ToString(), new Vector2(CenterPos.X, 0), null, fontId: "Monospace", alignment: TextAlignment.CENTER, rotation: .163f));
                }
                else
                {
                    Screen.Add(new MySprite(SpriteType.TEXT, $"{Controller.GetOwnerFactionTag()}.OS", new Vector2(-210f, -40f) * Scale + CenterPos, null, MetaData.FactionColor, "White", TextAlignment.LEFT, 2.7f * Scale));
                    Screen.Add(new MySprite(SpriteType.TEXTURE, MetaData.FactionIcon, new Vector2(175f, 0f) * Scale + CenterPos, new Vector2(150f, 150f) * Scale, MetaData.FactionColor, null, TextAlignment.CENTER, 0f));
                }
                if (!Me.Activated)
                {
                    Color Water = new Vector4(.8f, .8f, .8f, .75f);
                    string _Message = $"Go to Setting to activate T.OS\n{Me.Edition} - Build {Me.Version}";
                    Vector2 _ContentSize = Screen.MeasureStringInPixels(new StringBuilder(_Message), "White", .6f * (Scale / 2));
                    Screen.Add(new MySprite(SpriteType.TEXT, _Message, new Vector2((int)ViewPort.Width - (int)_ContentSize.X - (5 * Scale), (int)ViewPort.Height - (int)_ContentSize.Y - S32), null, Water, "White", TextAlignment.LEFT, .6f * (Scale / 2)));
                    _Message = $"Activate T.OS";
                    _ContentSize = Screen.MeasureStringInPixels(new StringBuilder(_Message), "White", .9f * (Scale / 2));
                    Screen.Add(new MySprite(SpriteType.TEXT, _Message, new Vector2(ViewPort.Width - _ContentSize.X - (5 * Scale), (int)ViewPort.Height - (int)(3 * _ContentSize.Y + S32)), null, Water, "White", TextAlignment.LEFT, .9f * (Scale / 2)));
                }
            }
            public void TaskBar()
            {
                Screen.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(ViewPort.X + ViewPort.Width / 2, ViewPort.Bottom - (S32 / 2)), new Vector2(ViewPort.Width, S32), MetaData.Theme));
                Rectangle StartMenu = new Rectangle((int)ViewPort.X, (int)ViewPort.Height - S32, S32, S32);
                Screen.Add(new MySprite(SpriteType.TEXTURE, MetaData.FactionIcon, new Vector2(StartMenu.Center.X, StartMenu.Center.Y), new Vector2(S32, S32), MetaData.FactionColor));
            }
            public void GetCursor()
            {
                Vector2I _Cursor = Cursor;
                _Cursor.X += (int)(Controller.RotationIndicator.Y * Scale); _Cursor.Y += (int)(Controller.RotationIndicator.X * Scale);
                if (_Cursor.X > (int)ViewPort.Size.X) _Cursor.X = (int)ViewPort.Size.X; if (_Cursor.Y > (int)ViewPort.Size.Y) _Cursor.Y = (int)ViewPort.Size.Y;
                if (_Cursor.X < 0) _Cursor.X = (int)ViewPort.X - 1; if (_Cursor.Y < 0) _Cursor.Y = (int)ViewPort.Y - 1;

                if (CursorStatus < 4)
                    CursorStatus = (!Tool.IsActivated && CursorStatus == 1) ? 2 : (Tool.IsActivated) ? 1 : 0;
                {
                    if (Windows.Count() != 0)
                    {
                        var hitbox = new Rectangle(_Cursor.X, _Cursor.Y, 1, 1);
                        CursorStatus = Windows.Last().CursorUpdate(hitbox, Cursor, CursorStatus, Tool.IsActivated);
                        if (CursorStatus == 1)
                            for (int i = Windows.Count - 1; i >= 0; i--)
                            {
                                if (hitbox.Intersects(Windows[i].BoundingBox))
                                {
                                    Windows.Add(Windows[i]); Windows.Remove(Windows[i]);
                                    break;
                                }
                            }
                    }
                }
                CursorSprite = Cursor + offset;
                Cursor = _Cursor;
            }
            public void DrawCursor()
            {
                Screen.Add(new MySprite(SpriteType.TEXTURE, "Textures\\FactionLogo\\Builders\\BuilderIcon_6.dds", new Vector2(CursorSprite.X - (10 * Scale), CursorSprite.Y + (9 * Scale)), new Vector2(S32, S32),
                ((CursorStatus == 3) ? Color.Orange : (CursorStatus == 2) ? Color.Red : (CursorStatus == 1) ? Color.Yellow : Color.Green),
                null, TextAlignment.LEFT, -.56f));
            }

            public void TaskKill(Window window)
            {
                Windows.Remove(window);
            }
            public void RefreshDisplay()
            {
                offset = (Vector2I)(Screen.TextureSize - Screen.SurfaceSize) / 2;
                ViewPort = new RectangleF(offset, new Vector2(Screen.SurfaceSize.X, Screen.SurfaceSize.Y));
                Scale = Math.Min((Screen.TextureSize / Screen.BasePanelSize).X, (Screen.TextureSize / Screen.BasePanelSize).Y);
                S32 = (int)(Scale * 32);
                CenterPos = (Vector2I)Screen.TextureSize / 2;
            }
        }
    }
}
