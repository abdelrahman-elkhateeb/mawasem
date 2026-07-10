namespace Mawasem.Domain.Common.ValueObjects;

public class LocalizedText
{
    public string English { get; private set; } = string.Empty;

    public string Arabic { get; private set; } = string.Empty;

    private LocalizedText()
    {
    }

    public LocalizedText( string english , string arabic )
    {
        English = english;
        Arabic = arabic;
    }

    public void Update( string english , string arabic )
    {
        English = english;
        Arabic = arabic;
    }

    public override string ToString()
    {
        return English;
    }
}