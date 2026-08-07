using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<DataDragonState> DataDragonState => Set<DataDragonState>();
    public DbSet<Summoner> Summoners => Set<Summoner>();
    public DbSet<Queue> Queues => Set<Queue>();
    public DbSet<Champion> Champions => Set<Champion>();
    public DbSet<SummonerSpell> SummonerSpells => Set<SummonerSpell>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Rune> Runes => Set<Rune>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchReference> MatchReferences => Set<MatchReference>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<ParticipantFrame> ParticipantFrames => Set<ParticipantFrame>();
    public DbSet<Event> Events => Set<Event>();

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Summoner>()
            .HasIndex(s => s.Puuid)
            .IsUnique();

        modelBuilder.Entity<Champion>()
            .HasIndex(c => c.Key)
            .IsUnique();

        modelBuilder.Entity<SummonerSpell>()
            .HasIndex(s => s.Key)
            .IsUnique();

        modelBuilder.Entity<Item>()
            .HasIndex(i => i.Key)
            .IsUnique();

        modelBuilder.Entity<MatchReference>()
            .HasIndex(m => m.MatchId)
            .IsUnique();

        modelBuilder.Entity<Queue>()
            .Property(q => q.Type)
            .HasConversion<string>();

        modelBuilder.Entity<Match>()
            .Property(m => m.QueueType)
            .HasConversion<string>();

        modelBuilder.Entity<Event>()
            .Property(e => e.Type)
            .HasConversion<string>();

        modelBuilder.Entity<Event>()
            .Property(e => e.TowerType)
            .HasConversion<string>();

        modelBuilder.Entity<Event>()
            .Property(e => e.BuildingType)
            .HasConversion<string>();

        modelBuilder.Entity<Event>()
            .Property(e => e.LaneType)
            .HasConversion<string>();

        modelBuilder.Entity<Event>()
            .Property(e => e.MonsterType)
            .HasConversion<string>();

        modelBuilder.Entity<Event>()
            .Property(e => e.MonsterSubType)
            .HasConversion<string>();

        modelBuilder.Entity<Item>()
            .Property(i => i.Stats)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, double>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new());

        modelBuilder.Entity<Participant>()
            .HasOne(p => p.PrimaryRune)
            .WithMany()
            .HasForeignKey(p => p.PrimaryRuneId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Participant>()
            .HasOne(p => p.SecondaryTree)
            .WithMany()
            .HasForeignKey(p => p.SecondaryTreeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Participant>()
            .HasMany(p => p.SummonerSpells)
            .WithMany(s => s.Participants)
            .UsingEntity(j => j.ToTable("ParticipantSummonerSpells"));

        modelBuilder.Entity<Participant>()
            .HasMany(p => p.Items)
            .WithMany(i => i.Participants)
            .UsingEntity(j => j.ToTable("ParticipantItems"));

        modelBuilder.Entity<Event>()
            .HasMany(e => e.AssistingParticipants)
            .WithMany(p => p.Assisted)
            .UsingEntity(j => j.ToTable("EventAssists"));

        modelBuilder.Entity<Event>()
            .HasOne(e => e.Item)
            .WithMany(i => i.Events)
            .HasForeignKey(e => e.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Event>()
            .HasOne(e => e.BeforeItem)
            .WithMany(i => i.BeforeEvents)
            .HasForeignKey(e => e.BeforeItemId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Event>()
            .HasOne(e => e.AfterItem)
            .WithMany(i => i.AfterEvents)
            .HasForeignKey(e => e.AfterItemId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Event>()
            .HasOne(e => e.Participant)
            .WithMany(p => p.ParticipantInEvent)
            .HasForeignKey(e => e.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Event>()
            .HasOne(e => e.Killer)
            .WithMany(p => p.KillsAsKiller)
            .HasForeignKey(e => e.KillerParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Event>()
            .HasOne(e => e.Team)
            .WithMany(t => t.Events)
            .HasForeignKey(e => e.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Event>()
            .HasOne(e => e.Victim)
            .WithMany(p => p.DeathsAsVictim)
            .HasForeignKey(e => e.VictimParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MatchReference>()
            .HasOne(mr => mr.Match)
            .WithOne(m => m.MatchReference)
            .HasForeignKey<MatchReference>(mr => mr.MatchDbId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private void UpdateTimestamps()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}