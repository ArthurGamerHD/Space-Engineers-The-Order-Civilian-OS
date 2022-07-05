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
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1 | UpdateFrequency.Update10 | UpdateFrequency.Update100;
            Renew(); Reload(); Start(); Grid = Me.CubeGrid;
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
            StartMenuButton = new List<Buttons> { new Buttons("Debug", new Event(_Debug, "New")), new Buttons("Ship Diagnostic", new Event(SelfDiagnostic, "New")), new Buttons("HW_Info", new Event(_HWInfo, "New")) };
        }
        void Main(string argument, UpdateType updateSource)
        {
            Echo("Something");

            foreach (Action<string, UpdateType> Run in Services) Run(argument, updateSource);

            foreach (Monitor surface in Monitors) { Echo(((IMyTextPanel)surface.Surface).CustomName); }
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
            if ((updateSource & UpdateType.Update1) != 0)
                foreach (Event _Task in HighPriorityTasks) _Task.Action(_Task.Parameter);
            if ((updateSource & UpdateType.Update10) != 0)
                foreach (Event _Task in Tasks) _Task.Action(_Task.Parameter);
            if ((updateSource & UpdateType.Update100) != 0)
                foreach (Event _Task in LowPriorityTasks) _Task.Action(_Task.Parameter);
            if ((updateSource & UpdateType.Update10) != 0)
            {
                Background();
                for (int i = 0; i < Monitors.Count; i++)
                    Monitors[i].Frame = Monitors[i].Surface.DrawFrame();
                for (int i = 0; i < Windows.Count; i++)
                    Comit(Windows[i]);
                if (MenuEnabled) StartMenu();
                TaskBar();
                DrawMouse();
            }
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
                    if (block is IMyShipConnector & block.CustomName.Contains("Here")) Connectors.Add((IMyShipConnector)block);
                    if (block is IMyShipWelder & block.CustomName.Contains("Mouse")) welder = (IMyShipWelder)block;
                    if (block is IMyTextPanel & block.CustomName.Contains("LCD Master")) { Monitors.RemoveAt(0); Panels.Add((IMyTextPanel)block); ((IMyTextPanel)block).ContentType = ContentType.SCRIPT; };
                    if (block is IMyTextPanel & block.CustomName.Contains("LCD Slave")) { Panels.Add((IMyTextPanel)block); ((IMyTextPanel)block).ContentType = ContentType.SCRIPT; };

                }
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
                        foreach (Buttons Button in Windows[i].MyToolbar.MyButton)
                        {
                            if (CursorHitbox.Intersects(Button.Hitbox)) { near = true; }
                            if (Clicked & !clicked) { if (CursorHitbox.Intersects(Button.Hitbox)) { Button.Payload.Action(Button.Payload.Parameter); } }
                        }
                        if (Windows[i].Content.MyButton != null)
                            foreach (Buttons Button in Windows[i].Content.MyButton)
                            {
                                if (CursorHitbox.Intersects(Button.Hitbox)) { near = true; }
                                if (Clicked & !clicked) { if (CursorHitbox.Intersects(Button.Hitbox)) { Button.Payload.Action(Button.Payload.Parameter); } }
                            }
                        if (!near & clicked & !Clicked)
                        {
                            if (CursorHitbox.Intersects(Windows[i].MyToolbar.Bar))
                            {
                                drag = true; drag_to = Cursor; Clicked = true;
                                Windows.Add(Windows[i]); Windows.Remove(Windows[i]);
                                break;
                            }
                        }
                    }
                    if (Clicked & clicked & drag)
                    {
                        Window _Window = Windows.Last(); _Window.Position = _Window.Position + (Cursor - new Vector2(drag_to.X, drag_to.Y));
                        if (_Window.Content.Sprites != null) { List<MySprite> _Sprites = _Window.Content.Sprites; for (int i = 0; i < _Sprites.Count; i++) _Sprites[i] = new MySprite(SpriteType.TEXTURE, _Sprites[i].Data, (_Sprites[i].Position + (Cursor - new Vector2(drag_to.X, drag_to.Y))), _Sprites[i].Size.Value, _Sprites[i].Color.Value); _Window.Content.ContentCanvas = new RectangleF(_Window.Content.MyContentBox.X, _Window.Content.MyContentBox.Y, _Window.Content.MyContentBox.Width, _Window.Content.MyContentBox.Height); }
                        WindowBuilder(_Window); drag_to = new Vector2(Cursor.X, Cursor.Y);
                    }
                    else if (!clicked & drag) { drag = false; }
                    Clicked = clicked;
                }
                catch (Exception Error) { NewWindow(Cursor, "GetCursor Error", new Content(Error.ToString())); };
            }
        }

        void ClickHandle(Event _Task)
        {
            try
            {
                _Task.Action(_Task.Parameter);
            }
            catch (Exception Error) { NewWindow(Cursor, "ClickHandle Error", new Content(Error.ToString())); };
        }
        void Kill(Action<string> _Target)
        {
            try { LowPriorityTasks.RemoveAt(LowPriorityTasks.FindIndex(a => a.Action == _Target)); } catch { };
            try { Tasks.RemoveAt(Tasks.FindIndex(a => a.Action == _Target)); } catch { };
            try { HighPriorityTasks.RemoveAt(HighPriorityTasks.FindIndex(a => a.Action == _Target)); } catch { };
            try { Windows.RemoveAt(Windows.FindIndex(a => a.Inheritance == _Target)); } catch { };
        }
        #endregion
        #region Render
        void Background()
        {
            Fill(new Rectangle((int)ViewPort.X, (int)ViewPort.Y, (int)ViewPort.Width, (int)ViewPort.Height), Theme);
            Color Water = new Vector4(.8f, .8f, .8f, .75f);
            string _Message = "Go to Setting to activate TcdOs\nTocOS - Build Beta 1.9";
            Vector2 _ContentSize = Monitors[0].Surface.MeasureStringInPixels(new StringBuilder(_Message), "White", .6f);
            Vector2 _ContentSize2 = Monitors[0].Surface.MeasureStringInPixels(new StringBuilder("Activate TcOS"), "White", .9f);
            Write(new Rectangle((int)ViewPort.Width - (int)_ContentSize.X - 5, (int)ViewPort.Height - (int)(_ContentSize2.Y + 32 + _ContentSize.Y), (int)_ContentSize.X, (int)_ContentSize2.Y), "Activate TcdOS", "White", .9f, Water);
            Write(new Rectangle((int)ViewPort.Width - (int)_ContentSize.X - 5, (int)ViewPort.Height - (int)_ContentSize.Y - 32, (int)_ContentSize.X, (int)_ContentSize.Y), _Message, "White", .6f, Water);
        }
        void TaskBar()
        {
            Fill(taskbar, Theme);
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
            Fill(Menu, Color.Gray);
            for (int i = StartMenuButton.Count - 1; i >= 0; i--)
            {
                StartMenuButton[i].Hitbox = new Rectangle(taskbar.Left, Menu.Top + (32 * i), Menu.Width, 30);
                Fill(StartMenuButton[i].Hitbox, Color.DarkGray);
                Write(StartMenuButton[i].Hitbox, " " + StartMenuButton[i].Name, "White", .7f);
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
            foreach (Monitor Surface in Monitors)
                Surface.Frame.Dispose();
        }
        public void Render(MySprite Sprite)
        {
            Rectangle _Hitbox = new Rectangle();
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
                _Hitbox = new Rectangle((Sprite.Alignment == TextAlignment.LEFT) ? (int)Sprite.Position.Value.X : (Sprite.Alignment == TextAlignment.CENTER) ? (int)Sprite.Position.Value.X - ((int)ContentSize.X / 2) : (int)Sprite.Position.Value.X - (int)ContentSize.X, (int)Sprite.Position.Value.Y, (int)ContentSize.X, (int)ContentSize.Y);
                foreach (Monitor _Screen in Monitors)
                {
                    if (_Hitbox.Intersects(_Screen.Hitbox))
                    {
                        _Screen.Frame.Add(new MySprite(Sprite.Type, Sprite.Data, Sprite.Position - _Screen.Viewport.Position, Sprite.Size, Sprite.Color, Sprite.FontId, Sprite.Alignment, Sprite.RotationOrScale));
                    }
                }
            }
            catch (Exception Error) {Me.CustomData = Me.CustomData + "\n\n<" + DateTime.Now + ">" + Error; }
        }
        #endregion
        #region Window Builder
        void NewWindow(Vector2 Position, string Title, Content _Content, int MinimalX = 128, int MinimalY = 128)
        {
            Position.X = Position.X + ViewPort.X;
            Position.Y = Position.Y + ViewPort.Y;
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
            WindowToolbarBuilder(_Window);
        }
        void WindowToolbarBuilder(Window _Window)
        {
            Rectangle _ToolBar = new Rectangle(_Window.MyFrame.Left, _Window.MyFrame.Top, _Window.MyFrame.Width, 32);
            Buttons _Red = new Buttons(null, new Event(_Window.Inheritance, "Kill"), new Rectangle((_ToolBar.Right - _ToolBar.Height), _ToolBar.Center.Y - (int)(_ToolBar.Height * .75 / 2), (int)(_ToolBar.Height * .75), (int)(_ToolBar.Height * .75)), null, new MySprite(SpriteType.TEXTURE, "Circle", null, null, Color.Red, null, TextAlignment.CENTER));
            Buttons _Ora = new Buttons(null, new Event(_Window.Inheritance, "Full"), new Rectangle((int)(_Red.Hitbox.Left - (_ToolBar.Height * .75)), _ToolBar.Center.Y - (int)(_ToolBar.Height * .75 / 2), (int)(_ToolBar.Height * .75), (int)(_ToolBar.Height * .75)), null, new MySprite(SpriteType.TEXTURE, "Circle", null, null, Color.Orange, null, TextAlignment.CENTER));
            Buttons _Gre = new Buttons(null, new Event(_Window.Inheritance, "Hide"), new Rectangle((int)(_Ora.Hitbox.Left - (_ToolBar.Height * .75)), _ToolBar.Center.Y - (int)(_ToolBar.Height * .75 / 2), (int)(_ToolBar.Height * .75), (int)(_ToolBar.Height * .75)), null, new MySprite(SpriteType.TEXTURE, "Circle", null, null, Color.Green, null, TextAlignment.CENTER));
            _Window.MyToolbar = new Toolbar(_ToolBar, _Red, _Ora, _Gre);
        }
        void WindowContentBuilder(Window _Window)
        {
            try
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
            catch (Exception Error) { NewWindow(Cursor, "WindowContentBuilder Error", new Content(Error.ToString())); };
        }
        void WindowFooterBuilder(Window _Window) { }
        void Comit(Window _Window)
        {
            if (_Window.MyToolbar == null) { WindowBuilder(_Window); };
            Fill(new Rectangle(_Window.MyFrame.X - 1, _Window.MyFrame.Y - 1, _Window.MyFrame.Width + 2, _Window.MyFrame.Height + 2), Color.White);
            Fill(_Window.MyToolbar.Bar, Theme);
            Write(_Window.MyToolbar.Bar, _Window.Title, "InfoMessageBoxCaption", 1f);
            foreach (Buttons Button in _Window.MyToolbar.MyButton)
                DrawSprite(Button.Hitbox, Button.Icon);
            DrawContent(_Window);
        }
        void DrawContent(Window _Window)
        {
            if (_Window.Content != null)
            {
                Fill(_Window.Content.MyContentBox, Color.DarkGray);
                if (_Window.Content.Sprites != null) foreach (MySprite Sprite in _Window.Content.Sprites) Render(Sprite);
                Write(_Window.Content.MyContentBox, _Window.Content.MyContent, _Window.Content.MyFont, _Window.Content.MyScale);
                if (_Window.Content.MyButton != null)
                    foreach (Buttons _MyButton in _Window.Content.MyButton)
                    {
                        Fill(_MyButton.Hitbox, Color.White);
                        if (_MyButton.Icon.Data != new MySprite().Data) DrawSprite(_MyButton.Hitbox, _MyButton.Icon);
                    }
            }
        }
        void Fill(Rectangle _Frame, Color _Color)
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
        void Write(Rectangle _Frame, string _Data, string Font, float _Scale, Color? Color = null)
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
        void NewButton(Rectangle A, string text, float scale, MySpriteDrawFrame frame)
        {
            Render(new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareSimple",
                Position = new Vector2(A.Left - 2, A.Top - 1 + ((A.Bottom - A.Top) / 2)),
                Size = new Vector2(A.Right - A.Left + 4, A.Bottom - A.Top + 4),
            }); //Button box shadow
            Render(new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareSimple",
                Position = new Vector2(A.Left, A.Top + ((A.Bottom - A.Top) / 2)),
                Size = new Vector2(A.Right - A.Left, A.Bottom - A.Top),
                Color = Color.Gray,
            }); //Button box
            Render(new MySprite()
            {
                Type = SpriteType.TEXT,
                Data = text,
                RotationOrScale = scale * 0.8f,
                Position = new Vector2(A.Center.X, A.Top),
                Alignment = TextAlignment.CENTER
            }); //Button text
        }
        void ToolbarButton(Rectangle A, float Y, Color color)
        {
            Render(new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "Circle",
                Position = new Vector2(A.Center.X, A.Center.Y),
                Size = new Vector2(Y, Y),
                Alignment = TextAlignment.CENTER,
                Color = color
            });
        }

        #endregion
        #region Custom Classes
        public class Window
        {
            public Vector2 Position { get; set; }
            public string Title { get; set; }
            public Action<string> Inheritance { get; set; }
            public Vector2 MinimumSize { get; set; }
            public Rectangle MyFrame { get; set; }
            public float Scale { get; set; }
            public Toolbar MyToolbar { get; set; }
            public Buttons MyButton { get; set; }
            public Content Content { get; set; }
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
            public Content(string _content)
            {
                MyContent = _content;
                MyFont = "White";
                MyScale = 1f;
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
            public Action<string> Action { get; set; }
            public string Parameter { get; set; }
            public Event(Action<string> _Action, string _Parameter)
            {
                Parameter = _Parameter;
                Action = _Action;
            }
        }
        public class Monitor
        {
            public IMyTextSurface Surface { get; set; }
            public MySpriteDrawFrame Frame { get; set; }
            public Rectangle Hitbox { get; set; }
            public RectangleF Viewport { get; set; }
            public Monitor(IMyTextSurface _Surface)
            {
                Surface = _Surface;
            }
        }
        #endregion
        #region Storage
        List<Monitor> Monitors = new List<Monitor> { };
        IMyCubeGrid Grid;
        RectangleF ViewPort;
        List<RectangleF> ViewPorts = new List<RectangleF> { };
        Rectangle taskbar, StartButton, Menu;
        Vector2 Cursor, CursorV, offset, drag_to;
        bool clicked, near, drag, Clicked, MenuEnabled, want_reset = true;
        int blocknumbers, tilesize, DiagHull;
        List<MySprite> SpriteBuffer = new List<MySprite> { }, SpriteConstructor = new List<MySprite> { };
        List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock> { };
        List<IMyShipConnector> Connectors = new List<IMyShipConnector> { };
        List<Window> Windows = new List<Window> { };
        List<Buttons> StartMenuButton = new List<Buttons> { };
        List<Event> Tasks = new List<Event> { };
        List<Event> HighPriorityTasks = new List<Event> { };
        List<Event> LowPriorityTasks = new List<Event> { };
        List<Action<string, UpdateType>> Services = new List<Action<string, UpdateType>> { };
        List<MySpriteDrawFrame> Frame = new List<MySpriteDrawFrame> { };
        Color Theme = new Vector4(.1f, .1f, .1f, .95f);
        Color Faction = new Vector3(21, 0, 25);
        Color Dark = new Vector3(16, 16, 16);
        Random rnd = new Random();
        IMyCockpit helm;
        IMyShipWelder welder;
        IEnumerator<bool> ResetDiag;
        IEnumerator<bool> DiagTask;
        #endregion


        public void Start()
        {
            //Custom Programs Startup Here

            Services.Add(new Action<string, UpdateType>(DiagMain));
        }

        #region Custom Programs


        #region Info
        void _Debug(string argument)
        {
            string debugText = "Cursor: " + Cursor.X.ToString("0.00") + " , " + Cursor.Y.ToString("0.00") +
            "\nVirtual Cursor: " + CursorV.X.ToString("0.00") + " , " + CursorV.Y.ToString("0.00") + "\nStart Menu: " + MenuEnabled +
            "\nResolution: " + ViewPort.Width + "_X" + ViewPort.Height;

            foreach (Monitor _Screen in Monitors)
            {
                debugText = debugText + "\nScreen" + Monitors.FindIndex(A => A == _Screen) + ": " + _Screen.Hitbox.Width + "_X" + _Screen.Hitbox.Height;
            }

            switch (argument)
            {
                case "New": NewWindow(new Vector2(15, 0), "Debug", new Content(debugText)); Windows.Last().Inheritance = _Debug; Tasks.Add(new Event(_Debug, "Run")); break;
                case "Kill": Kill(_Debug); break;
                case "Run": Windows[Windows.FindIndex(a => a.Inheritance == _Debug)].Content.MyContent = debugText; break;
                case "Log": Echo(debugText); break;
            }
        }

        void _HWInfo(string argument)
        {
            string HWText = "Instructions: " + Runtime.CurrentInstructionCount + " / " + Runtime.MaxInstructionCount + "\n" + "Last runtime: " + Math.
             Round(Runtime.LastRunTimeMs, 4);

            switch (argument)
            {
                case "New": NewWindow(new Vector2(15, 0), "Hardware Info", new Content(HWText)); Windows.Last().Inheritance = _HWInfo; Tasks.Add(new Event(_HWInfo, "Run")); break;
                case "Kill": Kill(_Debug); break;
                case "Run": Windows[Windows.FindIndex(a => a.Inheritance == _HWInfo)].Content.MyContent = HWText; break;
                case "Log": Echo(HWText); break;
            }
        }
        #endregion
        #region Ship Diagnostic

        public void DiagMain(string Argument, UpdateType updateSource)
        {
            if ((updateSource & UpdateType.Update100) != 0)
            {
                List<ShipDiagnostic> _Ships = new List<ShipDiagnostic> { new ShipDiagnostic(Me.CubeGrid) };
                foreach (IMyShipConnector Conn in Connectors)
                    if (Conn.OtherConnector != null) _Ships.Add(new ShipDiagnostic(Conn.OtherConnector.CubeGrid));
                if (_Ships.Count != Ships.Count)
                    Ships = _Ships.ToList();
            }
        }

        public void SelfDiagnostic(string argument)
        {
            try
            {
                Window _Window;
                switch (argument)
                {
                    case "New":
                        NewWindow(new Vector2(15, 30), "Ship Diagnostic", new Content("Ship Diagnostic"), (int)ViewPort.Width - 150, (int)ViewPort.Height - 150); Windows.Last().Inheritance = SelfDiagnostic; HighPriorityTasks.Add(new Event(SelfDiagnostic, "Run")); want_reset = true;
                        LowPriorityTasks.Add(new Event(SelfDiagnostic, "Run"));
                        _Window = Windows.Last(); _Window.Content.MyButton = new List<Buttons> {
                            new Buttons("<", new Event(SubAngle, 0.ToString()), new Rectangle(), null ,new MySprite(data:"Arrow", rotation:-1.570796f)),
                            new Buttons(">", new Event(AddAngle, 0.ToString()), new Rectangle(), null ,new MySprite(data:"Arrow", rotation:1.570796f)),
                            new Buttons("+", new Event(SubPlane, 0.ToString()), new Rectangle(), null ,new MySprite(data:"Arrow", rotation:3.1415926536f)),
                            new Buttons("-", new Event(AddPlane, 0.ToString()), new Rectangle(), null ,new MySprite(data:"Arrow", rotation:0)),
                            new Buttons("►", new Event(NextShip, ""), new Rectangle(), null ,new MySprite(data:"Arrow", rotation:1.570796f,color:Color.Red))
                        };
                        break;
                    case "Run":
                        _Window = Windows[Windows.FindIndex(a => a.Inheritance == SelfDiagnostic)];
                        _Window.Content.MyButton[0].Hitbox = new Rectangle(_Window.Content.MyContentBox.Center.X - 64, _Window.Content.MyContentBox.Bottom - 64, 64, 64);
                        _Window.Content.MyButton[1].Hitbox = new Rectangle(_Window.Content.MyContentBox.Center.X + 64, _Window.Content.MyContentBox.Bottom - 64, 64, 64);
                        _Window.Content.MyButton[2].Hitbox = new Rectangle(_Window.Content.MyContentBox.Right - 64, _Window.Content.MyContentBox.Center.Y + 64, 64, 64);
                        _Window.Content.MyButton[3].Hitbox = new Rectangle(_Window.Content.MyContentBox.Right - 64, _Window.Content.MyContentBox.Center.Y - 64, 64, 64);
                        _Window.Content.MyButton[4].Hitbox = new Rectangle(_Window.Content.MyContentBox.Right - 64, _Window.Content.MyContentBox.Center.Y, 64, 64);
                        _Window.Content.MyContent = "Ship Diagnostic\nSpriteBuffer: " + SpriteBuffer.Count + "\n" + Ships[0].DiagStatus;
                        _Window.Content.Sprites = SpriteBuffer;
                        if (ResetDiag == null)
                            ResetDiag = ClearDiag(Ships[0]).GetEnumerator();
                        if (ResetDiag.MoveNext() == false)
                        {
                            if (!drag)
                            {
                                if (DiagTask == null)
                                    DiagTask = RunDiag(Ships[0], Windows[Windows.FindIndex(a => a.Inheritance == SelfDiagnostic)].Content.ContentCanvas).GetEnumerator();
                                if (DiagTask.MoveNext() == false)
                                {
                                    DiagTask.Dispose();
                                    DiagTask = null;
                                }
                            }
                            else if (DiagTask != null) DiagTask.Dispose();
                        }
                        break;
                    case "Kill": ResetDiag.Dispose(); DiagTask.Dispose(); SpriteBuffer.Clear(); SpriteConstructor.Clear(); Kill(SelfDiagnostic); break;
                }
            }
            catch { SelfDiagnostic("Kill"); }
        }
        public void OtherDiagnostic(string argument)
        {
            try
            {
                Window _Window;
                switch (argument)
                {
                    case "New":
                        NewWindow(new Vector2(15, 30), "Ship Diagnostic", new Content("Ship Diagnostic"), (int)ViewPort.Width - 150, (int)ViewPort.Height - 150); Windows.Last().Inheritance = OtherDiagnostic; HighPriorityTasks.Add(new Event(OtherDiagnostic, "Run")); want_reset = true;
                        LowPriorityTasks.Add(new Event(OtherDiagnostic, "Run"));
                        _Window = Windows.Last(); _Window.Content.MyButton = new List<Buttons> {
                            new Buttons("<", new Event(SubAngle, 0.ToString()), new Rectangle(), null ,new MySprite(data:"Arrow", rotation:-1.570796f)),
                            new Buttons(">", new Event(AddAngle, 0.ToString()), new Rectangle(), null ,new MySprite(data:"Arrow", rotation:1.570796f)),
                            new Buttons("+", new Event(SubPlane, 0.ToString()), new Rectangle(), null ,new MySprite(data:"Arrow", rotation:3.1415926536f)),
                            new Buttons("-", new Event(AddPlane, 0.ToString()), new Rectangle(), null ,new MySprite(data:"Arrow", rotation:0)),
                            new Buttons("►", new Event(NextShip, ""), new Rectangle(), null ,new MySprite(data:"Arrow", rotation:1.570796f,color:Color.Red))
                        };
                        break;
                    case "Run":
                        _Window = Windows[Windows.FindIndex(a => a.Inheritance == OtherDiagnostic)];
                        _Window.Content.MyButton[0].Hitbox = new Rectangle(_Window.Content.MyContentBox.Center.X - 64, _Window.Content.MyContentBox.Bottom - 64, 64, 64);
                        _Window.Content.MyButton[1].Hitbox = new Rectangle(_Window.Content.MyContentBox.Center.X + 64, _Window.Content.MyContentBox.Bottom - 64, 64, 64);
                        _Window.Content.MyButton[2].Hitbox = new Rectangle(_Window.Content.MyContentBox.Right - 64, _Window.Content.MyContentBox.Center.Y + 64, 64, 64);
                        _Window.Content.MyButton[3].Hitbox = new Rectangle(_Window.Content.MyContentBox.Right - 64, _Window.Content.MyContentBox.Center.Y - 64, 64, 64);
                        _Window.Content.MyButton[4].Hitbox = new Rectangle(_Window.Content.MyContentBox.Right - 64, _Window.Content.MyContentBox.Center.Y, 64, 64);
                        _Window.Content.MyContent = "Ship Diagnostic\nSpriteBuffer: " + SpriteBuffer.Count + "\n" + Ships[0].DiagStatus;
                        _Window.Content.Sprites = SpriteBuffer;
                        if (ResetDiag == null)
                            ResetDiag = ClearDiag(Ships[0]).GetEnumerator();
                        if (ResetDiag.MoveNext() == false)
                        {
                            if (!drag)
                            {
                                if (DiagTask == null)
                                    DiagTask = RunDiag(Ships[0], Windows[Windows.FindIndex(a => a.Inheritance == OtherDiagnostic)].Content.ContentCanvas).GetEnumerator();
                                if (DiagTask.MoveNext() == false)
                                {
                                    DiagTask.Dispose();
                                    DiagTask = null;
                                }
                            }
                            else if (DiagTask != null) DiagTask.Dispose();
                        }
                        break;
                    case "Kill": ResetDiag.Dispose(); DiagTask.Dispose(); SpriteBuffer.Clear(); SpriteConstructor.Clear(); Kill(SelfDiagnostic); break;
                }
            }
            catch { SelfDiagnostic("Kill"); }
        }
        public void AddAngle(string argument)
        {
            Ships[int.Parse(argument)].Angle++;
            if (Ships[int.Parse(argument)].Angle > 3) Ships[int.Parse(argument)].Angle = 0;
        }
        public void SubAngle(string argument)
        {
            Ships[int.Parse(argument)].Angle--;
            if (Ships[int.Parse(argument)].Angle < 0) Ships[int.Parse(argument)].Angle = 3;
        }
        public void AddPlane(string argument)
        {
            Ships[int.Parse(argument)].Plane++;
            if (Ships[int.Parse(argument)].Plane > 5) Ships[int.Parse(argument)].Plane = 0;
        }
        public void SubPlane(string argument)
        {
            Ships[int.Parse(argument)].Plane--;
            if (Ships[int.Parse(argument)].Plane < 0) Ships[int.Parse(argument)].Plane = 5;
        }
        public void NextShip(string argument)
        {
            DiagTask.Dispose(); ResetDiag.Dispose();
            ShipIndex++;
            //if (ShipIndex < Ships.Count) { ShipIndex = 0; }
        }
        int ShipIndex = 0;

        #region Diagnostic Storage

        List<ShipDiagnostic> Ships = new List<ShipDiagnostic> { };

        public class ShipDiagnostic
        {
            public IMyCubeGrid Grid { get; set; }
            public int Angle { get; set; }
            public int Plane { get; set; }
            public string DiagStatus { get; set; }
            public BlockState[,,] saved_grid { get; set; }
            public Vector3I gridmin { get; set; }
            public Vector3I gridmax { get; set; }
            public List<TerminalBlockState> TerminalBlocks { get; set; }
            public TileState[,] Tiles;
            public ShipDiagnostic(IMyCubeGrid _Grid)
            {
                Grid = _Grid;
                Angle = 0;
                Plane = 0;
                TerminalBlocks = new List<TerminalBlockState>();
            }
        }
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
                Ship.gridmin = Ship.Grid.Min - Vector3I.One;
                Ship.gridmax = Ship.Grid.Max + Vector3I.One;
                Ship.saved_grid = new BlockState[Ship.gridmax.X - Ship.gridmin.X, Ship.gridmax.Y - Ship.gridmin.Y, Ship.gridmax.Z - Ship.gridmin.Z];
                tilesize = Math.Max(Ship.gridmax.X - Ship.gridmin.X, Math.Max(Ship.gridmax.Y - Ship.gridmin.Y, Ship.gridmax.Z - Ship.gridmin.Z)) + 1;
                Ship.Tiles = new TileState[tilesize, tilesize];
            }
            int total_healthy = 0, total = 0;
            for (int _X = Ship.gridmin.X; _X < Ship.gridmax.X; ++_X)
            {
                for (int y = Ship.gridmin.Y; y < Ship.gridmax.Y; ++y)
                {
                    for (int z = Ship.gridmin.Z; z < Ship.gridmax.Z; ++z)
                    {
                        Vector3I pos = new Vector3I(_X, y, z);
                        if (check_damaged)
                        {
                            if (Ship.saved_grid[_X - Ship.gridmin.X, y - Ship.gridmin.Y, z - Ship.gridmin.Z] != BlockState.Empty)
                            {
                                ++total;
                                if (!Ship.Grid.CubeExists(pos))
                                    Ship.saved_grid[_X - Ship.gridmin.X, y - Ship.gridmin.Y, z - Ship.gridmin.Z] = BlockState.Missing;
                                else
                                    ++total_healthy;
                            }
                        }
                        else
                            Ship.saved_grid[_X - Ship.gridmin.X, y - Ship.gridmin.Y, z - Ship.gridmin.Z] = Ship.Grid.CubeExists(pos) ? BlockState.Normal : BlockState.Empty;
                    }
                }
                yield return true;
            }
            DiagHull = total > 0 ? total_healthy * 100 / total : 100;
        }

        IEnumerable<bool> Draw(Rotation Rotate, RectangleF _Canvas, ShipDiagnostic Ship)
        {
            Vector3I _Scale = Ship.gridmax - Ship.gridmin;
            Vector3I _SpriteSize = Rotate(Ship.gridmax - Ship.gridmin, _Scale * 2);
            float _ScaleF = Math.Min(_Canvas.Width, _Canvas.Height) / Math.Max(_SpriteSize.X, _SpriteSize.Y);
            float _OffsetX = (_Canvas.Width - _SpriteSize.X * _ScaleF) * 0.5f + _Canvas.X;
            float _OffsetY = (_Canvas.Height - _SpriteSize.Y * _ScaleF) * 0.5f + _Canvas.Y;
            for (int _X = 0; _X <= _SpriteSize.X; _X++)
                for (int _Y = 0; _Y <= _SpriteSize.Y; _Y++)
                    Ship.Tiles[_X, _Y] = new TileState();
            for (int _X = Ship.gridmin.X; _X < Ship.gridmax.X; ++_X)
            {
                for (int _Y = Ship.gridmin.Y; _Y < Ship.gridmax.Y; ++_Y)
                {
                    for (int _Z = Ship.gridmin.Z; _Z < Ship.gridmax.Z; ++_Z)
                    {
                        BlockState state = Ship.saved_grid[_X - Ship.gridmin.X, _Y - Ship.gridmin.Y, _Z - Ship.gridmin.Z];
                        if (state != BlockState.Empty)
                        {
                            Vector3I pos = new Vector3I(_X, _Y, _Z);
                            Vector3I poscube = Rotate(pos - Ship.gridmin, _Scale);
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
                Vector3I poscube = Rotate(Ship.TerminalBlocks[i].Position - Ship.gridmin, _Scale);
                Vector3I possize = Rotate(Ship.TerminalBlocks[i].Size, Vector3I.Zero);
                if (possize.X < 0) { poscube.X += possize.X + 1; possize.X = -possize.X; }
                if (possize.Y < 0) { poscube.Y += possize.Y + 1; possize.Y = -possize.Y; }
                if (possize.Z < 0) { poscube.Z += possize.Z + 1; possize.Z = -possize.Z; }
                SpriteConstructor.Add(new MySprite(SpriteType.TEXTURE, "SquareHollow", new Vector2((poscube.X + possize.X * 0.5f - 0.5f) * _ScaleF + _OffsetX, (poscube.Y + possize.Y * 0.5f - 0.5f) * _ScaleF + _OffsetY), new Vector2(possize.X * _ScaleF, possize.Y * _ScaleF), Ship.TerminalBlocks[i].State == BlockState.Normal ? Color.Green : Ship.TerminalBlocks[i].State == BlockState.Damaged ? Color.Yellow : Color.Red));
            }
        }

        IEnumerable<bool> ClearDiag(ShipDiagnostic Ship)
        {
            if (want_reset)
            {
                want_reset = false;
                foreach (bool val in CheckGrid(Ship, false))
                    yield return val;
                Ship.TerminalBlocks.Clear();
                for (int i = 0; i < blocks.Count; ++i)
                    if (blocks[i].IsFunctional && blocks[i].CubeGrid == Ship.Grid)
                        Ship.TerminalBlocks.Add(new TerminalBlockState { Position = blocks[i].Min, Size = blocks[i].Max - blocks[i].Min + Vector3I.One, State = BlockState.Normal, Block = blocks[i] });
                HighPriorityTasks.Remove(new Event(SelfDiagnostic, "Run"));
                yield break;
            }
        }
        IEnumerable<bool> RunDiag(ShipDiagnostic Ship, RectangleF Rectangle)
        {
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
            Rotation Rotate = Bit[(Ship.Angle + (Ship.Plane * 4)) % Bit.Length];
            foreach (bool val in Draw(Rotate, Rectangle, Ship))
                yield return val;
            Ship.DiagStatus = string.Format("Hull Integrity: " + DiagHull.ToString("0") + "%\nSystems Integrity: " + DiagSystems.ToString("0") + "%");
            SpriteBuffer = SpriteConstructor.ToList();
            SpriteConstructor.Clear();
            if (want_reset) want_reset = false;
        }
        #endregion
        #endregion
    }
}