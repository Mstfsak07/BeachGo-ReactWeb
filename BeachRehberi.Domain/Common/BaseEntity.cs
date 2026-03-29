namespace BeachRehberi.Domain.Common;

public interface ITenantEntity
{
    int? TenantId { get; set; }
}

public abstract class BaseEntity : ITenantEntity
{
    public int Id { get; protected set; }
    public int? TenantId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
