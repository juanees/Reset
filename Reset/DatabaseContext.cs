using Microsoft.EntityFrameworkCore;

public class DatabaseContext : DbContext
{
  public DbSet<Reset> Resets { get; set; }
  public DbSet<Event> Events { get; set; }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    optionsBuilder.UseSqlite(@"Data Source=LocalDatabase.db");
  }
}

public class Reset
{
  public int Id { get; set; }
  public string CharacterName { get; set; }
  public int ResetNumber { get; set; }
  public int ResetAttempts { get; set; }
  public DateTimeOffset At { get; set; }

  public Reset(string characterName, int resetNumber, int resetAttempts, DateTimeOffset at)
  {
    CharacterName = characterName;
    ResetNumber = resetNumber;
    ResetAttempts = resetAttempts;
    At = at;
  }

  public Reset()
  {
  }
}

public enum EventType
{
  Error = 0,
  Level = 1,
  Reset = 2,
  NotFound = 3,
  Found = 4
}

public class Event
{
  public int Id { get; set; }
  public EventType Type { get; set; }
  public string CharacterName { get; set; }
  public string Message { get; set; }
  public DateTimeOffset At { get; set; }
  public Event( EventType type, string characterName, string message, DateTimeOffset at)
  {
    Type = type;
    CharacterName = characterName;
    Message = message;
    At = at;
  }

  public Event()
  {
  }
}
