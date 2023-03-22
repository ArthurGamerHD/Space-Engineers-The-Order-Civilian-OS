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
        #region Decoder

        public class MyDecoder
        {
            public long Origin { get; }
            public string Tag { get; }
            public Rectangle Frame = new Rectangle(0, 0, 512, 512);
            public List<MySprite> Sprites = new List<MySprite>();
            public MyDecoder(long origin, string tag)
            {
                Origin = origin;
                Tag = tag;
            }
            public void Decode(string InputString) 
            {
                string[] Contents = InputString.Split(';');
                if (Contents[0] == "ESS")
                    SEWDecoder(Contents);
            }
            public void SEWDecoder(string[] _Contents)
            {
                try
                {
                    Sprites = new List<MySprite>();

                    string[] Size = _Contents[1].Split(':');
                    //Frame = new Rectangle(0, 0, int.Parse(Size[0]), int.Parse(Size[1]));
                    foreach (string _Content in _Contents)
                    {
                        if (_Content.StartsWith("S")) { Sprites.Add(SpriteDecoder(_Content)); }
                    }
                }
                catch { }
            }
            public MySprite SpriteDecoder(string _Encoded)
            {
                try
                {
                    string[] _Component = _Encoded.Split(',');
                    SpriteType type = TypeDecoder(_Component[1]);
                    string data = StringDecoder(_Component[2]);
                    Vector2 position = ParsePosAndSize(_Component[3]);
                    Vector2 size = ParsePosAndSize(_Component[4]);
                    Color color = ParseColor(_Component[5]);
                    string fontId = _Component[6];
                    TextAlignment alignment = AlignmentDecoder(_Component[7]);
                    float rotation = FloatDecoder(_Component[8]);
                    return new MySprite(type, data, position, size, color, fontId, alignment, rotation);
                }
                catch
                {
                    return new MySprite();
                }

            }
            public SpriteType TypeDecoder(string _Type)
            {
                switch (_Type)
                {
                    case "1": return SpriteType.TEXT;
                    case "2": return SpriteType.CLIP_RECT;
                    default: return SpriteType.TEXTURE;
                }
            }
            public string StringDecoder(string _String)
            {
                _String = _String.Replace("%20", " ").Replace("$SS", "SquareSimple").Replace("$CC", "Circle").Replace("%3b", ";").Replace("%3a", ":").Replace("%2c", ",").Replace("%2b", "+").Replace("%b6", "\n").Replace("%5c", "\\");
                return _String;
            }
            public Vector2 ParsePosAndSize(string Vector)
            {
                try
                {
                    string[] Pos = Vector.Split(':');
                    return new Vector2(float.Parse(Pos[0]), float.Parse(Pos[1]));
                }
                catch { return new Vector2 { }; }
            }
            public Color ParseColor(string Color)
            {
                try
                {
                    string[] Pos = Color.Split(':'); return new Color(float.Parse(Pos[0]), float.Parse(Pos[1]), float.Parse(Pos[2]), float.Parse(Pos[3]));

                }
                catch
                {
                    return new Color();
                }
            }
            public TextAlignment AlignmentDecoder(string _Type)
            {
                switch (_Type)
                {
                    case "1": return TextAlignment.LEFT;
                    case "2": return TextAlignment.RIGHT;
                    default: return TextAlignment.CENTER;
                }
            }
            public float FloatDecoder(string _FloatS)
            {
                float Float;
                if (float.TryParse(_FloatS, out Float)) return Float; else return 0;
            }
        }
        #endregion
    }
}

