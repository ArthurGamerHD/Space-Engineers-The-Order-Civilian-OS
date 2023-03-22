using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRageMath;


namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        class BSoD
        {
            float scale = 1;
            Vector2 centerPos = new Vector2();
            MultiScreenSpriteSurface frame;
            public BSoD(Workstation Workstation, Exception Error)
            {
                var UWide = Workstation.Screen.TextureSize.X >= 2 * Workstation.Screen.TextureSize.Y;
                var Wide = Workstation.Screen.TextureSize.X > Workstation.Screen.TextureSize.Y;
                scale = (UWide ? Workstation.Scale*2 : Workstation.Scale);
                var W = Color.White;
                var B = Color.Blue;
                frame = Workstation.Screen;
                S(0f, 0f, Workstation.Screen.TextureSize.X, Workstation.Screen.TextureSize.Y, B);
                var text = $"Your Station ran into a problem{(Wide ? " " : "\n")}and needs to Restart.\nRun the Programable Block with\nthis argument: RESTART [{Workstation.Controller.CustomName.Split('[')[1]}";
                frame.Add(new MySprite(SpriteType.TEXT, text, new Vector2(5, 75)* scale, null,Color.White, "DEBUG", TextAlignment.LEFT, 1f*scale));
                text = $"For more information about this issue, visit \nhttps://docs.microsoft.com/dotnet/csharp/language-refere{(Wide ? "" : "\n")}nce/language-specification/exceptions \nOr Read this QR code";
                frame.Add(new MySprite(SpriteType.TEXT, text, new Vector2(120, 270)*scale, null,Color.White, "DEBUG", TextAlignment.LEFT, .5f* scale));
                text = $"If you call a support person,{(Wide ? " " : "\n")}give them this info:\nException code:\n {Error}";
                frame.Add(new MySprite(SpriteType.TEXT, text, new Vector2(120, Wide ? 320 : 330) * scale, null,Color.White, "DEBUG", TextAlignment.LEFT, .5f * scale));
                var QR = new RectangleF(10*scale, 270 * scale, 100 * scale, 100 * scale);
                centerPos = QR.Center;
                scale /= 5;
                S(0f, 0f, 52f, 52f, W); S(0f, 0f, 50f, 50f, B); S(18f, -18f, 10f, 10f, W); S(-18f, -18f, 10f, 10f, W); S(-18f, 18f, 10f, 10f, W); S(18f, -18f, 6f, 6f, B); S(-18f, -18f, 6f, 6f, B);
                S(-18f, 18f, 6f, 6f, B); S(17f, -10f, 16f, 2f, W); S(-17f, -10f, 16f, 2f, W); S(-17f, 10f, 16f, 2f, W); S(10f, -17f, 2f, 16f, W); S(-10f, -17f, 2f, 16f, W); S(-4f, 14f, 2f, 2f, W);
                S(-10f, 17f, 2f, 16f, W); S(-6f, 17f, 2f, 4f, W); S(-3f, 21f, 4f, 8f, W); S(7f, 20f, 4f, 2f, W); S(1f, -9f, 4f, 4f, W); S(1f, -3f, 4f, 4f, W); S(22f, -8f, 2f, 2f, W); S(-8f, 22f, 2f, 2f, W);
                S(24f, -7f, 2f, 4f, W); S(24f, 10f, 2f, 2f, W); S(-2f, 5f, 2f, 8f, W); S(9f, -2f, 4f, 2f, W); S(14f, 1f, 2f, 4f, W); S(24f, -1f, 2f, 4f, W); S(18f, 0f, 2f, 2f, W); S(-8f, 12f, 2f, 2f, W);
                S(19f, -4f, 4f, 6f, W); S(16f, -6f, 2f, 2f, W); S(12f, -6f, 2f, 2f, W); S(10f, -7f, 2f, 4f, W); S(6f, -11f, 2f, 4f, W); S(6f, -20f, 2f, 2f, W); S(8f, -18f, 2f, 2f, W); S(0f, 20f, 2f, 2f, W);
                S(4f, -16f, 6f, 2f, W); S(4f, -14f, 2f, 2f, W); S(2f, -12f, 2f, 2f, W); S(-2f, -12f, 2f, 2f, W); S(0f, -14f, 2f, 2f, W); S(4f, -4f, 2f, 2f, W); S(2f, -6f, 2f, 2f, W); S(2f, 24f, 2f, 2f, W);
                S(10f, 4f, 2f, 2f, W); S(6f, 2f, 6f, 2f, W); S(3f, 0f, 8f, 2f, W); S(5f, 6f, 4f, 2f, W); S(23f, 2f, 4f, 2f, W); S(5f, 22f, 4f, 2f, W); S(4f, 12f, 6f, 2f, W); S(18f, 4f, 6f, 6f, W);
                S(12f, 12f, 6f, 6f, W); S(18f, 4f, 2f, 2f, B); S(12f, 12f, 2f, 2f, B); S(2f, 16f, 6f, 2f, W); S(20f, 22f, 6f, 2f, W); S(14f, 24f, 6f, 2f, W); S(-20f, 1f, 2f, 8f, W); S(-24f, -3f, 2f, 8f, W);
                S(-16f, 4f, 2f, 6f, W); S(-8f, 5f, 2f, 4f, W); S(-12f, 6f, 2f, 2f, W); S(-10f, 8f, 2f, 2f, W); S(-14f, 8f, 2f, 2f, W); S(-18f, 7f, 2f, 4f, W); S(-22f, 6f, 2f, 6f, W); S(18f, 18f, 2f, 2f, W);
                S(14f, 18f, 2f, 2f, W); S(14f, 22f, 2f, 2f, W); S(16f, 20f, 2f, 2f, W); S(12f, 20f, 2f, 2f, W); S(8f, 18f, 2f, 2f, W); S(8f, 24f, 2f, 2f, W); S(-12f, -6f, 2f, 2f, W); S(19f, 11f, 4f, 4f, W);
                S(23f, 8f, 4f, 2f, W); S(21f, 14f, 4f, 2f, W); S(-17f, -6f, 4f, 2f, W); S(-9f, -8f, 4f, 2f, W); S(1f, 14f, 4f, 2f, W); S(-15f, 2f, 8f, 2f, W); S(-14f, -2f, 6f, 2f, W); S(-4f, -23f, 2f, 4f, W);
                S(-8f, -23f, 2f, 4f, W); S(-6f, -15f, 2f, 8f, W); S(-6f, -4f, 2f, 6f, W); S(-8f, -4f, 2f, 2f, W); S(-2f, -5f, 2f, 4f, W); S(-4f, -9f, 2f, 4f, W); S(0f, 4f, 2f, 2f, W); S(2f, 8f, 2f, 2f, W);
                S(-6f, 8f, 2f, 2f, W); S(-8f, 0f, 2f, 2f, W); S(0f, -19f, 2f, 4f, W); S(0f, 10f, 14f, 2f, W); S(3f, -21f, 4f, 4f, W); S(-3f, -17f, 4f, 4f, W); S(-2f, -22f, 2f, 2f, W); S(-10f, 4f, 2f, 2f, W);
                S(-14f, 4f, 2f, 2f, W); S(-14f, -4f, 2f, 2f, W); S(-18f, -4f, 2f, 2f, W); S(-22f, -2f, 2f, 2f, W); S(6f, 14f, 2f, 2f, W); S(22f, 20f, 2f, 2f, W); S(-22f, -8f, 2f, 2f, W);
                frame.Draw();
            }
            void S(float P1, float P2, float S1, float S2, Color C)
            {
                frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(10 * P1, 10 * P2) * scale + centerPos, new Vector2(10 * S1, 10 * S2) * scale, C, null, TextAlignment.CENTER));
            }
        }
    }
}
