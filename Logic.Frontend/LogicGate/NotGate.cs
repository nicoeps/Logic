using System.Numerics;
using ImGuiNET;

namespace Logic.Frontend.LogicGate
{
    public class NotGate : Core.LogicGate.NotGate, IDraw
    {

        public int ID { get; }
        public Vector2 Pos { get; set; }
        public Vector2 Size { get; set; }

        public NotGate(int id, Vector2 pos)
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
            return new Vector2(Pos.X + Size.X * 1.25f + 12, Pos.Y + Size.Y * ((float) index + 1) / 2f);
        }
        public void Draw(ImDrawListPtr drawList, Vector2 offset, uint col, uint outline)
        {
            Vector2 gatePos = offset + Pos;
            Vector2 gateSize = gatePos + Size;
            Vector2 p1 = gatePos;
            Vector2 p2 = new Vector2(gatePos.X, gateSize.Y);
            Vector2 p3 = new Vector2(gateSize.X, gatePos.Y + (gateSize.Y - gatePos.Y) / 2);
            drawList.AddTriangleFilled(p1, p2, p3, col);
            drawList.AddTriangle(gatePos, new Vector2(gatePos.X, gateSize.Y), new Vector2(gateSize.X, gatePos.Y + (gateSize.Y - gatePos.Y) / 2), outline, 8.0f);
            drawList.AddCircleFilled(p3 + new Vector2(6, 0), 6, col);
            drawList.AddCircle(p3 + new Vector2(6, 0), 6, outline, 12, 4.0f);
        }
    }
}
