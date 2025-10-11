namespace SearchMS.Settings
{
    public class AISearchSettings
    {
        public const string SectionName = "AzureAISearch";
 
        public string Endpoint { get; set; } = string.Empty;    
        public string ApiKey { get; set; } = string.Empty;
        public string IndexName { get; set; } = string.Empty;
    }
}