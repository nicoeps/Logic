using System.Numerics;
using ImGuiNET;

namespace Logic.Frontend.LogicGate
{
    public class Toggle : Core.LogicGate.Toggle, IDraw
    {

        public int ID { get; }
        public Vector2 Pos { get; set; }
        public Vector2 Size { get; set; }

        public Toggle(int id, Vector2 pos)
        {
            ID = id;
            Pos = pos;
            Size = new Vector2(64, 64);
        }

        public Vector2 InputPosition(int index)
        {
            return new Vector2(Pos.X - Size.X / 4 , Pos.Y + Size.Y * ((float) index + 1) / ((float) currentInputs + 1));
        }

        public Vector2 OutputPosition(int index)
        {
            return new Vector2(Pos.X + Size.X * 1.25f, Pos.Y + Size.Y * ((float) index + 1) / 2f);
        }

        public void Draw(ImDrawListPtr drawList, Vector2 offset, uint col, uint outline)
        {
            uint green = ImGui.ColorConvertFloat4ToU32(new Vector4(0f / 255f, 255f / 255f, 0f / 255f, 255 / 255f));
            col = output ? green : col;
            Vector2 gatePos = offset + Pos;
            Vector2 gateSize = gatePos + Size;
            drawList.AddRectFilled(gatePos, gateSize, col);
            drawList.AddRect(gatePos, gateSize, outline, 4.0f, ImDrawCornerFlags.All, 4.0f);
        }
    }
}
