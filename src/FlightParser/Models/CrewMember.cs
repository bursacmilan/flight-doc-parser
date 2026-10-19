namespace FlightParser.Models;

/// <summary>
/// A member of the operating crew of a flight.
/// </summary>
/// <param name="Name">The full name, e.g. <c>Max Mustermann</c>.</param>
/// <param name="Function">
/// The function code as printed in the Crew Briefing, e.g. <c>CMD</c> (commander), <c>COP</c> (copilot),
/// <c>CAB</c> (cabin attendant) or <c>SEN</c> (senior cabin attendant). Unknown codes are passed through unchanged.
/// </param>
public sealed record CrewMember(string Name, string Function);
