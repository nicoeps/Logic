using System.Linq;

namespace Logic.Core.LogicGate
{
    public class OrGate : Gate
    {
        public OrGate()
        {
            GenerateIO();
        }

        public override void UpdateInput()
        {
            newOutput = inputs.GetRange(0, currentInputs).Any(input => input.State);
        }
    }
}
