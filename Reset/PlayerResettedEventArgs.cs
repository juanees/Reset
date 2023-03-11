namespace Reset
{
    internal class PlayerResettedEventArgs
    {
        public string Name { get; set; }
        public int Resets { get; set; }
        public int ResetAttempts { get; set; }
    }
}
