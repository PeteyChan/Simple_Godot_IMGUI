using Godot;
using System;
using Internal.IMGUI;

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
   ColorRect color_rect;

   public IMGUI_Window MoveToMousePosition()
   {
      Position = (Position + GetMousePosition()).ToVec2i();
      return this;
   }

   public Color BackgroundColor
   {
      get => color_rect.Color;
      set => color_rect.Color = value;
   }

   public IMGUI_Window() => SetupWindow(new Vector2I(128, 128));
   void SetupWindow(Vector2I position)
   {
      color_rect = new ColorRect();
      AddChild(color_rect);
      color_rect.AnchorRight = 1;
      color_rect.AnchorBottom = 1;
      color_rect.Color = new Color(.2f, .2f, .2f, 1);


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

      Transparent = false;
      TransparentBg = false;

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
      InitialPosition = WindowInitialPosition.Absolute;

      Position = new Vector2I(128, 256);
      Size = new Vector2I(320, 480);
   }

   Action on_close, on_process, on_enter_tree, on_exit_tree;
   T IMGUI_Interface.GetGUIElement<T>() => Contents.GetGUIElement<T>();

   public IMGUI_Window OnEnterTree(Action action)
   {
      on_enter_tree = action;
      return this;
   }

   public IMGUI_Window OnExitTree(Action action)
   {
      on_exit_tree = action;
      return this;
   }

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

         case Node.NotificationEnterTree:
            on_enter_tree?.Invoke();
            break;

         case Node.NotificationExitTree:
            on_exit_tree?.Invoke();
            break;
      }
   }

   /// <summary>
   /// replaces current OnClose event with action, by default closing the window destroys the node
   /// </summary>
   public IMGUI_Window OnCloseWindow(Action<IMGUI_Window> action) => this.OnCloseWindow(() => action(this));

   /// <summary>
   /// returns true if mouse is within the current window 
   /// </summary>
   public bool ContainsMouse()
   {
      if (Borderless) return GetVisibleRect().HasPoint(GetMousePosition());

      var decoration_title_height = 40;
      var decoration_border_size = 8;

      var rect = GetVisibleRect();
      var pos = rect.Position;
      pos.Y -= decoration_title_height;
      pos.X -= decoration_border_size;
      rect.Position = pos;
      var size = rect.Size;
      size.Y += decoration_title_height + decoration_border_size;
      size.X += decoration_border_size * 2;
      rect.Size = size;
      return rect.HasPoint(GetMousePosition());
   }

   /// <summary>
   /// Adds window as a child to the current scene node in the Scene Tree.
   /// Must not already have a parent node.
   /// </summary>
   public IMGUI_Window AddToCurrentScene()
   {
      (Godot.Engine.GetMainLoop() as SceneTree).CurrentScene.AddChild(this);
      return this;
   }

   /// <summary>
   /// Adds window as a child to the root node in the Scene Tree.
   /// Must not already have a parent node.
   /// </summary>
   public IMGUI_Window AddToRootNode()
   {
      (Godot.Engine.GetMainLoop() as SceneTree).Root.AddChild(this);
      return this;
   }
}