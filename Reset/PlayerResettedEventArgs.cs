namespace ResetterBot
{
    internal class PlayerResettedEventArgs : PlayerBaseEventArgs
    {
        public int Resets { get; set; }
        public int ResetAttempts { get; set; }
    }
}
