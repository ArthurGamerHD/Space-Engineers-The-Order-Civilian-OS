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
        public class SingleScreenSpriteSurface : ISpriteSurface
        {
            public bool IsValid
            {
                get
                {
                    return Surface != null;
                }
            }

            public Vector2 TextureSize { get { return IsValid ? Surface.TextureSize : Vector2.Zero; } }
            public Vector2 SurfaceSize { get { return IsValid ? Surface.SurfaceSize : Vector2.Zero; } }
            public Color ScriptBackgroundColor
            {
                get { return IsValid ? Surface.ScriptBackgroundColor : Color.Black; }
                set { if (IsValid) { Surface.ScriptBackgroundColor = value; } }
            }
            public int SpriteCount { get; private set; } = 0;
            public Vector2 MeasureStringInPixels(StringBuilder text, string font, float scale)
            {
                return IsValid ? Surface.MeasureStringInPixels(text, font, scale) : Vector2.Zero;
            }

            public readonly IMyCubeBlock CubeBlock;
            public readonly IMyTextSurface Surface;
            public MySpriteDrawFrame? Frame = null;

            public void Add(MySprite sprite)
            {
                if (!IsValid)
                {
                    return;
                }
                if (Frame == null)
                {
                    Frame = Surface.DrawFrame();
                }
                Frame.Value.Add(sprite);
                SpriteCount++;
            }

            public void Draw()
            {
                Draw(Surface.ScriptBackgroundColor);
                SpriteCount = 0;
            }

            public void Draw(Color scriptBackgroundColor)
            {
                if (MetaData.VERYDANGEROUS60FPS) 
                {
                    Surface.ContentType = ContentType.TEXT_AND_IMAGE;
                    Surface.ContentType = ContentType.SCRIPT;
                }

                if (!IsValid)
                {
                    return;
                }
                Surface.ContentType = ContentType.SCRIPT;
                Surface.Script = "";
                Surface.ScriptBackgroundColor = scriptBackgroundColor;
                if (Frame == null)
                {
                    Surface.DrawFrame().Dispose();
                }
                else
                {
                    Frame.Value.Dispose();
                    Frame = null;
                }
            }

            public SingleScreenSpriteSurface(IMyTextSurface surf)
            {
                Surface = surf;
            }

            public SingleScreenSpriteSurface(IMyCubeGrid grid, Vector3I position)
            {
                var slim = grid.GetCubeBlock(position);
                if (slim != null && slim.FatBlock != null)
                {
                    CubeBlock = slim.FatBlock;
                    var surf = CubeBlock as IMyTextSurface;
                    if (surf != null)
                    {
                        Surface = surf;
                    }
                }
            }
        }
    }
}
