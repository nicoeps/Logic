namespace Logic.Core.IO
{
    public class Input
    {
        public IOutput output { get; set; }

        public bool State => output?.GetOutput(0) ?? false;

        public Input()
        {
            output = null;
        }
    }
}
