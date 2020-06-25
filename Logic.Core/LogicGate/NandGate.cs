using System.Linq;

namespace Logic.Core.LogicGate
{
    public class NandGate : Gate
    {
        public NandGate()
        {
            GenerateIO();
        }

        public override void UpdateInput()
        {
            newOutput = !inputs.GetRange(0, currentInputs).All(input => input.State);
        }
    }
}
