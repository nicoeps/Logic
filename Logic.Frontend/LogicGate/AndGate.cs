using System.Numerics;
using ImGuiNET;

namespace Logic.Frontend.LogicGate
{
    public class AndGate : Core.LogicGate.AndGate, IDraw
    {

        public int ID { get; }
        public Vector2 Pos { get; set; }
        public Vector2 Size { get; set; }

        public AndGate(int id, Vector2 pos)
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
            Vector2 gatePos = offset + Pos;
            Vector2 p1 = gatePos;
            Vector2 p2 = new Vector2(gatePos.X, gatePos.Y + Size.Y);
            Vector2 p3 = new Vector2(gatePos.X + Size.X / 2, gatePos.Y);
            Vector2 p4 = new Vector2(gatePos.X + Size.X / 2, gatePos.Y + Size.Y);
            drawList.PathLineTo(p4);
            drawList.PathLineTo(p2);
            drawList.PathLineTo(p1);
            drawList.PathLineTo(p3);
            drawList.PathArcTo(gatePos + (Size / 2), Size.X / 2, -0.5f * System.MathF.PI, 0.5f * System.MathF.PI);
            drawList.PathFillConvex(col);
            drawList.PathLineTo(p4);
            drawList.PathLineTo(p2);
            drawList.PathLineTo(p1);
            drawList.PathLineTo(p3);
            drawList.PathArcTo(gatePos + (Size / 2), Size.X / 2, -0.5f * System.MathF.PI, 0.5f * System.MathF.PI);
            drawList.PathStroke(outline, false, 4.0f);
        }
    }
}
