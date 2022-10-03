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
            MetaData MyMetadata = new MetaData();
            public List<MySprite> Sprites = new List<MySprite>();
            public MyDecoder(MyIni _ini)
            {
                List<string> DisplayList = new List<string> { };
                _ini.GetSections(DisplayList);
                foreach (string Line in DisplayList)
                    if (Line.StartsWith("Sprite Builder Display Script - Text Surface Config"))
                    {
                        float Scale = float.Parse(_ini.Get(Line, "Sprite scale").ToString());
                        string[] SpritesList = _ini.Get(Line, "Sprite list").ToString().Split('\n');

                        foreach (string Sprite in SpritesList)
                        {

                            string Prefix = null;
                            string Data = null;
                            string Position = null;
                            string Size = null;
                            string Color = null;
                            string Font = null;
                            string Rotation = null;

                            if (_ini.ContainsSection($"S:{Sprite}")) { Prefix = "S"; Data = "D"; Position = "P"; Size = "S"; Color = "C"; Rotation = "R"; }
                            else if (_ini.ContainsSection($"T:{Sprite}")) { Prefix = "T"; Data = "D"; Position = "P"; Font = "F"; Color = "C"; Rotation = "S"; }
                            else if (_ini.ContainsSection($"Sprite:{Sprite}")) { Prefix = "Sprite"; Data = "Type"; Position = "Position"; Size = "Size"; Color = "Color"; Rotation = "Rotation"; }
                            else if (_ini.ContainsSection($"Text:{Sprite}")) { Prefix = "Text"; Data = "Text"; Position = "Position"; Font = "Font"; Color = "Color"; Rotation = "Scale"; }

                            if (Prefix != null)
                            {
                                Sprites.Add(new MySprite(
                                            (Prefix == "T" | Prefix == "Text") ? SpriteType.TEXT : SpriteType.TEXTURE,
                                            _ini.Get($"{Prefix}:{Sprite}", Data).ToString(),
                                            ParsePosAndSize(_ini.Get($"{Prefix}:{Sprite}", Position).ToString()),
                                            (Prefix == "T" | Prefix == "Text") ? new Vector2() : ParsePosAndSize(_ini.Get($"{Prefix}:{Sprite}", Size).ToString()),
                                            ParseColor(_ini.Get($"{Prefix}:{Sprite}", Color).ToString()),
                                            (Prefix == "T" | Prefix == "Text") ? _ini.Get($"{Prefix}:{Sprite}", Font).ToString() : null,
                                            (Prefix == "T" | Prefix == "Text") ? TextAlignment.LEFT : TextAlignment.CENTER,
                                            float.Parse(_ini.Get($"{Prefix}:{Sprite}", Rotation).ToString())
                                            ));
                            };
                        }
                    }
            }
            public Vector2 ParsePosAndSize(string Vector)
            {
                try
                {
                    string VectorPreParse = Vector.Replace("{X:", "").Replace(" Y:", ",").Replace("}", "");
                    string[] Values = VectorPreParse.Split(',');
                    return new Vector2(float.Parse(Values[0]), float.Parse(Values[1]));
                }
                catch { return new Vector2 { }; }
            }
            public Color ParseColor(string Vector)
            {
                try
                {
                    if (Vector.Contains("Theme")) return MyMetadata.Theme;
                    else if (Vector.Contains("Background")) return MyMetadata.CanvasColor;
                    else
                    {
                        string VectorPreParse = Vector.Replace(" ", "");
                        string[] Values = VectorPreParse.Split(',');
                        return new Vector4(float.Parse(Values[0]) / 255f, float.Parse(Values[1]) / 255f, float.Parse(Values[2]) / 255f, float.Parse(Values[3]) / 255f);
                    }
                }
                catch
                {
                    return new Color();
                }
            }
            #endregion}
        }
    }
}
