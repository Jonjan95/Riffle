public static class CheckEntry
{
    public static int Main()
    {
        try { RiffleCreek.Editor.SimulationChecks.Run();RiffleCreek.Editor.ProgressionChecks.Run();return 0; }
        catch(System.Exception ex) {System.Console.Error.WriteLine(ex);return 1;}
    }
}
