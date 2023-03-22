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
        public interface ISpriteSurface
        {
            Vector2 TextureSize { get; }
            Vector2 SurfaceSize { get; }
            Color ScriptBackgroundColor { get; set; }
            int SpriteCount { get; }
            void Add(MySprite sprite);
            void Draw();
            Vector2 MeasureStringInPixels(StringBuilder text, string font, float scale);
        }
    }
}
