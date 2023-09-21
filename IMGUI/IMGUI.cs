using Godot;
using System;
using System.Collections.Generic;
using Internal.IMGUI;

/// <summary>
/// Implement interface on class or struct to change how it is rendered with IMGUI components
/// </summary>
public interface IMGUI_ProperyDrawer
{
    /// <summary>
    /// return true if property was updated
    /// </summary>
    bool DrawProperty(IMGUI_Interface gui);
}

public interface IMGUI_Interface
{
    T GetGUIElement<T>() where T : Godot.Control, new();

    public interface Label : IMGUI_Interface { }
}

public static class IMGUI_Extensions
{
    public static IMGUI_Interface.Label Label(this IMGUI_Interface self, string text)
    {
        var label = self.GetGUIElement<IMGUI_Label>();
        label.label.Text = text;
        label.element.SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin;
        return label;
    }
    public static IMGUI_Interface.Label Label(this IMGUI_Interface self, params object[] args)
    {
        var builder = new System.Text.StringBuilder();
        foreach (var arg in args)
            if (arg is null) builder.Append("null");
            else builder.Append(arg.ToString());
        return self.Label(builder.ToString());
    }

    public static IMGUI_Interface.Label SetTooltip(this IMGUI_Interface.Label self, params object[] args)
    {
        if (self is IMGUI_Label label)
        {
            var builder = new System.Text.StringBuilder();
            foreach (var arg in args)
            {
                builder.Append(arg is null ? "null" : arg.ToString());
                builder.Append(' ');
            }
            label.TooltipText = builder.ToString();
        }
        return self;
    }

    public static IMGUI_Interface.Label SetColor(this IMGUI_Interface.Label self, Godot.Color color)
    {
        if (self is IMGUI_Label label)
            label.label.Modulate = color;
        return self;
    }

    public static IMGUI_Interface.Label SetStretchRatio(this IMGUI_Interface.Label self, float ratio)
    {
        if (self is IMGUI_Label label)
            label.label.SizeFlagsStretchRatio = ratio;
        return self;
    }

    public static bool Button(this IMGUI_Interface self, string text)
    {
        var button = self.GetGUIElement<IMGUI_Button>();
        button.Text = text;
        button.ClipText = true;
        return button.IsPushed();
    }

    public static bool Button(this IMGUI_Interface self, params object[] args)
    {
        var builder = new System.Text.StringBuilder(); // object pool?
        foreach (var arg in args)
        {
            builder.Append(arg is null ? "null" : arg.ToString());
            builder.Append(' ');
        }
        return self.Button(builder.ToString());
    }

    public static bool CheckButton(this IMGUI_Interface self, bool input, out bool output)
    {
        return self.CheckButton(input, out output, default, default);
    }

    public static bool CheckButton(this IMGUI_Interface self, bool input, out bool output, string on_text, string off_text)
    {
        return self.GetGUIElement<IMGUI_CheckButton>().TryUpdate(input, out output, on_text, off_text);
    }

    public static bool CheckBox(this IMGUI_Interface self, bool input, out bool output)
    {
        return self.CheckButton(input, out output, default, default);
    }

    public static bool CheckBox(this IMGUI_Interface self, bool input, out bool output, string on_text, string off_text)
    {
        return self.GetGUIElement<IMGUI_CheckBox>().TryUpdate(input, out output, on_text, off_text);
    }

    public static bool TextEdit(this IMGUI_Interface self, string input, out string output)
        => self.GetGUIElement<IMGUI_TextEdit>().TryUpdate(input, out output);

    public static bool MultiLineTextEdit(this IMGUI_Interface self, string input, out string output, int pixel_height = 128, bool wrap = true)
    {
        var text_edit = self.GetGUIElement<IMGUI_MultiLineTextEdit>();
        text_edit.CustomMinimumSize = new Vector2(0, pixel_height);
        text_edit.WrapMode = wrap ? Godot.TextEdit.LineWrappingMode.Boundary : Godot.TextEdit.LineWrappingMode.None;
        return text_edit.TryUpdate(input, out output);
    }

    public static void VerticalSeparator(this IMGUI_Interface self, float height = 0)
    {
        self.GetGUIElement<Godot.HSeparator>().CustomMinimumSize = new Vector2(0, height);
    }

    public static void HorizontalSeparator(this IMGUI_Interface self, float width = 0)
    {
        self.GetGUIElement<Godot.VSeparator>().CustomMinimumSize = new Vector2(width, 0);
    }

    public static void Spacer(this IMGUI_Interface self, float width, float height)
    {
        var control = self.GetGUIElement<Godot.Control>();
        control.SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin;
        control.SizeFlagsVertical = Control.SizeFlags.ShrinkBegin;
        control.CustomMinimumSize = new Vector2(width, height);
    }

    public static void HBox(this IMGUI_Interface self, out IMGUI_Interface hbox)
    {
        hbox = self.GetGUIElement<IMGUI_HBoxContainer>();
    }

    public static void VBox(this IMGUI_Interface self, out IMGUI_Interface vbox)
    {
        vbox = self.GetGUIElement<IMGUI_VBoxContainer>();
    }

    public static void Grid(this IMGUI_Interface self, out IMGUI_Interface grid, int columns)
    {
        var node = self.GetGUIElement<IMGUI_GridContainer>();
        node.Columns = columns;
        grid = node;
    }

    public static bool Property<PropertyType>(this IMGUI_Interface self, PropertyType input, out PropertyType output)
    {
        if (self.GetGUIElement<PropertyDrawer>().Update(input, out var new_value))
        {
            output = (PropertyType)new_value;
            return true;
        }
        output = input;
        return false;
    }

    public static bool SpinBox(this IMGUI_Interface self, int input, out int output, int step = 1, int min = int.MinValue, int max = int.MaxValue, string tooltip = default)
    {
        output = input;
        var spin_box = self.GetGUIElement<IMGUI_Spinbox>();
        spin_box.TooltipText = tooltip;
        var updated = spin_box.TryUpdate(input, out var float_output, step);
        output = output < min ? min : output > max ? max : output;
        if (updated) output = (int)float_output;
        return updated;
    }

    public static bool SpinBox(this IMGUI_Interface self, float input, out float output, float step = 0.1f, float min = float.MinValue, float max = float.MaxValue, string tooltip = default)
    {
        output = input;
        var spin_box = self.GetGUIElement<IMGUI_Spinbox>();
        spin_box.TooltipText = tooltip;
        var updated = spin_box.TryUpdate(input, out var float_output, step);
        output = output < min ? min : output > max ? max : output;
        if (updated) output = (float)float_output;
        return updated;
    }

    public static bool SpinBox(this IMGUI_Interface self, double input, out double output, double step = 0.1f, double min = double.MinValue, double max = double.MaxValue, string tooltip = default)
    {
        output = input;
        var spin_box = self.GetGUIElement<IMGUI_Spinbox>();
        spin_box.TooltipText = tooltip;
        var updated = spin_box.TryUpdate(input, out output, step);
        output = output < min ? min : output > max ? max : output;
        return updated;
    }

    public static bool Tabs<E>(this IMGUI_Interface self, E input, out E output)
        where E : struct, System.Enum
        => self.GetGUIElement<IMGUI_Tabs<E>>().TryUpdate(input, out output);

    public static bool ColorPicker(this IMGUI_Interface self, Color input, out Color output)
    {
        return self.GetGUIElement<IMGUI_ColorPicker>().TryUpdate(input, out output);
    }

    public static bool Vectot2(this IMGUI_Interface self, Godot.Vector2 input, out Godot.Vector2 output, float step = 0.01f)
    {
        var node = self.GetGUIElement<IMGUI_HBoxContainer>();
        if (node.SpinBox(input.X, out input.X, step, tooltip: "X")
        | node.SpinBox(input.Y, out input.Y, step, tooltip: "Y"))
        {
            output = input;
            return true;
        }
        output = input;
        return false;
    }

    public static bool Vectot3(this IMGUI_Interface self, Godot.Vector3 input, out Godot.Vector3 output, float step = 0.01f)
    {
        var node = self.GetGUIElement<IMGUI_HBoxContainer>();
        if (node.SpinBox(input.X, out input.X, step, tooltip: "X")
        | node.SpinBox(input.Y, out input.Y, step, tooltip: "Y")
        | node.SpinBox(input.Z, out input.Z, step, tooltip: "Z"))
        {
            output = input;
            return true;
        }
        output = input;
        return false;
    }

    public static bool Vectot4(this IMGUI_Interface self, Godot.Vector4 input, out Godot.Vector4 output, float step = 0.01f)
    {
        var node = self.GetGUIElement<IMGUI_HBoxContainer>();
        if (node.SpinBox(input.X, out input.X, step, tooltip: "X")
        | node.SpinBox(input.Y, out input.Y, step, tooltip: "Y")
        | node.SpinBox(input.Z, out input.Z, step, tooltip: "Z")
        | node.SpinBox(input.W, out input.W, step, tooltip: "W"))
        {
            output = input;
            return true;
        }
        output = input;
        return false;
    }

    public static bool Option<T>(this IMGUI_Interface self, T input, out T output, IEnumerable<T> options)
    {
        return self.Option(input, out output, options, static t => t == null ? "null" : t.ToString());
    }

    public static bool Option<T>(this IMGUI_Interface self, T input, out T output, IEnumerable<T> options, Func<T, string> format_display)
    {
        return self.GetGUIElement<IMGUI_Option<T>>()
            .TryUpdate(input, out output, options, format_display);
    }

    public static bool Option<T>(this IMGUI_Interface self, T input, out T output) where T : struct, System.Enum
    {
        return self.Option<T>(input, out output, System.Enum.GetValues<T>());
    }

    public static void Panel(this IMGUI_Interface self, out IMGUI_Interface panel_gui)
    {
        panel_gui = self.GetGUIElement<IMGUI_PanelContainer>();
    }

    public static void Scroll(this IMGUI_Interface self, out IMGUI_Interface scroll_gui, float width = 0, float height = 0)
    {
        var scroll = self.GetGUIElement<IMGUI_ScrollContainer>();
        scroll.CustomMinimumSize = new Vector2(width, height);
        if (width == 0) scroll.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        if (height == 0) scroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        scroll_gui = scroll;
    }

    public static void ScrollPanel(this IMGUI_Interface self, out IMGUI_Interface scroll_panel, float width = 0, float height = 0)
    {
        var scroll = self.GetGUIElement<IMGUI_ScrollPanel>();
        scroll.CustomMinimumSize = new Vector2(width, height);
        if (width <= 0) scroll.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        if (height <= 0) scroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        scroll_panel = scroll;
    }
}

namespace Internal.IMGUI
{
    partial class GUI_Element : BoxContainer, IMGUI_Interface
    {
        public GUI_Element(Godot.Control parent)
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            ClipContents = true;
            parent.AddChild(this);
        }
        Godot.Control element;
        public T GetGUIElement<T>() where T : Godot.Control, new()
        {
            if (element is T value && element.GetType() == typeof(T))
            {
                Visible = true;
                SizeFlagsHorizontal = element.SizeFlagsHorizontal;
                SizeFlagsVertical = element.SizeFlagsVertical;
                return value;
            }
            if (Node.IsInstanceValid(element))
                element.QueueFree();

            Visible = false;
            element = new T() { };
            element.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            element.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            AddChild(element);
            return (T)element;
        }
    }

    public partial class IMGUI_Button : Godot.Button
    {
        public IMGUI_Button()
        {
            this.ButtonDown += () => pushed = true;
            this.ButtonUp += () => this.ReleaseFocus();
            ClipText = true;
        }

        bool pushed;

        public bool IsPushed()
        {
            var push = pushed;
            pushed = false;
            return push;
        }
    }

    public partial class IMGUI_CheckButton : Godot.CheckButton
    {
        bool pushed;
        public IMGUI_CheckButton()
        {
            this.ButtonDown += () => pushed = true;
            Alignment = HorizontalAlignment.Left;
            ClipText = true;
        }

        public bool TryUpdate(bool input, out bool output, string on, string off)
        {
            ButtonPressed = input;
            Text = input ? on : off;
            if (pushed)
            {
                output = !input;
                pushed = false;
                return true;
            }
            else output = input;
            return false;
        }
    }

    public partial class IMGUI_CheckBox : Godot.CheckBox
    {
        bool pushed;
        public IMGUI_CheckBox()
        {
            this.ButtonDown += () => pushed = true;
            Alignment = HorizontalAlignment.Right;
            ClipText = true;
        }

        public bool TryUpdate(bool input, out bool output, string on, string off)
        {
            ButtonPressed = input;
            Text = input ? on : off;
            if (pushed)
            {
                output = !input;
                pushed = false;
                return true;
            }
            else output = input;
            return false;
        }
    }

    public partial class IMGUI_Tabs<E> : Godot.TabContainer where E : struct, System.Enum
    {
        public IMGUI_Tabs()
        {
            this.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            this.SizeFlagsVertical = SizeFlags.ExpandFill;

            foreach (var value in System.Enum.GetValues<E>())
                AddChild(new Control { Name = value.ToString() });

            this.TabChanged += t =>
            {
                tab = (int)t;
                CurrentTab = (int)t;
            };
        }

        int? tab;
        public bool TryUpdate(E input, out E output)
        {
            if (tab.HasValue)
            {
                output = System.Enum.GetValues<E>()[tab.GetValueOrDefault()];
                tab = default;
                return true;
            }
            else
            {
                var items = System.Enum.GetValues<E>();
                for (int i = 0; i < items.Length; ++i)
                    if (EqualityComparer<E>.Default.Equals(input, items[i]))
                    {
                        if (CurrentTab != i)
                            CurrentTab = i;
                        break;
                    }
            }

            output = input;
            tab = default;
            return false;
        }
    }

    public partial class IMGUI_Spinbox : Godot.SpinBox
    {
        public IMGUI_Spinbox()
        {
            this.ValueChanged += f =>
            {
                new_value = f;
                foreach (var child in GetChildren(true))
                    if (child is Control control && control.HasFocus())
                        control.ReleaseFocus();
            };

            MinValue = int.MinValue;
            MaxValue = int.MaxValue;
            SizeFlagsHorizontal = SizeFlags.ExpandFill;
        }

        double? new_value;
        bool init = true;

        public bool TryUpdate(double input, out double output, double step)
        {
            Step = step;
            if (init)
            {
                init = false;
                Value = output = input;
                new_value = null;
                return false;
            }

            if (new_value.HasValue)
            {
                output = new_value.Value;
                new_value = default;
                return true;
            }

            Value = output = input;
            new_value = default;
            return false;
        }
    }

    public partial class IMGUI_TextEdit : LineEdit
    {
        string new_value;
        bool init = true;
        public bool TryUpdate(string input, out string output)
        {
            output = input = input == null ? "" : input;
            if (init)
            {
                init = false;
                Text = input;
                TextChanged += value => new_value = value;
                TextSubmitted += value =>
                {
                    ReleaseFocus();
                };
                return false;
            }

            if (!HasFocus())
                Text = input;
            else if (Input.IsKeyPressed(Key.Escape))
                ReleaseFocus();

            bool updated = new_value != null;
            if (updated)
            {
                output = new_value;
                new_value = null;
            }
            return updated;
        }
    }

    public partial class IMGUI_MultiLineTextEdit : TextEdit
    {
        string new_value;
        bool init = true;
        public bool TryUpdate(string input, out string output)
        {
            output = input = input == null ? "" : input;
            if (init)
            {
                init = false;
                Text = input;
                this.TextChanged += () => new_value = this.Text;
                return false;
            }

            if (!HasFocus())
                Text = input;
            else if (Input.IsKeyPressed(Key.Escape))
                this.ReleaseFocus();

            bool updated = new_value != null;
            if (updated)
            {
                output = new_value;
                new_value = null;
            }
            return updated;
        }
    }

    partial class IMGUI_Label : HBoxContainer, IMGUI_Interface.Label
    {
        public IMGUI_Label()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill;
            ClipContents = true;
            label = new Label
            {
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsVertical = SizeFlags.ShrinkBegin,
                VerticalAlignment = VerticalAlignment.Center,
                SizeFlagsStretchRatio = .5f,
                ClipText = true,
            };
            this.AddChild(label);
            element = new GUI_Element(this);
            element.SizeFlagsStretchRatio = 1;
        }

        public Label label;
        public GUI_Element element;
        public T GetGUIElement<T>() where T : Godot.Control, new() => element.GetGUIElement<T>();
    }

    partial class IMGUI_ColorPicker : ColorPickerButton
    {
        public IMGUI_ColorPicker()
        {
            this.ColorChanged += new_color => color = new_color;
            CustomMinimumSize = new Vector2(0, 24);
        }

        Color? color;

        bool init = true;

        public bool TryUpdate(Godot.Color input, out Godot.Color output)
        {
            output = input;

            if (color.HasValue && !init)
            {
                output = color.GetValueOrDefault();
                color = default;
                return true;
            }
            init = false;
            Color = output = input;
            color = default;
            return false;
        }
    }

    partial class IMGUI_Option<T> : OptionButton
    {
        long? new_value;
        long current_value;
        bool init;
        public bool TryUpdate(T input, out T output, IEnumerable<T> options, Func<T, string> format_name)
        {
            Text = format_name.Invoke(input);

            output = input;
            if (!init)
            {
                _Draw();
                init = true;
                this.ButtonDown += () =>
                {
                    Clear();
                    int index = 0;
                    foreach (var item in options)
                    {
                        AddItem(format_name(item), index);
                        if (input.Equals(item))
                            Selected = index;
                        index++;
                    }
                    Selected = -1;
                };
                this.ItemSelected += value =>
                {
                    new_value = value;
                    current_value = value;
                };
            }

            if (new_value.HasValue)
            {
                int index = 0;
                foreach (var item in options)
                {
                    if (new_value.GetValueOrDefault() == index)
                    {
                        output = (T)item;
                        new_value = default;
                        return true;
                    }
                    index++;
                }
            }

            new_value = default;
            return false;
        }
    }

    public partial class IMGUI_ScrollContainer : Godot.ScrollContainer, IMGUI_Interface
    {
        public IMGUI_ScrollContainer()
        {
            AddChild(container);
        }
        IMGUI_VBoxContainer container = new IMGUI_VBoxContainer();
        public T GetGUIElement<T>() where T : Godot.Control, new() => ((IMGUI_Interface)container).GetGUIElement<T>();
    }

    public partial class IMGUI_PanelContainer : Godot.PanelContainer, IMGUI_Interface
    {
        public IMGUI_PanelContainer()
        {
            AddChild(container);
            this.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            container.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            this.OffsetLeft = 8;
            this.OffsetRight = -8;
            this.OffsetTop = 8;
            this.OffsetBottom = -8;
        }

        public T GetGUIElement<T>() where T : Godot.Control, new() => ((IMGUI_Interface)container).GetGUIElement<T>();
        IMGUI_VBoxContainer container = new IMGUI_VBoxContainer();
    }

    public partial class IMGUI_ScrollPanel : Godot.PanelContainer, IMGUI_Interface
    {
        public IMGUI_ScrollPanel()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill;
            SizeFlagsVertical = SizeFlags.ExpandFill;
            ClipContents = true;

            var scroll = new ScrollContainer
            {
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsVertical = SizeFlags.ExpandFill
            };
            AddChild(scroll);
            scroll.AddChild(container);
        }

        IMGUI_VBoxContainer container = new IMGUI_VBoxContainer();
        T IMGUI_Interface.GetGUIElement<T>() => ((IMGUI_Interface)container).GetGUIElement<T>();
    }

    public partial class PropertyDrawer : BoxContainer
    {
        IMGUI_VBoxContainer gui = new IMGUI_VBoxContainer() { LayoutDirection = LayoutDirectionEnum.Ltr };
        public PropertyDrawer()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill;
            this.AddChild(gui);
        }

        /// <summary>
        /// returns true when drawer values were updated
        /// </summary>
        public bool Update(object input, out object output)
        {
            return TryUpdate(gui, input, out output);

            bool TryUpdate(IMGUI_Interface gui, object input, out object output)
            {
                output = input;
                var updated = false;
                switch (input)
                {
                    case null:
                        {
                            gui.Label("null");
                            return true;
                        }
                    case bool bool_val:
                        {
                            if (updated = gui.Button(bool_val ? "ON" : "OFF"))
                                output = !bool_val;
                            return updated;
                        }
                    case char char_val:
                        {
                            if (updated = gui.TextEdit(char_val.ToString(), out var new_value))
                            {
                                char_val = new_value.Length == 0 ? default : new_value[new_value.Length - 1];
                            }
                            output = char_val;
                            return updated;
                        }
                    case byte byte_val:
                        {
                            updated = gui.SpinBox(System.Convert.ToDouble(byte_val), out var new_value, 1);
                            output = (byte)new_value;
                            return updated;
                        }
                    case sbyte sbyte_val:
                        {
                            updated = gui.SpinBox(System.Convert.ToDouble(sbyte_val), out var new_value, 1);
                            output = (sbyte)new_value;
                            return updated;
                        }
                    case short short_val:
                        {
                            updated = gui.SpinBox(System.Convert.ToDouble(short_val), out var new_value, 1);
                            output = (short)new_value;
                            return updated;
                        }
                    case ushort ushort_val:
                        {
                            updated = gui.SpinBox(System.Convert.ToDouble(ushort_val), out var new_value, 1);
                            output = (ushort)new_value;
                            return updated;
                        }
                    case int int_val:
                        {
                            updated = gui.SpinBox(System.Convert.ToDouble(int_val), out var new_value, 1);
                            output = (int)new_value;
                            return updated;
                        }
                    case uint uint_val:
                        {
                            updated = gui.SpinBox(System.Convert.ToDouble(uint_val), out var new_value, 1);
                            output = (uint)new_value;
                            return updated;
                        }
                    case long long_val:
                        {
                            updated = gui.SpinBox(System.Convert.ToDouble(long_val), out var new_value, 1);
                            output = (long)new_value;
                            return updated;
                        }
                    case ulong ulong_val:
                        {
                            updated = gui.SpinBox(System.Convert.ToDouble(ulong_val), out var new_value, 1);
                            output = (ulong)new_value;
                            return updated;
                        }
                    case System.Half half_val:
                        {
                            updated = gui.SpinBox(System.Convert.ToDouble(half_val), out var new_value, 1);
                            output = (System.Half)new_value;
                            return updated;
                        }
                    case float float_val:
                        {
                            updated = gui.SpinBox(System.Convert.ToDouble(float_val), out var new_value, 0.01);
                            output = (float)new_value;
                            return updated;
                        }
                    case double double_val:
                        {
                            updated = gui.SpinBox(System.Convert.ToDouble(double_val), out var new_value, 1);
                            output = new_value;
                            return updated;
                        }
                    case string str_val:
                        {
                            updated = gui.TextEdit(str_val, out var new_value);
                            output = new_value;
                            return updated;
                        }
                    case System.Enum:
                        {
                            var values = (System.Enum.GetValues(input.GetType()) as System.Collections.IEnumerable);
                            return gui.Option(input, out output, System.Linq.Enumerable.Cast<object>(values));
                        }

                    case Godot.Color color_val:
                        {
                            updated = gui.ColorPicker(color_val, out var new_value);
                            output = new_value;
                            return updated;
                        }

                    case Godot.Vector2 vec2_val:
                        {
                            updated = gui.Vectot2(vec2_val, out vec2_val);
                            output = vec2_val;
                            return updated;
                        }

                    case Godot.Vector3 vec3_val:
                        {
                            updated = gui.Vectot3(vec3_val, out vec3_val);
                            output = vec3_val;
                            return updated;
                        }

                    case Godot.Vector4 vec4_val:
                        {
                            updated = gui.Vectot4(vec4_val, out vec4_val);
                            output = vec4_val;
                            return updated;
                        }

                    case IMGUI_ProperyDrawer property_drawer:
                        {
                            updated = property_drawer.DrawProperty(gui);
                            if (updated) output = property_drawer;
                            return updated;
                        }

                    case System.Collections.IList list:
                        {
                            var drawer = gui.GetGUIElement<IMGUI_Property>();
                            if (drawer.Button(list.Count, "items"))
                                drawer.show = !drawer.show;
                            if (drawer.show)
                            {
                                drawer.HBox(out var hbox);
                                if (hbox.Label("min").SetTooltip("Min Shown Index").SetStretchRatio(1).Property(drawer.min, out drawer.min))
                                    drawer.min = drawer.min < 0 ? 0 : drawer.min;
                                if (hbox.Label("max").SetTooltip("Max Shown Items").SetStretchRatio(1).Property(drawer.max, out drawer.max))
                                    drawer.max = drawer.max < 1 ? 1 : drawer.max;

                                drawer.VerticalSeparator();
                                int render_count = 0, index = drawer.min;
                                while (render_count < drawer.max && index < list.Count)
                                {
                                    if (drawer.Label(index).SetStretchRatio(.25f).Property(list[index], out var new_value))
                                    {
                                        updated = true;
                                        list[index] = new_value;
                                    }
                                    render_count++;
                                    index++;
                                }
                            }
                            return updated;
                        }

                    default: // default drawer only draws public fields
                        {
                            var drawer = gui.GetGUIElement<IMGUI_Property>();
                            if (drawer.Button(input.ToString()))
                                drawer.show = !drawer.show;

                            if (drawer.show)
                            {
                                foreach (var field in input.GetType().GetFields())
                                {
                                    if (drawer.Label(field.Name).SetStretchRatio(.3f).Property(field.GetValue(input), out var field_value))
                                    {
                                        updated = true;
                                        field.SetValue(output, field_value);
                                    }

                                    if (field_value == null && field.FieldType == typeof(string))
                                    {
                                        field_value = "";
                                        field.SetValue(output, field_value);
                                        updated = true;
                                    }
                                }
                            }
                            return updated;
                        }
                }
            }
        }

        partial class IMGUI_Property : Internal.IMGUI.IMGUI_PanelContainer
        {
            public int min, max = 1000;
            public bool show;
        }
    }
}
