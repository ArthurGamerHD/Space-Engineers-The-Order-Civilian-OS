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
        #region WorkInProgress

        class Pong
        {
            public Pong()
            {
            }

            class Paddle
            {
                public Rectangle Hitbox { get; }
                public MySprite Sprite { get; }
                public Paddle(Vector2 Pos, Vector2 Size)
                {
                    Hitbox = new Rectangle((int)Pos.X, (int)Pos.Y, (int)Size.X, (int)Size.Y);
                    Sprite = new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(Hitbox.Center.X, Hitbox.Center.Y), new Vector2(Hitbox.Width, Hitbox.Height));
                }

            }
            class Ball
            {
                public Rectangle Hitbox { get; }
                public MySprite Sprite { get; }
                public Vector2 Pos { get; set; }
                public int Radius { get; }
                public Vector2 Vel { get; set; }
                public Ball(Vector2 Size)
                {
                    Hitbox = new Rectangle((int)Pos.X, (int)Pos.Y, (int)Size.X, (int)Size.Y);
                    Radius = Hitbox.Width / 2;
                    Pos = new Vector2Point(Hitbox.Center).Vector;
                    Sprite = new MySprite(SpriteType.TEXTURE, "Circle", Pos, new Vector2(Hitbox.Width, Hitbox.Height));
                    Vel = new Vector2(6, 2);
                }

            }
            Ball MyBall;
            Paddle Player1, Player2;
            int P1Y, P2Y;
            int P1S, P2S;
            List<MySprite> frame;
            Rectangle GameArea;
            Window Window;
            float scale;
            Vector2I centerPos;
            TextAlignment Ta = TextAlignment.CENTER;
            Color R = Color.Red, G = Color.Green, W = Color.White, D = Color.DarkGray, C, T;
            public void MyPong(Window _Window, byte Action)
            {
                Window = _Window;
                frame = Window.SpritesBuilder;
                switch (Action)
                {
                    case 0:
                        Window.Base = new Rectangle((int)(Window.Screen.TextureSize.X / 2 - (192 * Window.Scale)), (int)(Window.Screen.TextureSize.Y / 2 - (128 * Window.Scale)), (int)(384 * Window.Scale), (int)(256 * Window.Scale));
                        Window.MyFrame = Window.Base;
                        Window.Configs[1] = 2;
                        Window.Configs[4] = $"Pong";
                        C = Window.Meta.CanvasColor; T = Window.Meta.Theme;
                        scale = (Window.MyFrame.Width / 512f > Window.MyFrame.Height / 512f) ? (float)Window.MyFrame.Height / 512f : (float)Window.MyFrame.Width / 512f;
                        centerPos = new Vector2I(Window.MyFrame.Center.X, Window.MyFrame.Center.Y);
                        break;
                    case 1:
                        if (frame.Count == 0)
                        {

                            scale = (Window.MyFrame.Width / 512f > Window.MyFrame.Height / 512f) ? (float)Window.MyFrame.Height / 512f : (float)Window.MyFrame.Width / 512f;
                            centerPos = new Vector2I(Window.MyFrame.Center.X, Window.MyFrame.Center.Y);

                            if (MyBall == null) MyBall = new Ball(new Vector2(10, 10) * scale);

                            Window.SpritesBuilder = Window.Content();
                            frame = Window.SpritesBuilder;
                            frame.Add(new MySprite(SpriteType.TEXT, "Pong - Version 0.0.1", new Vector2(-65f, -235f) * scale + centerPos, null, W, "DEBUG", TextAlignment.LEFT, 1f * scale)); // Title
                            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(0f, -200f) * scale + centerPos, new Vector2(740f, 5f) * scale, W, null, Ta, 0f)); // WhiteLine
                            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(0f, -4f) * scale + centerPos, new Vector2(740f, 380f) * scale, T, null, Ta, 0f)); // InfoDisplayPanel
                            frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", new Vector2(-320f, -200f) * scale + centerPos, new Vector2(100f, 100f) * scale, T, null, Ta, 0f)); // ThemeCircle
                            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(15f, -170f) * scale + centerPos, new Vector2(530f, 30f) * scale, W, null, Ta, 0f)); // Name Frame
                            frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", new Vector2(-320f, -200f) * scale + centerPos, new Vector2(80f, 80f) * scale, W, null, Ta)); // FactionIconBack
                            frame.Add(new MySprite(SpriteType.TEXTURE, Window.Meta.FactionIcon, new Vector2(-320f, -200f) * scale + centerPos, new Vector2(80f, 80f) * scale, Window.Meta.FactionColor, null, Ta)); // TheOrderLogo
                            frame.Add(new MySprite(SpriteType.TEXT, $"{P1S}", new Vector2(70f, -187f) * scale + centerPos, null, new Color(255, 0, 0, 255), "DEBUG", TextAlignment.LEFT, 1f * scale)); // Player2Score
                            frame.Add(new MySprite(SpriteType.TEXT, $"{P2S}", new Vector2(-70f, -187f) * scale + centerPos, null, new Color(255, 0, 0, 255), "DEBUG", TextAlignment.LEFT, 1f * scale)); // Player1Score


                            GameArea = new Rectangle((int)(0f + centerPos.X - (720f * scale / 2)), (int)(10f * scale + centerPos.Y - (320f * scale / 2)), (int)(720f * scale), (int)(320f * scale));
                            frame.Add(new MySprite(SpriteType.CLIP_RECT, "SquareSimple", new Vector2(GameArea.X, GameArea.Y), new Vector2(GameArea.Width, GameArea.Height))); // ClipRect
                            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(GameArea.Center.X, GameArea.Center.Y), new Vector2(GameArea.Width, GameArea.Height), new Color(0, 0, 0, 255), null, Ta, 0f)); // InfoDisplayBlack

                            Player1 = new Paddle(new Vector2(-340f * scale + centerPos.X, GameArea.Center.Y + (P1Y * scale) - (30 * scale)), new Vector2(20f, 60f) * scale);
                            Player2 = new Paddle(new Vector2(320f * scale + centerPos.X, GameArea.Center.Y + (P2Y * scale) - (30 * scale)), new Vector2(20f, 60f) * scale);

                            frame.Add(Player1.Sprite); // sprite1CopyCopy
                            frame.Add(Player2.Sprite); // sprite1CopyCopy

                            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(0f, 10f) * scale + centerPos, new Vector2(2f, 320f) * scale, W, null, Ta, 0f)); // sprite1
                            frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", new Vector2(0f, 0f) * scale + centerPos, new Vector2(16f, 16f) * scale, W, null, Ta, 0f)); // sprite2
                            foreach (MySprite Sprite in Window.ToolBar()) { frame.Add(Sprite); }
                            foreach (MySprite Sprite in Window.Footer()) { frame.Add(Sprite); }
                        }
                        Vector3 Vector = ((IMyShipController)Window.IOs[0]).MoveIndicator;

                        MyBall.Pos += MyBall.Vel;


                        P1Y += 2 * (int)Vector.Z;
                        if (P1Y > 80) P1Y = 80;
                        if (P1Y < -80) P1Y = -80;
                        P2Y += 2 * (int)Vector.X;
                        if (P2Y > 80) P2Y = 80;
                        if (P2Y < -80) P2Y = -80;

                        if (MyBall.Pos.X - MyBall.Radius < GameArea.X   ) { MyBall.Pos = new Vector2Point(GameArea.Center).Vector; P1S++; }
                        if (MyBall.Pos.X + MyBall.Radius > GameArea.Left) { MyBall.Pos = new Vector2Point(GameArea.Center).Vector; P2S++; }

                        if (MyBall.Pos.X - MyBall.Radius <= Player1.Hitbox.X + Player1.Hitbox.Width)
                        {
                            MyBall.Vel *= -1;
                        }
                        else if (MyBall.Pos.X + MyBall.Radius >= Player2.Hitbox.X)
                            MyBall.Vel *= -1;

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
