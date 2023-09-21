using Godot;
using System;

public sealed partial class IMGUI_Window : Window, IMGUI_Interface
{
    /// <summary>
    /// Adds items starting from the top of the window
    /// </summary>
    public IMGUI_Interface Header => header;
    /// <summary>
    /// Adds items between the header and footer. By default all gui elements are added here.
    /// </summary>
    public IMGUI_Interface Contents => contents;
    /// <summary>
    /// Adds items starting from the bottom of the window
    /// </summary>
    public IMGUI_Interface Footer => footer;
    IMGUI_VBoxContainer header;
    IMGUI_VBoxContainer contents;
    IMGUI_VBoxContainer footer;
    public IMGUI_Window()
    {
        on_close = () =>
        {
            if (Node.IsInstanceValid(this))
                QueueFree();
        };
        CloseRequested += () => on_close?.Invoke();
        var vbox = new VBoxContainer
        {
            AnchorRight = 1,
            AnchorBottom = 1,
            OffsetLeft = 8,
            OffsetRight = -8,
            OffsetTop = 8,
            OffsetBottom = -8,
            Name = "Immediate Window"
        };

        AddChild(vbox);
        vbox.AddChild(header = new IMGUI_VBoxContainer() { Name = "Header" });
        header.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        header.SizeFlagsVertical = Control.SizeFlags.ShrinkBegin;

        var scroll = new ScrollContainer
        {
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            Name = "Contents",
        };
        vbox.AddChild(scroll);
        contents = new IMGUI_VBoxContainer();
        scroll.AddChild(contents);
        vbox.AddChild(footer = new IMGUI_VBoxContainer() { Name = "Footer" });
        footer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        footer.SizeFlagsVertical = Control.SizeFlags.ShrinkEnd;

        Position = new Vector2I(12, 36);
        Size = new Vector2I(320, 480);
    }

    Action on_close, on_process;
    T IMGUI_Interface.GetGUIElement<T>() => Contents.GetGUIElement<T>();

    /// <summary>
    /// repaces current Process event with action
    /// </summary>
    public IMGUI_Window OnProcess(Action action)
    {
        SetProcess(true);
        on_process = action;
        return this;
    }

    /// <summary>
    /// replaces current Process event with action
    /// </summary>
    public IMGUI_Window OnProcess(Action<float> action)
    {
        OnProcess(() => action?.Invoke((float)GetProcessDeltaTime()));
        return this;
    }

    /// <summary>
    /// replaces current OnClose event with action, by default closing the window destroys the node
    /// </summary>
    public IMGUI_Window OnCloseWindow(Action action)
    {
        on_close = action;
        return this;
    }

    public override void _Notification(int what)
    {
        switch ((long)what)
        {
            case Node.NotificationProcess:
                on_process?.Invoke();
                break;
        }
    }

    /// <summary>
    /// adds window to current scene if not in tree
    /// </summary>
    public IMGUI_Window AddToScene()
    {
        if (!this.IsInsideTree())
            ((Godot.Engine.GetMainLoop()) as SceneTree).CurrentScene.CallDeferred("add_child", this);
        return this;
    }

    /// <summary>
    /// replaces current OnClose event with action, by default closing the window destroys the node
    /// </summary>
    public IMGUI_Window OnCloseWindow(Action<IMGUI_Window> action) => this.OnCloseWindow(() => action(this));
}