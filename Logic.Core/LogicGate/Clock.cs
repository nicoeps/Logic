namespace Logic.Core.LogicGate
{
    public class Clock : Gate
    {
        public Clock()
        {
            minInputs = 0;
            maxInputs = 0;
            GenerateIO();
        }

        public override void UpdateInput()
        {
            newOutput = output;
        }
    }
}
