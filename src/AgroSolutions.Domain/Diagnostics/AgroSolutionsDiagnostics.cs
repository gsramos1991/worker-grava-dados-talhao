using System.Diagnostics;

namespace AgroSolutions.Domain.Diagnostics
{
    public static class AgroSolutionsDiagnostics
    {
        public const string SourceName = "AgroSolutions.Dados.Sensor";

        public static readonly ActivitySource Source = new(SourceName, "1.0.0");
    }
}
