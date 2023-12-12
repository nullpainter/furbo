using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace AlexaInvoker.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public record Configuration
{
    public const string ConfigurationKey = "AlexaInvoker";

    [Required]
    public string AccessKey { get; init; }

    [Required]
    public string KeywordPath { get; init; }
    
    /// <summary>
    ///     Path to the Alexa sample application shell script.
    /// </summary>
    [Required]
    public string AlexaPath { get; init; }
}