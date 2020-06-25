using System.Collections.Generic;
using Logic.Core.IO;

namespace Logic.Core.LogicGate
{
    public abstract class Gate : ILogic, IInput, IOutput
    {
        protected void GenerateIO()
        {
            inputs = new List<Input>(maxInputs);
            for (int i = 0; i < maxInputs; ++i)
            {
                inputs.Add(new Input());
            }

            currentInputs = minInputs;
        }

        public int minInputs = 2;
        public int maxInputs = 8;
        public int currentInputs { private set; get; }

        public List<Input> inputs;
        protected bool newOutput;
        public bool output;

        public void SetInput(int index, IOutput o)
        {
            if (index >= currentInputs) return;
            inputs[index].output = o;
        }

        public bool GetOutput(int index)
        {
            return index <= 0 && output;
        }

        public void IncreaseInputs()
        {
            if (currentInputs < maxInputs)
            {
                ++currentInputs;
            }
        }

        public void DecreaseInputs()
        {
            if (currentInputs > minInputs)
            {
                --currentInputs;
            }
        }

        public abstract void UpdateInput();

        public void UpdateOutput()
        {
            output = newOutput;
        }
    }
}
