public static class CommandFactory
{
    public static ICommand GetCommand(string key)
    {
        return key switch
        {
            "Cmd_Attack_Burn" => new Cmd_Attack_Burn(),
            "Cmd_Detonate" => new Cmd_Detonate(),
            _ => null
        };
    }
}
