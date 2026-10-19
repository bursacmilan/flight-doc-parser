using FlightParser.Pdf;

namespace FlightParser.Parsing.PageClassifiers;

internal interface IPageClassifier
{
    bool IsMatch(PageContent page);
}
