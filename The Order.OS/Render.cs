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
        #region Multi-screen Sprite Surface

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
            readonly List<MySprite> _sprites = new List<MySprite>(64);

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

        // Assumes that all text panels are the same size
        public class MultiScreenSpriteSurface : ISpriteSurface
        {
            public bool Initialized { get; private set; } = false;

            float Rotation
            {
                get
                {
                    return _rotationAngle;
                }
                set
                {
                    _rotationAngle = value;
                    _spanVectorAbs = RotateToDisplayOrientation(new Vector2(Cols, Rows), RotationRads);
                    _spanVectorAbs *= Vector2.SignNonZero(_spanVectorAbs);
                }
            }
            float RotationRads
            {
                get
                {
                    return MathHelper.ToRadians(Rotation);
                }
            }
            public Vector2 TextureSize
            {
                get
                {
                    if (!_textureSize.HasValue)
                    {
                        _textureSize = BasePanelSize * _spanVectorAbs;
                    }
                    return _textureSize.Value;
                }
            }
            public Vector2 SurfaceSize
            {
                get { return TextureSize; }
            }
            public int SpriteCount { get; private set; } = 0;
            public Vector2 MeasureStringInPixels(StringBuilder text, string font, float scale)
            {
                return _anchor.MeasureStringInPixels(text, font, scale);
            }
            public Vector2 BasePanelSize
            {
                get { return _anchor.TextureSize; }
            }
            Vector2 BasePanelSizeNoRotation
            {
                get
                {
                    if (!_basePanelSizeNoRotation.HasValue)
                    {
                        Vector2 size = RotateToBaseOrientation(BasePanelSize, RotationRads);
                        size *= Vector2.SignNonZero(size);
                        _basePanelSizeNoRotation = size;
                    }
                    return _basePanelSizeNoRotation.Value;
                }
            }
            Vector2 TextureSizeNoRotation
            {
                get
                {
                    if (!_textureSizeNoRotation.HasValue)
                    {
                        _textureSizeNoRotation = BasePanelSizeNoRotation * new Vector2(Cols, Rows);
                    }
                    return _textureSizeNoRotation.Value;
                }
            }
            public readonly int Rows;
            public readonly int Cols;

            public Color ScriptBackgroundColor { get; set; } = new MetaData().BackgroundColor;
            StringBuilder _stringBuilder = new StringBuilder(128);
            public IMyTextPanel _anchor;
            ITerminalProperty<float> _rotationProp;
            float _rotationAngle = 0f;
            Vector2? _textureSize;
            Vector2? _basePanelSizeNoRotation;
            Vector2? _textureSizeNoRotation;
            Vector2 _spanVectorAbs;

            readonly SingleScreenSpriteSurface[,] _surfaces;
            readonly Vector2[,] _screenOrigins;
            MyIni _ini = new MyIni();
            public MultiScreenSpriteSurface(IMyTextPanel anchor)
            {
                MyIniParseResult result;
                Rows = 1;
                Cols = 1;
                if (_ini.TryParse(anchor.CustomData, out result))
                {
                    if (_ini.ContainsSection("TOS"))
                    {
                        if (_ini.ContainsKey("TOS", "Rows")) { Rows = int.Parse(_ini.Get("TOS", "Rows").ToString()); } else { _ini.Set("TOS", "Rows", "1"); anchor.CustomData = _ini.ToString(); }
                        if (_ini.ContainsKey("TOS", "Cols")) { Cols = int.Parse(_ini.Get("TOS", "Cols").ToString()); } else { _ini.Set("TOS", "Cols", "1"); anchor.CustomData = _ini.ToString(); }
                    }
                    else { anchor.CustomData += "[TOS]\nRows = 1\nCols = 1"; }
                }
                else { anchor.CustomData += "[TOS]\nRows = 1\nCols = 1"; }
                _anchor = anchor;
                _surfaces = new SingleScreenSpriteSurface[Rows, Cols];
                _screenOrigins = new Vector2[Rows, Cols];

                _rotationProp = anchor.GetProperty("Rotate").Cast<float>();
                Rotation = _rotationProp.GetValue(anchor);

                Vector3I anchorPos = anchor.Position;
                Vector3I anchorRight = -Base6Directions.GetIntVector(anchor.Orientation.Left);
                Vector3I anchorDown = -Base6Directions.GetIntVector(anchor.Orientation.Up);
                Vector3I anchorBlockSize = anchor.Max - anchor.Min + Vector3I.One;
                Vector3I stepRight = Math.Abs(Vector3I.Dot(anchorBlockSize, anchorRight)) * anchorRight;
                Vector3I stepDown = Math.Abs(Vector3I.Dot(anchorBlockSize, anchorDown)) * anchorDown;
                IMyCubeGrid grid = anchor.CubeGrid;
                for (int r = 0; r < Rows; ++r)
                {
                    for (int c = 0; c < Cols; ++c)
                    {
                        Vector3I blockPosition = anchorPos + r * stepDown + c * stepRight;
                        var surf = new SingleScreenSpriteSurface(grid, blockPosition);
                        _surfaces[r, c] = surf;
                        if (surf.CubeBlock != null)
                        {
                            _rotationProp.SetValue(surf.CubeBlock, Rotation);
                        }

                        // Calc screen coords
                        Vector2 screenCenter = BasePanelSizeNoRotation * new Vector2(c + 0.5f, r + 0.5f);
                        Vector2 fromCenter = screenCenter - 0.5f * TextureSizeNoRotation;
                        Vector2 fromCenterRotated = RotateToDisplayOrientation(fromCenter, RotationRads);
                        Vector2 screenCenterRotated = fromCenterRotated + 0.5f * TextureSize;
                        _screenOrigins[r, c] = screenCenterRotated - 0.5f * BasePanelSize;
                    }
                }
            }

            Vector2 RotateToDisplayOrientation(Vector2 vec, float angleRad)
            {
                int caseIdx = (int)Math.Round(angleRad / MathHelper.ToRadians(90));
                switch (caseIdx)
                {
                    default:
                    case 0:
                        return vec;
                    case 1: // 90 deg
                        return new Vector2(vec.Y, -vec.X);
                    case 2: // 180 deg
                        return -vec;
                    case 3: // 270 deg
                        return new Vector2(-vec.Y, vec.X);
                }
            }

            Vector2 RotateToBaseOrientation(Vector2 vec, float angleRad)
            {
                int caseIdx = (int)Math.Round(angleRad / MathHelper.ToRadians(90));
                switch (caseIdx)
                {
                    default:
                    case 0:
                        return vec;
                    case 1: // 90 deg
                        return new Vector2(-vec.Y, vec.X);
                    case 2: // 180 deg
                        return -vec;
                    case 3: // 270 deg
                        return new Vector2(vec.Y, -vec.X);
                }
            }

            public void Add(MySprite sprite)
            {
                Vector2 pos = sprite.Position ?? TextureSize * 0.5f;
                Vector2 spriteSize;
                if (sprite.Size != null && sprite.Type == SpriteType.TEXTURE)
                {
                    spriteSize = sprite.Size.Value;
                        switch (sprite.Alignment)
                        {
                            case TextAlignment.LEFT:
                            sprite.Position = (sprite.Position.Value + new Vector2(sprite.Size.Value.X, 0) / 2);
                            sprite.Alignment = TextAlignment.CENTER;
                            break;

                            case TextAlignment.RIGHT:
                            sprite.Position = (sprite.Position.Value - new Vector2(sprite.Size.Value.X,0) / 2);
                            sprite.Alignment = TextAlignment.CENTER;
                            break;
                        }
                        pos = sprite.Position ?? TextureSize * 0.5f;
                }
                else
                {
                    spriteSize = TextureSize;
                }
                float rad = spriteSize.Length() * 0.5f;


                Vector2 fromCenter = pos - (TextureSize * 0.5f);
                Vector2 fromCenterRotated = RotateToBaseOrientation(fromCenter, RotationRads);
                Vector2 basePos = TextureSizeNoRotation * 0.5f + fromCenterRotated;

                var lowerCoords = Vector2I.Floor((basePos - rad) / BasePanelSizeNoRotation);
                var upperCoords = Vector2I.Floor((basePos + rad) / BasePanelSizeNoRotation);

                int lowerCol = Math.Max(0, lowerCoords.X);
                int upperCol = Math.Min(Cols - 1, upperCoords.X);

                int lowerRow = Math.Max(0, lowerCoords.Y);
                int upperRow = Math.Min(Rows - 1, upperCoords.Y);

                if (sprite.Type == SpriteType.CLIP_RECT || sprite.Size == null)
                {
                    for (int r = 0; r < Rows; ++r)
                    {
                        for (int c = 0; c < Cols; ++c)
                        {
                            sprite.Position = pos - _screenOrigins[r, c];
                            _surfaces[r, c].Add(sprite);
                            SpriteCount++;
                        }
                    }
                }
                else
                    for (int r = lowerRow; r <= upperRow; ++r)
                    {
                        for (int c = lowerCol; c <= upperCol; ++c)
                        {
                            sprite.Position = pos - _screenOrigins[r, c];
                            _surfaces[r, c].Add(sprite);
                            SpriteCount++;
                        }
                    }
            }
            public void Draw()
            {
                for (int r = 0; r < Rows; ++r)
                {
                    for (int c = 0; c < Cols; ++c)
                    {
                        _surfaces[r, c].Draw(ScriptBackgroundColor);
                    }
                }
                SpriteCount = 0;
            }
        }
        #endregion
    }
}
