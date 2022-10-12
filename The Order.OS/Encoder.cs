using Sandbox.ModAPI.Ingame;
using System.Text;
using VRage.Game.GUI.TextPanel;
using VRageMath;


namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        #region Encoder

        public class MyRemoteScreenEncoder
        {
            public bool IsValid
            {get{ return Surface != null;}}
            public Vector2 TextureSize { get { return IsValid ? Surface.TextureSize : Vector2.Zero; } }
            public Vector2 SurfaceSize { get { return IsValid ? Surface.SurfaceSize : Vector2.Zero; } }
            public int SpriteCount { get; private set; } = 0;
            public Vector2 MeasureStringInPixels(StringBuilder text, string font, float scale)
            {
                return IsValid ? Surface.MeasureStringInPixels(text, font, scale) : Vector2.Zero;
            }
            public IMyTextSurface Surface;
            public Vector2I Size;
            public string Sprites;
            public long RemoteScreen { get; }
            public IMyIntergridCommunicationSystem IGC;
            public MyRemoteScreenEncoder(IMyTextSurface surface, long remotescreen, IMyIntergridCommunicationSystem igc, Vector2I? size = null)
            {
                IGC = igc;
                RemoteScreen = remotescreen;
                if (size == null) { Size = (Vector2I)surface.TextureSize; } else Size = size.Value;
                Surface = surface;
            }
            public void Add(MySprite Sprite)
            { Sprites += SpriteEncoder(Sprite); }

            public void Draw(){
                IGC.SendUnicastMessage(RemoteScreen, "1", $"ESS;{Size.X}:{Size.Y};{Sprites}");
            }

            string SpriteEncoder(MySprite Sprite)
            {
                try
                {
                    return $"S,{TypeEncoder(Sprite.Type)},{StringEncoder(Sprite.Data)},{EncodePosAndSize(Sprite)},{EncodeColor(Sprite.Color.GetValueOrDefault())},{Sprite.FontId},{AlignmentEncoder(Sprite.Alignment)},{Sprite.RotationOrScale};";
                }
                catch
                {
                    return "";
                }

            }
            string TypeEncoder(SpriteType _Type)
            {
                if (_Type == SpriteType.TEXT)
                    return "1";
                if (_Type == SpriteType.CLIP_RECT)
                    return "2";
                return "0";
            }
            string StringEncoder(string _String)
            {
                _String = _String.Replace("SquareSimple", "$SS").Replace("Circle", "$CC").Replace(" ", "%20").Replace(";", "%3b").Replace(":", "%3a").Replace(",", "%2c").Replace("+", "%2b").Replace("\n", "%b6").Replace("\\", "%5c");
                return _String;
            }
            string EncodePosAndSize(MySprite Sprite)
            {

                try
                {
                    string Output;
                    if (Sprite.Position.HasValue)
                    {
                        Output = $"{Sprite.Position.Value.X}:{ Sprite.Position.Value.Y}";
                    }
                    else Output = "0:0";
                    Output += ",";
                    if (Sprite.Size.HasValue)
                    {
                        Output += $"{Sprite.Size.Value.X}:{ Sprite.Size.Value.Y}";
                    }
                    else Output += "0:0";
                    return Output;
                }
                catch { return "0:0,0:0"; }
            }
            string EncodeColor(Color Color)
            {
                try
                {
                    var _Color = Color.ToVector4();
                    return $"{_Color.X}:{_Color.Y}:{_Color.Z}:{_Color.W}";
                }
                catch
                {
                    return "1:1:1:1";
                }
            }
            string AlignmentEncoder(TextAlignment _Alignment)
            {
                if (_Alignment == TextAlignment.LEFT)
                    return "1";
                if (_Alignment == TextAlignment.RIGHT)
                    return "2";
                return "0";
            }
        }
        #endregion
    }
}

