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
      label.label.GetParent<Control>().TooltipText = text;
      label.label.Modulate = Colors.White;
      return label;
   }
   public static IMGUI_Interface.Label Label(this IMGUI_Interface self, params object[] args)
   {
      var builder = new System.Text.StringBuilder();
      foreach (var arg in args)
      {
         if (arg is null) builder.Append("null");
         else builder.Append(arg.ToString());
         builder.Append(' ');
      }
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
         label.label.GetParent<Control>().TooltipText = builder.ToString();
      }
      return self;
   }

   public static IMGUI_Interface.Label SetColor(this IMGUI_Interface.Label self, Godot.Color color)
   {
      if (self is IMGUI_Label label)
         label.label.Modulate = color;
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

   public static bool TextureButton(this IMGUI_Interface self, float width, float height, Texture2D normal, Texture2D pressed = null, Texture2D hover = null, bool flip_horizontal = false, bool flip_vertical = false)
   {
      var button = self.GetGUIElement<IMGUI_TextureButton>();
      button.TextureNormal = normal;
      button.TexturePressed = pressed;
      button.TextureHover = hover;
      button.FlipH = flip_horizontal;
      button.FlipV = flip_vertical;
      button.CustomMinimumSize = new Vector2(width, height);
      return button.IsPushed();
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
      return self.CheckBox(input, out output, default, default);
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
      control.CustomMinimumSize = new Vector2(width, height);
      control.GetParent<Control>().SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin;
      control.GetParent<Control>().SizeFlagsVertical = Control.SizeFlags.ShrinkBegin;
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

   public static bool Property(this IMGUI_Interface self, Type type, object input, out object output, string tool_tip = default, bool collapsable = default)
   {
      var property = self.GetGUIElement<PropertyDrawer>();
      if (tool_tip != null) property.TooltipText = tool_tip;
      if (property.Update(type, input, out var new_value, collapsable))
      {
         output = new_value;
         return true;
      }
      output = input;
      return false;
   }
   public static bool Property<PropertyType>(this IMGUI_Interface self, ref PropertyType property, string tool_tip = default, bool collapsable = true)
      => Property(self, property, out property, tool_tip, collapsable);

   public static bool Property<PropertyType>(this IMGUI_Interface self, PropertyType input, out PropertyType output, string tool_tip = default, bool collapsable = true)
   {
      var property = self.GetGUIElement<PropertyDrawer>();
      if (tool_tip != null) property.TooltipText = tool_tip;
      if (property.Update(typeof(PropertyType), input, out var new_value, collapsable))
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
      var updated = spin_box.TryUpdate(input, out var float_output, step, min, max);
      if (updated) output = (int)float_output;
      return updated;
   }

   public static bool SpinBox(this IMGUI_Interface self, float input, out float output, float step = 0.1f, float min = int.MinValue, float max = int.MaxValue, string tooltip = default)
   {
      output = input;
      var spin_box = self.GetGUIElement<IMGUI_Spinbox>();
      spin_box.TooltipText = tooltip;
      var updated = spin_box.TryUpdate(input, out var float_output, step, min, max);
      if (updated) output = (float)float_output;
      return updated;
   }

   public static bool SpinBox(this IMGUI_Interface self, double input, out double output, double step = 0.01f, double min = int.MinValue, double max = int.MaxValue, string tooltip = default)
   {
      output = input;
      var spin_box = self.GetGUIElement<IMGUI_Spinbox>();
      spin_box.TooltipText = tooltip;
      var updated = spin_box.TryUpdate(input, out output, step, min, max);
      return updated;
   }

   public static bool HSlider(this IMGUI_Interface self, int input, out int output, int step = 1, float min = 0, float max = 100)
   {
      var slider = self.GetGUIElement<IMGUI_HSlider>();
      if (slider.TryUpdate(input, out var value, step, min, max))
      {
         output = (int)value;
         return true;
      }
      output = input;
      return false;
   }

   public static bool HSlider(this IMGUI_Interface self, float input, out float output, float step = .1f, float min = 0, float max = 1)
   {
      var slider = self.GetGUIElement<IMGUI_HSlider>();
      if (slider.TryUpdate(input, out var value, step, min, max))
      {
         output = (float)value;
         return true;
      }
      output = input;
      return false;
   }

   public static bool HSlider(this IMGUI_Interface self, double input, out double output, double step = .1f, double min = 0, double max = 1)
   {
      var slider = self.GetGUIElement<IMGUI_HSlider>();
      return slider.TryUpdate(input, out output, step, min, max);
   }

   public static bool VSlider(this IMGUI_Interface self, int input, out int output, int step = 1, float min = 0, float max = 100)
   {
      var slider = self.GetGUIElement<IMGUI_VSlider>();
      if (slider.TryUpdate(input, out var value, step, min, max))
      {
         output = (int)value;
         return true;
      }
      output = input;
      return false;
   }

   public static bool VSlider(this IMGUI_Interface self, float input, out float output, float step = .1f, float min = 0, float max = 1)
   {
      var slider = self.GetGUIElement<IMGUI_VSlider>();
      if (slider.TryUpdate(input, out var value, step, min, max))
      {
         output = (float)value;
         return true;
      }
      output = input;
      return false;
   }

   public static bool VSlider(this IMGUI_Interface self, double input, out double output, double step = .1f, double min = 0, double max = 1)
   {
      var slider = self.GetGUIElement<IMGUI_VSlider>();
      return slider.TryUpdate(input, out output, step, min, max);
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

   public static bool Option<T>(this IMGUI_Interface self, T input, out T output, params T[] options)
      => self.Option(input, out output, () => options);

   public static bool Option<T>(this IMGUI_Interface self, T input, out T output, Func<IEnumerable<T>> options)
      => self.Option(input, out output, options, static t => t == null ? "null" : t.ToString());

   public static bool Option<T>(this IMGUI_Interface self, T input, out T output, IEnumerable<T> options)
      => self.Option(input, out output, options, static t => t == null ? "null" : t.ToString());

   public static bool Option<T>(this IMGUI_Interface self, T input, out T output, IEnumerable<T> options, Func<T, string> format_display)
      => self.Option(input, out output, () => options, format_display);

   public static bool Option<T>(this IMGUI_Interface self, T input, out T output, Func<IEnumerable<T>> options, Func<T, string> format_display)
      => self.Option(format_display.Invoke(input), input, out output, options, format_display);

   public static bool Option<T>(this IMGUI_Interface self, string label, T input, out T output, Func<IEnumerable<T>> options)
      => self.Option(label, input, out output, options, static t => t == null ? "null" : t.ToString());

   public static bool Option<T>(this IMGUI_Interface text, string label, T input, out T output, Func<IEnumerable<T>> options, Func<T, string> format_display)
      => text.GetGUIElement<IMGUI_Option<T>>()
         .TryUpdate(label, input, out output, options, format_display);

   public static bool Option<T>(this IMGUI_Interface self, T input, out T output) where T : struct, System.Enum
   {
      return self.Option<T>(input, out output, System.Enum.GetValues<T>());
   }

   public static void Panel(this IMGUI_Interface self, out IMGUI_Interface panel_gui)
   {
      panel_gui = self.GetGUIElement<IMGUI_PanelContainer>();
   }

   public static void Panel(this IMGUI_Interface self, Color color, out IMGUI_Interface panel_gui)
   {
      panel_gui = self.GetGUIElement<IMGUI_PanelContainer>();
      (panel_gui as PanelContainer).SelfModulate = color;
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

   public static void DrawFields(this IMGUI_Interface self, object target) => DrawFields(self, target, static field => true);

   public static void DrawFields(this IMGUI_Interface self, object target, Func<System.Reflection.FieldInfo, bool> field_predicate)
   {
      var type = target?.GetType();
      if (type == null || !type.IsClass) return;

      foreach (var field in type.GetFields())
      {
         if (!field_predicate(field)) continue;
         if (self.Label(field.Name).Property(field.GetValue(target), out var new_value))
            field.SetValue(target, new_value);
      }
   }

   public static IMGUI_Interface Prefix(this IMGUI_Interface self, out IMGUI_Interface prefix)
   {
      var item = self.GetGUIElement<IMGUI_Prefix>();
      item.OnResize();
      prefix = item.left;
      return item.right;
   }
}

namespace Internal.IMGUI
{
   partial class GUI_Element : BoxContainer, IMGUI_Interface
   {
      public GUI_Element()
      {
         SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
         SizeFlagsVertical = Control.SizeFlags.Fill;
         ClipContents = true;
      }
      Godot.Control element;
      public T GetGUIElement<T>() where T : Godot.Control, new()
      {
         if (element is T value && element.GetType() == typeof(T))
         {
            element.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            element.SizeFlagsVertical = SizeFlags.ExpandFill;
            element.SizeFlagsStretchRatio = 1f;
            Visible = true;
            element.Visible = true;
            return value;
         }
         if (Node.IsInstanceValid(element))
            element.QueueFree();

         Visible = false;
         element = new T() { };
         element.Visible = false;
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

   public partial class IMGUI_HSlider : Godot.HSlider
   {
      public IMGUI_HSlider()
      {
         this.ValueChanged += f =>
         {
            new_value = f;
         };
         SizeFlagsHorizontal = SizeFlags.ExpandFill;
      }

      double? new_value;
      public bool TryUpdate(double input, out double output, double step, double min, double max)
      {
         Step = step;
         MinValue = min;
         MaxValue = max;


         if (new_value.HasValue)
         {
            Value = output = new_value.Value;
            new_value = default;
            return true;
         }

         Value = output = input;
         new_value = default;
         return false;
      }
   }

   public partial class IMGUI_VSlider : Godot.VSlider
   {
      public IMGUI_VSlider()
      {
         this.ValueChanged += f =>
         {
            new_value = f;
         };
         SizeFlagsHorizontal = SizeFlags.ExpandFill;
      }

      double? new_value;
      public bool TryUpdate(double input, out double output, double step, double min, double max)
      {
         Step = step;
         MinValue = min;
         MaxValue = max;


         if (new_value.HasValue)
         {
            Value = output = new_value.Value;
            new_value = default;
            return true;
         }

         Value = output = input;
         new_value = default;
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
         };

         MinValue = int.MinValue;
         MaxValue = int.MaxValue;
         SizeFlagsHorizontal = SizeFlags.ExpandFill;
      }

      double? new_value;

      public override void _Input(InputEvent @event)
      {
         switch (@event)
         {
            case InputEventMouseButton mouse:
               if (mouse.ButtonIndex is not MouseButton.Left) return;
               if (mouse.Pressed && this.ContainsMouse())
                  this.RemoveClickFocus();
               break;

            case InputEventKey key:
               switch (key.Keycode)
               {
                  case Key.Enter:
                  case Key.KpEnter:
                     if (key.Pressed)
                        this.RemoveClickFocus();
                     break;
               }
               break;
         }
      }

      public bool TryUpdate(double input, out double output, double step, double min, double max)
      {
         Step = step;
         MinValue = min;
         MaxValue = max;

         if (new_value.HasValue)
         {
            output = new_value.Value;
            new_value = default;
            return true;
         }

         SetValueNoSignal(output = input);
         return false;
      }
   }

   public sealed partial class IMGUI_TextEdit : LineEdit
   {
      string new_value;
      public IMGUI_TextEdit()
      {
         TextChanged += value => new_value = value;
         TextSubmitted += value => this.RemoveClickFocus();
      }

      public override void _Input(InputEvent @event)
      {
         if (@event is not InputEventMouseButton mouse) return;
         if (mouse.ButtonIndex is not MouseButton.Left) return;
         on_left_click = mouse.Pressed;
      }
      bool on_left_click;

      public bool TryUpdate(string input, out string output)
      {
         if (!HasFocus())
            Text = input;

         if (on_left_click && !this.ContainsMouse())
            this.RemoveClickFocus();

         if (new_value != default)
         {
            output = new_value;
            new_value = default;
            return true;
         }

         output = input;
         return false;
      }
   }

   public partial class IMGUI_MultiLineTextEdit : TextEdit
   {
      string new_value;

      public IMGUI_MultiLineTextEdit()
      {
         this.TextChanged += () => new_value = Text;
      }

      public override void _Input(InputEvent @event)
      {
         if (@event is not InputEventMouseButton mouse) return;
         if (mouse.ButtonIndex is not MouseButton.Left) return;
         if (mouse.Pressed && this.ContainsMouse())
            this.RemoveClickFocus();
      }

      public bool TryUpdate(string input, out string output)
      {
         if (!HasFocus())
            Text = input;

         if (new_value != default)
         {
            output = new_value;
            new_value = default;
            return true;
         }

         output = input;
         return false;
      }
   }

   partial class IMGUI_Prefix : HBoxContainer
   {
      public GUI_Element left;
      public Control resize;
      public GUI_Element right;
      public virtual float default_stretch_ratio => .5f;
      bool resizing = false;

      public override void _Input(InputEvent e)
      {
         if (e is not InputEventMouseButton mouse) return;
         switch (mouse.ButtonIndex)
         {
            case MouseButton.Left:
               if (!mouse.Pressed) resizing = false;
               if (mouse.IsPressed() && resize.ContainsMouse()) resizing = true;
               break;

            case MouseButton.Right:
               if (mouse.IsPressed() && resize.ContainsMouse())
               {
                  left.SizeFlagsStretchRatio = default_stretch_ratio;
                  right.SizeFlagsStretchRatio = 1f;
               }
               break;
         }
      }

      public void OnResize()
      {
         if (!resizing) return;
         var mouse_position = resize.GetLocalMousePosition();
         var x = GetLocalMousePosition().X.Clamp(0, Size.X);
         var ratio = x / Size.X;

         left.SizeFlagsStretchRatio = ratio;
         right.SizeFlagsStretchRatio = 1f - ratio;
      }

      public IMGUI_Prefix()
      {
         left = new()
         {
            ClipContents = true,
            SizeFlagsStretchRatio = default_stretch_ratio,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
         };
         resize = new Control
         {
            CustomMinimumSize = new(8, 0),
            SizeFlagsStretchRatio = 0,
            MouseFilter = MouseFilterEnum.Stop,
            MouseDefaultCursorShape = CursorShape.Hsize
         };
         right = new()
         {
            ClipContents = true,
            SizeFlagsStretchRatio = 1f
         };

         AddChild(left);
         AddChild(resize);
         AddChild(right);
      }
   }

   partial class IMGUI_Label : IMGUI_Prefix, IMGUI_Interface.Label
   {
      public override float default_stretch_ratio => .25f;

      int frame;
      public Label label
      {
         get
         {
            if (frame < Godot.Engine.GetFramesDrawn())
               right.Visible = false;

            OnResize();
            var label = left.GetGUIElement<Label>();
            label.ClipText = true;
            label.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            label.SizeFlagsVertical = SizeFlags.ExpandFill;
            label.VerticalAlignment = VerticalAlignment.Center;
            return label;
         }
      }

      public T GetGUIElement<T>() where T : Godot.Control, new()
      {
         frame = Godot.Engine.GetFramesDrawn() + 1;
         right.Visible = true;
         return right.GetGUIElement<T>();
      }
   }

   partial class IMGUI_TextureButton : TextureButton
   {
      public IMGUI_TextureButton()
      {
         this.ButtonDown += () => pushed = true;
         this.ButtonUp += () => this.ReleaseFocus();
         IgnoreTextureSize = true;
         StretchMode = StretchModeEnum.KeepAspect;
      }

      bool pushed;

      public bool IsPushed()
      {
         var push = pushed;
         pushed = false;
         return push;
      }
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

   sealed partial class IMGUI_Option<T> : OptionButton
   {
      Func<IEnumerable<T>> options;
      Func<T, string> format_name;
      Dictionary<long, T> index_to_data = new();
      object new_value;

      public IMGUI_Option()
      {
         ClipText = true;
         AddItem(" ");
         Selected = 0;

         this.ButtonDown += () =>
         {
            var items = options?.Invoke();
            if (items is null) return;
            Clear();
            index_to_data.Clear();
            int index = 0;

            if (format_name is null) format_name = t => t.ToString();

            foreach (var item in items)
            {
               if (item is null) continue;
               AddItem(format_name(item), index);
               index_to_data[index] = item;
               index++;
            }
            Selected = -1;
         };

         this.ItemSelected += value =>
         {
            if (index_to_data.TryGetValue(value, out var selected))
               new_value = selected;
            index_to_data.Clear();
         };
      }

      public bool TryUpdate(string label, T input, out T output, Func<IEnumerable<T>> options, Func<T, string> format_name)
      {
         TooltipText = label;
         this.options = options;
         this.format_name = format_name;

         if (new_value is not null)
         {
            output = (T)new_value;
            new_value = default;
            return true;
         }

         Text = label;
         output = input;
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
         Set("theme_override_styles/panel", new StyleBoxFlat { BgColor = Colors.White });

         var margins = new MarginContainer { };
         margins.SizeFlagsHorizontal = SizeFlags.Fill;
         margins.SizeFlagsVertical = SizeFlags.Fill;
         SelfModulate = new Color(.25f, .25f, .25f, 1f);


         float margin_size = 2;
         margins.Set("theme_override_constants/margin_left", margin_size + 2);
         margins.Set("theme_override_constants/margin_right", margin_size + 2);
         margins.Set("theme_override_constants/margin_top", margin_size);
         margins.Set("theme_override_constants/margin_bottom", margin_size);
         AddChild(margins);

         container = new IMGUI_VBoxContainer();
         margins.AddChild(container);
      }

      public T GetGUIElement<T>() where T : Godot.Control, new() => ((IMGUI_Interface)container).GetGUIElement<T>();
      IMGUI_VBoxContainer container;
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
      IMGUI_Interface gui = new IMGUI_VBoxContainer() { LayoutDirection = LayoutDirectionEnum.Ltr };
      public PropertyDrawer()
      {
         SizeFlagsHorizontal = SizeFlags.ExpandFill;
         this.AddChild(gui as Godot.Node);
      }

      /// <summary>
      /// returns true when drawer values were updated
      /// </summary>
      public bool Update(Type type, object input, out object output, bool collapsable)
      {
         output = input;
         var updated = false;

         switch (input)
         {
            case null when type == typeof(string):
               input = "";
               break;
         }

         switch (input)
         {
            case null:
               gui.Label("null");
               return false;

            case bool bool_val:
               {
                  updated = gui.CheckButton(bool_val, out bool_val);
                  output = bool_val;
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
                  updated = gui.SpinBox(System.Convert.ToDouble(double_val), out var new_value, 0.0001);
                  output = new_value;
                  return updated;
               }
            case string str_val:
               {
                  updated = gui.TextEdit(str_val, out var new_value);
                  output = new_value;
                  return updated;
               }
            case System.Guid guid_val:
               {
                  updated = gui.TextEdit(guid_val.ToString(), out var new_value);
                  if (updated)
                  {
                     if (System.Guid.TryParse(new_value, out var new_guid))
                     {
                        output = new_guid;
                        return true;
                     }
                  }
                  return false;
               }
            case System.Enum:
               {
                  var values = System.Enum.GetValues(input.GetType()) as System.Collections.IEnumerable;
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
                     if (hbox.Label("min").SetTooltip("Min Shown Index").Property(drawer.min, out drawer.min))
                        drawer.min = drawer.min < 0 ? 0 : drawer.min;
                     if (hbox.Label("max").SetTooltip("Max Shown Items").Property(drawer.max, out drawer.max))
                        drawer.max = drawer.max < 1 ? 1 : drawer.max;

                     drawer.VerticalSeparator();
                     int render_count = 0, index = drawer.min;
                     while (render_count < drawer.max && index < list.Count)
                     {
                        if (drawer.Label(index).Property(list[index], out var new_value))
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

            case System.Collections.IDictionary dic:
               {
                  var drawer = gui.GetGUIElement<IMGUI_Property>();
                  if (drawer.Button(dic.Count, "values"))
                     drawer.show = !drawer.show;
                  if (drawer.show)
                  {
                     drawer.HBox(out var hbox);
                     if (hbox.Label("max").SetTooltip("Max Shown Items").Property(drawer.max, out drawer.max))
                        drawer.max = drawer.max < 1 ? 1 : drawer.max;
                     drawer.Label("Search Key").Property(ref drawer.search);
                     int count = 0;
                     object new_key = default, new_value = default;
                     foreach (var key in dic.Keys)
                     {
                        if (!key.ToString().Contains(drawer.search, StringComparison.OrdinalIgnoreCase))
                           continue;
                        if (count++ > drawer.max) break;
                        if (drawer.Label(key).Property(dic[key], out new_value))
                        {
                           new_key = key;
                           break;
                        }
                     }
                     if (new_key != null)
                        dic[new_key] = new_value;


                  }
                  return updated;
               }

            case System.Collections.IEnumerable enumerable:
               {
                  var drawer = gui.GetGUIElement<IMGUI_Property>();
                  if (drawer.Button("Show"))
                     drawer.show = !drawer.show;
                  if (drawer.show)
                  {
                     drawer.HBox(out var hbox);
                     if (hbox.Label("max").SetTooltip("Max Shown Items").Property(drawer.max, out drawer.max))
                        drawer.max = drawer.max < 1 ? 1 : drawer.max;
                     int count = 0;
                     foreach (var item in enumerable)
                     {
                        if (count++ > drawer.max) break;
                        drawer.Label(count).Label(item);
                     }
                  }
               }
               return updated;

            default: // default drawer only draws public fields
               {
                  var drawer = gui.GetGUIElement<IMGUI_Property>();
                  if (collapsable && drawer.Button(input.ToString()))
                     drawer.show = !drawer.show;

                  if (!collapsable || drawer.show)
                  {
                     foreach (var field in input.GetType().GetFields())
                     {
                        if (drawer.Label(field.Name).Property(field.FieldType, field.GetValue(input), out var field_value))
                        {
                           updated = true;
                           field.SetValue(output, field_value);
                        }
                     }
                  }
                  return updated;
               }
         }
      }

      partial class IMGUI_Property : Internal.IMGUI.IMGUI_PanelContainer
      {
         public int min, max = 1000;
         public bool show;
         public string search = "";
      }
   }

   static class Extensions
   {
      public static bool ContainsMouse(this Godot.Control control)
      {
         var size = control.Size;
         var mouse_pos = control.GetLocalMousePosition();
         return mouse_pos.X > 0
            && mouse_pos.X < size.X
            && mouse_pos.Y > 0
            && mouse_pos.Y < size.Y;
      }

      public static float Clamp(this float target, float min, float max)
         => target < min ? min : target > max ? max : target;

      // hack to easily remove click focus
      public static void RemoveClickFocus(this Control control)
      {
         var visible = control.Visible;
         control.Visible = false;
         control.Visible = visible;
      }

      public static Vector2I ToVec2i(this Vector2 target)
         => new Vector2I(Mathf.RoundToInt(target.X), Mathf.RoundToInt(target.Y));

      public static T SetParent<T>(this T node, Godot.Node parent) where T : Node
      {
         parent.AddChild(node);
         return node;
      }
   }
}
