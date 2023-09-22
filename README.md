# Simple Godot IMGUI
A fairly basic Immediate mode GUI for godot similar to IMGUI.
The standard godot nodes are too much of a pain when all you want is to mock up a basic ui, so I created this.
I've tried to keep many of the function names similar to their node counterparts.

## Installation
Just drop the IMGUI folder into your godot project. Requires Godot4 with .Net.

## Basic Usage
```C#
using Godot;

public partial class Test : Node
{
    enum Selection { A, B, C }
    public override void _Ready()
    {
        bool toggle = default;
        float some_value = default;
        string some_text = default;
        (int, double) tuple = default;
        Selection selection = default;

        // IMGUI window inherit's from the godot window node
        // by default when you close the window it will free itself
        // you can change that behaviour with OnClose()
        var window = new IMGUI_Window();
        // adds window to the current scene
        // alternatively you can just call AddChild(window);
        window.AddToScene();
        window.Title = "IMGUI Test";

        // OnProcess is just a way to update the window every frame
        // without having to override _Process()
        // makes it easy to write fire and forget guis
        // for instance when you push a button
        window.OnProcess(() =>
        {
            // Draws a button, returns true when pressed
            if (window.Button("Button"))
            { }

            // Draws a label
            window.Label("Label");

            // You can chain other IMGUI functions after label
            // to make them a labelled version
            if (window.Label("Label").Button("Button"))
            { }

            // labels come with a couple other functions prefixed
            // with set that lets you change some of it's properties
            window.Label("My Label").SetColor(Colors.Cyan);

            // functions that modify values will follow more or less the
            // same pattern
            // - have both an input and output value
            // - returns true if the value was updated

            // draws the godot LineEdit
            if (window.TextEdit(some_text, out some_text))
            { }

            // multi-line text edit
            if (window.MultiLineTextEdit(some_text, out some_text))
            { }

            // draws the godot spin box
            if (window.SpinBox(some_value, out some_value))
            { }

            // will draw a vertical separator, has a horizontal counterpart
            window.VerticalSeparator();

            // draws a godot HBoxContainter which you can then add components to
            // there are various different container types but they all work basically the same
            window.HBox(out var hbox);

            // draws a check button
            if (hbox.CheckButton(toggle, out toggle))
            { }

            // draws a check box
            if (hbox.CheckBox(toggle, out toggle))
            { }

            // property will work on most data types
            // only draws public fields with structs and classes
            // output can be customized by implementing the IMGUI_PropertyDrawer
            // interface on the target class
            if (window.Property(tuple, out tuple))
            { }

            // draws the godot option button
            // when pressed a dropdown menu will show all possible selections
            // when used with enums, will show all possible enums by default
            if (window.Option(selection, out selection))
            { }

            //  draws tabs
            // current only impleneted with enums, the other tabs will show all possible enums
            if (window.Tabs(selection, out selection))
            { }
        });
    }
}
```
## Result
![image](https://github.com/PeteyChan/Simple_Godot_IMGUI/assets/21060636/5aa9796d-fe4a-4538-895d-5af914c44418)
