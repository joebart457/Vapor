
using CliParser;
using Logger;
using Vapor.Cmd;

args.ResolveWithTryCatch(new StartupService(), ex => CliLogger.LogError(ex.Message));
