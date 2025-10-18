namespace price_bot.Demo_feature;

public enum Version
{
    Demo,
    Full
}

public class VersionRetreiver
{
    private readonly Version version = Version.Full;

    public Version GetVersion() 
    { 
        return version; 
    }

    public bool IsDemoVersion() 
    { 
        if (version == Version.Demo)
        {
            return true;
        }

        return false; 
    }
}