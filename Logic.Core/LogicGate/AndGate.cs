using System.Linq;

namespace Logic.Core.LogicGate
{
    public class AndGate : Gate
    {
        public AndGate()
        {
            GenerateIO();
        }

        public override void UpdateInput()
        {
            newOutput = inputs.GetRange(0, currentInputs).All(input => input.State);
        }
    }
}
