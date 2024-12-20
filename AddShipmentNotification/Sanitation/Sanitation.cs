namespace interview.Sanitation;

public class Sanitation : ISanitation
{
    /// <summary>
    /// Declarative convenience method to hard strip a string to alphanumerics only.
    /// </summary>
    /// <param name="untrustedMessage"></param>
    /// <returns></returns>
    public string AlphaNumericsOnly(string untrustedMessage)
    {
        return AlphaNumericsWithSpecialCharacters(untrustedMessage, []);
    }

    /// <summary>
    /// Declarative convenience method to hard strip a string to alphanumerics and special characters used in datetimes only.
    /// </summary>
    /// <param name="untrustedDateString"></param>
    /// <returns></returns>
    public string DateTimeOnly(string untrustedDateString)
    {
        return AlphaNumericsWithSpecialCharacters(untrustedDateString, ['-', ':']);
    }

    /// <summary>
    /// Method to clean a string to include only alphanumerics and characters contained in the provided array.
    /// </summary>
    /// <param name="untrustedMessage">The string to sanitise</param>
    /// <param name="specialCharacters">A array containing valid characters to allow into the output</param>
    /// <returns>A sanitised string containing only alphanumerics and special characters present in the provided array</returns>
    public string AlphaNumericsWithSpecialCharacters(
        string untrustedMessage,
        char[] specialCharacters
    )
    {
        // Creates a string from only alphanumerics and an array of trusted chars
        char[] result = untrustedMessage
            .Where(ch => char.IsLetterOrDigit(ch) || specialCharacters.Contains(ch))
            .ToArray();

        return new string(result);
    }
}
