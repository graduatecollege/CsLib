namespace Grad.CsLib.Options;

public class Cors
{
    public const string SectionName = "CORS";
    
    public string[] AllowedOrigins { get; set; } = null!;
    public string[] AllowedMethods { get; set; } = null!;
    public string[] AllowedHeaders { get; set; } = null!;
}

