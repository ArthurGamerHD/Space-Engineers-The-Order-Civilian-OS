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
            Debug = false;
            Runtime.UpdateFrequency = UpdateFrequency.Update1 | UpdateFrequency.Update10;
            Reload();
            ViewPort = new RectangleF((Surface.TextureSize - Surface.SurfaceSize) / 2f, Surface.SurfaceSize);
            Cursor = ViewPort.Size / 2;
            offset = (Surface.TextureSize - Surface.SurfaceSize) / 2f;
            taskbar = new Rectangle((int)ViewPort.Position.X, (int)ViewPort.Bottom - 32, (int)ViewPort.Size.X, 32);
            StartButton = new Rectangle(taskbar.Left, taskbar.Top, 32, 32);
            StartMenuButton = new List<Buttons> { new Buttons("Debug", "Debug\\"), new Buttons("Ship Diagnostic", "ShipDiagnostic\\"), new Buttons("Hello", "Hello\\"), new Buttons("Custom", "Custom\\"), new Buttons("HW_Info", "HW_Info\\"), new Buttons("Sleep", "Sleep\\") };

        }
        void Main(string argument, UpdateType updateSource)
        {
            GetCursor((updateSource & UpdateType.Update10) != 0);
            debugText = "Cursor: " + Cursor.X.ToString("0.00") + " , " + Cursor.Y.ToString("0.00") + "\nVirtual Cursor: " + CursorV.X.ToString("0.00") + " , " + CursorV.Y.ToString("0.00") + "\nScreen 1: " + ViewPort + "\nResolution: " + Surface.SurfaceSize + "\nStart Menu: " + MenuEnabled;
            Echo(debugText);
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
                    NewWindow(Cursor, arg[1], new Content(arg[2]));//, answer, arg[4]));
                    //OldWindows.Last().Code = int.Parse(arg[5]);
                }
                catch { }
            }
            if ((updateSource & UpdateType.Update10) != 0)
            {
                frame = Surface.DrawFrame();
                Background();
                for (int i = 0; i < Windows.Count; i++)
                    Render(Windows[i]);
                if (Debug) _Debug();
                if (DiagnosticEnabled) RunDiagnostic("");
                if (MenuEnabled) StartMenu();
                TaskBar();
                DrawMouse();
            }
        }
        void Reload()
        {
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks);
            Surface = Me.GetSurface(0);
            if (blocknumbers != blocks.Count())
            {
                blocknumbers = blocks.Count();
                foreach (IMyTerminalBlock block in blocks)
                {
                    if (block is IMyCockpit & block.CustomName.Contains("Helm")) helm = (IMyCockpit)block;
                    if (block is IMyShipWelder & block.CustomName.Contains("Welder")) welder = (IMyShipWelder)block;
                    if (block is IMyTextPanel & block.CustomName.Contains("LCD-Display")) { Surface = (IMyTextPanel)block; Surface.ContentType = ContentType.SCRIPT; };
                }
            }
        }
        void Background()
        {
            try { 
            string[] Custom = Me.CustomData.Split(',');
            Write(new Rectangle((int)ViewPort.Position.X, (int)ViewPort.Position.Y, (int)ViewPort.Size.X, (int)ViewPort.Size.Y), Custom[1], "Monospace", .1f);
            }
            catch { }
        }
        void TaskBar()
        {
            Fill(taskbar, Theme);
            frame.Add(new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "Textures\\FactionLogo\\Others\\OtherIcon_18.dds",
                Position = new Vector2(StartButton.Left, StartButton.Center.Y),
                Size = new Vector2(32, 32),
                Color = new Vector3(21, 0, 25)
            }); //Task menu Icon
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
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = " " + StartMenuButton[i].Name,
                    RotationOrScale = .7f,
                    Position = new Vector2(StartMenuButton[i].Hitbox.Left, StartMenuButton[i].Hitbox.Top)
                });
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
                    else { foreach (Buttons _Button in StartMenuButton) { if (CursorHitbox.Intersects(_Button.Hitbox)) { near = true; if (Clicked & !clicked) ClickHandle(_Button.Payload); } } }
                    for (int i = Windows.Count - 1; i >= 0; i--)
                    {
                        if (CursorHitbox.Intersects(Windows[i].MyToolbar.Red) | CursorHitbox.Intersects(Windows[i].MyToolbar.Ora) | CursorHitbox.Intersects(Windows[i].MyToolbar.Gre)) { near = true; }
                        if (Clicked & !clicked) { if (CursorHitbox.Intersects(Windows[i].MyToolbar.Red)) { Windows.Remove(Windows[i]); } }
                        else if (!near & clicked & !Clicked)
                        {
                            if (CursorHitbox.Intersects(Windows[i].MyToolbar.Bar))
                            {
                                drag = true; /*current_work.Dispose();*/ drag_to = Cursor; Clicked = true;
                                Windows.Add(Windows[i]); Windows.Remove(Windows[i]);
                                break;
                            }
                        }
                    }
                    if (Clicked & clicked & drag) { Windows.Last().Position = Windows.Last().Position + (Cursor - new Vector2(drag_to.X, drag_to.Y)); WindowBuilder(Windows.Last()); drag_to = new Vector2(Cursor.X, Cursor.Y); }
                    else if (!clicked & drag) { drag = false; }
                    Clicked = clicked;
                }
                catch (Exception Error) { NewWindow(Cursor, "GetCursor Error", new Content(Error.ToString())); };
            }
        }

        void ClickHandle(string _Payload)
        {
            try
            {
                if (_Payload.Contains("Debug\\")) { if (Debug) { Debug = false; Windows.RemoveAt(Windows.FindIndex(a => a.Discriminator == "#debug")); } else Debug = true; }
                if (_Payload.Contains("ShipDiagnostic\\")) { if (DiagnosticEnabled) { DiagnosticEnabled = false; Windows.RemoveAt(Windows.FindIndex(a => a.Discriminator == "#diagnostic")); } else DiagnosticEnabled = true; }

                else if (_Payload.Contains("Net\\")) { }
                else if (_Payload.Contains("Sleep\\")) { frame = Surface.DrawFrame(); frame.Dispose(); Me.Enabled = false; }
                else if (_Payload.Contains("Bye\\")) { Windows.Remove(Windows.Last()); }
            }
            catch (Exception Error) { NewWindow(Cursor, "ClickHandle Error", new Content(Error.ToString())); };
        }
        void DrawMouse()
        {
            frame.Add(new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "Textures\\FactionLogo\\Builders\\BuilderIcon_6.dds",
                Position = new Vector2(CursorV.X - 10, CursorV.Y + 9),
                Size = new Vector2(32, 32),
                RotationOrScale = -.56f,
                Color = (drag ? Color.Orange : clicked ? Color.Red : near ? Color.Yellow : Color.Green),
                Alignment = TextAlignment.LEFT
            }); ;
            frame.Dispose();
        }
        void _Debug()
        {
            try { Windows[Windows.FindIndex(a => a.Discriminator == "#debug")].Content.MyContent = debugText; } catch { NewWindow(new Vector2(15, 0), "Debug", new Content(debugText)); Windows.Last().Discriminator = "#debug"; }
            debugText = "";
        }
        void NewWindow(Vector2 Position, string Title, Content _Content)
        {
            Vector2 Size = Surface.MeasureStringInPixels(new StringBuilder(Title), "InfoMessageBoxCaption", 1.2f);
            Windows.Add(new Window(Position, Title, new Vector2(Size.X, 32)));
            if (_Content.MyContent != "")
            {
                Windows.Last().Content = _Content;
            }
            Render(Windows.Last());

        }
        void WindowBuilder(Window _Window)
        {
            Vector2 Size = Surface.MeasureStringInPixels(new StringBuilder(_Window.Title), "InfoMessageBoxCaption", 1f);
            _Window.MyFrame = new Rectangle((int)_Window.Position.X, (int)_Window.Position.Y, (int)Size.X + 96, 32);
            WindowContentBuilder(_Window);
            WindowToolbarBuilder(_Window);
        }
        void WindowToolbarBuilder(Window _Window)
        {
            Rectangle _ToolBar = new Rectangle(_Window.MyFrame.Left, _Window.MyFrame.Top, _Window.MyFrame.Width, 32);
            Rectangle _Red = new Rectangle((_ToolBar.Right - _ToolBar.Height), _ToolBar.Center.Y - (int)(_ToolBar.Height * .75 / 2), (int)(_ToolBar.Height * .75), (int)(_ToolBar.Height * .75));
            Rectangle _Ora = new Rectangle((int)(_Red.Left - (_ToolBar.Height * .75)), _ToolBar.Center.Y - (int)(_ToolBar.Height * .75 / 2), (int)(_ToolBar.Height * .75), (int)(_ToolBar.Height * .75));
            Rectangle _Gre = new Rectangle((int)(_Ora.Left - (_ToolBar.Height * .75)), _ToolBar.Center.Y - (int)(_ToolBar.Height * .75 / 2), (int)(_ToolBar.Height * .75), (int)(_ToolBar.Height * .75));
            _Window.MyToolbar = new Toolbar(_ToolBar, _Red, _Ora, _Gre);
        }
        void WindowContentBuilder(Window _Window)
        {
            try
            {
                if (_Window.Content.MyContent != null)
                {
                    string _ContenString = _Window.Content.MyContent;
                    Vector2 ContentSize = Surface.MeasureStringInPixels(new StringBuilder(_ContenString), _Window.Content.MyFont, 1.2f);
                    if (ContentSize.X < ViewPort.Size.X) ContentSize.X = ViewPort.Size.X;
                    if (ContentSize.Y < ViewPort.Size.Y) ContentSize.Y = ViewPort.Size.Y;
                    float scale = Math.Min((ViewPort.Size.X - 40) / ContentSize.X, (ViewPort.Size.Y) / ContentSize.Y);
                    ContentSize = Surface.MeasureStringInPixels(new StringBuilder(_ContenString), _Window.Content.MyFont, scale);
                    if (_Window.MinimumSize != null)
                    {
                        _Window.Content.MyContentBox = new Rectangle(_Window.MyFrame.Left, _Window.MyFrame.Top + 32, (int)_Window.MinimumSize.X, (int)_Window.MinimumSize.Y);
                        _Window.MyFrame = new Rectangle(_Window.MyFrame.Left, _Window.MyFrame.Top, (int)(Math.Max(_Window.MyFrame.Width, _Window.MinimumSize.X)), _Window.MyFrame.Height + (int)_Window.MinimumSize.Y);
                    }
                    else
                    {
                        _Window.Content.MyContentBox = new Rectangle(_Window.MyFrame.Left, _Window.MyFrame.Top + 32, _Window.MyFrame.Width, (int)ContentSize.Y);
                        _Window.MyFrame = new Rectangle(_Window.MyFrame.Left, _Window.MyFrame.Top, (int)(Math.Max(_Window.MyFrame.Width, ContentSize.X)), _Window.MyFrame.Height + (int)ContentSize.Y);
                    }
                    _Window.Content.MyScale = (scale);
                }
            }
            catch (Exception Error) { NewWindow(Cursor, "WindowContentBuilder Error", new Content(Error.ToString())); };
        }
        void WindowFooterBuilder(Window _Window) { }
        void Render(Window _Window)
        {
            if (_Window.MyToolbar == null) { WindowBuilder(_Window); };
            Fill(_Window.MyToolbar.Bar, Theme);
            Write(_Window.MyToolbar.Bar, _Window.Title, "InfoMessageBoxCaption", 1f);
            Circle(_Window.MyToolbar.Red, Color.Red);
            Circle(_Window.MyToolbar.Ora, Color.Orange);
            Circle(_Window.MyToolbar.Gre, Color.Green);
            if (_Window.Content != null)
            {
                Fill(_Window.Content.MyContentBox, Color.DarkGray);
                Write(_Window.Content.MyContentBox, _Window.Content.MyContent, _Window.Content.MyFont, _Window.Content.MyScale);
            }
        }

        void Fill(Rectangle _Frame, Color _Color)
        {
            frame.Add(new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareSimple",
                Position = new Vector2(_Frame.Left, _Frame.Top + (_Frame.Height / 2)),
                Size = new Vector2(_Frame.Width, _Frame.Height),
                Color = _Color,
            });
        }
        void Write(Rectangle _Frame, string _Data, string Font, float _Scale)
        {

            frame.Add(new MySprite()
            {
                Type = SpriteType.TEXT,
                Data = _Data,
                RotationOrScale = _Scale,
                Position = new Vector2(_Frame.Left, _Frame.Top),
                FontId = Font
            });
        }
        void Circle(Rectangle _Frame, Color color)
        {
            frame.Add(new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "Circle",
                Position = new Vector2(_Frame.Center.X, _Frame.Center.Y),
                Size = new Vector2(_Frame.Width / 2, _Frame.Height / 2),
                Alignment = TextAlignment.CENTER,
                Color = color
            });
        }
        void NewButton(Rectangle A, string text, float scale, MySpriteDrawFrame frame)
        {
            frame.Add(new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareSimple",
                Position = new Vector2(A.Left - 2, A.Top - 1 + ((A.Bottom - A.Top) / 2)),
                Size = new Vector2(A.Right - A.Left + 4, A.Bottom - A.Top + 4),
            }); //Button box shadow
            frame.Add(new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareSimple",
                Position = new Vector2(A.Left, A.Top + ((A.Bottom - A.Top) / 2)),
                Size = new Vector2(A.Right - A.Left, A.Bottom - A.Top),
                Color = Color.Gray,
            }); //Button box
            frame.Add(new MySprite()
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
            frame.Add(new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "Circle",
                Position = new Vector2(A.Center.X, A.Center.Y),
                Size = new Vector2(Y, Y),
                Alignment = TextAlignment.CENTER,
                Color = color
            });
        }
        public class Window
        {
            public Vector2 Position { get; set; }
            public string Title { get; set; }
            public string Discriminator { get; set; }
            public Vector2 MinimumSize { get; set; }
            public Rectangle MyFrame { get; set; }
            public float Scale { get; set; }
            public Toolbar MyToolbar { get; set; }
            public Buttons Buttons { get; set; }
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
            public Rectangle Red { get; set; }
            public Rectangle Ora { get; set; }
            public Rectangle Gre { get; set; }
            public Toolbar(Rectangle _Bar, Rectangle _Red, Rectangle _Ora, Rectangle _Gre)
            {
                Bar = _Bar;
                Red = _Red;
                Ora = _Ora;
                Gre = _Gre;
            }
        }
        public class Content
        {
            public string MyContent { get; set; }
            public Rectangle MyContentBox { get; set; }
            public float MyScale { get; set; }
            public string MyFont { get; set; }
            public Color Color { get; set; }
            public RectangleF ContentFrame { get; set; }
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
            public string Payload { get; set; }
            public Rectangle Hitbox { get; set; }
            public Buttons(string _Name, string _Payload)
            {
                Name = _Name;
                Payload = _Payload;
            }
        }
        #region Storage

        IMyTextSurface Surface;
        RectangleF ViewPort;
        Rectangle taskbar, StartButton, Menu;
        string debugText;
        Vector2 Cursor, CursorV, offset, drag_to;
        bool clicked, near, Debug, drag, Clicked, MenuEnabled, DiagnosticEnabled;
        int blocknumbers;
        List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock> { };
        List<Window> Windows = new List<Window> { };
        List<Buttons> StartMenuButton = new List<Buttons> { };
        public MySpriteDrawFrame frame;
        Color Theme = new Vector4(.1f, .1f, .1f, .95f);
        Color Faction = new Vector3(21, 0, 25);
        Color Dark = new Vector3(16, 16, 16);
        Random rnd = new Random();
        IMyCockpit helm;
        IMyShipWelder welder;
        #endregion


        #region ShipDiagnostic

        IEnumerator<bool> current_work;
        public void RunDiagnostic(string argument)
        {
            try
            {
                Window _Window = Windows[Windows.FindIndex(a => a.Discriminator == "#diagnostic")];
                if (_Window.Content.ContentFrame == null) { _Window.Content.ContentFrame = new RectangleF(_Window.Content.MyContentBox.X, _Window.Content.MyContentBox.Y, _Window.Content.MyContentBox.Width, _Window.Content.MyContentBox.Height); }
                if (_Window.Content.ContentFrame.X != _Window.Content.MyContentBox.X | _Window.Content.ContentFrame.Y != _Window.Content.MyContentBox.Y) { for (int i = 0; i < SpriteBuffer.Count; i++) SpriteBuffer[i] = new MySprite(SpriteType.TEXTURE, SpriteBuffer[i].Data, (SpriteBuffer[i].Position - (_Window.Content.ContentFrame.Position - new Vector2(_Window.Content.MyContentBox.Location.X, _Window.Content.MyContentBox.Location.Y))), SpriteBuffer[i].Size.Value, SpriteBuffer[i].Color.Value); _Window.Content.ContentFrame = new RectangleF(_Window.Content.MyContentBox.X, _Window.Content.MyContentBox.Y, _Window.Content.MyContentBox.Width, _Window.Content.MyContentBox.Height); }
                foreach (MySprite Sprite in SpriteBuffer) frame.Add(Sprite);
                _Window.Content.MyContent = "Ship Diagnostic\n" + SpriteBuffer.Count;
                if (!drag)
                {
                    if (current_work == null)
                        current_work = RunDiag(_Window.Content.ContentFrame).GetEnumerator();
                    if (current_work.MoveNext() == false)
                    {
                        current_work.Dispose();
                        current_work = null;
                    }
                }else if (current_work != null) current_work.Dispose();
            }
            catch { NewWindow(new Vector2(15, 30), "Ship Diagnostic", new Content("Ship Diagnostic")); Windows.Last().Discriminator = "#diagnostic"; Windows.Last().MinimumSize = new Vector2(400, 400); want_reset = true; }
        }

        public delegate Vector3I RotateFunc(Vector3I pos, Vector3I size);
        static Vector3I Rot1(Vector3I pos, Vector3I size) { return new Vector3I(size.Y - pos.Y, pos.X, pos.Z); }
        static Vector3I Rot2(Vector3I pos, Vector3I size) { return new Vector3I(size.X - pos.X, size.Y - pos.Y, pos.Z); }
        static Vector3I Rot3(Vector3I pos, Vector3I size) { return new Vector3I(pos.Y, size.X - pos.X, pos.Z); }
        static Vector3I XUp(Vector3I pos, Vector3I size) { return new Vector3I(pos.Z, pos.Y, pos.X); }
        static Vector3I YUp(Vector3I pos, Vector3I size) { return new Vector3I(size.Z - pos.Z, pos.X, pos.Y); }
        static Vector3I ZUp(Vector3I pos, Vector3I size) { return new Vector3I(size.X - pos.X, pos.Y, pos.Z); }
        static Vector3I XUp1(Vector3I pos, Vector3I size) { return Rot1(XUp(pos, size), Vector3I.Abs(XUp(size, Vector3I.Zero))); }
        static Vector3I XUp2(Vector3I pos, Vector3I size) { return Rot2(XUp(pos, size), Vector3I.Abs(XUp(size, Vector3I.Zero))); }
        static Vector3I XUp3(Vector3I pos, Vector3I size) { return Rot3(XUp(pos, size), Vector3I.Abs(XUp(size, Vector3I.Zero))); }
        static Vector3I YUp1(Vector3I pos, Vector3I size) { return Rot1(YUp(pos, size), Vector3I.Abs(YUp(size, Vector3I.Zero))); }
        static Vector3I YUp2(Vector3I pos, Vector3I size) { return Rot2(YUp(pos, size), Vector3I.Abs(YUp(size, Vector3I.Zero))); }
        static Vector3I YUp3(Vector3I pos, Vector3I size) { return Rot3(YUp(pos, size), Vector3I.Abs(YUp(size, Vector3I.Zero))); }
        static Vector3I ZUp1(Vector3I pos, Vector3I size) { return Rot1(ZUp(pos, size), Vector3I.Abs(ZUp(size, Vector3I.Zero))); }
        static Vector3I ZUp2(Vector3I pos, Vector3I size) { return Rot2(ZUp(pos, size), Vector3I.Abs(ZUp(size, Vector3I.Zero))); }
        static Vector3I ZUp3(Vector3I pos, Vector3I size) { return Rot3(ZUp(pos, size), Vector3I.Abs(ZUp(size, Vector3I.Zero))); }
        static Vector3I XDown(Vector3I pos, Vector3I size) { return new Vector3I(pos.Z, size.Y - pos.Y, size.X - pos.X); }
        static Vector3I YDown(Vector3I pos, Vector3I size) { return new Vector3I(size.Z - pos.Z, size.X - pos.X, size.Y - pos.Y); }
        static Vector3I ZDown(Vector3I pos, Vector3I size) { return new Vector3I(size.X - pos.X, size.Y - pos.Y, size.Z - pos.Z); }
        static Vector3I XDown1(Vector3I pos, Vector3I size) { return Rot1(XDown(pos, size), Vector3I.Abs(XDown(size, Vector3I.Zero))); }
        static Vector3I XDown2(Vector3I pos, Vector3I size) { return Rot2(XDown(pos, size), Vector3I.Abs(XDown(size, Vector3I.Zero))); }
        static Vector3I XDown3(Vector3I pos, Vector3I size) { return Rot3(XDown(pos, size), Vector3I.Abs(XDown(size, Vector3I.Zero))); }
        static Vector3I YDown1(Vector3I pos, Vector3I size) { return Rot1(YDown(pos, size), Vector3I.Abs(YDown(size, Vector3I.Zero))); }
        static Vector3I YDown2(Vector3I pos, Vector3I size) { return Rot2(YDown(pos, size), Vector3I.Abs(YDown(size, Vector3I.Zero))); }
        static Vector3I YDown3(Vector3I pos, Vector3I size) { return Rot3(YDown(pos, size), Vector3I.Abs(YDown(size, Vector3I.Zero))); }
        static Vector3I ZDown1(Vector3I pos, Vector3I size) { return Rot1(ZDown(pos, size), Vector3I.Abs(ZDown(size, Vector3I.Zero))); }
        static Vector3I ZDown2(Vector3I pos, Vector3I size) { return Rot2(ZDown(pos, size), Vector3I.Abs(ZDown(size, Vector3I.Zero))); }
        static Vector3I ZDown3(Vector3I pos, Vector3I size) { return Rot3(ZDown(pos, size), Vector3I.Abs(ZDown(size, Vector3I.Zero))); }

        int idx = 0;
        static RotateFunc[] funcs = new RotateFunc[] {
            XUp, XUp1, XUp2, XUp3, YUp, YUp1, YUp2, YUp3, ZUp, ZUp1, ZUp2, ZUp3,
            XDown, XDown1, XDown2, XDown3, YDown, YDown1, YDown2, YDown3, ZDown, ZDown1, ZDown2, ZDown3
        };
        enum BlockState
        {
            Empty, Destroyed, Damaged, Normal
        }
        BlockState[,,] saved_grid;
        Vector3I gridmin, gridmax;
        struct TileState
        {
            public int Healthy, Total;
            public int Depth;
        }
        int tilesize;
        TileState[,] tiles;

        int hull_percent = 0;

        IEnumerable<bool> CheckGrid(IMyCubeGrid grid, bool check_damaged)
        {
            if (check_damaged == false)
            {
                gridmin = grid.Min - Vector3I.One;
                gridmax = grid.Max + Vector3I.One;
                saved_grid = new BlockState[gridmax.X - gridmin.X, gridmax.Y - gridmin.Y, gridmax.Z - gridmin.Z];
                tilesize = Math.Max(gridmax.X - gridmin.X, Math.Max(gridmax.Y - gridmin.Y, gridmax.Z - gridmin.Z)) + 1;
                tiles = new TileState[tilesize, tilesize];
            }
            int total_healthy = 0, total = 0;
            for (int x = gridmin.X; x < gridmax.X; ++x)
            {
                for (int y = gridmin.Y; y < gridmax.Y; ++y)
                {
                    for (int z = gridmin.Z; z < gridmax.Z; ++z)
                    {
                        Vector3I pos = new Vector3I(x, y, z);
                        if (check_damaged)
                        {
                            if (saved_grid[x - gridmin.X, y - gridmin.Y, z - gridmin.Z] != BlockState.Empty)
                            {
                                ++total;
                                if (!grid.CubeExists(pos))
                                    saved_grid[x - gridmin.X, y - gridmin.Y, z - gridmin.Z] = BlockState.Destroyed;
                                else
                                    ++total_healthy;
                            }
                        }
                        else
                            saved_grid[x - gridmin.X, y - gridmin.Y, z - gridmin.Z] = grid.CubeExists(pos) ? BlockState.Normal : BlockState.Empty;
                    }
                }
                yield return true;
            }
            hull_percent = total > 0 ? total_healthy * 100 / total : 100;
        }

        IEnumerable<bool> Draw(RotateFunc swizzle, RectangleF screen)
        {
            List<MySprite> Sprites = new List<MySprite> { };
            List<Content> BackgroundRectangle = new List<Content> { };
            Vector3I size = gridmax - gridmin;
            Vector3I sizecube = swizzle(gridmax - gridmin, size * 2);
            float scale = Math.Min(screen.Width, screen.Height) / Math.Max(sizecube.X, sizecube.Y);
            Vector2 blocksize = new Vector2(scale, scale);
            float xoff = (screen.Width - sizecube.X * scale) * 0.5f + screen.X;
            float yoff = (screen.Width - sizecube.Y * scale) * 0.5f + screen.Y;
            for (int x = 0; x <= sizecube.X; x++)
                for (int y = 0; y <= sizecube.Y; y++)
                    tiles[x, y] = new TileState();
            for (int x = gridmin.X; x < gridmax.X; ++x)
            {
                for (int y = gridmin.Y; y < gridmax.Y; ++y)
                {
                    for (int z = gridmin.Z; z < gridmax.Z; ++z)
                    {
                        BlockState state = saved_grid[x - gridmin.X, y - gridmin.Y, z - gridmin.Z];
                        if (state != BlockState.Empty)
                        {
                            Vector3I pos = new Vector3I(x, y, z);
                            Vector3I poscube = swizzle(pos - gridmin, size);
                            TileState tile = tiles[poscube.X, poscube.Y];
                            tile.Depth = Math.Max(tile.Depth, poscube.Z);
                            tile.Total++;
                            if (state == BlockState.Normal)
                                tile.Healthy++;
                            tiles[poscube.X, poscube.Y] = tile;
                        }
                    }
                }
                yield return true;
            }
            for (int x = 0; x <= sizecube.X; x++)
            {
                for (int y = 0; y <= sizecube.Y; y++)
                {
                    TileState tile = tiles[x, y];
                    if (tile.Total == 0)
                        continue;
                    float depth = ((float)tile.Depth / (float)sizecube.Z);
                    depth = depth * depth * depth * depth + 0.05f;
                    float health = tile.Healthy / (float)tile.Total;
                    if (tile.Healthy < tile.Total)
                        health *= 0.5f;
                    var MyHitbox = new RectangleF(x * scale + xoff, y * scale + yoff, scale, scale);
                    var MyColor = new Color(depth, depth * health, depth * health);
                    try { BackgroundRectangle.Find(A => A.ContentFrame == MyHitbox).Color = MyColor; }
                    catch { BackgroundRectangle.Add(new Content("")); BackgroundRectangle.Last().ContentFrame = MyHitbox; BackgroundRectangle.Last().Color = MyColor; }
                }
            }
            foreach (Content cube in BackgroundRectangle)
            {
                Sprites.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", cube.ContentFrame.Position, cube.ContentFrame.Size, cube.Color));
            }

            for (int i = 0; i < blockstates.Count; ++i)
            {
                Vector3I poscube = swizzle(blockstates[i].Position - gridmin, size);
                Vector3I possize = swizzle(blockstates[i].Size, Vector3I.Zero);
                if (possize.X < 0) { poscube.X += possize.X + 1; possize.X = -possize.X; }
                if (possize.Y < 0) { poscube.Y += possize.Y + 1; possize.Y = -possize.Y; }
                if (possize.Z < 0) { poscube.Z += possize.Z + 1; possize.Z = -possize.Z; }
                Sprites.Add(new MySprite(SpriteType.TEXTURE, "SquareHollow", new Vector2((poscube.X + possize.X * 0.5f - 0.5f) * scale + xoff, (poscube.Y + possize.Y * 0.5f - 0.5f) * scale + yoff), new Vector2(possize.X * scale, possize.Y * scale), blockstates[i].State == BlockState.Normal ? Color.Green : blockstates[i].State == BlockState.Damaged ? Color.Yellow : Color.Red));
            }
            SpriteBuffer.Clear();
            foreach (MySprite Sprite in Sprites) SpriteBuffer.Add(Sprite);
            Sprites.Clear();
        }

        struct TerminalBlockState
        {
            public Vector3I Position, Size;
            public BlockState State;
            public IMyTerminalBlock Block;
        }
        List<TerminalBlockState> blockstates = new List<TerminalBlockState>();
        List<RectangleF> rects = new List<RectangleF>();
        List<IMyTerminalBlock> lcds = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> healthbars = new List<IMyTerminalBlock>();
        string health_string = "";
        bool want_reset = true;

        IEnumerable<bool> RunDiag(RectangleF WindowContent)
        {
            IMyCubeGrid grid = Me.CubeGrid;
            if (want_reset)
            {
                want_reset = false;
                foreach (bool val in CheckGrid(grid, false))
                    yield return val;
                List<IMyTerminalBlock> full_blocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocks(full_blocks);
                blockstates.Clear();
                for (int i = 0; i < full_blocks.Count; ++i)
                    if (full_blocks[i].IsFunctional && full_blocks[i].CubeGrid == grid)
                        blockstates.Add(new TerminalBlockState { Position = full_blocks[i].Min, Size = full_blocks[i].Max - full_blocks[i].Min + Vector3I.One, State = BlockState.Normal, Block = full_blocks[i] });
                yield break;
            }
            //Update grid
            {
                idx = (idx + 1) % funcs.Length;
                foreach (bool val in CheckGrid(grid, true))
                    yield return val;
                int terminal_healthy = 0;
                for (int i = 0; i < blockstates.Count; ++i)
                {
                    bool exists = grid.CubeExists(blockstates[i].Position);
                    bool working = blockstates[i].Block.IsWorking;
                    if (exists)
                        ++terminal_healthy;
                    TerminalBlockState s = blockstates[i];
                    s.State = exists && working ? BlockState.Normal : exists ? BlockState.Damaged : BlockState.Destroyed;
                    blockstates[i] = s;
                }
                int terminal_percent = blockstates.Count > 0 ? terminal_healthy * 100 / blockstates.Count : 100;
                health_string = string.Format("Hull " + hull_percent.ToString("000") + "% Systems " + terminal_percent.ToString("000") + "%");
            }
            //Update healthbars
            for (int i = 0; i < healthbars.Count; ++i)
            {
                IMyTextSurface surf = (IMyTextSurface)healthbars[i];
                surf.ContentType = ContentType.TEXT_AND_IMAGE;
                surf.WriteText(health_string);
            }
            //Draw LCD screen
            {
                RotateFunc swizzle = funcs[idx];

                string[] bits = "ShipLayout 7".Split(' ');
                if (bits.Length >= 2)
                {
                    int mode;
                    int.TryParse(bits[1], out mode);
                    swizzle = funcs[mode % funcs.Length];
                }
                foreach (bool val in Draw(swizzle, WindowContent))
                    yield return val;
            }
        }
        List<MySprite> SpriteBuffer = new List<MySprite> { };


        #endregion
    }
}
