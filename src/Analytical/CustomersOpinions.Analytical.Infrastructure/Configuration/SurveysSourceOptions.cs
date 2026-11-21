using System.ComponentModel.DataAnnotations;

namespace CustomersOpinions.Analytical.Infrastructure.Configuration;

/// <summary>Section <c>Sources:Surveys</c>.</summary>
public sealed class SurveysSourceOptions
{
    public const string SectionName = "Sources:Surveys";

    public bool Enabled { get; set; } = true;

    /// <summary>Survey file; a relative path is resolved against <see cref="AppContext.BaseDirectory"/>.</summary>
    [Required(AllowEmptyStrings = false)]
    public string FilePath { get; set; } = "Data/surveys_part1.csv";

    /// <summary>Absolute path of <see cref="FilePath"/>.</summary>
    public string ResolveFilePath() =>
        Path.IsPathRooted(FilePath) ? FilePath : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, FilePath));
}
