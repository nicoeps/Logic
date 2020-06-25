using System.Numerics;
using ImGuiNET;

namespace Logic.Frontend.LogicGate
{
    public class NorGate : Core.LogicGate.NorGate, IDraw
    {

        public int ID { get; }
        public Vector2 Pos { get; set; }
        public Vector2 Size { get; set; }

        public NorGate(int id, Vector2 pos)
        {
            ID = id;
            Pos = pos;
            Size = new Vector2(64, 64);
        }

        public Vector2 InputPosition(int index)
        {
            return new Vector2(Pos.X - Size.X / 10, Pos.Y + Size.Y * ((float) index + 1) / ((float) currentInputs + 1));
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
            Vector2 p2 = new Vector2(gatePos.X, gatePos.Y + Size.Y);
            Vector2 p3 = new Vector2(gatePos.X + Size.X / 4, gatePos.Y);
            Vector2 p4 = new Vector2(gatePos.X + Size.X / 4, gatePos.Y + Size.Y);
            Vector2 p5 = new Vector2(gateSize.X, gatePos.Y + (gateSize.Y - gatePos.Y) / 2);
            Vector2 p6 = new Vector2(gatePos.X + Size.X / 4.5f, gatePos.Y + Size.Y / 2);
            drawList.PathLineTo(p6);
            drawList.PathLineTo(p1);
            drawList.PathBezierCurveTo(p1, new Vector2(p1.X + Size.X / 2, p1.Y), p5);
            drawList.PathBezierCurveTo(new Vector2(p2.X + Size.X / 2, p2.Y), p2, p2);
            drawList.PathLineTo(p6);
            drawList.PathFillConvex(col);
            drawList.PathLineTo(p2);
            drawList.PathBezierCurveTo(p4, p3, p1);
            drawList.PathBezierCurveTo(p1, new Vector2(p1.X + Size.X / 2, p1.Y), p5);
            drawList.PathBezierCurveTo(new Vector2(p2.X + Size.X / 2, p2.Y), p2, p2);
            drawList.PathStroke(outline, false, 4.0f);
            drawList.AddCircleFilled(p5 + new Vector2(6, 0), 6, col);
            drawList.AddCircle(p5 + new Vector2(6, 0), 6, outline, 12, 4.0f);
        }
    }
}
