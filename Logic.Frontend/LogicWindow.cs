using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Timers;
using ImGuiNET;
using Logic.Core.IO;
using Logic.Core.LogicGate;
using Logic.Frontend.Binding;
using Logic.Frontend.LogicGate;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AndGate = Logic.Frontend.LogicGate.AndGate;
using Clock = Logic.Frontend.LogicGate.Clock;
using Light = Logic.Frontend.LogicGate.Light;
using NandGate = Logic.Frontend.LogicGate.NandGate;
using NorGate = Logic.Frontend.LogicGate.NorGate;
using NotGate = Logic.Frontend.LogicGate.NotGate;
using Num = System.Numerics;
using OrGate = Logic.Frontend.LogicGate.OrGate;
using Toggle = Logic.Frontend.LogicGate.Toggle;
using XnorGate = Logic.Frontend.LogicGate.XnorGate;
using XorGate = Logic.Frontend.LogicGate.XorGate;

namespace Logic.Frontend
{
    public sealed class LogicWindow : Game
    {
        private GraphicsDeviceManager _graphics;
        private ImGuiRenderer _imGuiRenderer;

        private Texture2D _xnaTexture;
        private IntPtr _imGuiTexture;

        private static Timer aTimer;

        public LogicWindow()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1024,
                PreferredBackBufferHeight = 768,
                PreferMultiSampling = true
            };

            IsMouseVisible = true;

            // D Latch example
            Clock clock1 = new Clock(0, new Num.Vector2(64, 192));
            Toggle toggle1 = new Toggle(1, new Num.Vector2(64, 320));
            NotGate not1 = new NotGate(2, new Num.Vector2(224, 128));
            AndGate and1 = new AndGate(3, new Num.Vector2(384, 192));
            AndGate and2 = new AndGate(4, new Num.Vector2(384, 320));
            NorGate nor1 = new NorGate(5, new Num.Vector2(512, 208));
            NorGate nor2 = new NorGate(6, new Num.Vector2(512, 304));
            Light light1 = new Light(7, new Num.Vector2(640, 208));
            Light light2 = new Light(8, new Num.Vector2(640, 304));

            not1.SetInput(0, toggle1);
            and1.SetInput(0, not1);
            and1.SetInput(1, clock1);
            and2.SetInput(0, clock1);
            and2.SetInput(1, toggle1);
            nor1.SetInput(0, and1);
            nor1.SetInput(1, nor2);
            nor2.SetInput(0, nor1);
            nor2.SetInput(1, and2);
            light1.SetInput(0, nor1);
            light2.SetInput(0, nor2);

            Gates.Add(0, clock1);
            Gates.Add(1, toggle1);
            Gates.Add(2, not1);
            Gates.Add(3, and1);
            Gates.Add(4, and2);
            Gates.Add(5, nor1);
            Gates.Add(6, nor2);
            Gates.Add(7, light1);
            Gates.Add(8, light2);

            SetTimer();
        }

        protected override void Initialize()
        {
            _imGuiRenderer = new ImGuiRenderer(this);
            _imGuiRenderer.RebuildFontAtlas();

            Window.Title = "Logic";
            Window.AllowUserResizing = true;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _xnaTexture = CreateTexture(GraphicsDevice, 300, 150, pixel =>
            {
                int red = (pixel % 300) / 2;
                return new Color(red, 1, 1);
            });

            _imGuiTexture = _imGuiRenderer.BindTexture(_xnaTexture);

            base.LoadContent();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(clearColor.X, clearColor.Y, clearColor.Z));

            _imGuiRenderer.BeforeLayout(gameTime);

            ImGuiLayout();

            _imGuiRenderer.AfterLayout();

            base.Draw(gameTime);
        }

        private readonly Num.Vector3 clearColor = new Num.Vector3(114f / 255f, 144f / 255f, 154f / 255f);

        private GateType TypeSelected = GateType.NOT;
        private static Dictionary<int, IDraw> Gates = new Dictionary<int, IDraw>();
        private Num.Vector2 Scrolling = Num.Vector2.Zero;
        private bool ShowGrid = true;
        private int GateSelected = -1;
        private (int ID, int index) InputSelected = (-1, -1);
        private Gate OutputSelected = null;

        private void ImGuiLayout()
        {
            ImGui.Begin("Logic",
                ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar);

            ImGui.SetWindowSize(new Num.Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));
            ImGui.SetWindowPos(Num.Vector2.Zero);

            ImGuiIOPtr io = ImGui.GetIO();

            bool openContextMenu = false;
            bool openGateMenu = false;
            bool openInputMenu = false;
            bool openOutputMenu = false;
            int gateHoveredInScene = -1;

            // Draw list of selectable gates
            ImGui.BeginChild("GateList", new Num.Vector2(100, 0));
            ImGui.Text("Gates");
            ImGui.Separator();
            foreach(GateType gate in Enum.GetValues(typeof(GateType)))
            {
                ImGui.PushID((int) gate);
                if (ImGui.Selectable(gate.ToString(), gate == TypeSelected))
                {
                    TypeSelected = gate;
                }
                ImGui.PopID();
            }
            ImGui.EndChild();

            ImGui.SameLine();
            ImGui.BeginGroup();

            ImGui.Text($"Hold middle mouse button to scroll {Scrolling.X:F2},{Scrolling.Y:F2}");
            ImGui.SameLine(ImGui.GetWindowWidth() - 352);
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Converters = { new GateDictionaryConverter() }, WriteIndented = true
            };
            if (ImGui.Button("Save"))
            {
                string jsonString = JsonSerializer.Serialize(Gates, typeof(Dictionary<int, IDraw>), options);
                File.WriteAllText("gates.json", jsonString);
            }
            ImGui.SameLine();
            if (ImGui.Button("Load"))
            {
                if (File.Exists("gates.json"))
                {
                    string jsonString = File.ReadAllText("gates.json");
                    Dictionary<int, IDraw> gates = JsonSerializer.Deserialize<Dictionary<int, IDraw>>(jsonString, options);
                    Gates = gates;
                }

            }
            ImGui.SameLine();
            if (ImGui.Button("Clear")) { Gates.Clear(); }
            ImGui.SameLine();
            ImGui.Checkbox("Show grid", ref ShowGrid);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Num.Vector2.One);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Num.Vector2.Zero);
            ImGui.PushStyleColor(ImGuiCol.ChildBg, new Num.Vector4(60f / 255f, 60f / 255f, 70f / 255f, 200f / 255f));
            ImGui.BeginChild("ScrollingRegion", Num.Vector2.Zero, true,
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoMove);
            ImGui.PopStyleVar();
            ImGui.PushItemWidth(120.0f);

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            // Display grid
            if (ShowGrid)
            {
                uint gridColor = ImGui.ColorConvertFloat4ToU32(new Num.Vector4(200f / 255f, 200f / 255f, 200f / 255f, 40f / 255f));
                const float gridSize = 64.0f;
                Num.Vector2 winPos = ImGui.GetCursorScreenPos();
                Num.Vector2 canvasSize = ImGui.GetWindowSize();
                for (float x = Scrolling.X % gridSize; x < canvasSize.X; x += gridSize)
                {
                    drawList.AddLine(new Num.Vector2(x, 0.0f) + winPos,
                        new Num.Vector2(x, canvasSize.Y) + winPos, gridColor);
                }

                for (float y = Scrolling.Y % gridSize; y < canvasSize.Y; y += gridSize)
                {
                    drawList.AddLine(new Num.Vector2(0.0f, y) + winPos,
                        new Num.Vector2(canvasSize.X, y) + winPos, gridColor);
                }
            }

            Num.Vector2 offset = ImGui.GetCursorScreenPos() + Scrolling;

            // Display connections
            drawList.ChannelsSplit(2);
            drawList.ChannelsSetCurrent(0); // Background

            uint green = ImGui.ColorConvertFloat4ToU32(new Num.Vector4(0f / 255f, 255f / 255f, 0f / 255f, 255 / 255f));
            uint outline = ImGui.ColorConvertFloat4ToU32(new Num.Vector4(100f / 255f, 100f / 255f, 100f / 255f, 255f / 255f));

            if (InputSelected.ID != -1 && InputSelected.index != -1 && OutputSelected == null)
            {
                if (Gates.TryGetValue(InputSelected.ID, out IDraw gate))
                {
                    Num.Vector2 p2 = offset + gate.InputPosition(InputSelected.index);
                    Num.Vector2 p1 = ImGui.GetMousePos();
                    drawList.AddBezierCurve(p1, p1 + new Num.Vector2(50, 0), p2 + new Num.Vector2(-50, 0), p2, outline, 4.0f);
                }
            }
            else if (InputSelected.ID == -1 && InputSelected.index == -1 && OutputSelected != null)
            {
                Num.Vector2 p1 = offset + ((IDraw) OutputSelected).OutputPosition(0);
                Num.Vector2 p2 = ImGui.GetMousePos();
                drawList.AddBezierCurve(p1, p1 + new Num.Vector2(50, 0), p2 + new Num.Vector2(-50, 0), p2, outline, 4.0f);
            }
            else if (InputSelected.ID != -1 && InputSelected.index != -1 && OutputSelected != null)
            {
                if (Gates.TryGetValue(InputSelected.ID, out IDraw gate))
                {
                    ((Gate) gate).SetInput(InputSelected.index, OutputSelected);
                    InputSelected = (-1, -1);
                    OutputSelected = null;
                }
            }

            foreach (Gate gate in Gates.Values.Cast<Gate>())
            {
                for (int i = 0; i < gate.currentInputs; ++i)
                {
                    if (gate.inputs[i].output == null) continue;
                    Num.Vector2 p2 = offset + ((IDraw) gate).InputPosition(i);
                    Num.Vector2 p1 = offset + ((IDraw) gate.inputs[i].output).OutputPosition(0);
                    drawList.AddBezierCurve(p1, p1 + new Num.Vector2(50, 0), p2 + new Num.Vector2(-50, 0), p2,
                        gate.inputs[i].output.GetOutput(0) ? green : outline, 4.0f);
                }
            }

            // Display gates
            foreach (IDraw gate in Gates.Values)
            {
                ImGui.PushID(gate.ID);

                drawList.ChannelsSetCurrent(1);
                bool oldAnyActive = ImGui.IsAnyItemActive();
                ImGui.SetCursorScreenPos(offset + gate.Pos);
                ImGui.BeginGroup(); // Lock horizontal position
                // Inputs
                for (int index = 0; index < ((Gate) gate).currentInputs; ++index)
                {
                    ImGui.SetCursorScreenPos(offset + gate.InputPosition(index) - new Num.Vector2(0, 4));
                    ImGui.InvisibleButton("Input", new Num.Vector2(16, 8));
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        InputSelected.ID = gate.ID;
                        InputSelected.index = index;
                    }
                    else if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        InputSelected.ID = gate.ID;
                        InputSelected.index = index;
                        openInputMenu = true;
                    }
                }

                // Outputs
                for (int index = 0; index < 1; ++index)
                {
                    ImGui.SetCursorScreenPos(offset + gate.OutputPosition(index) - new Num.Vector2(gate.Size.X / 4, 4));
                    ImGui.InvisibleButton("Output", new Num.Vector2(16, 8));
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        OutputSelected = (Gate) gate;
                    }
                    else if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        OutputSelected = (Gate) gate;
                        openOutputMenu = true;
                    }
                }
                ImGui.EndGroup();

                ImGui.SetCursorScreenPos(offset + gate.Pos);

                // Save the size of what we have emitted and whether any of the widgets are being used
                bool gateWidgetsActive = (!oldAnyActive && ImGui.IsAnyItemActive());

                // Display gate box
                drawList.ChannelsSetCurrent(0); // Background
                ImGui.SetCursorScreenPos(offset + gate.Pos);
                ImGui.InvisibleButton("Gate", gate.Size);
                if (ImGui.IsItemHovered())
                {
                    gateHoveredInScene = gate.ID;
                    openGateMenu |= ImGui.IsMouseClicked(ImGuiMouseButton.Right);
                    if (gate is Toggle toggle && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        toggle.output = !toggle.output;
                    }
                }
                bool gateMovingActive = ImGui.IsItemActive();
                if (gateWidgetsActive || gateMovingActive)
                {
                    GateSelected = gate.ID;
                }

                if (gateMovingActive && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
                {
                    gate.Pos += io.MouseDelta;
                }

                uint gateBGColor = (gateHoveredInScene == gate.ID || (GateSelected == gate.ID))
                    ? ImGui.ColorConvertFloat4ToU32(new Num.Vector4(75f / 255f, 75f / 255f, 75f / 255f, 255f / 255f))
                    : ImGui.ColorConvertFloat4ToU32(new Num.Vector4(60f / 255f, 60f / 255f, 60f / 255f, 255f / 255f));
                gate.Draw(drawList, offset, gateBGColor, outline);

                // Inputs
                for (int index = 0; index < ((Gate) gate).currentInputs; ++index)
                {
                    drawList.AddLine(offset + gate.InputPosition(index),
                        offset + gate.InputPosition(index) + new Num.Vector2(gate.Size.X / 4, 0),
                        outline, 4.0f);
                }

                // Outputs
                for (int index = 0; index < 1; ++index)
                {
                    drawList.AddLine(offset + gate.OutputPosition(index),
                        offset + gate.OutputPosition(index) - new Num.Vector2(gate.Size.X / 4, 0),
                        outline, 4.0f);
                }

                ImGui.PopID();
            }
            drawList.ChannelsMerge();

            // Open context menu
            if (!ImGui.IsAnyItemHovered())
            {
                if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                {
                    GateSelected = gateHoveredInScene = -1;
                    InputSelected = (-1, -1);
                    OutputSelected = null;
                    openContextMenu = true;
                }
                else if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    GateSelected = gateHoveredInScene = -1;
                    InputSelected = (-1, -1);
                    OutputSelected = null;
                }
            }

            if (openGateMenu)
            {
                GateSelected = gateHoveredInScene;
                ImGui.OpenPopup("GateMenu");
            }
            else if (openContextMenu)
            {
                ImGui.OpenPopup("ContextMenu");
            }
            else if (openInputMenu)
            {
                ImGui.OpenPopup("InputMenu");
            }
            else if (openOutputMenu)
            {
                ImGui.OpenPopup("OutputMenu");
            }

            // Draw gate menu
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Num.Vector2(8, 8));
            if (ImGui.BeginPopup("GateMenu"))
            {
                IDraw gate = GateSelected != -1 ? Gates[GateSelected] : null;
                if (gate != null)
                {
                    string type = gate switch
                    {
                        NotGate _ => "NOT",
                        AndGate _ => "AND",
                        NandGate _ => "NAND",
                        OrGate _ => "OR",
                        NorGate _ => "NOR",
                        XorGate _ => "XOR",
                        XnorGate _ => "XNOR",
                        Light _ => "Light",
                        Toggle _ => "Toggle Switch",
                        Clock _ => "Clock",
                        _ => "Error"
                    };
                    ImGui.Text(type);
                    ImGui.Separator();

                    if (gate is Toggle toggle)
                    {
                        if (ImGui.MenuItem("Toggle"))
                        {
                            toggle.output = !toggle.output;
                        }
                    }

                    if (ImGui.MenuItem("Delete"))
                    {
                        foreach (Gate gateInput in Gates.Values.Cast<Gate>())
                        {
                            foreach (Input input in gateInput.inputs.Where(input => ((IDraw) input.output)?.ID == gate.ID))
                            {
                                input.output = null;
                            }
                        }

                        Gates.Remove(gate.ID);
                    }

                    if (ImGui.MenuItem("Copy", null, false, false))
                    {
                    }

                    if (((Gate) gate).minInputs != ((Gate) gate).maxInputs)
                    {
                        if (ImGui.MenuItem("Increase Inputs"))
                        {
                            ((Gate) gate).IncreaseInputs();
                        }

                        if (ImGui.MenuItem("Decrease Inputs"))
                        {
                            ((Gate) gate).DecreaseInputs();
                        }
                    }
                }
                ImGui.EndPopup();
            }

            // Draw context menu
            if (ImGui.BeginPopup("ContextMenu"))
            {
                if (ImGui.MenuItem("Add"))
                {
                    Num.Vector2 scenePos = ImGui.GetMousePosOnOpeningCurrentPopup() - offset;
                    int id = Gates.Count > 0 ? Gates.Keys.Max() + 1 : 0;
                    switch (TypeSelected)
                    {
                        case GateType.NOT:
                            Gates.Add(id, new NotGate(id, scenePos));
                            break;
                        case GateType.AND:
                            Gates.Add(id, new AndGate(id, scenePos));
                            break;
                        case GateType.NAND:
                            Gates.Add(id, new NandGate(id, scenePos));
                            break;
                        case GateType.OR:
                            Gates.Add(id, new OrGate(id, scenePos));
                            break;
                        case GateType.NOR:
                            Gates.Add(id, new NorGate(id, scenePos));
                            break;
                        case GateType.XOR:
                            Gates.Add(id, new XorGate(id, scenePos));
                            break;
                        case GateType.XNOR:
                            Gates.Add(id, new XnorGate(id, scenePos));
                            break;
                        case GateType.Light:
                            Gates.Add(id, new Light(id, scenePos));
                            break;
                        case GateType.Toggle:
                            Gates.Add(id, new Toggle(id, scenePos));
                            break;
                        case GateType.Clock:
                            Gates.Add(id, new Clock(id, scenePos));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (ImGui.MenuItem("Paste", null, false, false))
                {
                }

                ImGui.EndPopup();
            }

            if (ImGui.BeginPopup("InputMenu"))
            {
                if (ImGui.MenuItem("Delete"))
                {
                    if (Gates.TryGetValue(InputSelected.ID, out IDraw gate))
                    {
                        ((Gate) gate).SetInput(InputSelected.index, null);
                    }
                    InputSelected = (-1, -1);
                    OutputSelected = null;
                }
                ImGui.EndPopup();
            }

            if (ImGui.BeginPopup("OutputMenu"))
            {
                if (ImGui.MenuItem("Delete"))
                {
                    foreach (Gate gateInput in Gates.Values.Cast<Gate>())
                    {
                        foreach (Input input in gateInput.inputs.Where(input => ((IDraw) input.output)?.ID == ((IDraw) OutputSelected)?.ID))
                        {
                            input.output = null;
                        }
                    }
                    InputSelected = (-1, -1);
                    OutputSelected = null;
                }
                ImGui.EndPopup();
            }

            ImGui.PopStyleVar();

            // Scrolling
            if (ImGui.IsWindowHovered() && !ImGui.IsAnyItemActive() &&
                ImGui.IsMouseDragging(ImGuiMouseButton.Middle, 0.0f))
            {
                Scrolling += io.MouseDelta;
            }

            ImGui.PopItemWidth();
            ImGui.EndChild();
            ImGui.PopStyleColor();
            ImGui.PopStyleVar();
            ImGui.EndGroup();

            ImGui.End();
        }

        private static Texture2D CreateTexture(GraphicsDevice device, int width, int height, Func<int, Color> paint)
        {
            Texture2D texture = new Texture2D(device, width, height);

            Color[] data = new Color[width * height];
            for(int pixel = 0; pixel < data.Length; ++pixel)
            {
                data[pixel] = paint(pixel);
            }

            texture.SetData(data);

            return texture;
        }

        private static void SetTimer()
        {
            aTimer = new Timer(100);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            foreach (Clock gate in Gates.Values.OfType<Clock>())
            {
                gate.output = !gate.output;
            }
            foreach (ILogic logic in Gates.Values.Cast<ILogic>())
            {
                logic.UpdateInput();
                logic.UpdateOutput();
            }
        }
    }
}
