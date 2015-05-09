using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using ManyConsole;
using ManyConsole.Internal;
using Nerven.Immutabler.Machine;

namespace Nerven.Immutabler.Runners.Cli
{
    [PublicAPI]
    public static class Program
    {
        public static IEnumerable<ConsoleCommand> Commands => new ConsoleCommand[]
            {
                new _PatchMsbuildSolutionConsoleCommand(),
                new _PatchMsbuildProjectConsoleCommand(),
                new _InteractiveModeConsoleCommand(),
            };

        public static int Main() => ConsoleCommandDispatcher.DispatchCommand(Commands, CommandLineParser.Parse(Environment.CommandLine).Skip(1).ToArray(), Console.Out);

        private sealed class _PatchMsbuildSolutionConsoleCommand : ConsoleCommand
        {
            public _PatchMsbuildSolutionConsoleCommand()
            {
                IsCommand("patch-msbuild-solution");
                HasAdditionalArguments(1);
            }

            public override int Run(string[] remainingArguments)
            {
                return Task.Run(() => _RunAsync(remainingArguments[0])).Result;
            }

            private static async Task<int> _RunAsync(
                string solutionFile)
            {
                var _immutabler = new ImmutablerMachine();

                var _succeeded = await _immutabler.PatchMsbuildSolutionAsync(solutionFile);
                return _succeeded ? 0 : 1;
            }
        }

        private sealed class _PatchMsbuildProjectConsoleCommand : ConsoleCommand
        {
            public _PatchMsbuildProjectConsoleCommand()
            {
                IsCommand("patch-msbuild-project");
                HasAdditionalArguments(1);
            }

            public override int Run(string[] remainingArguments)
            {
                return Task.Run(() => _RunAsync(remainingArguments[0])).Result;
            }

            private static async Task<int> _RunAsync(
                string projectFile)
            {
                var _immutabler = new ImmutablerMachine();

                var _succeeded = await _immutabler.PatchMsbuildProjectAsync(projectFile);
                return _succeeded ? 0 : 1;
            }
        }

        private sealed class _InteractiveModeConsoleCommand : ConsoleCommand
        {
            public _InteractiveModeConsoleCommand()
            {
                IsCommand("interactive");
            }

            public override int Run(string[] remainingArguments)
            {
                var _exit = false;
                while (!_exit)
                {
                    ConsoleCommandDispatcher.DispatchCommand(Commands.Append(new _ExitInteractiveModeConsoleCommand(() => _exit = true)), CommandLineParser.Parse(Console.ReadLine()), Console.Out);
                }

                return 0;
            }
        }

        private sealed class _ExitInteractiveModeConsoleCommand : ConsoleCommand
        {
            private readonly Action _Exit;

            public _ExitInteractiveModeConsoleCommand(Action exit)
            {
                _Exit = exit;

                IsCommand("exit");
            }

            public override int Run(string[] remainingArguments)
            {
                _Exit();
                return 0;
            }
        }
    }
}
