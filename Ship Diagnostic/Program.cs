﻿using Sandbox.Game.EntityComponents;
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

        Color Theme = new Vector4(9 / 255f, 98 / 255f, 166 / 255f, 255);
        Color BackgroundColor = new Vector4(2 / 255f, 4 / 255f, 76 / 255f, 255);

        public void Shortcuts()
        {
            //Custom Programs Shortcuts Here
            StartMenuButton = new List<Buttons> {
                new Buttons("Debug", new Event(Debug, "New", null)),
                new Buttons("Ship Diagnostic", new Event(SelfDiagnostic, "New", null)),
                new Buttons("HW_Info", new Event(HWInfo, "New", null)),
                new Buttons("DisplayRefresh", new Event(DisplayManagment, "New", null))
        };
        }
        public void Startup()
        {
            //Custom Programs Auto Start Here

            Services.Add(new Action<string, UpdateType>(DiagMain));
            if (FlyByWireOOBE()) { Services.Add(new Action<string, UpdateType>(FlyByWire)); StartMenuButton.Add(new Buttons("FlyByWireSubRotine", new Event(FlyByWireSubRotine, "New", null))); }
            DisplayManagmentSetup();
        }



        //--------------------------------------------------------------//



        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1 | UpdateFrequency.Update10 | UpdateFrequency.Update100;
            Renew(); Reload(); Startup();
            ViewPort = new RectangleF((Monitors[0].Surface.TextureSize - Monitors[0].Surface.SurfaceSize) / 2f, new Vector2(Monitors[0].Surface.SurfaceSize.X * Monitors.Count(), Monitors[0].Surface.SurfaceSize.Y));
            for (int i = 0; i < Monitors.Count; ++i)
            {
                Monitors[i].Viewport = new RectangleF(((Monitors[i].Surface.TextureSize.X - Monitors[i].Surface.SurfaceSize.X) / 2) + (Monitors[i].Surface.SurfaceSize.X * i), (Monitors[i].Surface.TextureSize.Y - Monitors[i].Surface.SurfaceSize.Y) / 2, Monitors[i].Surface.SurfaceSize.X, Monitors[i].Surface.SurfaceSize.Y);
                Monitors[i].Hitbox = new Rectangle((int)Monitors[i].Viewport.X, (int)Monitors[i].Viewport.Y, (int)Monitors[i].Viewport.Width, (int)Monitors[i].Viewport.Height);
            }
            Cursor = ViewPort.Size / 2;
            offset = (Monitors[0].Surface.TextureSize - Monitors[0].Surface.SurfaceSize) / 2f;
            taskbar = new Rectangle((int)ViewPort.Position.X, (int)ViewPort.Bottom - 32, (int)ViewPort.Size.X, 32);
            StartButton = new Rectangle(taskbar.Left, taskbar.Top, 32, 32);
            Shortcuts();
        }
        void Main(string argument, UpdateType updateSource)
        {
            if (argument == "RED") {

                Theme = Color.Red;
                BackgroundColor = Color.DarkMagenta;
            }

            DisplayManagment("", null);
            Echo("Something");

            foreach (Action<string, UpdateType> Run in Services) Run(argument, updateSource);
            if (Monitors.Count > 1)
            {
                foreach (Monitor surface in Monitors) { Echo(((IMyTextPanel)surface.Surface).CustomName); }
            }
            GetCursor((updateSource & UpdateType.Update10) != 0);
            string[] _Custom = Me.CustomData.Split(',');
            if (argument == "Click") Clicked = true;
            if (argument == "New") { Content _Content = new Content(_Custom[1]); if (_Custom.Length == 3) { _Content.MyFont = _Custom[2]; }; NewWindow(Cursor, _Custom[0], _Content); }
            if (argument.Contains("New:"))
            {
                string[] arg = argument.Split(':');
                try
                {
                    string[] answer = (arg[3] != "") ? new string[] { arg[3] } : new string[] { };
                    if (arg[3].Contains("\\"))
                    {
                        answer = arg[3].Split('\\');
                    }
                    NewWindow(Cursor, arg[1], new Content(arg[2]));
                }
                catch { }
            }
            if (ScreenUpdate == null)
                ScreenUpdate = UpdateScreen().GetEnumerator();
            if (ScreenUpdate.MoveNext() == false)
            {
                ScreenUpdate.Dispose();
                if ((updateSource & UpdateType.Update1) != 0)
                    foreach (Event _Task in HighPriorityTasks) _Task.Action(_Task.Parameter, _Task.Window);
                if ((updateSource & UpdateType.Update10) != 0)
                    foreach (Event _Task in Tasks) _Task.Action(_Task.Parameter, _Task.Window);
                if ((updateSource & UpdateType.Update100) != 0)
                    foreach (Event _Task in LowPriorityTasks) _Task.Action(_Task.Parameter, _Task.Window);
                ScreenUpdate = null;
            }
            SystemMetrics();
        }

        IEnumerator<bool> ScreenUpdate;

        IEnumerable<bool> UpdateScreen()
        {
            for (int i = 0; i < Monitors.Count; i++)
            {
                Monitors[i].Frame.Dispose();
                Monitors[i].Frame = Monitors[i].Surface.DrawFrame();
            }
            Background();
            for (int i = 0; i < Windows.Count; i++)
            {
                Comit(Windows[i]);
                yield return true;
            }
            if (MenuEnabled) StartMenu();
            TaskBar();
            DrawMouse();

        }

        #region Actions
        void Renew()
        {
            blocks.Clear();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks);
        }
        void Reload()
        {
            Monitors.Add(new Monitor(Me.GetSurface(0)));
            if (blocknumbers != blocks.Count())
            {
                List<IMyTextPanel> Panels = new List<IMyTextPanel> { };
                blocknumbers = blocks.Count();
                foreach (IMyTerminalBlock block in blocks)
                {
                    if (block is IMyCockpit & block.CustomName.Contains("Main Computer Station")) helm = (IMyCockpit)block;
                    if (block is IMyShipConnector && block.CubeGrid == Me.CubeGrid) Connectors.Add((IMyShipConnector)block);
                    if (block is IMyShipWelder & block.CustomName.Contains("Mouse")) welder = (IMyShipWelder)block;
                    if (block is IMyTextPanel & block.CustomName.Contains("LCD Master")) { Monitors.RemoveAt(0); Panels.Add((IMyTextPanel)block); ((IMyTextPanel)block).ContentType = ContentType.SCRIPT; };
                    if (block is IMyTextPanel & block.CustomName.Contains("LCD Slave")) { Panels.Add((IMyTextPanel)block); ((IMyTextPanel)block).ContentType = ContentType.SCRIPT; };

                }
                if (Monitors[0] != Me.GetSurface(0))
                    Panels.Sort((A, B) => A.CustomName.CompareTo(B.CustomName));
                foreach (IMyTextPanel Panel in Panels)
                {
                    if (Panel.BlockDefinition.SubtypeId.Equals(Panels[0].BlockDefinition.SubtypeId) && Panel.BlockDefinition.Equals(Panels[0].BlockDefinition))
                    {
                        Monitors.Add(new Monitor(Panel));
                    }
                }
            }
        }
        void GetCursor(bool _update)
        {

            Cursor.X += helm.RotationIndicator.Y; Cursor.Y += helm.RotationIndicator.X;
            if (Cursor.X > ViewPort.Size.X) Cursor.X = ViewPort.Size.X; if (Cursor.Y > ViewPort.Size.Y) Cursor.Y = ViewPort.Size.Y;
            if (Cursor.X < 0) Cursor.X = ViewPort.X - 0.1f; if (Cursor.Y < 0) Cursor.Y = ViewPort.Y - 0.1f;
            CursorV = Cursor + offset;
            clicked = welder.IsActivated;
            if (_update)
            {
                try
                {
                    Rectangle CursorHitbox = new Rectangle((int)CursorV.X, (int)CursorV.Y, 1, 1);
                    near = false;

                    if (!MenuEnabled & CursorHitbox.Intersects(StartButton)) { near = true; if (Clicked & !clicked) MenuEnabled = true; Clicked = false; }
                    else if (Clicked & !clicked & !CursorHitbox.Intersects(Menu)) MenuEnabled = false;
                    else if (MenuEnabled) { foreach (Buttons _Button in StartMenuButton) { if (CursorHitbox.Intersects(_Button.Hitbox)) { near = true; if (Clicked & !clicked) ClickHandle(_Button.Payload); } } }

                    for (int i = Windows.Count - 1; i >= 0; i--)
                    {
                        if (!near & clicked & !Clicked)
                        {
                            if (CursorHitbox.Intersects(Windows[i].MyToolbar.Bar))
                            {
                                drag = true; drag_to = Cursor; Clicked = true;
                                Windows.Add(Windows[i]); Windows.Remove(Windows[i]);
                                break;
                            }
                        }
                        if (Windows[i].Content.MyButton != null)
                            CheckColision(Windows[i].Content.MyButton);
                        if (Windows[i].Footer.MyButton != null)
                            CheckColision(Windows[i].Footer.MyButton);
                        if (Windows[i].MyToolbar.MyButton != null)
                            CheckColision(Windows[i].MyToolbar.MyButton);
                    }
                    if (Clicked & clicked & drag)
                    {
                        Window _Window = Windows.Last(); _Window.Position += (Cursor - new Vector2(drag_to.X, drag_to.Y));
                        if (_Window.Content.Sprites != null) { List<MySprite> _Sprites = _Window.Content.Sprites; for (int i = 0; i < _Sprites.Count; i++) _Sprites[i] = new MySprite(SpriteType.TEXTURE, _Sprites[i].Data, (_Sprites[i].Position + (Cursor - new Vector2(drag_to.X, drag_to.Y))), _Sprites[i].Size.Value, _Sprites[i].Color.Value); _Window.Content.ContentCanvas = new RectangleF(_Window.Content.MyContentBox.X, _Window.Content.MyContentBox.Y, _Window.Content.MyContentBox.Width, _Window.Content.MyContentBox.Height); }
                        WindowBuilder(_Window); drag_to = new Vector2(Cursor.X, Cursor.Y);
                    }
                    else if (!clicked & drag) { drag = false; }
                    Clicked = clicked;
                }
                catch (Exception Error) { Me.CustomData = Me.CustomData + "\n\n<" + DateTime.Now + ">" + Error; GenericErrorMessage("GetCursorÇ" + Error.ToString(), null); };
            }
        }

        void CheckColision(List<Buttons> Buttons)
        {
            Rectangle Mouse = new Rectangle((int)CursorV.X, (int)CursorV.Y, 1, 1);
            foreach (Buttons Button in Buttons)
            {
                if (Mouse.Intersects(Button.Hitbox)) { near = true; }
                if (Clicked & !clicked) { if (Mouse.Intersects(Button.Hitbox)) { Button.Payload.Action(Button.Payload.Parameter, Button.Payload.Window); break; } }
            }
        }

        void ClickHandle(Event _Task)
        {
            try
            {
                _Task.Action(_Task.Parameter, _Task.Window);
            }
            catch (Exception Error) { Me.CustomData = Me.CustomData + "\n\n<" + DateTime.Now + ">" + Error; GenericErrorMessage("ClickHandleÇ" + Error.ToString(), null); };
        }
        void Kill(Window _Window)
        {
            try { LowPriorityTasks.RemoveAt(LowPriorityTasks.FindIndex(a => a.Window == _Window)); } catch { };
            try { Tasks.RemoveAt(Tasks.FindIndex(a => a.Window == _Window)); } catch { };
            try { HighPriorityTasks.RemoveAt(HighPriorityTasks.FindIndex(a => a.Window == _Window)); } catch { };
            Windows.Remove(_Window);
        }
        #endregion
        #region Render
        void Background()
        {
            Color Water = new Vector4(.8f, .8f, .8f, .75f);
            string _Message = "Go to Setting to activate TcdOs\nTocOS - Build Beta 1.9";
            Vector2 _ContentSize = Monitors[0].Surface.MeasureStringInPixels(new StringBuilder(_Message), "White", .6f);
            Vector2 _ContentSize2 = Monitors[0].Surface.MeasureStringInPixels(new StringBuilder("Activate TcOS"), "White", .9f);
            WriteText(new Rectangle((int)ViewPort.Width - (int)_ContentSize.X - 5, (int)ViewPort.Height - (int)(_ContentSize2.Y + 32 + _ContentSize.Y), (int)_ContentSize.X, (int)_ContentSize2.Y), "Activate TcdOS", "White", .9f, Water);
            WriteText(new Rectangle((int)ViewPort.Width - (int)_ContentSize.X - 5, (int)ViewPort.Height - (int)_ContentSize.Y - 32, (int)_ContentSize.X, (int)_ContentSize.Y), _Message, "White", .6f, Water);
        }
        void TaskBar()
        {
            DrawRectangle(taskbar, Theme);
            Render(new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "Textures\\FactionLogo\\Others\\OtherIcon_18.dds",
                Position = new Vector2(StartButton.Left, StartButton.Center.Y),
                Size = new Vector2(32, 32),
                Color = new Vector3(21, 0, 25)
            });
        }
        void StartMenu()
        {
            int MySize = StartMenuButton.Count * 32;
            Menu = new Rectangle(taskbar.Left, taskbar.Top - MySize, (int)(ViewPort.Width / 4), MySize);
            DrawRectangle(Menu, Color.Gray);
            for (int i = StartMenuButton.Count - 1; i >= 0; i--)
            {
                StartMenuButton[i].Hitbox = new Rectangle(taskbar.Left, Menu.Top + (32 * i), Menu.Width, 30);
                DrawRectangle(StartMenuButton[i].Hitbox, Color.DarkGray);
                WriteText(StartMenuButton[i].Hitbox, " " + StartMenuButton[i].Name, "White", .7f);
            }
        }
        void DrawMouse()
        {
            Render(new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "Textures\\FactionLogo\\Builders\\BuilderIcon_6.dds",
                Position = new Vector2(CursorV.X - 10, CursorV.Y + 9),
                Size = new Vector2(32, 32),
                RotationOrScale = -.56f,
                Color = (drag ? Color.Orange : clicked ? Color.Red : near ? Color.Yellow : Color.Green),
                Alignment = TextAlignment.LEFT
            }); ;
        }
        public void Render(MySprite Sprite)
        {
            Rectangle _Hitbox;
            Vector2 ContentSize;
            if (Sprite.Type == SpriteType.TEXT)
            {
                ContentSize = Monitors[0].Surface.MeasureStringInPixels(new StringBuilder(Sprite.Data), Sprite.FontId, Sprite.RotationOrScale);
            }
            else
            {
                ContentSize = Sprite.Size.Value;
            }
            try
            {
                _Hitbox = new Rectangle(
                    (int)Sprite.Position.Value.X - (int)ContentSize.X,
                    (int)Sprite.Position.Value.Y - (int)ContentSize.Y,
                    (int)ContentSize.X * 2,
                    (int)ContentSize.Y * 2);
                foreach (Monitor _Screen in Monitors)
                {
                    if (_Hitbox.Intersects(_Screen.Hitbox))
                    {
                        _Screen.Frame.Add(new MySprite(Sprite.Type, Sprite.Data, Sprite.Position - _Screen.Viewport.Position + _Screen.Offset, Sprite.Size, Sprite.Color, Sprite.FontId, Sprite.Alignment, Sprite.RotationOrScale));
                    }
                }
            }
            catch (Exception Error) { Me.CustomData = Me.CustomData + "\n\n<" + DateTime.Now + ">" + Error; GenericErrorMessage("RenderÇ" + Error.ToString(), null); }
        }
        #endregion
        #region Window Builder
        void NewWindow(Vector2 Position, string Title, Content _Content, int MinimalX = 128, int MinimalY = 128)
        {
            Position.X = (Position.X == 0) ? ViewPort.Center.X : Position.X + ViewPort.X;
            Position.Y = (Position.Y == 0) ? ViewPort.Center.Y : Position.Y + ViewPort.Y;
            Windows.Add(new Window(Position, Title, new Vector2(MinimalX, MinimalY)));
            if (_Content.MyContent != "")
            {
                Windows.Last().Content = _Content;
            }
            Comit(Windows.Last());

        }
        void WindowBuilder(Window _Window)
        {
            Vector2 Size = Monitors[0].Surface.MeasureStringInPixels(new StringBuilder(_Window.Title), "InfoMessageBoxCaption", 1f);
            _Window.MyFrame = new Rectangle((int)_Window.Position.X, (int)_Window.Position.Y, (int)Size.X + 96, 32);
            WindowContentBuilder(_Window);
            WindowFooterBuilder(_Window);
            WindowToolbarBuilder(_Window);
        }
        void WindowToolbarBuilder(Window _Window)
        {
            Rectangle _ToolBar = new Rectangle(_Window.MyFrame.Left, _Window.MyFrame.Top, _Window.MyFrame.Width, 32);
            Buttons _Red = new Buttons(null, new Event(_Window.Inheritance, "Kill", _Window), new Rectangle((_ToolBar.Right - _ToolBar.Height), _ToolBar.Center.Y - (int)(_ToolBar.Height * .75 / 2), (int)(_ToolBar.Height * .75), (int)(_ToolBar.Height * .75)), null, new MySprite(SpriteType.TEXTURE, "Circle", null, null, Color.Red, null, TextAlignment.CENTER));
            Buttons _Ora = new Buttons(null, new Event(_Window.Inheritance, "Full", _Window), new Rectangle((int)(_Red.Hitbox.Left - (_ToolBar.Height * .75)), _ToolBar.Center.Y - (int)(_ToolBar.Height * .75 / 2), (int)(_ToolBar.Height * .75), (int)(_ToolBar.Height * .75)), null, new MySprite(SpriteType.TEXTURE, "Circle", null, null, Color.Orange, null, TextAlignment.CENTER));
            Buttons _Gre = new Buttons(null, new Event(_Window.Inheritance, "Hide", _Window), new Rectangle((int)(_Ora.Hitbox.Left - (_ToolBar.Height * .75)), _ToolBar.Center.Y - (int)(_ToolBar.Height * .75 / 2), (int)(_ToolBar.Height * .75), (int)(_ToolBar.Height * .75)), null, new MySprite(SpriteType.TEXTURE, "Circle", null, null, Color.Green, null, TextAlignment.CENTER));
            _Window.MyToolbar = new Toolbar(_ToolBar, _Red, _Ora, _Gre);
        }
        void WindowContentBuilder(Window _Window)
        {
            if (_Window.Content.MyContent != null)
            {
                string _ContenString = _Window.Content.MyContent;
                Vector2 ContentSize = Monitors[0].Surface.MeasureStringInPixels(new StringBuilder(_ContenString), _Window.Content.MyFont, 1.2f);
                if (ContentSize.X < ViewPort.Size.X) ContentSize.X = ViewPort.Size.X;
                if (ContentSize.Y < ViewPort.Size.Y) ContentSize.Y = ViewPort.Size.Y;
                float scale = Math.Min((ViewPort.Size.X - 40) / ContentSize.X, (ViewPort.Size.Y) / ContentSize.Y);
                ContentSize = Monitors[0].Surface.MeasureStringInPixels(new StringBuilder(_ContenString), _Window.Content.MyFont, scale);
                _Window.MyFrame = new Rectangle(_Window.MyFrame.Left, _Window.MyFrame.Top, (int)Math.Max(Math.Max(_Window.MyFrame.Width, ContentSize.X), _Window.MinimumSize.X), _Window.MyFrame.Height + (int)Math.Max(ContentSize.Y, _Window.MinimumSize.Y));
                _Window.Content.MyContentBox = new Rectangle(_Window.MyFrame.Left, _Window.MyFrame.Top + 32, _Window.MyFrame.Width, _Window.MyFrame.Height - 32);
                _Window.Content.ContentCanvas = new RectangleF(_Window.Content.MyContentBox.X, _Window.Content.MyContentBox.Y, _Window.Content.MyContentBox.Width, _Window.Content.MyContentBox.Height);
                _Window.Content.MyScale = (scale);
            }

        }
        void WindowFooterBuilder(Window _Window)
        {
            if (_Window.Footer == null)
                _Window.Footer = new Content("") { MyContentBox = new Rectangle(_Window.Content.MyContentBox.Left, _Window.Content.MyContentBox.Bottom, _Window.Content.MyContentBox.Width, 8) };
            else
                _Window.Footer.MyContentBox = new Rectangle(_Window.Content.MyContentBox.Left, _Window.Content.MyContentBox.Bottom, _Window.Content.MyContentBox.Width, _Window.Footer.MyContentBox.Height);
            _Window.MyFrame = new Rectangle(_Window.MyFrame.X, _Window.MyFrame.Y, _Window.MyFrame.Width, _Window.MyFrame.Height + _Window.Footer.MyContentBox.Height);
        }
        void Comit(Window _Window)
        {
            if (_Window.MyToolbar == null) { WindowBuilder(_Window); };
            DrawRectangle(new Rectangle(_Window.MyFrame.X - 1, _Window.MyFrame.Y - 1, _Window.MyFrame.Width + 2, _Window.MyFrame.Height + 2), Color.White);
            DrawRectangle(_Window.MyToolbar.Bar, Theme);
            WriteText(_Window.MyToolbar.Bar, _Window.Title, "InfoMessageBoxCaption", 1f);
            foreach (Buttons Button in _Window.MyToolbar.MyButton)
                DrawSprite(Button.Hitbox, Button.Icon);
            Populate(_Window);
        }
        void Populate(Window _Window)
        {
            if (_Window.Content != null) { DrawContent(_Window.Content, Color.DarkGray); }
            if (_Window.Footer != null) { DrawContent(_Window.Footer, Theme); }
        }
        void DrawContent(Content Content, Color Cor)
        {
            DrawRectangle(Content.MyContentBox, Cor);
            if (Content.Sprites != null) foreach (MySprite Sprite in Content.Sprites) Render(Sprite);
            WriteText(Content.MyContentBox, Content.MyContent, Content.MyFont, Content.MyScale);
            if (Content.MyButton != null)
                foreach (Buttons _MyButton in Content.MyButton)
                {
                    DrawRectangle(_MyButton.Hitbox, Color.White);
                    if (_MyButton.Icon.Data != new MySprite().Data) DrawSprite(_MyButton.Hitbox, _MyButton.Icon);
                }
            if (Content.MyBar != null)
                foreach (LoadingBar _MyBar in Content.MyBar)
                {
                    foreach (MySprite Sprite in _MyBar.Sprites) { Render(Sprite); }
                }
        }

        #endregion

        #region Pre-Sets
        void WriteText(Rectangle _Frame, string _Data, string Font, float _Scale, Color? Color = null)
        {
            Render(new MySprite()
            {
                Type = SpriteType.TEXT,
                Data = _Data,
                RotationOrScale = _Scale,
                Position = new Vector2(_Frame.Left, _Frame.Top),
                FontId = Font,
                Color = Color
            });
        }
        void DrawRectangle(Rectangle _Frame, Color _Color)
        {
            Render(new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareSimple",
                Position = new Vector2(_Frame.Left, _Frame.Top + (_Frame.Height / 2)),
                Size = new Vector2(_Frame.Width, _Frame.Height),
                Color = _Color,
            });
        }
        void DrawSprite(Rectangle _Frame, MySprite _Sprite)
        {
            Render(new MySprite(
                SpriteType.TEXTURE,
                _Sprite.Data,
                new Vector2(_Frame.Center.X, _Frame.Center.Y),
                new Vector2(_Frame.Width / 2, _Frame.Height / 2), _Sprite.Color, _Sprite.FontId,
                _Sprite.Alignment, _Sprite.RotationOrScale
            ));
        }
        void DrawLine(Vector2 _Point1, Vector2 _Point2, float _Width, Color _Color)
        {
            Vector2 _Position = 0.5f * (_Point1 + _Point2);
            Vector2 _Diff = _Point1 - _Point2;
            float _Length = _Diff.Length();
            if (_Length > 0)
                _Diff /= _Length;

            Vector2 _Size = new Vector2(_Length, _Width);
            float _Angle = (float)Math.Acos(Vector2.Dot(_Diff, Vector2.UnitX));
            _Angle *= Math.Sign(Vector2.Dot(_Diff, Vector2.UnitY));

            MySprite _Sprite = MySprite.CreateSprite("SquareSimple", _Position, _Size);
            _Sprite.RotationOrScale = _Angle;
            _Sprite.Color = _Color;
            Render(_Sprite);
        }
        void GenericErrorMessage(string argument, Window _Window)
        {
            switch (argument)
            {
                case "Kill": Kill(_Window); break;
                default:
                    string[] Log = argument.Split('Ç');
                    NewWindow(new Vector2(15, 0), "Error At: " + Log[0], new Content(Log[1])); _Window = Windows.Last(); _Window.Inheritance = GenericErrorMessage; WindowBuilder(_Window); break;
            }
        }


        #endregion
        #region Custom Classes
        public class Window
        {
            public Vector2 Position { get; set; }
            public string Title { get; set; }
            public Action<string, Window> Inheritance { get; set; }
            public Vector2 MinimumSize { get; set; }
            public Rectangle MyFrame { get; set; }
            public float Scale { get; set; }
            public Toolbar MyToolbar { get; set; }
            public Buttons MyButton { get; set; }
            public Content Content { get; set; }
            public Content Footer { get; set; }
            public Window(Vector2 _Position, string _Title, Vector2 _MinimumSize)
            {
                Position = _Position;
                Title = _Title;
                MinimumSize = _MinimumSize;
            }
        }
        public class Toolbar
        {
            public Rectangle Bar { get; set; }
            public List<Buttons> MyButton { get; set; }
            public Toolbar(Rectangle _Bar, Buttons _Red, Buttons _Ora, Buttons _Gre)
            {
                Bar = _Bar;
                MyButton = new List<Buttons> { _Red, _Ora, _Gre };
            }
        }
        public class Content
        {
            public string MyContent { get; set; }
            public Rectangle MyContentBox { get; set; }
            public float MyScale { get; set; }
            public string MyFont { get; set; }
            public RectangleF ContentCanvas { get; set; }
            public List<MySprite> Sprites { get; set; }
            public List<Buttons> MyButton { get; set; }
            public List<LoadingBar> MyBar { get; set; }
            public Content SubClass { get; set; }
            public Content(string _content)
            {
                MyContent = _content;
                MyFont = "White";
                MyScale = 1f;
            }
        }
        public class LoadingBar
        {
            public string Name { get; set; }
            public float Porcentage { get; set; }
            public Rectangle MyContentBox { get; set; }
            public Rectangle Foreground;
            public List<MySprite> Sprites { get; set; }
            public LoadingBar(Rectangle _MyContentBox, float _Porcentage)
            {
                MyContentBox = _MyContentBox;
                Porcentage = _Porcentage;
                Foreground = new Rectangle(_MyContentBox.X, _MyContentBox.Y, (int)(_MyContentBox.Width * _Porcentage), _MyContentBox.Height);
            }
        }
        public class Buttons
        {
            public string Name { get; set; }
            public Event Payload { get; set; }
            public Rectangle Hitbox { get; set; }
            public string Text { get; set; }
            public MySprite Icon { get; set; }
            public Buttons(string _Name, Event _Payload = null, Rectangle _Hitbox = new Rectangle(), string _Text = null, MySprite _Icon = new MySprite())
            {
                Name = _Name;
                Payload = _Payload;
                Hitbox = _Hitbox;
                Icon = _Icon;
                Text = _Text;
            }
        }
        public class Event
        {
            public Action<string, Window> Action { get; set; }
            public string Parameter { get; set; }
            public Window Window { get; set; }
            public Event(Action<string, Window> _Action, string _Parameter, Window _Window)
            {
                Parameter = _Parameter;
                Action = _Action;
                Window = _Window;
            }
        }
        public class Monitor
        {
            public IMyTextSurface Surface { get; set; }
            public MySpriteDrawFrame Frame { get; set; }
            public Rectangle Hitbox { get; set; }
            public RectangleF Viewport { get; set; }
            public Vector2 Offset { get; set; }
            public Monitor(IMyTextSurface _Surface)
            {
                Surface = _Surface;
                Offset = (Surface.TextureSize - Surface.SurfaceSize) / 2f;
            }
        }
        #endregion
        #region Storage
        string HWText;
        List<Monitor> Monitors = new List<Monitor> { };
        RectangleF ViewPort;
        Rectangle taskbar, StartButton, Menu;
        Vector2 Cursor, CursorV, offset, drag_to;
        bool clicked, near, drag, Clicked, MenuEnabled;
        int blocknumbers, tilesize, DiagHull;
        List<MySprite> SpriteConstructor = new List<MySprite> { };
        List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock> { };
        List<IMyShipConnector> Connectors = new List<IMyShipConnector> { };
        List<Window> Windows = new List<Window> { };
        List<Buttons> StartMenuButton = new List<Buttons> { };
        List<Event> Tasks = new List<Event> { };
        List<Event> HighPriorityTasks = new List<Event> { };
        List<Event> LowPriorityTasks = new List<Event> { };
        List<Action<string, UpdateType>> Services = new List<Action<string, UpdateType>> { };
        IMyCockpit helm;
        IMyShipWelder welder;
        #endregion
        #region Custom Programs


        #region Debug
        void Debug(string argument, Window _Window)
        {
            string debugText = "Cursor: " + Cursor.X.ToString("0.00") + " , " + Cursor.Y.ToString("0.00") +
            "\nVirtual Cursor: " + CursorV.X.ToString("0.00") + " , " + CursorV.Y.ToString("0.00") + "\nStart Menu: " + MenuEnabled +
            "\nResolution: " + ViewPort.Width + "X" + ViewPort.Height;

            foreach (Monitor _Screen in Monitors)
            {
                debugText = debugText + "\nScreen" + Monitors.FindIndex(A => A == _Screen) + ": " + _Screen.Hitbox.Width + "_X" + _Screen.Hitbox.Height;
            }

            switch (argument)
            {
                case "New": NewWindow(new Vector2(15, 0), "Debug", new Content(debugText), 450, 350); _Window = Windows.Last(); _Window.Inheritance = Debug; Tasks.Add(new Event(Debug, "Run", _Window)); break;
                case "Kill": Kill(_Window); break;
                case "Run": _Window.Content.MyContent = debugText; break;
                case "Log": Echo(debugText); break;
            }
        }
        #endregion

        #region Metrics

        public class TelemetryData
        {
            public List<int> Instructions { get; set; }
            public List<double> Runtime { get; set; }
            public TelemetryData() { Runtime = new List<double> { }; Instructions = new List<int> { }; }
        }
        TelemetryData Telemetry = new TelemetryData();

        void SystemMetrics()
        {
            Telemetry.Runtime.Add(Math.Round(Runtime.LastRunTimeMs, 4));
            if (Telemetry.Runtime.Count > 100) Telemetry.Runtime.RemoveAt(0);
            Telemetry.Instructions.Add(Runtime.CurrentInstructionCount);
            double Max = Telemetry.Instructions.Max(a => a);
            if (Telemetry.Instructions.Count > 100) Telemetry.Instructions.RemoveAt(0);
            HWText = "CPU Usage:" + (Max / Runtime.MaxInstructionCount).ToString("P") + "\n"
                    + "Max Instruction: " + Max + " / " + Runtime.MaxInstructionCount + "\n"
                    + "Average Instruction:" + Telemetry.Instructions.Average().ToString("0") + "\n"
                    + "Last runtime: " + Telemetry.Runtime.Last() + "\n"
                    + "Max runtime: " + Telemetry.Runtime.Max(a => a);

        }

        void HWInfo(string argument, Window _Window)
        {
            switch (argument)
            {
                case "New": NewWindow(new Vector2(15, 0), "Hardware Info", new Content(HWText), 450, 250); _Window = Windows.Last(); _Window.Inheritance = HWInfo; Tasks.Add(new Event(HWInfo, "Run", _Window)); WindowBuilder(_Window); break;
                case "Kill": Kill(_Window); break;
                case "Run":
                    SystemMetrics(); _Window.Content.MyContent = HWText;
                    _Window.Content.MyBar = new List<LoadingBar> { new LoadingBar(new Rectangle(_Window.Content.MyContentBox.Left, _Window.Content.MyContentBox.Bottom - 32, _Window.Content.MyContentBox.Width, 32), ((float)Telemetry.Instructions.Max(a => a) / Runtime.MaxInstructionCount)) };
                    Rectangle _Frame = _Window.Content.MyBar[0].MyContentBox;
                    _Window.Content.MyBar[0].Sprites = new List<MySprite> { (new MySprite()
                    {
                        Type = SpriteType.TEXTURE,
                        Data = "SquareSimple",
                        Position = new Vector2(_Frame.Left, _Frame.Top + (_Frame.Height / 2)),
                        Size = new Vector2(_Frame.Width, _Frame.Height),
                        Color = Color.White
                    })};
                    _Frame = _Window.Content.MyBar[0].Foreground;
                    _Window.Content.MyBar[0].Sprites.Add(new MySprite()
                    {
                        Type = SpriteType.TEXTURE,
                        Data = "SquareSimple",
                        Position = new Vector2(_Frame.Left, _Frame.Top + (_Frame.Height / 2)),
                        Size = new Vector2(_Frame.Width, _Frame.Height),
                        Color = (_Window.Content.MyBar[0].Porcentage > .75f) ? Color.Red : (_Window.Content.MyBar[0].Porcentage > .5f) ? Color.Yellow : Color.Green
                    }); ;

                    break;
                case "Log": Echo(HWText); break;
            }
        }
        #endregion
        #region Ship Diagnostic

        public void DiagMain(string Argument, UpdateType updateSource)
        {

            List<ShipDiagnostic> _Ships = new List<ShipDiagnostic> { new ShipDiagnostic(Me.CubeGrid) };
            foreach (IMyShipConnector Conn in Connectors)
                if (Conn.OtherConnector != null) _Ships.Add(new ShipDiagnostic(Conn.OtherConnector.CubeGrid));
            if (_Ships.Count != Ships.Count)
            {
                Ships = _Ships.ToList();
            }
        }

        public void SelfDiagnostic(string argument, Window _Window)
        {
            try
            {
                DisplayDiagnostic Me;
                switch (argument)
                {

                    case "New":
                        NewWindow(new Vector2(15, 30), "Ship Diagnostic", new Content("Ship Diagnostic"), (int)ViewPort.Height - 128, (int)ViewPort.Height - 128); Windows.Last().Inheritance = SelfDiagnostic; HighPriorityTasks.Add(new Event(SelfDiagnostic, "Run", Windows.Last()));
                        LowPriorityTasks.Add(new Event(SelfDiagnostic, "Run", Windows.Last()));
                        _Window = Windows.Last();
                        DisplaysDiag.Add(new DisplayDiagnostic(_Window));
                        string index = DisplaysDiag.FindIndex(a => a.Me == _Window).ToString();
                        _Window.Footer.MyButton = new List<Buttons> {
                            new Buttons("<", new Event(Selector, "Angleç-ç"+index,_Window), new Rectangle(), null ,new MySprite(data:"Arrow", rotation:-1.570796f)),
                            new Buttons(">", new Event(Selector, "Angleç+ç"+index,_Window), new Rectangle(), null ,new MySprite(data:"Arrow", rotation:1.570796f)),
                            new Buttons("+", new Event(Selector, "Planeç+ç"+index,_Window), new Rectangle(), null ,new MySprite(data:"Arrow", rotation:3.1415926536f)),
                            new Buttons("-", new Event(Selector, "Planeç+ç"+index,_Window), new Rectangle(), null ,new MySprite(data:"Arrow", rotation:0)),
                            new Buttons("◄", new Event(Selector, "Shipsç+ç"+index,_Window), new Rectangle(), null ,new MySprite(data:"Arrow", rotation:-1.570796f,color:Color.Red)),
                            new Buttons("►", new Event(Selector, "Shipsç+ç"+index,_Window), new Rectangle(), null ,new MySprite(data:"Arrow", rotation:1.570796f,color:Color.Red))
                        };
                        _Window.Footer.MyContentBox = new Rectangle(_Window.Content.MyContentBox.Left, _Window.Content.MyContentBox.Bottom, _Window.Content.MyContentBox.Width, 64);
                        WindowBuilder(_Window);
                        break;
                    case "Run":
                        Me = DisplaysDiag[DisplaysDiag.FindIndex(a => a.Me == _Window)];
                        _Window.Footer.MyButton[0].Hitbox = new Rectangle(_Window.Footer.MyContentBox.Center.X - 64, _Window.Footer.MyContentBox.Bottom - 64, 64, 64);
                        _Window.Footer.MyButton[1].Hitbox = new Rectangle(_Window.Footer.MyContentBox.Center.X, _Window.Footer.MyContentBox.Bottom - 64, 64, 64);
                        _Window.Footer.MyButton[2].Hitbox = new Rectangle(_Window.Footer.MyContentBox.Center.X - 128, _Window.Footer.MyContentBox.Bottom - 64, 64, 64);
                        _Window.Footer.MyButton[3].Hitbox = new Rectangle(_Window.Footer.MyContentBox.Center.X + 64, _Window.Footer.MyContentBox.Bottom - 64, 64, 64);
                        _Window.Footer.MyButton[4].Hitbox = new Rectangle(_Window.Footer.MyContentBox.Center.X - 192, _Window.Footer.MyContentBox.Bottom - 64, 64, 64);
                        _Window.Footer.MyButton[5].Hitbox = new Rectangle(_Window.Footer.MyContentBox.Center.X + 128, _Window.Footer.MyContentBox.Bottom - 64, 64, 64);
                        _Window.Content.MyContent = "Ship Diagnostic\n" + Ships[Me.ShipID].Grid.CustomName + "\nSpriteBuffer: " + Me.SpriteBuffer.Count + "\n" + Me.DiagStatus;
                        _Window.Content.Sprites = Me.SpriteBuffer;

                        if (Me.ResetDiag == null) Me.ResetDiag = ClearDiag(Me).GetEnumerator();
                        if (Me.ResetDiag.MoveNext() == false)
                        {
                            if (!drag)
                            {
                                if (Me.DiagTask == null) Me.DiagTask = RunDiag(Me).GetEnumerator();
                                if (Me.DiagTask.MoveNext() == false) { Me.DiagTask.Dispose(); Me.DiagTask = null; }
                            }
                            else if (Me.DiagTask != null) Me.DiagTask.Dispose();
                        }
                        break;
                    case "Kill": Me = DisplaysDiag[DisplaysDiag.FindIndex(a => a.Me == _Window)]; Me.ResetDiag.Dispose(); Me.DiagTask.Dispose(); Me.SpriteBuffer.Clear(); SpriteConstructor.Clear(); Kill(_Window); break;
                }
            }
            catch { SelfDiagnostic("Kill", _Window); }
        }
        public void Selector(string argument, Window _Window)
        {
            string[] inputs = argument.Split('ç');
            DisplayDiagnostic Me = DisplaysDiag[int.Parse(inputs[2])];
            switch (inputs[0])
            {
                case "Angle":
                    {
                        switch (inputs[1])
                        {
                            case "+": Me.Angle++; if (Me.Angle > 3) Me.Angle = 0; break;
                            case "-": Me.Angle--; if (Me.Angle < 0) Me.Angle = 3; break;
                        }
                    }
                    break;
                case "Plane":
                    {
                        switch (inputs[1])
                        {
                            case "+": Me.Plane++; if (Me.Angle > 5) Me.Angle = 0; break;
                            case "-": Me.Plane--; if (Me.Angle < 0) Me.Angle = 5; break;
                        }
                    }
                    break;
                case "Ships":
                    {
                        switch (inputs[1])
                        {
                            case "+": Me.ShipID++; if (Me.Angle > Ships.Count - 1) Me.Angle = 0; break;
                            case "-": Me.ShipID--; if (Me.Angle < 0) Me.Angle = Ships.Count - 1; break;
                        }
                    }
                    break;
            }
            Me.ResetDiag = null; Me.DiagTask = null; Me.SpriteBuffer.Clear();
        }

        #region Diagnostic Storage

        List<ShipDiagnostic> Ships = new List<ShipDiagnostic> { };

        public class ShipDiagnostic
        {
            public IMyCubeGrid Grid { get; set; }
            public BlockState[,,] Saved_Grid { get; set; }
            public Vector3I Grid_Min { get; set; }
            public Vector3I Grid_Max { get; set; }
            public List<TerminalBlockState> TerminalBlocks { get; set; }
            public TileState[,] Tiles;
            public ShipDiagnostic(IMyCubeGrid _Grid)
            {
                Grid = _Grid;
                TerminalBlocks = new List<TerminalBlockState>();
            }
        }
        public class DisplayDiagnostic
        {
            public Window Me { get; set; }
            public int Angle { get; set; }
            public int Plane { get; set; }
            public int ShipID { get; set; }
            public string DiagStatus { get; set; }
            public List<MySprite> SpriteBuffer { get; set; }
            public IEnumerator<bool> ResetDiag { get; set; }
            public IEnumerator<bool> DiagTask { get; set; }
            public DisplayDiagnostic(Window _Me = null)
            {
                Me = _Me;
                Angle = 0;
                Plane = 0;
                ShipID = 0;
                DiagStatus = "";
                SpriteBuffer = new List<MySprite> { };
            }
        }
        public List<DisplayDiagnostic> DisplaysDiag = new List<DisplayDiagnostic> { };
        public delegate Vector3I Rotation(Vector3I pos, Vector3I size);
        public enum BlockState { Empty, Missing, Damaged, Normal }
        public struct TileState { public int Healthy, Total; public int Depth; }
        public struct TerminalBlockState
        {
            public Vector3I Position, Size;
            public BlockState State;
            public IMyTerminalBlock Block;
        }

        #endregion

        #region Diagnostic Rotation
        static Vector3I Rotate_Rigt(Vector3I pos, Vector3I size) { return new Vector3I(size.Y - pos.Y, pos.X, pos.Z); }
        static Vector3I Rotate_Down(Vector3I pos, Vector3I size) { return new Vector3I(size.X - pos.X, size.Y - pos.Y, pos.Z); }
        static Vector3I Rotate_Left(Vector3I pos, Vector3I size) { return new Vector3I(pos.Y, size.X - pos.X, pos.Z); }
        static Vector3I X_Positive_Up(Vector3I pos, Vector3I size) { return new Vector3I(pos.Z, pos.Y, pos.X); }
        static Vector3I Y_Positive_Up(Vector3I pos, Vector3I size) { return new Vector3I(size.Z - pos.Z, pos.X, pos.Y); }
        static Vector3I Z_Positive_Up(Vector3I pos, Vector3I size) { return new Vector3I(size.X - pos.X, pos.Y, pos.Z); }
        static Vector3I X_Negative_Up(Vector3I pos, Vector3I size) { return new Vector3I(pos.Z, size.Y - pos.Y, size.X - pos.X); }
        static Vector3I Y_Negative_Up(Vector3I pos, Vector3I size) { return new Vector3I(size.Z - pos.Z, size.X - pos.X, size.Y - pos.Y); }
        static Vector3I Z_Negative_Up(Vector3I pos, Vector3I size) { return new Vector3I(size.X - pos.X, size.Y - pos.Y, size.Z - pos.Z); }
        static Vector3I X_Positive_Rigt(Vector3I pos, Vector3I size) { return Rotate_Rigt(X_Positive_Up(pos, size), Vector3I.Abs(X_Positive_Up(size, new Vector3I()))); }
        static Vector3I X_Positive_Down(Vector3I pos, Vector3I size) { return Rotate_Down(X_Positive_Up(pos, size), Vector3I.Abs(X_Positive_Up(size, new Vector3I()))); }
        static Vector3I X_Positive_Left(Vector3I pos, Vector3I size) { return Rotate_Left(X_Positive_Up(pos, size), Vector3I.Abs(X_Positive_Up(size, new Vector3I()))); }
        static Vector3I Y_Positive_Rigt(Vector3I pos, Vector3I size) { return Rotate_Rigt(Y_Positive_Up(pos, size), Vector3I.Abs(Y_Positive_Up(size, new Vector3I()))); }
        static Vector3I Y_Positive_Down(Vector3I pos, Vector3I size) { return Rotate_Down(Y_Positive_Up(pos, size), Vector3I.Abs(Y_Positive_Up(size, new Vector3I()))); }
        static Vector3I Y_Positive_Left(Vector3I pos, Vector3I size) { return Rotate_Left(Y_Positive_Up(pos, size), Vector3I.Abs(Y_Positive_Up(size, new Vector3I()))); }
        static Vector3I Z_Positive_Rigt(Vector3I pos, Vector3I size) { return Rotate_Rigt(Z_Positive_Up(pos, size), Vector3I.Abs(Z_Positive_Up(size, new Vector3I()))); }
        static Vector3I Z_Positive_Down(Vector3I pos, Vector3I size) { return Rotate_Down(Z_Positive_Up(pos, size), Vector3I.Abs(Z_Positive_Up(size, new Vector3I()))); }
        static Vector3I Z_Positive_Left(Vector3I pos, Vector3I size) { return Rotate_Left(Z_Positive_Up(pos, size), Vector3I.Abs(Z_Positive_Up(size, new Vector3I()))); }
        static Vector3I X_Negative_Rigt(Vector3I pos, Vector3I size) { return Rotate_Rigt(X_Negative_Up(pos, size), Vector3I.Abs(X_Negative_Up(size, new Vector3I()))); }
        static Vector3I X_Negative_Down(Vector3I pos, Vector3I size) { return Rotate_Down(X_Negative_Up(pos, size), Vector3I.Abs(X_Negative_Up(size, new Vector3I()))); }
        static Vector3I X_Negative_Left(Vector3I pos, Vector3I size) { return Rotate_Left(X_Negative_Up(pos, size), Vector3I.Abs(X_Negative_Up(size, new Vector3I()))); }
        static Vector3I Y_Negative_Rigt(Vector3I pos, Vector3I size) { return Rotate_Rigt(Y_Negative_Up(pos, size), Vector3I.Abs(Y_Negative_Up(size, new Vector3I()))); }
        static Vector3I Y_Negative_Down(Vector3I pos, Vector3I size) { return Rotate_Down(Y_Negative_Up(pos, size), Vector3I.Abs(Y_Negative_Up(size, new Vector3I()))); }
        static Vector3I Y_Negative_Left(Vector3I pos, Vector3I size) { return Rotate_Left(Y_Negative_Up(pos, size), Vector3I.Abs(Y_Negative_Up(size, new Vector3I()))); }
        static Vector3I Z_Negative_Rigt(Vector3I pos, Vector3I size) { return Rotate_Rigt(Z_Negative_Up(pos, size), Vector3I.Abs(Z_Negative_Up(size, new Vector3I()))); }
        static Vector3I Z_Negative_Down(Vector3I pos, Vector3I size) { return Rotate_Down(Z_Negative_Up(pos, size), Vector3I.Abs(Z_Negative_Up(size, new Vector3I()))); }
        static Vector3I Z_Negative_Left(Vector3I pos, Vector3I size) { return Rotate_Left(Z_Negative_Up(pos, size), Vector3I.Abs(Z_Negative_Up(size, new Vector3I()))); }
        static Rotation[] Bit = new Rotation[] {
            X_Positive_Up, X_Positive_Rigt, X_Positive_Down, X_Positive_Left, X_Negative_Up, X_Negative_Rigt, X_Negative_Down, X_Negative_Left,
            Y_Positive_Up, Y_Positive_Rigt, Y_Positive_Down, Y_Positive_Left, Y_Negative_Up, Y_Negative_Rigt, Y_Negative_Down, Y_Negative_Left,
            Z_Positive_Up, Z_Positive_Rigt, Z_Positive_Down, Z_Positive_Left, Z_Negative_Up, Z_Negative_Rigt, Z_Negative_Down, Z_Negative_Left };
        #endregion

        IEnumerable<bool> CheckGrid(ShipDiagnostic Ship, bool check_damaged)
        {
            if (check_damaged == false)
            {
                Ship.Grid_Min = Ship.Grid.Min - Vector3I.One;
                Ship.Grid_Max = Ship.Grid.Max + Vector3I.One;
                Ship.Saved_Grid = new BlockState[Ship.Grid_Max.X - Ship.Grid_Min.X, Ship.Grid_Max.Y - Ship.Grid_Min.Y, Ship.Grid_Max.Z - Ship.Grid_Min.Z];
                tilesize = Math.Max(Ship.Grid_Max.X - Ship.Grid_Min.X, Math.Max(Ship.Grid_Max.Y - Ship.Grid_Min.Y, Ship.Grid_Max.Z - Ship.Grid_Min.Z)) + 1;
                Ship.Tiles = new TileState[tilesize, tilesize];
            }
            int total_healthy = 0, total = 0;
            for (int _X = Ship.Grid_Min.X; _X < Ship.Grid_Max.X; ++_X)
            {
                for (int y = Ship.Grid_Min.Y; y < Ship.Grid_Max.Y; ++y)
                {
                    for (int z = Ship.Grid_Min.Z; z < Ship.Grid_Max.Z; ++z)
                    {
                        Vector3I pos = new Vector3I(_X, y, z);
                        if (check_damaged)
                        {
                            if (Ship.Saved_Grid[_X - Ship.Grid_Min.X, y - Ship.Grid_Min.Y, z - Ship.Grid_Min.Z] != BlockState.Empty)
                            {
                                ++total;
                                if (!Ship.Grid.CubeExists(pos))
                                    Ship.Saved_Grid[_X - Ship.Grid_Min.X, y - Ship.Grid_Min.Y, z - Ship.Grid_Min.Z] = BlockState.Missing;
                                else
                                    ++total_healthy;
                            }
                        }
                        else
                            Ship.Saved_Grid[_X - Ship.Grid_Min.X, y - Ship.Grid_Min.Y, z - Ship.Grid_Min.Z] = Ship.Grid.CubeExists(pos) ? BlockState.Normal : BlockState.Empty;
                    }
                }
                yield return true;
            }
            DiagHull = total > 0 ? total_healthy * 100 / total : 100;
        }

        IEnumerable<bool> Draw(Rotation Rotate, RectangleF _Canvas, ShipDiagnostic Ship)
        {
            Vector3I _Scale = Ship.Grid_Max - Ship.Grid_Min;
            Vector3I _SpriteSize = Rotate(Ship.Grid_Max - Ship.Grid_Min, _Scale * 2);
            float _ScaleF = Math.Min(_Canvas.Width, _Canvas.Height) / Math.Max(_SpriteSize.X, _SpriteSize.Y);
            float _OffsetX = (_Canvas.Width - _SpriteSize.X * _ScaleF) * 0.5f + _Canvas.X;
            float _OffsetY = (_Canvas.Height - _SpriteSize.Y * _ScaleF) * 0.5f + _Canvas.Y;
            for (int _X = 0; _X <= _SpriteSize.X; _X++)
                for (int _Y = 0; _Y <= _SpriteSize.Y; _Y++)
                    Ship.Tiles[_X, _Y] = new TileState();
            for (int _X = Ship.Grid_Min.X; _X < Ship.Grid_Max.X; ++_X)
            {
                for (int _Y = Ship.Grid_Min.Y; _Y < Ship.Grid_Max.Y; ++_Y)
                {
                    for (int _Z = Ship.Grid_Min.Z; _Z < Ship.Grid_Max.Z; ++_Z)
                    {
                        BlockState state = Ship.Saved_Grid[_X - Ship.Grid_Min.X, _Y - Ship.Grid_Min.Y, _Z - Ship.Grid_Min.Z];
                        if (state != BlockState.Empty)
                        {
                            Vector3I pos = new Vector3I(_X, _Y, _Z);
                            Vector3I poscube = Rotate(pos - Ship.Grid_Min, _Scale);
                            TileState _MyTile = Ship.Tiles[poscube.X, poscube.Y];
                            _MyTile.Depth = Math.Max(_MyTile.Depth, poscube.Z);
                            _MyTile.Total++;
                            if (state == BlockState.Normal)
                                _MyTile.Healthy++;
                            Ship.Tiles[poscube.X, poscube.Y] = _MyTile;
                        }
                    }
                }
                yield return true;
            }
            for (int _X = 0; _X <= _SpriteSize.X; _X++)
            {
                for (int _Y = 0; _Y <= _SpriteSize.Y; _Y++)
                {
                    TileState _MyTile = Ship.Tiles[_X, _Y];
                    if (_MyTile.Total == 0)
                        continue;
                    float depth = ((float)_MyTile.Depth / (float)_SpriteSize.Z);
                    depth = depth * depth * depth * depth + 0.05f;
                    float health = _MyTile.Healthy / (float)_MyTile.Total;
                    if (_MyTile.Healthy < _MyTile.Total)
                        health *= 0.5f;
                    SpriteConstructor.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(_X * _ScaleF + _OffsetX, _Y * _ScaleF + _OffsetY), new Vector2(_ScaleF, _ScaleF), new Color(depth, depth * health, depth * health)));
                }
            }
            for (int i = 0; i < Ship.TerminalBlocks.Count; ++i)
            {
                Vector3I poscube = Rotate(Ship.TerminalBlocks[i].Position - Ship.Grid_Min, _Scale);
                Vector3I possize = Rotate(Ship.TerminalBlocks[i].Size, Vector3I.Zero);
                if (possize.X < 0) { poscube.X += possize.X + 1; possize.X = -possize.X; }
                if (possize.Y < 0) { poscube.Y += possize.Y + 1; possize.Y = -possize.Y; }
                if (possize.Z < 0) { poscube.Z += possize.Z + 1; possize.Z = -possize.Z; }
                SpriteConstructor.Add(new MySprite(SpriteType.TEXTURE, "SquareHollow", new Vector2((poscube.X + possize.X * 0.5f - 0.5f) * _ScaleF + _OffsetX, (poscube.Y + possize.Y * 0.5f - 0.5f) * _ScaleF + _OffsetY), new Vector2(possize.X * _ScaleF, possize.Y * _ScaleF), Ship.TerminalBlocks[i].State == BlockState.Normal ? Color.Green : Ship.TerminalBlocks[i].State == BlockState.Damaged ? Color.Yellow : Color.Red));
            }
        }

        IEnumerable<bool> ClearDiag(DisplayDiagnostic _Display)
        {
            ShipDiagnostic Ship = Ships[_Display.ShipID];
            foreach (bool val in CheckGrid(Ship, false))
                yield return val;
            Ship.TerminalBlocks.Clear();
            for (int i = 0; i < blocks.Count; ++i)
                if (blocks[i].IsFunctional && blocks[i].CubeGrid == Ship.Grid)
                    Ship.TerminalBlocks.Add(new TerminalBlockState { Position = blocks[i].Min, Size = blocks[i].Max - blocks[i].Min + Vector3I.One, State = BlockState.Normal, Block = blocks[i] });
            HighPriorityTasks.Remove(new Event(SelfDiagnostic, "Run", Windows.Last()));
            yield break;
        }
        IEnumerable<bool> RunDiag(DisplayDiagnostic _Display)
        {
            ShipDiagnostic Ship = Ships[_Display.ShipID];
            RectangleF Rectangle = _Display.Me.Content.ContentCanvas;
            foreach (bool val in CheckGrid(Ship, true))
                yield return val;
            int terminal_healthy = 0;
            for (int i = 0; i < Ship.TerminalBlocks.Count; ++i)
            {
                bool exists = Ship.Grid.CubeExists(Ship.TerminalBlocks[i].Position);
                bool working = Ship.TerminalBlocks[i].Block.IsWorking;
                if (exists)
                    ++terminal_healthy;
                TerminalBlockState s = Ship.TerminalBlocks[i];
                s.State = exists && working ? BlockState.Normal : exists ? BlockState.Damaged : BlockState.Missing;
                Ship.TerminalBlocks[i] = s;
            }
            int DiagSystems = Ship.TerminalBlocks.Count > 0 ? terminal_healthy * 100 / Ship.TerminalBlocks.Count : 100;
            Rotation Rotate = Bit[(_Display.Angle + (_Display.Plane * 4)) % Bit.Length];
            foreach (bool val in Draw(Rotate, Rectangle, Ship))
                yield return val;
            _Display.DiagStatus = string.Format("Hull Integrity: " + DiagHull.ToString("0") + "%\nSystems Integrity: " + DiagSystems.ToString("0") + "%");
            _Display.SpriteBuffer = SpriteConstructor.ToList();
            SpriteConstructor.Clear();
        }
        #endregion

        #region Fly-By-Wire

        const string nameTag = "Fly-By-Wire";
        const double dampenerScalingFactor = 50;
        const double dampenerPrecisionDistance = 5.00;
        const double fullBurnToleranceAngle = 30;
        const double maxThrustAngle = 65;
        const double maxDampeningAngle = 65;
        const float YawSensivity = 1.0f;
        const float PitchSensivity = 1.0f;
        const float RollSensivity = 1.0f;
        const float YawAngularInertia = 0.75f;
        const float PitchAngularInertia = 0.75f;
        const float RollAngularInertia = 0.75f;
        class AdvGyroList
        {
            public List<AdvGyro> Gyros { get; set; }
            public IMyShipController Controller
            { get; set; }
            public AdvGyroList(List<IMyGyro> _Gyros, IMyTerminalBlock _Controller)
            { Controller = (IMyShipController)_Controller; Gyros = new List<AdvGyro> { }; foreach (IMyGyro gyro in _Gyros) Gyros.Add(new AdvGyro(gyro, _Controller)); }
        }
        class AdvGyro
        {
            public IMyTerminalBlock Gyro
            { get; private set; }
            public float Pitch
            {
                get
                { return Gyro.GetValueFloat(strPitch) * intPitch; }
                set
                { Gyro.SetValueFloat(strPitch, value * intPitch); }
            }
            public float Yaw
            {
                get
                { return Gyro.GetValueFloat(strYaw) * intYaw; }
                set
                { Gyro.SetValueFloat(strYaw, value * intYaw); }
            }
            public float Roll
            {
                get
                { return Gyro.GetValueFloat(strRoll) * intRoll; }
                set
                { Gyro.SetValueFloat(strRoll, value * intRoll); }
            }
            public float Power
            {
                get
                { return Gyro.GetValueFloat("Power"); }
                set
                { Gyro.SetValueFloat("Power", value); }
            }
            public bool Override
            {
                get
                { return Gyro.GetValue<bool>("Override"); }
                set
                { Gyro.SetValue<bool>("Override", value); }
            }
            public bool Enabled
            {
                get
                { return Gyro.GetValue<bool>("OnOff"); }
                set
                { Gyro.SetValue<bool>("OnOff", value); }
            }
            private string strPitch; private int intPitch; private string strYaw; private int intYaw; private string strRoll; private int intRoll; public AdvGyro(IMyGyro _Gyro, IMyTerminalBlock ForwardCockpit)
            { Gyro = _Gyro; Orientate(ForwardCockpit); }
            public void Free()
            { this.Pitch = 0; this.Yaw = 0; this.Roll = 0; this.Override = false; }
            public static void SetAllGyros(List<AdvGyro> AllGyros, bool AutoOverride = true, float? NewPitch = null, float? NewYaw = null, float? NewRoll = null)
            {
                foreach (AdvGyro _Gyro in AllGyros)
                {
                    if (NewPitch.HasValue)
                        _Gyro.Pitch = (float)NewPitch; if (NewYaw.HasValue)
                        _Gyro.Yaw = (float)NewYaw; if (NewRoll.HasValue)
                        _Gyro.Roll = (float)NewRoll; if (AutoOverride)
                    {
                        if (_Gyro.Override == false)
                            _Gyro.Override = true;
                    }
                }
            }
            public static void FreeAllGyros(List<AdvGyro> AllGyros)
            {
                foreach (AdvGyro _Gyro in AllGyros)
                {
                    _Gyro.Free();
                }
            }

            private void Orientate(IMyTerminalBlock ReferencePoint)
            {
                Vector3 V3For = Base6Directions.GetVector(ReferencePoint.Orientation.TransformDirection(Base6Directions.Direction.Forward));
                Vector3 V3Up = Base6Directions.GetVector(ReferencePoint.Orientation.TransformDirection(Base6Directions.Direction.Up));
                V3For.Normalize();
                V3Up.Normalize();
                Base6Directions.Direction B6DFor = Base6Directions.GetDirection(V3For);
                Base6Directions.Direction B6DTop = Base6Directions.GetDirection(V3Up);
                Base6Directions.Direction B6DLeft = Base6Directions.GetLeft(B6DTop, B6DFor);
                Base6Directions.Direction GyroUp = Gyro.Orientation.TransformDirectionInverse(B6DTop);
                Base6Directions.Direction GyroForward = Gyro.Orientation.TransformDirectionInverse(B6DFor);
                Base6Directions.Direction GyroLeft = Gyro.Orientation.TransformDirectionInverse(B6DLeft);
                switch (GyroUp)
                {
                    case Base6Directions.Direction.Up:
                        strYaw = "Yaw";
                        intYaw = 1;
                        break;
                    case Base6Directions.Direction.Down:
                        strYaw = "Yaw";
                        intYaw = -1;
                        break;
                    case Base6Directions.Direction.Left:
                        strYaw = "Pitch";
                        intYaw = 1;
                        break;
                    case Base6Directions.Direction.Right:
                        strYaw = "Pitch";
                        intYaw = -1;
                        break;
                    case Base6Directions.Direction.Backward:
                        strYaw = "Roll";
                        intYaw = 1;
                        break;
                    case Base6Directions.Direction.Forward:
                        strYaw = "Roll";
                        intYaw = -1;
                        break;
                }
                switch (GyroLeft)
                {
                    case Base6Directions.Direction.Up:
                        strPitch = "Yaw";
                        intPitch = 1;
                        break;
                    case Base6Directions.Direction.Down:
                        strPitch = "Yaw";
                        intPitch = -1;
                        break;
                    case Base6Directions.Direction.Left:
                        strPitch = "Pitch";
                        intPitch = 1;
                        break;
                    case Base6Directions.Direction.Right:
                        strPitch = "Pitch";
                        intPitch = -1;
                        break;
                    case Base6Directions.Direction.Backward:
                        strPitch = "Roll";
                        intPitch = 1;
                        break;
                    case Base6Directions.Direction.Forward:
                        strPitch = "Roll";
                        intPitch = -1;
                        break;
                }
                switch (GyroForward)
                {
                    case Base6Directions.Direction.Up:
                        strRoll = "Yaw";
                        intRoll = -1;
                        break;
                    case Base6Directions.Direction.Down:
                        strRoll = "Yaw";
                        intRoll = 1;
                        break;
                    case Base6Directions.Direction.Left:
                        strRoll = "Pitch";
                        intRoll = -1;
                        break;
                    case Base6Directions.Direction.Right:
                        strRoll = "Pitch";
                        intRoll = 1;
                        break;
                    case Base6Directions.Direction.Backward:
                        strRoll = "Roll";
                        intRoll = -1;
                        break;
                    case Base6Directions.Direction.Forward:
                        strRoll = "Roll";
                        intRoll = 1;
                        break;
                }
            }
        }
        List<AdvGyroList> AdvGyroscope = new List<AdvGyroList> { };
        bool WasPiloted;
        public void FlyByWire(string argument, UpdateType updateSource) { }
        public void FlyByWireSubRotine(string argument, Window _Window)
        {

            string FlyByWireText = $"The Order Fly-By-Wire System {RunningSymbol()}" +
    $"\nGravity Generators: {gravityGeneratorsExclude.Count + gravityGeneratorsSphereExclude.Count}" +
    $"\nImpulse Drive Generators: {gravityGenerators.Count}" +
    $"\nImpulse Drive Masses: {virtualMasses.Count}" +
    $"\nAdvanced Gyroscopes: {MyRawGyroscope.Count}" +
    $"\n\n Fly-By-Wire status: " + DriveState() +
    (PrecisionMode ? "\n Precision Mode On" : "");

            switch (argument)
            {
                case "New": NewWindow(new Vector2(15, 0), "Fly-By-Wire Info", new Content(FlyByWireText), 450, 250); _Window = Windows.Last(); _Window.Inheritance = FlyByWireSubRotine; HighPriorityTasks.Add(new Event(FlyByWireSubRotine, "Run", _Window)); WindowBuilder(_Window); break;
                case "Kill": Kill(_Window); break;
                case "Run":
                    try
                    {
                        _Window.Content.MyContent = FlyByWireText; ;

                        Controller = GetControlledShipController(referenceList);

                        if (Controller != null && Controller.IsUnderControl | isSoftShutdown)
                        {
                            // Travel Vector.   
                            var travelVec = Controller.GetShipVelocities().LinearVelocity;
                            if (travelVec.LengthSquared() > 0)
                            {
                                travelVec = Vector3D.Normalize(travelVec);
                            }

                            // Ship Speed.   
                            var shipSpeed = Controller.GetShipSpeed();

                            // Desired Direction Vector.   
                            var referenceMatrix = Controller.WorldMatrix;
                            var inputVec = Controller.MoveIndicator; // Raw input vector.   
                            var desiredDirection = referenceMatrix.Backward * inputVec.Z + referenceMatrix.Right * inputVec.X + referenceMatrix.Up * inputVec.Y; // World relative input vector.   
                            if (desiredDirection.LengthSquared() > 0)
                            {
                                desiredDirection = Vector3D.Normalize(desiredDirection);
                            }
                            if (!WasPiloted && !isSoftShutdown)
                            {
                                WasPiloted = true;
                                isSoftShutdown = false; isHardShutdown = false;
                            }

                            if (AdvGyroscope != null && !AdvGyroscope[0].Gyros[0].Gyro.IsWorking)
                            {
                                AdvGyroscope[0].Gyros[0].Free();
                                AdvGyroscope.Add(new AdvGyroList(MyRawGyroscope, Controller));
                            }

                            Vector3D WorldAngularVelocity = Controller.GetShipVelocities().AngularVelocity;
                            MatrixD LocalOrientationMatrix = Matrix.Transpose(Controller.WorldMatrix.GetOrientation());

                            Vector3D LocalAngularVelocity = Vector3D.Transform(WorldAngularVelocity, LocalOrientationMatrix) * 9.5492965964254;

                            float YawInput = Controller.RotationIndicator.Y * YawSensivity - ((float)LocalAngularVelocity.Y * YawAngularInertia);
                            float PitchInput = -Controller.RotationIndicator.X * PitchSensivity + ((float)LocalAngularVelocity.X * PitchAngularInertia);
                            float RollInput = Controller.RollIndicator * RollSensivity * 60 - ((float)LocalAngularVelocity.Z * RollAngularInertia);

                            foreach (IMyGravityGenerator Gravity in gravityGenerators)
                                Gravity.Enabled = true;
                            foreach (AdvGyroList Gyro in AdvGyroscope)
                            {
                                if (Gyro.Controller == Controller)
                                    foreach (AdvGyro _Gyro in Gyro.Gyros)
                                    {
                                        if (Controller.IsUnderControl)
                                            _Gyro.Override = true;
                                        else _Gyro.Override = false;
                                        _Gyro.Yaw = YawInput;
                                        _Gyro.Pitch = PitchInput;
                                        _Gyro.Roll = RollInput;
                                    }
                            }
                            bool dampenersOn = Controller.DampenersOverride;

                            ApplyThrust(travelVec, shipSpeed, desiredDirection, dampenersOn);
                            if (isSoftShutdown && shipSpeed < dampenerPrecisionDistance)
                            {
                                isHardShutdown = true;
                                isSoftShutdown = false;
                            }
                        }
                        else
                        {
                            if (WasPiloted)
                            {
                                WasPiloted = false;
                                AdvGyroscope[0].Gyros[0].Free();
                                isSoftShutdown = true;
                            }
                            if (isHardShutdown)
                            {

                                ShutdownDrive();
                            }
                        }
                    }
                    catch (Exception Error) { Me.CustomData = Me.CustomData + "\n\n<" + DateTime.Now + ">" + Error; GenericErrorMessage("FlyByWire ErrorÇSystem has not shutdown properly\n" + Error.ToString(), null); }
                    break;
            }
        }

        bool isSoftShutdown = false;
        bool isHardShutdown = false;
        bool PrecisionMode = true;

        double maxThrustDotProduct = Math.Cos(maxThrustAngle * Math.PI / 180);
        double minDampeningDotProduct = Math.Cos(maxDampeningAngle * Math.PI / 180);
        double fullBurnDotProduct = Math.Cos(fullBurnToleranceAngle * Math.PI / 180);

        List<IMyShipController> referenceList = new List<IMyShipController>();
        List<IMyGyro> MyRawGyroscope = new List<IMyGyro>();
        List<IMyGravityGenerator> gravityGenerators = new List<IMyGravityGenerator>();
        List<IMyVirtualMass> virtualMasses = new List<IMyVirtualMass>();

        bool gravityGeneratorsExcludeStandby = false;
        List<IMyGravityGenerator> gravityGeneratorsExclude = new List<IMyGravityGenerator>();
        List<bool> gravityGeneratorsExcludeEnable = new List<bool>();

        List<IMyGravityGeneratorSphere> gravityGeneratorsSphereExclude = new List<IMyGravityGeneratorSphere>();
        List<bool> gravityGeneratorsSphereExcludeEnable = new List<bool>();

        IMyShipController Controller = null;

        bool FlyByWireOOBE()
        {
            List<IMyBlockGroup> blockGroups = new List<IMyBlockGroup>();
            IMyBlockGroup theGroup = null;

            GridTerminalSystem.GetBlockGroups(blockGroups);
            foreach (IMyBlockGroup blockGroup in blockGroups)
            {
                if (blockGroup.Name.Contains(nameTag))
                {
                    theGroup = blockGroup;
                }
            }

            if (theGroup == null)
            {
                Me.CustomData = Me.CustomData + "\n\n<" + DateTime.Now + ">" + $"FlyByWireÇ[ERROR]: No group with name tag '{nameTag}' was found"; GenericErrorMessage($"FlyByWireÇ[ERROR]: No group with name tag '{nameTag}' was found", null);
                return false;
            }

            theGroup.GetBlocksOfType(referenceList, block => block.CubeGrid == Me.CubeGrid);
            if (referenceList.Count == 0)
            {
                Me.CustomData = Me.CustomData + "\n\n<" + DateTime.Now + ">" + $"FlyByWireÇ[ERROR]: No remote or control seat inside group name tag '{nameTag}' was found"; GenericErrorMessage($"FlyByWireÇ[ERROR]: No remote or control seat inside group name tag '{nameTag}' was found", null);
                return false;
            }
;
            theGroup.GetBlocksOfType(MyRawGyroscope, block => block.CubeGrid == Me.CubeGrid);
            if (MyRawGyroscope.Count == 0)
            {
                Me.CustomData = Me.CustomData + "\n\n<" + DateTime.Now + ">" + $"FlyByWireÇ[ERROR]: No gyroscope inside group name tag '{nameTag}' was found"; GenericErrorMessage($"FlyByWireÇ[ERROR]: No gyroscope inside group name tag '{nameTag}' was found", null);
                return false;
            }
            AdvGyroscope.Add(new AdvGyroList(MyRawGyroscope, referenceList[0]));

            theGroup.GetBlocksOfType(gravityGenerators, block => block.CubeGrid == referenceList[0].CubeGrid);
            if (gravityGenerators.Count == 0)
            {
                Me.CustomData = Me.CustomData + "\n\n<" + DateTime.Now + ">" + $"FlyByWireÇ[ERROR]: No Gravity Generators inside group name tag '{nameTag}' was found"; GenericErrorMessage($"FlyByWireÇ[ERROR]: No Gravity Generators inside group name tag '{nameTag}' was found", null);
                return false;
            }

            theGroup.GetBlocksOfType(virtualMasses, block => block.CubeGrid == referenceList[0].CubeGrid);
            if (gravityGenerators.Count == 0)
            {
                Me.CustomData = Me.CustomData + "\n\n<" + DateTime.Now + ">" + $"FlyByWireÇ[ERROR]: No Artificial Mass inside group name tag '{nameTag}' was found"; GenericErrorMessage($"FlyByWireÇ[ERROR]: No Artificial Mass inside group name tag '{nameTag}' was found", null);
                return false;
            }

            GridTerminalSystem.GetBlocksOfType(gravityGeneratorsExclude, block => block.CubeGrid == referenceList[0].CubeGrid);
            foreach (IMyGravityGenerator thisgravityGenerator in gravityGenerators)
            {
                gravityGeneratorsExclude.Remove(thisgravityGenerator);
            }
            foreach (IMyGravityGenerator thisgravityGenerator in gravityGeneratorsExclude)
            {
                gravityGeneratorsExcludeEnable.Add(thisgravityGenerator.Enabled);
            }

            GridTerminalSystem.GetBlocksOfType(gravityGeneratorsSphereExclude, block => block.CubeGrid == referenceList[0].CubeGrid);
            foreach (IMyGravityGeneratorSphere thisgravityGenerator in gravityGeneratorsSphereExclude)
            {
                gravityGeneratorsSphereExcludeEnable.Add(thisgravityGenerator.Enabled);
            }

            return true;
        }

        IMyShipController GetControlledShipController(List<IMyShipController> SCs)
        {
            foreach (IMyShipController thisController in SCs)
            {
                if (thisController.IsUnderControl && thisController.CanControlShip)
                    return thisController;
            }

            return SCs[0];
        }

        void ShutdownDrive()
        {
            foreach (IMyGravityGenerator thisGravityGenerator in gravityGenerators)
            {
                thisGravityGenerator.ApplyAction("OnOff_Off");
            }

            foreach (IMyVirtualMass thisVirtualMass in virtualMasses)
            {
                thisVirtualMass.ApplyAction("OnOff_Off");
            }

            foreach (IMyGravityGenerator thisgravityGenerator in gravityGeneratorsExclude)
            {
                thisgravityGenerator.ApplyAction("OnOff_On");
            }
        }
        void ApplyThrust(Vector3D travelVec, double speed, Vector3D desiredDirectionVec, bool dampenersOn)
        {
            bool isActive = false;

            /// Fly-By-Wire Gravity Generator ///   
            foreach (IMyGravityGenerator thisGravityGenerator in gravityGenerators)
            {
                var thrustDirection = thisGravityGenerator.WorldMatrix.Up; //gets the direction that the thruster flame fires   
                float scale = -(float)thrustDirection.Dot(desiredDirectionVec); //projection of the thruster's direction onto the desired direction    

                if (scale >= maxThrustDotProduct)
                {
                    scale /= (float)fullBurnDotProduct; //scales it so that the thruster output ramps down after the fullBurnToleranceAngle is exceeded    

                    var velocityInThrustDirection = thrustDirection.Dot(travelVec) * speed;
                    double targetOverride = 0;

                    if (velocityInThrustDirection < 1)
                        targetOverride = velocityInThrustDirection * dampenerScalingFactor;
                    else
                        targetOverride = velocityInThrustDirection * Math.Abs(velocityInThrustDirection) * dampenerScalingFactor;

                    SetGravityGeneratorOverride(thisGravityGenerator, (float)Math.Max(scale * 100f, targetOverride));
                    isActive = true;
                    continue;
                }

                thrustDirection = thisGravityGenerator.WorldMatrix.Down; //gets the direction that the thruster flame fires    
                scale = -(float)thrustDirection.Dot(desiredDirectionVec); //projection of the thrust's direction onto the desired direction    

                if (scale >= maxThrustDotProduct)
                {
                    scale /= (float)fullBurnDotProduct; //scales it so that the thruster output ramps down after the fullBurnToleranceAngle is exceeded    

                    var velocityInThrustDirection = thrustDirection.Dot(travelVec) * speed;
                    double targetOverride = 0;

                    if (velocityInThrustDirection < 1)
                        targetOverride = velocityInThrustDirection * dampenerScalingFactor;
                    else
                        targetOverride = velocityInThrustDirection * Math.Abs(velocityInThrustDirection) * dampenerScalingFactor;

                    SetGravityGeneratorOverride(thisGravityGenerator, -(float)Math.Max(scale * 100f, targetOverride));
                    isActive = true;
                    continue;
                }

                /// Dampeners ///   
                if (dampenersOn && (!PrecisionMode || (PrecisionMode && (speed > dampenerPrecisionDistance))))
                {
                    thrustDirection = thisGravityGenerator.WorldMatrix.Up; //gets the direction that the thruster flame fires    
                    scale = -(float)thrustDirection.Dot(travelVec); //projection of the thrust's direction onto the desired direction    

                    if ((scale >= maxThrustDotProduct) && (thrustDirection.Dot(desiredDirectionVec) <= minDampeningDotProduct))
                    {
                        var velocityInThrustDirection = thrustDirection.Dot(travelVec) * speed;
                        double targetOverride = 0;

                        if (velocityInThrustDirection < 1)
                            targetOverride = velocityInThrustDirection * dampenerScalingFactor;
                        else
                            targetOverride = velocityInThrustDirection * velocityInThrustDirection * dampenerScalingFactor;

                        SetGravityGeneratorOverride(thisGravityGenerator, (float)targetOverride);
                        isActive = true;
                        continue;
                    }

                    thrustDirection = thisGravityGenerator.WorldMatrix.Down; //gets the direction that the thruster flame fires    
                    scale = -(float)thrustDirection.Dot(travelVec); //projection of the thrust's direction onto the desired direction    

                    if ((scale >= maxThrustDotProduct) && (thrustDirection.Dot(desiredDirectionVec) <= minDampeningDotProduct))
                    {
                        var velocityInThrustDirection = thrustDirection.Dot(travelVec) * speed;
                        double targetOverride = 0;

                        if (velocityInThrustDirection < 1)
                            targetOverride = velocityInThrustDirection * dampenerScalingFactor;
                        else
                            targetOverride = velocityInThrustDirection * velocityInThrustDirection * dampenerScalingFactor;

                        SetGravityGeneratorOverride(thisGravityGenerator, -(float)targetOverride);
                        isActive = true;
                        continue;
                    }
                }

                SetGravityGeneratorOverride(thisGravityGenerator, 0);
            }
            if (isActive)
            {
                for (int i = 0; i < gravityGeneratorsExclude.Count && !gravityGeneratorsExcludeStandby; ++i)
                {
                    gravityGeneratorsExcludeEnable[i] = gravityGeneratorsExclude[i].Enabled;
                }
                for (int i = 0; i < gravityGeneratorsSphereExclude.Count && !gravityGeneratorsExcludeStandby; ++i)
                {
                    gravityGeneratorsSphereExcludeEnable[i] = gravityGeneratorsSphereExclude[i].Enabled;
                }
                foreach (IMyGravityGeneratorSphere thisGravityGenerator in gravityGeneratorsSphereExclude)
                {
                    thisGravityGenerator.ApplyAction("OnOff_Off");
                }

                gravityGeneratorsExcludeStandby = true;
            }
            else
            {
                for (int i = 0; i < gravityGeneratorsExclude.Count && gravityGeneratorsExcludeStandby; ++i)
                {
                    gravityGeneratorsExclude[i].ApplyAction((gravityGeneratorsExcludeEnable[i]) ? "OnOff_On" : "OnOff_Off");
                }
                for (int i = 0; i < gravityGeneratorsSphereExclude.Count && gravityGeneratorsExcludeStandby; ++i)
                {
                    gravityGeneratorsSphereExclude[i].ApplyAction((gravityGeneratorsSphereExcludeEnable[i]) ? "OnOff_On" : "OnOff_Off");
                }

                gravityGeneratorsExcludeStandby = false;
            }
            if (isActive)
            {
                foreach (IMyVirtualMass thisVirtualMass in virtualMasses)
                {
                    thisVirtualMass.ApplyAction("OnOff_On");
                }
            }
            else
            {
                foreach (IMyVirtualMass thisVirtualMass in virtualMasses)
                {
                    thisVirtualMass.ApplyAction("OnOff_Off");
                }
            }
        }

        void SetGravityGeneratorOverride(IMyGravityGenerator gravityGenerator, float overrideValue)
        {
            gravityGenerator.SetValue<Single>("Gravity", overrideValue);
        }
        string DriveState()
        {
            string strDriveState;

            if (isHardShutdown)
                strDriveState = "Shutdown. ";
            else if (isSoftShutdown)
                strDriveState = "Shuting Down... ";
            else
                strDriveState = "Running... ";
            return strDriveState;

        }
        int runningSymbolVariant = 0;
        string RunningSymbol()
        {
            runningSymbolVariant++;
            string strRunningSymbol = "";

            if (runningSymbolVariant == 0)
                strRunningSymbol = "|";
            else if (runningSymbolVariant == 1)
                strRunningSymbol = "/";
            else if (runningSymbolVariant == 2)
                strRunningSymbol = "--";
            else if (runningSymbolVariant == 3)
            {
                strRunningSymbol = "\\";
                runningSymbolVariant = 0;
            }

            return strRunningSymbol;
        }

        #endregion

        #region Display


        public List<IMyTerminalBlock> DisplayManaged;
        public void DisplayManagmentSetup()
        {

            DisplayManaged = new List<IMyTerminalBlock> { };
            foreach (IMyTerminalBlock block in blocks)
            {
                if (block.CustomData.Contains("[Sprite Builder Display Script - Text Surface Config - Screen "))
                {
                    DisplayManaged.Add(block);
                }
            }
        }

        public void DisplayManagment(string argument, Window _Window)
        {

            foreach (IMyTerminalBlock _Block in DisplayManaged)
            {
                MyIni _CanvasIni = new MyIni { };
                MyIniParseResult result;
                if (!_CanvasIni.TryParse(_Block.CustomData, out result))
                {
                    Echo($"CustomData error:\nLine {result}");
                }

                DisplayManagmentDraw(_CanvasIni, _Block);
            }
        }
        public void DisplayManagmentDraw(MyIni _CanvasIni, IMyTerminalBlock _Block)
        {
            IMyTextSurface _MySurface = ((IMyTextSurfaceProvider)_Block).GetSurface(0);
            List<string> DisplayList = new List<string> { };
            _CanvasIni.GetSections(DisplayList);
            Me.CustomData = DisplayList[0];
            foreach (string Line in DisplayList)
                if (Line.StartsWith("Sprite Builder Display Script - Text Surface Config"))
                {
                    _MySurface = ((IMyTextSurfaceProvider)_Block).GetSurface(int.Parse(Line.Replace("Sprite Builder Display Script - Text Surface Config - Screen ", "")));
                    _MySurface.ContentType = ContentType.SCRIPT;
                    _MySurface.Script = "";
                    _MySurface.ScriptBackgroundColor = ParseColor(_CanvasIni.Get(Line, "Background color").ToString());
                    float Scale = float.Parse(_CanvasIni.Get(Line, "Sprite scale").ToString());
                    MySpriteDrawFrame DisplayFrame = _MySurface.DrawFrame();
                    string[] SpritesList = _CanvasIni.Get(Line, "Sprite list").ToString().Split('\n');
                    foreach (string Sprite in SpritesList)
                    {
                        Me.CustomData = Me.CustomData + "+" + Sprite;
                        try
                        {
                            if (_CanvasIni.ContainsSection($"Sprite:{Sprite}"))
                            {
                                DisplayFrame.Add(new MySprite(
                                SpriteType.TEXTURE,
                                _CanvasIni.Get($"Sprite:{Sprite}", "Type").ToString(),
                                ParsePosAndSize(_CanvasIni.Get($"Sprite:{Sprite}", "Position").ToString()) * Scale + (_MySurface.TextureSize / 2),
                                ParsePosAndSize(_CanvasIni.Get($"Sprite:{Sprite}", "Size").ToString())*Scale,
                                ParseColor(_CanvasIni.Get($"Sprite:{Sprite}", "Color").ToString()),
                                null,
                                TextAlignment.CENTER,
                                float.Parse(_CanvasIni.Get($"Sprite:{Sprite}", "Rotation").ToString())));
                                Me.CustomData = Me.CustomData + "\n Done";
                            }
                            else if (_CanvasIni.ContainsSection($"Text:{Sprite}"))
                            {
                                DisplayFrame.Add(new MySprite(
                                SpriteType.TEXT,
                                StringProcessament(_CanvasIni.Get($"Text:{Sprite}", "Text").ToString(), _Block),
                                ParsePosAndSize(_CanvasIni.Get($"Text:{Sprite}", "Position").ToString()) * Scale + (_MySurface.TextureSize / 2),
                                null,
                                ParseColor(_CanvasIni.Get($"Text:{Sprite}", "Color").ToString()),
                                _CanvasIni.Get($"Text:{Sprite}", "Font").ToString(),
                                TextAlignment.LEFT,
                                float.Parse(_CanvasIni.Get($"Text:{Sprite}", "Scale").ToString()) * Scale));
                                Me.CustomData = Me.CustomData + "\n Done";
                            }
                        }
                        catch { }
                    }
                    DisplayFrame.Dispose();
                }



        }
        public Vector2 ParsePosAndSize(string Vector)
        {
            try
            {
                Me.CustomData = Me.CustomData + Vector;
                string VectorPreParse = Vector.Replace("{X:", "").Replace(" Y:", ",").Replace("}", "");
                string[] Values = VectorPreParse.Split(',');
                return new Vector2(float.Parse(Values[0]), float.Parse(Values[1]));
            }
            catch (Exception Error) { Me.CustomData = Me.CustomData + "\n\n<" + DateTime.Now + ">" + Vector + "\n" + Error; GenericErrorMessage("ParsePosAndSize ErrorÇFailure to Parse Vector\n" + $"{Vector} \n {Error}", null); return new Vector2 { }; }
        }
        public Color ParseColor(string Vector)
        {
            try
            {
                Me.CustomData = Me.CustomData + Vector;
                if (Vector.Contains("Theme")) return Theme;
                else if (Vector.Contains("Background")) return BackgroundColor;
                else
                {
                    string VectorPreParse = Vector.Replace(" ", "");
                    string[] Values = VectorPreParse.Split(',');
                    return new Vector4(float.Parse(Values[0]) / 255f, float.Parse(Values[1]) / 255f, float.Parse(Values[2]) / 255f, float.Parse(Values[3]) / 255f);
                }
            }
            catch (Exception Error)
            {
                Me.CustomData = Me.CustomData + "\n\n<" + DateTime.Now + ">" + Vector + "\n" + Error; GenericErrorMessage("ParseColor ErrorÇFailure to Parse Vector\n" + $"{Vector} \n {Error}", null); return new Vector4 { };
            }
        }
        public String StringProcessament(string _String, IMyTerminalBlock _Block)
        {

            _String = _String.Replace("{Status}", _Block.IsWorking ? "Active" : "Inactive");
            _String = _String.Replace("{ID}", _Block.EntityId.ToString());
            return _String;
        }




        #endregion
        #endregion
    }
}