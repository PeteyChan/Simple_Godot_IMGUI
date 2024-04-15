using Godot;
using System.Collections.Generic;
using Internal.IMGUI;

[Tool, GlobalClass]
public sealed partial class IMGUI_VBoxContainer : VBoxContainer, IMGUI_Interface
{
   public IMGUI_VBoxContainer()
   {
      SizeFlagsHorizontal = SizeFlags.ExpandFill;
      ClipContents = true;
   }

   public override sealed void _Process(double delta)
   {
      if (element_count == 0)
      {
         Visible = false;
         return;
      }
      Visible = true;
      int i = 0;
      for (; i < element_count; ++i)
         elements[i].Visible = true;

      var new_active_count = i;
      for (; i < active_count; ++i)
         elements[i].Visible = false;
      element_count = 0;
      active_count = new_active_count;
   }

   List<GUI_Element> elements = new List<GUI_Element>();
   int element_count, active_count;
   T IMGUI_Interface.GetGUIElement<T>()
   {
      if (element_count == elements.Count)
         elements.Add(new GUI_Element().SetParent(this));
      element_count++;
      return elements[element_count - 1].GetGUIElement<T>();
   }
}