namespace Logic.Core.LogicGate
{
    public class Toggle : Gate
    {
        public Toggle()
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
