namespace FlightParser.Pdf;

internal sealed record PageWord(string Text, double Top, double Right, double Bottom, double Left)
{
    public double Height => Top - Bottom;
}
