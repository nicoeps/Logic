using System.Linq;

namespace Logic.Core.LogicGate
{
    public class XnorGate : Gate
    {
        public XnorGate()
        {
            GenerateIO();
        }

        public override void UpdateInput()
        {
            newOutput = !inputs.GetRange(0, currentInputs)
                .Select(input => input.State).Aggregate((a, b) => a ^ b );
        }
    }
}
