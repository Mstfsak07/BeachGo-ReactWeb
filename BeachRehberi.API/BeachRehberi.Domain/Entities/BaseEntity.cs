using System;

namespace BeachRehberi.Domain.Entities;

/// <summary>
/// Multi-tenant yapı için temel entity sınıfı.
/// Tüm entity'ler bu sınıftan türetilmelidir.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; protected set; }
    public Guid TenantId { get; protected set; }
    public bool IsDeleted { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public string? CreatedBy { get; protected set; }
    public string? UpdatedBy { get; protected set; }

    protected BaseEntity()
    {
        TenantId = Guid.Empty; // Default tenant, runtime'da set edilecek
        CreatedAt = DateTime.UtcNow;
        IsDeleted = false;
    }

    protected BaseEntity(Guid tenantId)
    {
        TenantId = tenantId;
        CreatedAt = DateTime.UtcNow;
        IsDeleted = false;
    }

    public void SetTenantId(Guid tenantId)
    {
        if (TenantId != Guid.Empty)
            throw new InvalidOperationException("TenantId cannot be changed once set.");

        TenantId = tenantId;
    }

    public void MarkAsDeleted(string? deletedBy = null)
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = deletedBy;
    }

    public void MarkAsUpdated(string? updatedBy = null)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
}