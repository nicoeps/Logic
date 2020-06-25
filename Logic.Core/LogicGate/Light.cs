namespace Logic.Core.LogicGate
{
    public class Light : Gate
    {
        public Light()
        {
            minInputs = 1;
            maxInputs = 1;
            GenerateIO();
        }

        public override void UpdateInput()
        {
            newOutput = inputs[0].State;
        }
    }
}
