using System.Collections.Generic;
using Internal.IMGUI;

[Godot.Tool]
public sealed partial class IMGUI_GridContainer : Godot.GridContainer, IMGUI_Interface
{
    public IMGUI_GridContainer()
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
        for (; i < elements.Count; ++i)
            elements[i].Visible = false;
        element_count = 0;
    }

    List<GUI_Element> elements = new List<GUI_Element>();
    int element_count;
    T IMGUI_Interface.GetGUIElement<T>()
    {
        if (element_count == elements.Count)
            elements.Add(new GUI_Element().SetParent(this));
        element_count++;
        return elements[element_count - 1].GetGUIElement<T>();
    }
}