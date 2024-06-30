using System.Runtime.CompilerServices;
using Verse;

namespace RimWorldAddXColonistsMod
{
    static class Logger
    {
        private const string ModIdentifier = "RimWorldAddXColonistsMod";

        public static void LogMessage(string message, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerName = "")
        {
            string className = System.IO.Path.GetFileNameWithoutExtension(callerFilePath);
            Log.Message($"{ModIdentifier}.{className}.{callerName} {message}");
        }
    }
}
