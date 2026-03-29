using BeachRehberi.Application.Common;
using BeachRehberi.Application.DTOs;

namespace BeachRehberi.Application.Commands.Beaches;

/// <summary>
/// Create beach command
/// </summary>
public class CreateBeachCommand : CommandBase<BeachDto>
{
    public CreateBeachDto Beach { get; set; } = null!;
}

/// <summary>
/// Update beach command
/// </summary>
public class UpdateBeachCommand : CommandBase<BeachDto>
{
    public int Id { get; set; }
    public UpdateBeachDto Beach { get; set; } = null!;
}

/// <summary>
/// Delete beach command
/// </summary>
public class DeleteBeachCommand : CommandBase
{
    public int Id { get; set; }
}

/// <summary>
/// Update beach occupancy command
/// </summary>
public class UpdateBeachOccupancyCommand : CommandBase
{
    public int Id { get; set; }
    public int OccupancyPercent { get; set; }
}

/// <summary>
/// Update beach rating command
/// </summary>
public class UpdateBeachRatingCommand : CommandBase
{
    public int Id { get; set; }
}