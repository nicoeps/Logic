using System.Numerics;
using ImGuiNET;

namespace Logic.Frontend.LogicGate
{
    public interface IDraw
    {
        int ID { get; }
        Vector2 Pos { get; set; }
        Vector2 Size { get; set; }
        public Vector2 InputPosition(int index);
        public Vector2 OutputPosition(int index);

        public void Draw(ImDrawListPtr drawList, Vector2 offset, uint col, uint outline);
    }
}
