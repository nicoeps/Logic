using System;

namespace Logic.Core.LogicGate
{
    public class NotGate : Gate
    {
        public NotGate()
        {
            minInputs = 1;
            maxInputs = 1;
            GenerateIO();
        }

        public override void UpdateInput()
        {
            newOutput = !inputs[0].State;
        }
    }
}
