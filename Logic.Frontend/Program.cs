namespace Logic.Frontend
{
    public static class Program
    {
        public static void Main()
        {
            using (var LW = new LogicWindow())
            {
                LW.Run();
            }
        }
    }
}
