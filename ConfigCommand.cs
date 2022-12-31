public class ConfigCommand : Command
{
    public ConfigCommand()
        : base("config", "Manage configuration")
    {
        var key = new Argument<string>("key", "The config setting to manage")
        {
            HelpName = "key",
        }.FromAmong("download");
        var value = new Argument<string>("value", "The new value to set. If not supplied, existing value is displayed instead")
        {
            Arity = ArgumentArity.ZeroOrOne
        };

        this.Add(key);
        this.Add(value);

        this.SetHandler((key, value) =>
        {
            if (string.IsNullOrEmpty(value))
            {
                Console.WriteLine(Config.Get(key));
            }
            else
            {
                Config.Set(key, value);
            }
        }, key, value);
    }
}