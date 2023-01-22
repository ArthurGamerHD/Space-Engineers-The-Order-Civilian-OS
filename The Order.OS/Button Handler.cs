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

        public class Style
        {
            private static Color _fallbackHover = new Vector4(255, 255, 0, .5f);
            private static Color _fallbackClick = new Vector4(0, 0, 0, .5f);
            private static Color _fallbackClicked = new Vector4(100, 100, 100, .5f);

            public Color Hover { get { return (_userHover == null) ? _fallbackHover : _userHover.Value; } }
            public Color Click { get { return (_userClick == null) ? _fallbackClick : _userClick.Value; } }
            public Color Clicked { get { return (_userClicked == null) ? _fallbackClicked : _userClicked.Value; } }

            private Color? _userHover;
            private Color? _userClick;
            private Color? _userClicked;

            public Style(Color? hover = null, Color? click = null, Color? clicked = null)
            {
                _userHover = hover;
                _userClick = click;
                _userClicked = clicked;

            }

        }

        public enum Interaction
        {
            Hover,
            Clicked,
            Click,
        }

        public interface MyInteractiveObject
        {
            Rectangle Hitbox { get; }
            Action Clicked { get; }
            Action Hover { get; }
            Action Click { get; }
            Object Content { get; }
            Style Style { get; }
            float Scale { get; }
            Interaction Interaction { set; }
        }

        public class Button : MyInteractiveObject
        {
            private Rectangle _hitbox { get; set; }
            public Rectangle Hitbox
            {
                get
                {
                    return _hitbox;
                }
                set
                {
                    _hitbox = value; NewSprite();
                }
            }
            public Action Clicked { get; }
            public Action Hover { get; }
            public Action Click { get; }
            public Style Style { get; }
            public float Scale { get; }

            private Interaction _interaction;

            public Interaction Interaction
            {
                get
                {
                    return _interaction;
                }

                set
                {
                    _interaction = value;
                    SetOverlay(value, Hitbox, Style, out Overlay);
                    switch (value)
                    {
                        case Interaction.Hover:
                            Hover?.Invoke(); break;
                        case Interaction.Click:
                            Click?.Invoke(); break;
                        case Interaction.Clicked:
                            Clicked?.Invoke(); break;
                        default: return;
                    }
                }

            }

            private Object _content { get; set; }
            public Object Content
            {
                get
                {
                    if (Overlay != null)
                    {
                        var _Value = ((List<MySprite>)_content).ToList();
                        _Value.Add(Overlay.Value);
                        return _Value;
                    }
                    else return _content;
                }
                set
                {
                    _content = new List<MySprite>() { new MySprite(SpriteType.CLIP_RECT, null, new Vector2(Hitbox.Left, Hitbox.Top), new Vector2(Hitbox.Width, Hitbox.Height)) };
                    if (value is MySprite) { ((List<MySprite>)_content).Add((MySprite)value); }
                    else if (value is string)
                    {
                        ((List<MySprite>)_content).Add(MySprite.CreateClipRect(Hitbox));
                        ((List<MySprite>)_content).Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(Hitbox.Left, Hitbox.Top + (Hitbox.Height / 2)), new Vector2(Hitbox.Width, Hitbox.Height), BackColor, null, TextAlignment.LEFT));
                        ((List<MySprite>)_content).Add(new MySprite(SpriteType.TEXT, (string)value, new Vector2(Hitbox.Left, Hitbox.Top + (Hitbox.Height / 2)), null, ForeColor, "White", TextAlignment.LEFT, Scale));
                        ((List<MySprite>)_content).Add(MySprite.CreateClearClipRect());
                    }
                    else if (value is List<MySprite>)
                    {
                        foreach (var sprite in (List<MySprite>)value)
                            ((List<MySprite>)_content).Add(sprite);
                    }
                }
            }
            public Color BackColor = Color.Gray;
            public Color ForeColor = Color.Black;

            MySprite? Overlay;

            public Button(int x, int y, int w, int h, Action MyAction, Action MyClick = null, Action MyHover = null, Object _content = null, Style style = null, float scale = 1f)
            {
                Hitbox = new Rectangle(x, y, w, h);
                Clicked = MyAction;
                Click = MyClick;
                Hover = MyHover;
                Scale = scale;
                Style = (style != null) ? style : new Style();
                Content = _content ?? new List<MySprite> { new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(Hitbox.Left, Hitbox.Top + (Hitbox.Height / 2)), new Vector2(Hitbox.Width, Hitbox.Height), BackColor, null, TextAlignment.LEFT) };

            }

            public static void SetOverlay(Interaction interaction, Rectangle hitbox, Style style, out MySprite? overlay)
            {

                Color Color;

                switch (interaction)
                {
                    case Interaction.Hover:
                        Color = style.Hover; break;
                    case Interaction.Click:
                        Color = style.Click; break;
                    case Interaction.Clicked:
                        Color = style.Clicked; break;
                    default:
                        overlay = null; return;
                }

                overlay = new MySprite(
                    SpriteType.TEXTURE,
                    "SquareSimple",
                    new Vector2(hitbox.Left, hitbox.Top + (hitbox.Height / 2)),
                    new Vector2(hitbox.Width, hitbox.Height),
                    Color,
                    null,
                    TextAlignment.LEFT);
            }


            private void NewSprite()
            {
                if (!(Content is List<MySprite>)) { Content = new List<MySprite>(); }
            }
        }
    }

}
