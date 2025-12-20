namespace CustomerAgreements.Options;

public class FeatureFlagsOptions
{
    public bool UseApiForAgreementLoad { get; set; }
    public bool UseApiForAgreementSave { get; set; }
    public bool AllowDbFallback { get; set; } = true;
}
