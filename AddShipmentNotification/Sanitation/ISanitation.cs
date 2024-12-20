namespace interview.Sanitation;

public interface ISanitation
{
    string AlphaNumericsOnly(string untrustedMessage);
    string AlphaNumericsWithSpecialCharacters(string untrustedMessage, char[] specialCharacters);
    string DateTimeOnly(string untrustedMessage);
}
