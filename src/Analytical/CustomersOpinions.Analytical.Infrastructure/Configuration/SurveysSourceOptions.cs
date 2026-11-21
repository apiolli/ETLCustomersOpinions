using System.ComponentModel.DataAnnotations;

namespace CustomersOpinions.Analytical.Infrastructure.Configuration;

public sealed class SurveysSourceOptions
{
    public const string SectionName = "Sources:Surveys";

    public bool Enabled { get; set; } = true;

    [Required(AllowEmptyStrings = false)]
    public string FilePath { get; set; } = "Data/surveys_part1.csv";

    public string ResolveFilePath() =>
        Path.IsPathRooted(FilePath) ? FilePath : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, FilePath));
}
