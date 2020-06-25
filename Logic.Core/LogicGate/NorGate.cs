using System.Linq;

namespace Logic.Core.LogicGate
{
    public class NorGate : Gate
    {
        public NorGate()
        {
            GenerateIO();
        }

        public override void UpdateInput()
        {
            newOutput = !inputs.GetRange(0, currentInputs).Any(input => input.State);
        }
    }
}
