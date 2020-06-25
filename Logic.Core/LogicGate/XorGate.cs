using System.Linq;

namespace Logic.Core.LogicGate
{
    public class XorGate : Gate
    {
        public XorGate()
        {
            GenerateIO();
        }

        public override void UpdateInput()
        {
            newOutput = inputs.GetRange(0, currentInputs)
                .Select(input => input.State).Aggregate((a, b) => a ^ b );
        }
    }
}
