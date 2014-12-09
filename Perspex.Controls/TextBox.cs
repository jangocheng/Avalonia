﻿// -----------------------------------------------------------------------
// <copyright file="TextBox.cs" company="Steven Kirk">
// Copyright 2014 MIT Licence. See licence.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace Perspex.Controls
{
    using System;
    using System.Linq;
    using Perspex.Controls.Primitives;
    using Perspex.Input;
    using Perspex.Styling;

    public class TextBox : TemplatedControl
    {
        public static readonly PerspexProperty<bool> AcceptsReturnProperty =
            PerspexProperty.Register<TextBox, bool>("AcceptsReturn");

        public static readonly PerspexProperty<bool> AcceptsTabProperty =
            PerspexProperty.Register<TextBox, bool>("AcceptsTab");

        public static readonly PerspexProperty<int> CaretIndexProperty =
            PerspexProperty.Register<TextBox, int>("CaretIndex", coerce: CoerceCaretIndex);

        public static readonly PerspexProperty<int> SelectionStartProperty =
            PerspexProperty.Register<TextBox, int>("SelectionStart", coerce: CoerceCaretIndex);

        public static readonly PerspexProperty<int> SelectionEndProperty =
            PerspexProperty.Register<TextBox, int>("SelectionEnd", coerce: CoerceCaretIndex);

        public static readonly PerspexProperty<string> TextProperty =
            TextBlock.TextProperty.AddOwner<TextBox>();

        private TextBoxView textBoxView;

        static TextBox()
        {
            FocusableProperty.OverrideDefaultValue(typeof(TextBox), true);
        }

        public TextBox()
        {
            this.GotFocus += (s, e) => this.textBoxView.GotFocus();
            this.LostFocus += (s, e) => this.textBoxView.LostFocus();
            this.KeyDown += this.OnKeyDown;
            this.PointerPressed += this.OnPointerPressed;
        }

        public bool AcceptsReturn
        {
            get { return this.GetValue(AcceptsReturnProperty); }
            set { this.SetValue(AcceptsReturnProperty, value); }
        }

        public bool AcceptsTab
        {
            get { return this.GetValue(AcceptsTabProperty); }
            set { this.SetValue(AcceptsTabProperty, value); }
        }

        public int CaretIndex
        {
            get { return this.GetValue(CaretIndexProperty); }
            set { this.SetValue(CaretIndexProperty, value); }
        }

        public int SelectionStart
        {
            get { return this.GetValue(SelectionStartProperty); }
            set { this.SetValue(SelectionStartProperty, value); }
        }

        public int SelectionEnd
        {
            get { return this.GetValue(SelectionEndProperty); }
            set { this.SetValue(SelectionEndProperty, value); }
        }

        public string Text
        {
            get { return this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        protected override void OnTemplateApplied()
        {
            Decorator textContainer = this.GetVisualDescendents()
                .OfType<Decorator>()
                .FirstOrDefault(x => x.Id == "textContainer");

            if (textContainer == null)
            {
                throw new Exception(
                    "TextBox template doesn't contain a textContainer " +
                    "or textContainer is not a Decorator.");
            }

            textContainer.Content = this.textBoxView = new TextBoxView(this);
        }

        private static int CoerceCaretIndex(PerspexObject o, int value)
        {
            var text = o.GetValue(TextProperty);
            var length = (text != null) ? text.Length : 0;
            return Math.Max(0, Math.Min(length, value));
        }

        private void MoveHorizontal(int count, ModifierKeys modifiers)
        {
            if (modifiers == ModifierKeys.None)
            {
                this.CaretIndex += count;
                this.SelectionStart = this.SelectionEnd = this.CaretIndex;
            }
            else if ((modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                this.SelectionEnd = (this.CaretIndex += count);
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            string text = this.Text ?? string.Empty;
            var caretIndex = this.CaretIndex;

            switch (e.Key)
            {
                case Key.Left:
                    this.MoveHorizontal(-1, e.Device.Modifiers);
                    break;

                case Key.Right:
                    this.MoveHorizontal(1, e.Device.Modifiers);
                    break;

                case Key.Back:
                    if (this.CaretIndex > 0)
                    {
                        this.Text = text.Substring(0, caretIndex - 1) + text.Substring(caretIndex);
                        --this.CaretIndex;
                    }

                    break;

                case Key.Delete:
                    if (caretIndex < text.Length)
                    {
                        this.Text = text.Substring(0, caretIndex) + text.Substring(caretIndex + 1);
                    }

                    break;

                case Key.Enter:
                    if (this.AcceptsReturn)
                    {
                        goto default;
                    }

                    break;

                case Key.Tab:
                    if (this.AcceptsTab)
                    {
                        goto default;
                    }

                    break;

                default:
                    if (!string.IsNullOrEmpty(e.Text))
                    {
                        this.Text = text.Substring(0, caretIndex) + e.Text + text.Substring(caretIndex);
                        ++this.CaretIndex;
                    }

                    break;
            }

            e.Handled = true;
        }

        private void OnPointerPressed(object sender, PointerEventArgs e)
        {
            var point = e.GetPosition(this.textBoxView);
            this.CaretIndex = this.textBoxView.GetCaretIndex(point);
        }
    }
}
