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
        #region Ship Diagnostic

        class DiagInterface
        {
            List<IMyTerminalBlock> Blocks;
            IMyCubeGrid Grid;
            public DiagInterface(List<IMyTerminalBlock> _Blocks, IMyCubeGrid _Grid)
            {
                Blocks = _Blocks; Grid = _Grid;
            }

            public List<string> Dictionary = new List<string> { "00-HUL(indiferente esse n aparece)", "01-HPS", "02-BUK", "03-GRV", "04-LSS", "05-EPS", "06-BTP", "07-PDS", "08-ORS", "09-STT", "10-CDP", "11-STG", "12-H2S", "13-GYR", "14-HTS", "15-ITS", "16-ATS", "17-JPD", "18-WEP", "19-SSR", "20-COM", "21-ILT", "22-EXT", "TOGGLE", "OVRRD" };
            List<MySprite> frame;
            int Page = 1, rotation = 0;
            List<bool> SubSystem;
            Window Window;
            float zoom = 2f, rotationF = 0, Z;
            Vector2I P;
            TextAlignment Ta = TextAlignment.CENTER;
            Color R = Color.Red, G = Color.Green, W = Color.White, D = Color.DarkGray, C, T;
            public void MyDiagInterface(Window _Window, byte Action)
            {
                Window = _Window;
                frame = Window.SpritesBuilder;
                SubSystem = (List<bool>)Window.Configs[15];
                switch (Action)
                {
                    case 0:
                        Window.Base = new Rectangle((int)(Window.Screen.TextureSize.X / 2 - (192 * Window.Scale)), (int)(Window.Screen.TextureSize.Y / 2 - (128 * Window.Scale)), (int)(384 * Window.Scale), (int)(256 * Window.Scale));
                        Window.MyFrame = Window.Base;
                        Window.Configs[1] = 2;
                        Window.Configs[4] = $"Ship diagnostic, Displaying: {Grid.CustomName}";
                        Window.Configs[5] = "Ship diagnostic info Display Version 3.0.7                (c) The Order-All rights reserved";

                        C = Window.Meta.CanvasColor; T = Window.Meta.Theme;
                        if (Window.Configs[10] == null)
                        {
                            Window.Configs[10] = new Diagnostic(Blocks, Grid);
                            Window.Configs[14] = 0; //Angle
                            Window.Configs[15] = new List<bool> { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, false, false };
                        }
                        ((Diagnostic)Window.Configs[10]).Update(Window);
                        break;
                    case 1:
                        if (frame.Count == 0)
                        {
                            rotationF = MathHelper.ToRadians(rotation);
                            Z = (Window.MyFrame.Width / 512f > Window.MyFrame.Height / 512f) ? (float)Window.MyFrame.Height / 512f : (float)Window.MyFrame.Width / 512f;
                            P = new Vector2I(Window.MyFrame.Center.X, Window.MyFrame.Center.Y);
                            Window.SpritesBuilder = Window.Content();
                            frame = Window.SpritesBuilder;

                            TD("Ship diagnostic info Display V 3.0.7", 0 - 251f, -237f, W, 1f, Z, P); // Title
                            SS(-50f, -200f, 500f, 5f, W, Z, P); // WhiteLine
                            SS(-40f, -4f, 660f, 380f, T, Z, P); // InfoDisplayPanel
                            frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", new Vector2(-320f, -200f) * Z + P, new Vector2(100f, 100f) * Z, T, null, Ta)); // ThemeCircle
                            frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", new Vector2(-320f, -200f) * Z + P, new Vector2(90f, 90f) * Z, C, null, Ta)); // CircleMask
                            SS(195f, -200f, 10f, 7f, C, Z, P); // MaskLine10
                            SS(145f, -200f, 10f, 7f, C, Z, P); // MaskLine9
                            SS(95f, -200f, 10f, 7f, C, Z, P); // MaskLine8
                            SS(45f, -200f, 10f, 7f, C, Z, P); // MaskLine7
                            SS(-5f, -200f, 10f, 7f, C, Z, P); // MaskLine6
                            SS(-55f, -200f, 10f, 7f, C, Z, P); // MaskLine5
                            SS(-105f, -200f, 10f, 7f, C, Z, P); // MaskLine4
                            SS(-155f, -200f, 10f, 7f, C, Z, P); // MaskLine3
                            SS(-205f, -200f, 10f, 7f, C, Z, P); // MaskLine2
                            SS(-255f, -200f, 10f, 7f, C, Z, P); // MaskLine1
                            NewButton(340f, 170f, BV2);// View-Button
                            NewButton(340f, 135f, BRS);// Rotate-Button
                            NewButton(340f, 105f, BRP);// Rotate+Button
                            NewButton(340f, 70f, BV1); // View+Button
                            NewButton(340f, 20f, BP);  // PrevButton
                            NewButton(340f, -20f, B4); // Act4Button
                            NewButton(340f, -60f, B3); // Act3Button
                            NewButton(340f, -100f, B2);// Act2Button
                            NewButton(340f, -140f, B1);// Act1Button
                            NewButton(340f, -180f, BN);// NextButton
                            frame.Add(new MySprite(SpriteType.TEXTURE, "Arrow", new Vector2(340f, 70f) * Z + P, new Vector2(20f, 20f) * Z, new Color(0, 0, 0, 255), null, Ta)); // View+
                            frame.Add(new MySprite(SpriteType.TEXTURE, "Arrow", new Vector2(340f, 105f) * Z + P, new Vector2(20f, 20f) * Z, new Color(0, 0, 0, 255), null, Ta, 1.5708f)); // Rotate+
                            frame.Add(new MySprite(SpriteType.TEXTURE, "Arrow", new Vector2(340f, 135f) * Z + P, new Vector2(20f, 20f) * Z, new Color(0, 0, 0, 255), null, Ta, -1.5708f)); // Rotate-
                            frame.Add(new MySprite(SpriteType.TEXTURE, "Arrow", new Vector2(340f, 170f) * Z + P, new Vector2(20f, 20f) * Z, new Color(0, 0, 0, 255), null, Ta, 3.1416f)); // View-
                            TM("PREV", 342f, 15f, new Color(0, 0, 0, 255), 0.4f, Z, P); // PreviousButtonN
                            TM(Dictionary[Page], 342f, -145f, new Color(0, 0, 0, 255), 0.4f, Z, P); // Button1N
                            TM(Dictionary[Page + 1], 342f, -105f, new Color(0, 0, 0, 255), 0.4f, Z, P); // Button2N
                            TM(Dictionary[Page + 2], 342f, -65f, new Color(0, 0, 0, 255), 0.4f, Z, P); // Button3N
                            TM(Dictionary[Page + 3], 342f, -25f, new Color(0, 0, 0, 255), 0.4f, Z, P); // Button4N
                            TM("NEXT", 342f, -185f, new Color(0, 0, 0, 255), 0.4f, Z, P); // NextButtonN
                            SS(360f, -28f, 5f, 5f, (SubSystem[Page + 3]) ? G : R, Z, P); // L4
                            SS(360f, -68f, 5f, 5f, (SubSystem[Page + 2]) ? G : R, Z, P); // L3
                            SS(360f, -108f, 5f, 5f, (SubSystem[Page + 1]) ? G : R, Z, P); // L2
                            SS(360f, -148f, 5f, 5f, (SubSystem[Page]) ? G : R, Z, P); // L1
                            frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", new Vector2(-320f, -200f) * Z + P, new Vector2(80f, 80f) * Z, W, null, Ta)); // FactionIconBack
                            frame.Add(new MySprite(SpriteType.TEXTURE, Window.Meta.FactionIcon, new Vector2(-320f, -200f) * Z + P, new Vector2(80f, 80f) * Z, Window.Meta.FactionColor, null, Ta)); // TheOrderLogo
                            var size = new Vector2(640f, 320f) * Z;
                            RectangleF DrawArea = new RectangleF(new Vector2(-40f, 10f) * Z + P - size / 2, size);
                            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", DrawArea.Position + new Vector2(0, DrawArea.Size.Y / 2), DrawArea.Size, new Color(0, 0, 0, 255), null, TextAlignment.LEFT)); // InfoDisplayBlack
                            frame.Add(new MySprite(SpriteType.TEXTURE, "Grid", DrawArea.Position + new Vector2(0, DrawArea.Size.Y / 2), new Vector2(DrawArea.Size.Y, DrawArea.Size.Y), W, null, TextAlignment.LEFT)); // InfoDisplayBlack
                            frame.Add(new MySprite(SpriteType.TEXTURE, "Grid", DrawArea.Position + new Vector2(DrawArea.Size.Y, DrawArea.Size.Y / 2), new Vector2(DrawArea.Size.Y, DrawArea.Size.Y), W, null, TextAlignment.LEFT)); // InfoDisplayBlack
                            frame.Add(new MySprite(SpriteType.CLIP_RECT, "SquareSimple", DrawArea.Position, DrawArea.Size, null, null, TextAlignment.LEFT)); // InfoDisplayBack
                            foreach (MySprite S in ((Diagnostic)Window.Configs[10]).Get(Window))
                            {
                                float sin = (float)Math.Sin(rotationF);
                                float cos = (float)Math.Cos(rotationF);
                                frame.Add(new MySprite(S.Type, S.Data, (S.Type == SpriteType.TEXT) ? S.Position * Z + DrawArea.Center : new Vector2(cos * S.Position.Value.X - sin * S.Position.Value.Y, sin * S.Position.Value.X + cos * S.Position.Value.Y) * (Z * zoom) + DrawArea.Center, (S.Size != null) ? S.Size * (Z * zoom) : null, S.Color, S.FontId, S.Alignment, (S.Type == SpriteType.TEXT) ? S.RotationOrScale * Z : (S.Data == "SquareSimple" || S.Data == "SquareHollow") ? rotationF : S.RotationOrScale));
                            }
                            SmallButton(260f, 120f, BZS);
                            SmallButton(260f, 150f, BZP);

                            if (rotation != 0 && zoom > 1) frame.Add(new MySprite(SpriteType.TEXT, "Due to screens limitations\nrotate a image larger than\ncanvas cause visual glitch\nCheck: Shorturl.at/vCR89", new Vector2(40f, -140f) * Z + P, null, Color.Red, "Monospace", TextAlignment.LEFT, .4f * Z)); // ShipName

                            frame.Add(new MySprite(SpriteType.CLIP_RECT, "SquareSimple", (new Vector2(15f - 260f, -170f - 15f)) * Z + P, new Vector2(520f, 30f) * Z, null, null, TextAlignment.LEFT)); //NameCLIP_RECT
                            SS(15f, -170f, 530f, 30f, W, Z, P); // Name Frame
                            frame.Add(new MySprite(SpriteType.TEXT, Grid.CustomName, new Vector2(-243f, -187f) * Z + P, null, T, "DEBUG", TextAlignment.LEFT, 1f * Z)); // ShipName
                            frame.Add(MySprite.CreateClearClipRect());

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
            public void NewButton(float X, float Y, Action ButtonAction)
            {
                Vector2 ButtonPos = (new Vector2(X, Y) * Z) + P - new Vector2(25, 12.5f) * Z;
                SL(ButtonPos.X, ButtonPos.Y + 12.5f * Z, 50f, 25f, W, Z, P); // PrevButton
                Window.Buttons.Add(new Button((int)ButtonPos.X, (int)ButtonPos.Y, (int)(50f * Z), (int)(25f * Z), ButtonAction));
                frame.Add(new MySprite(SpriteType.TEXTURE, "RightTriangle", new Vector2(X - 20, Y - 7.5f) * Z + P, new Vector2(10f, 10f) * Z, D, null, Ta, 1.5708f));
            }
            public void SmallButton(float X, float Y, Action ButtonAction)
            {
                Vector2 ButtonPos = (new Vector2(X, Y) * Z) + P - new Vector2(12.5f, 12.5f) * Z;
                SL(ButtonPos.X, ButtonPos.Y + 12.5f * Z, 25, 25, W, Z, P); // PrevButton
                Window.Buttons.Add(new Button((int)ButtonPos.X, (int)ButtonPos.Y, (int)(50f * Z), (int)(25f * Z), ButtonAction));
            }

            public void SS(float P1, float P2, float S1, float S2, Color C, float S, Vector2 P)
            { frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(P1, P2) * S + P, new Vector2(S1, S2) * S, C, null, TextAlignment.CENTER)); }
            public void SL(float P1, float P2, float S1, float S2, Color C, float S, Vector2 P)
            { frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(P1, P2), new Vector2(S1, S2) * S, C, null, TextAlignment.LEFT)); }

            public void TD(string text, float P1, float P2, Color C, float S, float s, Vector2 P)
            { frame.Add(new MySprite(SpriteType.TEXT, text, new Vector2(P1, P2) * s + P, null, C, "DEBUG", TextAlignment.LEFT, s * S)); }
            public void TM(string text, float P1, float P2, Color C, float S, float s, Vector2 P)
            { frame.Add(new MySprite(SpriteType.TEXT, text, new Vector2(P1, P2) * s + P, null, C, "Monospace", TextAlignment.CENTER, s * S)); }

            public void CH(float P1, float P2, Color C, float R, float S, Vector2 P)
            {
                frame.Add(new MySprite(SpriteType.TEXTURE, "RightTriangle", new Vector2(P1, P2) * S + P, new Vector2(10f, 10f) * S, C, null, TextAlignment.CENTER, R)); // NextButtonCh
            }

            public void B1() { ((List<bool>)Window.Configs[15])[Page] = !SubSystem[Page]; }
            public void B2() { ((List<bool>)Window.Configs[15])[Page + 1] = !SubSystem[Page + 1]; }
            public void B3() { ((List<bool>)Window.Configs[15])[Page + 2] = !SubSystem[Page + 2]; }
            public void B4() { ((List<bool>)Window.Configs[15])[Page + 3] = !SubSystem[Page + 3]; }
            public void BN() { Page += 4; if (Page > 21) Page = 1; }
            public void BP() { Page -= 4; if (Page < 1) Page = 21; }
            public void BV1()
            {
                int I = (int)Window.Configs[14];
                I += 1; if (I > 5) I = 0;
                Window.Configs[14] = I;
            }
            public void BV2()
            {
                int I = (int)Window.Configs[14];
                I -= 1; if (I < 0) I = 5;
                Window.Configs[14] = I;
            }
            public void BRP() { rotation += 15; if (rotation > 345) rotation = 0; }
            public void BRS() { rotation -= 15; if (rotation < 0) rotation = 345; }
            public void BZP() { zoom += .4f; if (zoom > 10f) zoom = 10f; }
            public void BZS() { zoom -= .4f; if (zoom < .6f) zoom = .6f; }
        }

        public class SpriteListData
        {
            public List<MySprite> Sprites { get; set; }
            public DateTime Time { get; set; }
            public SpriteListData(List<MySprite> _Sprites)
            {
                Sprites = _Sprites;
                Time = DateTime.Now;
            }
        }
        public class Diagnostic
        {
            readonly List<List<SpriteListData>> LoL;
            ShipDiagnostic MyShipDiagnostic;
            List<MySprite> SpriteList = new List<MySprite>();
            List<IMyTerminalBlock> Blocks, Sensors;
            int? Current;
            IEnumerator<bool> Task;
            IEnumerator<bool> Clear;
            public Diagnostic() { }
            public Diagnostic(List<IMyTerminalBlock> _Blocks, IMyCubeGrid Grid)
            {
                MyShipDiagnostic = new ShipDiagnostic(Grid);
                Blocks = _Blocks;
                LoL = new List<List<SpriteListData>>(new List<SpriteListData>[23]);
                for (int i = 0; i < LoL.Count(); ++i)
                {
                    LoL[i] = new List<SpriteListData>(new SpriteListData[6]);
                }
                foreach (IMyTerminalBlock Block in Blocks) { if (Sensors == null) Sensors = new List<IMyTerminalBlock>(); if (Block is IMySensorBlock || Block is IMyTurretControlBlock || Block is IMyLargeTurretBase) Sensors.Add(Block); }
            }
            public List<MySprite> Get(Window Window)
            {
                int I = (int)Window.Configs[14];
                SpriteList = new List<MySprite>();
                if (LoL[0] != null)
                {
                    if (LoL[0][I] != null)
                    {
                        if (LoL[0][I].Sprites != null)
                        {
                            SpriteList = LoL[0][I].Sprites.ToList();
                            for (int i = 1; i < LoL.Count(); ++i)
                            {
                                if (LoL[i][I] != null && ((List<bool>)Window.Configs[15])[i]) foreach (MySprite Sprite in LoL[i][I].Sprites) SpriteList.Add(Sprite);
                            }
                            foreach (MySprite Sprite in MyShipDiagnostic.Sensor(I, Sensors)) SpriteList.Add(Sprite);
                            SpriteList.Add(new MySprite(SpriteType.TEXT, $"Last Update: {new TimeAgo(LoL[0][I].Time).Time}", new Vector2(0, 140f), null, Color.White, "Monospace", TextAlignment.CENTER, .4f));
                        }
                        else
                        {
                            Update(Window);
                        }
                        if (DateTime.Now.Ticks > LoL[0][I].Time.Ticks + 3000000000)
                        {
                            Update(Window);
                        }
                    }
                    else
                    {
                        Update(Window);
                    }
                }
                else
                {
                    Update(Window);
                }
                return SpriteList;
            }
            public void Update(Window Window)
            {
                if (Task == null)
                {
                    Current = (int)Window.Configs[14];
                    MyShipDiagnostic.Stat = 0;
                }
                if (Clear == null)
                    Clear = MyShipDiagnostic.ClearDiag(Blocks).GetEnumerator();
                if (Clear.MoveNext() == false)
                {
                    if (Task == null)
                        Task = MyShipDiagnostic.RunDiag(Current.Value).GetEnumerator();
                    if (Task.MoveNext() == false)
                    {
                        Task.Dispose();
                        Task = null;
                        for (int i = 0; i < LoL.Count(); ++i)
                        {
                            if (MyShipDiagnostic.Sprites[i] != null) { LoL[i][Current.Value] = new SpriteListData(MyShipDiagnostic.Sprites[i].ToList()); }
                        }
                        Current = null;
                    }
                }
                SpriteList.Add(new MySprite(SpriteType.TEXT, $"Loading...\n{MyShipDiagnostic.Stat}/5", new Vector2(0, 0), null, Color.White, "Monospace", TextAlignment.CENTER, 1f)); // Title);

            }
        }

        #region Diagnostic Storage

        public class ShipDiagnostic
        {
            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock> { };
            public IMyCubeGrid Grid { get; set; }
            public BlockState[,,] Saved_Grid { get; set; }
            public Vector3I Grid_Min { get; set; }
            public Vector3I Grid_Max { get; set; }
            public List<TerminalBlockState> TerminalBlocks { get; set; }
            public TileState[,] Tiles;
            public int DiagHull, tilesize, Stat;
            public Window Me { get; set; }
            public int Angle { get; set; }
            public int Plane { get; set; }
            public string DiagStatus { get; set; }
            public List<List<MySprite>> Sprites { get; set; }
            public IEnumerator<bool> ResetDiag { get; set; }
            public IEnumerator<bool> DiagTask { get; set; }
            public ShipDiagnostic(IMyCubeGrid _Grid)
            {
                Grid = _Grid;
                TerminalBlocks = new List<TerminalBlockState>();
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
            static Vector3I X_P(Vector3I pos, Vector3I size) { return new Vector3I(pos.Z, pos.Y, pos.X); }
            static Vector3I Y_P(Vector3I pos, Vector3I size) { return new Vector3I(size.Z - pos.Z, pos.X, pos.Y); }
            static Vector3I Z_P(Vector3I pos, Vector3I size) { return new Vector3I(size.X - pos.X, pos.Y, pos.Z); }
            static Vector3I X_N(Vector3I pos, Vector3I size) { return new Vector3I(pos.Z, size.Y - pos.Y, size.X - pos.X); }
            static Vector3I Y_N(Vector3I pos, Vector3I size) { return new Vector3I(size.Z - pos.Z, size.X - pos.X, size.Y - pos.Y); }
            static Vector3I Z_N(Vector3I pos, Vector3I size) { return new Vector3I(size.X - pos.X, size.Y - pos.Y, size.Z - pos.Z); }
            static Rotation[] Bit = new Rotation[] { X_N, X_P, Y_N, Y_P, Z_N, Z_P, };
            #endregion

            public IEnumerable<bool> ClearDiag(List<IMyTerminalBlock> _Blocks)
            {
                Blocks = _Blocks;
                foreach (bool val in CheckGrid(false))
                    yield return val;
                TerminalBlocks.Clear();
                for (int i = 0; i < Blocks.Count; ++i)
                    if (Blocks[i].IsFunctional && Blocks[i].CubeGrid == Grid)
                        TerminalBlocks.Add(new TerminalBlockState { Position = Blocks[i].Min, Size = Blocks[i].Max - Blocks[i].Min + Vector3I.One, State = BlockState.Normal, Block = Blocks[i] });
                yield break;
            }

            IEnumerable<bool> CheckGrid(bool check_damaged)
            {
                if (check_damaged == false)
                {
                    Grid_Min = Grid.Min - Vector3I.One;
                    Grid_Max = Grid.Max + Vector3I.One;
                    Saved_Grid = new BlockState[Grid_Max.X - Grid_Min.X, Grid_Max.Y - Grid_Min.Y, Grid_Max.Z - Grid_Min.Z];
                    tilesize = Math.Max(Grid_Max.X - Grid_Min.X, Math.Max(Grid_Max.Y - Grid_Min.Y, Grid_Max.Z - Grid_Min.Z)) + 1;
                    Tiles = new TileState[tilesize, tilesize];
                }
                int total_healthy = 0, total = 0;
                for (int _X = Grid_Min.X; _X < Grid_Max.X; ++_X)
                {
                    for (int y = Grid_Min.Y; y < Grid_Max.Y; ++y)
                    {
                        for (int z = Grid_Min.Z; z < Grid_Max.Z; ++z)
                        {
                            Vector3I pos = new Vector3I(_X, y, z);
                            if (check_damaged)
                            {
                                if (Saved_Grid[_X - Grid_Min.X, y - Grid_Min.Y, z - Grid_Min.Z] != BlockState.Empty)
                                {
                                    ++total;
                                    if (!Grid.CubeExists(pos))
                                        Saved_Grid[_X - Grid_Min.X, y - Grid_Min.Y, z - Grid_Min.Z] = BlockState.Missing;
                                    else
                                        ++total_healthy;
                                }
                            }
                            else
                                Saved_Grid[_X - Grid_Min.X, y - Grid_Min.Y, z - Grid_Min.Z] = Grid.CubeExists(pos) ? BlockState.Normal : BlockState.Empty;
                        }
                    }
                    yield return true;
                }
                DiagHull = total > 0 ? total_healthy * 100 / total : 100;
            }
            RectangleF _Canvas = new RectangleF(-320, -160, 640, 320);

            IEnumerable<bool> Draw(Rotation Rotate)
            {
                Vector3I _Scale = Grid_Max - Grid_Min;
                Vector3I _SpriteSize = Rotate(Grid_Max - Grid_Min, _Scale * 2);
                float _ScaleF = Math.Min(_Canvas.Width, _Canvas.Height) / Math.Max(_SpriteSize.X, _SpriteSize.Y);
                float _OffsetX = (_Canvas.Width - _SpriteSize.X * _ScaleF) * 0.5f + _Canvas.X;
                float _OffsetY = (_Canvas.Height - _SpriteSize.Y * _ScaleF) * 0.5f + _Canvas.Y;
                for (int _X = 0; _X <= _SpriteSize.X; _X++)
                    for (int _Y = 0; _Y <= _SpriteSize.Y; _Y++)
                        Tiles[_X, _Y] = new TileState();
                for (int _X = Grid_Min.X; _X < Grid_Max.X; ++_X)
                {
                    for (int _Y = Grid_Min.Y; _Y < Grid_Max.Y; ++_Y)
                    {
                        for (int _Z = Grid_Min.Z; _Z < Grid_Max.Z; ++_Z)
                        {
                            BlockState state = Saved_Grid[_X - Grid_Min.X, _Y - Grid_Min.Y, _Z - Grid_Min.Z];
                            if (state != BlockState.Empty)
                            {
                                Vector3I pos = new Vector3I(_X, _Y, _Z);
                                Vector3I poscube = Rotate(pos - Grid_Min, _Scale);
                                TileState _MyTile = Tiles[poscube.X, poscube.Y];
                                _MyTile.Depth = Math.Max(_MyTile.Depth, poscube.Z);
                                _MyTile.Total++;
                                if (state == BlockState.Normal)
                                    _MyTile.Healthy++;
                                Tiles[poscube.X, poscube.Y] = _MyTile;
                            }
                        }
                    }
                    yield return true;
                }
                Sprites[0] = new List<MySprite>();
                for (int _Y = 0; _Y <= _SpriteSize.Y; _Y++)
                {
                    int _Length = 1;
                    for (int _X = 0; _X <= _SpriteSize.X; _X++)
                    {
                        if (_Length != 1) { _Length--; }
                        else
                        {
                            TileState _MyTile = Tiles[_X, _Y];
                            if (_MyTile.Total == 0)
                                continue;
                            try
                            {
                                bool check = true;
                                while (check)
                                    if (_MyTile.Healthy == Tiles[_X + _Length, _Y].Healthy && _MyTile.Depth == Tiles[_X + _Length, _Y].Depth) { _Length++; }
                                    else
                                    { check = false; }
                            }
                            finally
                            {
                                float depth = ((float)_MyTile.Depth / (float)_SpriteSize.Z);
                                depth = depth * depth * depth * depth + 0.05f;
                                float health = _MyTile.Healthy / (float)_MyTile.Total;
                                if (_MyTile.Healthy < _MyTile.Total)
                                    health *= 0.5f;
                                Sprites[0].Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(_X * _ScaleF + _OffsetX + ((_ScaleF / 2) * (_Length - 1)), _Y * _ScaleF + _OffsetY), new Vector2(_ScaleF * _Length, _ScaleF), new Color(depth, depth * health, depth * health)));
                            }
                        }
                    }
                    yield return true;
                }
                Sprites[4] = new List<MySprite>();
                Sprites[22] = new List<MySprite>();
                Stat++;
                for (int i = 0; i < TerminalBlocks.Count; ++i)
                {
                    var B = TerminalBlocks[i].Block;
                    Vector3I poscube = Rotate(TerminalBlocks[i].Position - Grid_Min, _Scale);
                    Vector3I possize = Rotate(TerminalBlocks[i].Size, Vector3I.Zero);
                    if (possize.X < 0) { poscube.X += possize.X + 1; possize.X = -possize.X; }
                    if (possize.Y < 0) { poscube.Y += possize.Y + 1; possize.Y = -possize.Y; }
                    if (possize.Z < 0) { poscube.Z += possize.Z + 1; possize.Z = -possize.Z; }
                    MySprite Sprite = new MySprite(SpriteType.TEXTURE, "SquareHollow", new Vector2((poscube.X + possize.X * 0.5f - 0.5f) * _ScaleF + _OffsetX, (poscube.Y + possize.Y * 0.5f - 0.5f) * _ScaleF + _OffsetY), new Vector2(possize.X * _ScaleF, possize.Y * _ScaleF), TerminalBlocks[i].State == BlockState.Normal ? Color.Green : TerminalBlocks[i].State == BlockState.Damaged ? Color.Yellow : Color.Red);
                    if (B is IMyBatteryBlock) { if (Sprites[6] == null) Sprites[6] = new List<MySprite>(); Sprites[6].Add(Sprite); }
                    else if (B is IMyPowerProducer) { if (Sprites[5] == null) Sprites[5] = new List<MySprite>(); Sprites[5].Add(Sprite); }
                    else
                        Sprites[22].Add(Sprite);
                    if (i % 60 == 0) yield return true;
                }
                yield return true;
            }
            int terminal_healthy, DiagSystems;
            public IEnumerable<bool> RunDiag(int R)
            {
                Sprites = new List<List<MySprite>>(new List<MySprite>[23]);
                Stat++;
                foreach (bool val in CheckGrid(true))
                    yield return val;
                Stat++;
                int Count = 0;
                for (int i = 0; i < TerminalBlocks.Count; ++i)
                {
                    bool exists = Grid.CubeExists(TerminalBlocks[i].Position);
                    bool working = TerminalBlocks[i].Block.IsWorking;
                    if (exists)
                        ++terminal_healthy;
                    TerminalBlockState s = TerminalBlocks[i];
                    s.State = exists && working ? BlockState.Normal : exists ? BlockState.Damaged : BlockState.Missing;
                    TerminalBlocks[i] = s;
                    Count++;
                    if (Count % 60 == 0)
                    {
                        yield return true;
                    }
                }
                DiagSystems = TerminalBlocks.Count > 0 ? terminal_healthy * 100 / TerminalBlocks.Count : 100;
                Stat++;
                Rotation Rotate = Bit[R];
                foreach (bool val in Draw(Rotate))
                    yield return val;
                Stat++;
            }
            public List<MySprite> Sensor(int R, List<IMyTerminalBlock> SensorList)
            {
                List<MySprite> sprites = new List<MySprite>();
                try
                {
                    Rotation Rotate = Bit[R];
                    Vector3I _Scale = Grid_Max - Grid_Min;
                    Vector3I _SpriteSize = Rotate(Grid_Max - Grid_Min, _Scale * 2);

                    float _ScaleF = Math.Min(_Canvas.Width, _Canvas.Height) / Math.Max(_SpriteSize.X, _SpriteSize.Y);
                    float _OffsetX = (_Canvas.Width - _SpriteSize.X * _ScaleF) * 0.5f + _Canvas.X;
                    float _OffsetY = (_Canvas.Height - _SpriteSize.Y * _ScaleF) * 0.5f + _Canvas.Y;

                    List<MyDetectedEntityInfo> Detected = new List<MyDetectedEntityInfo>();
                    List<MyDetectedEntityInfo> entities = new List<MyDetectedEntityInfo>();
                    foreach (IMyTerminalBlock Sensor in SensorList)
                    {
                        List<MyDetectedEntityInfo> SensorEntity = new List<MyDetectedEntityInfo>();
                        if (Sensor is IMySensorBlock)
                        {
                            ((IMySensorBlock)Sensor).DetectedEntities(SensorEntity);
                            foreach (MyDetectedEntityInfo entity in SensorEntity)
                            { Detected.Add(entity); }
                        }
                        if (Sensor is IMyTurretControlBlock)
                            Detected.Add(((IMyTurretControlBlock)Sensor).GetTargetedEntity());
                        if (Sensor is IMyLargeTurretBase)
                            Detected.Add(((IMyLargeTurretBase)Sensor).GetTargetedEntity());
                    }
                    List<long> IDs = new List<long> { Grid.EntityId };
                    foreach (MyDetectedEntityInfo entity in Detected)
                    {
                        if (!IDs.Contains(entity.EntityId))
                        {
                            if (entity.Position != new Vector3D())
                            {
                                IDs.Add(entity.EntityId);
                                entities.Add(entity);
                            }
                        }
                    };
                    foreach (MyDetectedEntityInfo entity in entities)
                    {
                        var EntityPos = Grid.WorldToGridInteger(entity.Position);
                        Vector3I pos = new Vector3I(EntityPos.X, EntityPos.Y, EntityPos.Z);
                        Vector3I Entity = Rotate(pos - Grid_Min, _Scale);

                        sprites.Add(new MySprite(SpriteType.TEXTURE, (entity.Type == MyDetectedEntityType.CharacterHuman || entity.Type == MyDetectedEntityType.CharacterOther) ? "Circle" : (entity.Type == MyDetectedEntityType.LargeGrid || entity.Type == MyDetectedEntityType.SmallGrid) ? "TriangleHollow" : "Danger",
                            new Vector2(Entity.X * _ScaleF + _OffsetX, Entity.Y * _ScaleF + _OffsetY),
                            (entity.Type == MyDetectedEntityType.CharacterHuman || entity.Type == MyDetectedEntityType.CharacterOther) ? new Vector2(_ScaleF, _ScaleF) : new Vector2(2 * _ScaleF, 2 * _ScaleF),
                            entity.Relationship == MyRelationsBetweenPlayerAndBlock.Friends ? Color.Green : entity.Relationship == MyRelationsBetweenPlayerAndBlock.Owner ? Color.Cyan : entity.Relationship == MyRelationsBetweenPlayerAndBlock.Neutral ? Color.Yellow : entity.Relationship == MyRelationsBetweenPlayerAndBlock.FactionShare || entity.Relationship == MyRelationsBetweenPlayerAndBlock.Friends ? Color.Green : Color.Red));
                    }
                    DiagStatus = string.Format($"Hull Integrity:{DiagHull:0}%\nSystems Integrity: {DiagSystems:0}%\nSensor Count: {SensorList.Count()}\nTracking {entities.Count()} Entities");
                    sprites.Add(new MySprite(SpriteType.TEXT, $"Ship diagnostic V1.6\nSprite Count:{Sprites[0].Count}\n{DiagStatus}", new Vector2(-310f, -150f), null, Color.White, "Monospace", TextAlignment.LEFT, .4f));
                }
                catch
                {
                    sprites.Add(new MySprite(SpriteType.TEXT, $"Ship diagnostic V1.6\nWARNING: Sensor Array Disabled", new Vector2(-310f, -150f), null, Color.Red, "Monospace", TextAlignment.LEFT, .4f));
                }
                return sprites;
            }
        }
        #endregion
    }
}
