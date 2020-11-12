using System.IO;

namespace CDPHGenServices
{
    public interface ICcdToFhireService
    {
        string ConvertCcdToFhir(string ccd);

        string ConvertCcdToFhir(Stream ccd, bool resetStreamPosition = true);
    }
}