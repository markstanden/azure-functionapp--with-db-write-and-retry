using interview.Sanitation;
using JetBrains.Annotations;

namespace AddShipmentNotification.Tests.Unit.SanitationTest;

[TestSubject(typeof(Sanitation))]
public class SanitationTest
{
    [Fact]
    public void SanitationAlphaNumericsOnly_WithMixedAlphaNumericSpecialArgument_RemovesNonAlphaNumericCharacters()
    {
        var sut = new Sanitation();
        const string unsanitisedString = "!@#$%^&*()_+=SHIP!@#$%^&*()_+=12345!@#$%^&*()_+=";
        const string expected = "SHIP12345";

        var result = sut.AlphaNumericsOnly(unsanitisedString);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void SanitationAlphaNumericsWithSpecialCharacters_WithAlphaNumericArgument_RemainsUnaltered()
    {
        var sut = new Sanitation();
        const string unsanitisedString = "SHIP12345";
        const string expected = "SHIP12345";

        var result = sut.AlphaNumericsWithSpecialCharacters(unsanitisedString, []);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void SanitationAlphaNumericsWithSpecialCharacters_WithAlphaOnlyArgument_RemainsUnaltered()
    {
        var sut = new Sanitation();
        const string unsanitisedString = "SHIP";
        const string expected = "SHIP";

        var result = sut.AlphaNumericsWithSpecialCharacters(unsanitisedString, []);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void SanitationAlphaNumericsWithSpecialCharacters_WithNumericOnlyArgument_RemainsUnaltered()
    {
        var sut = new Sanitation();
        const string unsanitisedString = "12345";
        const string expected = "12345";

        var result = sut.AlphaNumericsWithSpecialCharacters(unsanitisedString, []);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void SanitationAlphaNumericsWithSpecialCharacters_WithEmptyStringArgument_ReturnsEmptyString()
    {
        var sut = new Sanitation();
        const string unsanitisedString = "";
        const string expected = "";

        var result = sut.AlphaNumericsWithSpecialCharacters(unsanitisedString, []);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void SanitationAlphaNumericsWithSpecialCharacters_WithSpaceOnlyArgument_ReturnsEmptyString()
    {
        var sut = new Sanitation();
        const string unsanitisedString = " ";
        const string expected = "";

        var result = sut.AlphaNumericsWithSpecialCharacters(unsanitisedString, []);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void SanitationAlphaNumericsWithSpecialCharacters_WithNonAlphaNumericArgument_ReturnsEmptyString()
    {
        var sut = new Sanitation();
        const string unsanitisedString = "#";
        const string expected = "";

        var result = sut.AlphaNumericsWithSpecialCharacters(unsanitisedString, []);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void SanitationAlphaNumericsWithSpecialCharacters_WithMultipleNonAlphaNumericArgument_ReturnsEmptyString()
    {
        var sut = new Sanitation();
        const string unsanitisedString = "!@#$%^&*()_+={}[]|:;\'\"\\/?";
        const string expected = "";

        var result = sut.AlphaNumericsWithSpecialCharacters(unsanitisedString, []);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void SanitationAlphaNumericsWithSpecialCharacters_WithMixedAlphaNumericSpecialArgument_RemovesNonAlphaNumericCharacters()
    {
        var sut = new Sanitation();
        const string unsanitisedString = "SHIP12345!@#$%^&*()_+=";
        const string expected = "SHIP12345";

        var result = sut.AlphaNumericsWithSpecialCharacters(unsanitisedString, []);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void SanitationAlphaNumericsWithSpecialCharacters_WithSpecialCharArgument_RemovesNonAlphaNumericCharacters()
    {
        var sut = new Sanitation();
        const string unsanitisedString = "!@#$%^&*()_+=SHIP!@#$%^&*()_+=12345!@#$%^&*()_+=";
        const string expected = "!SHIP!12345!";

        var result = sut.AlphaNumericsWithSpecialCharacters(unsanitisedString, ['!']);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void SanitationAlphaNumericsWithSpecialCharacters_WithMultipleSpecialCharArgument_RemovesNonAlphaNumericCharacters()
    {
        var sut = new Sanitation();
        const string unsanitisedString = "!@#$%^&*()_+=SHIP!@#$%^&*()_+=12345!@#$%^&*()_+=";
        const string expected = "!@#$%^&*()_+=SHIP!@#$%^&*()_+=12345!@#$%^&*()_+=";

        var result = sut.AlphaNumericsWithSpecialCharacters(
            unsanitisedString,
            ['!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '_', '+', '=']
        );

        Assert.Equal(expected, result);
    }
}
