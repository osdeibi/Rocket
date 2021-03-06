﻿using System;
using System.IO;
using System.Linq;
using Rocket.API.Commands;
using Rocket.API.Configuration;
using Rocket.API.User;
using Rocket.Core.Configuration;
using Rocket.Core.User;

namespace Rocket.Core.Commands.RocketCommands
{
    public class CommandMigrateConfig : ICommand
    {
        public string Name => "MigrateConfig";
        public string[] Aliases => null;
        public string Summary => "Migrates configs from one type to another.";
        public string Description => null;
        public string Syntax => "[<from type> <to type> <path>]";
        public IChildCommand[] ChildCommands { get; }

        public bool SupportsUser(UserType type) => type == UserType.Console;

        public void Execute(ICommandContext context)
        {
            if (context.Parameters.Length != 0 && context.Parameters.Length < 3) throw new CommandWrongUsageException();

            IConfiguration[] configProviders = context.Container.ResolveAll<IConfiguration>().ToArray();

            if (context.Parameters.Length == 0)
            {
                context.User.SendMessage(GetConfigTypes(configProviders));
                context.SendCommandUsage();
                return;
            }

            string from = context.Parameters.Get<string>(0);
            string to = context.Parameters.Get<string>(1);
            string path = context.Parameters.GetArgumentLine(2);

            if (from.Equals(to, StringComparison.OrdinalIgnoreCase))
            {
                context.User.SendMessage("\"from\" and \"to\" can not be the same config type!");
                return;
            }

            IConfiguration fromProvider =
                configProviders.FirstOrDefault(c => c.Name.Equals(from, StringComparison.OrdinalIgnoreCase));

            if (fromProvider == null)
                throw new CommandWrongUsageException($"\"{from}\" is not a valid config type. "
                    + GetConfigTypes(configProviders));

            IConfiguration toProvider =
                configProviders.FirstOrDefault(c => c.Name.Equals(to, StringComparison.OrdinalIgnoreCase));

            if (toProvider == null)
                throw new CommandWrongUsageException($"\"{to}\" is not a valid config type. "
                    + GetConfigTypes(configProviders));

            string workingDir = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            ConfigurationContext cc = new ConfigurationContext(workingDir, fileName);

            fromProvider.Load(cc);
            toProvider.ConfigurationContext = cc;
            toProvider.LoadEmpty();

            CopyConfigElement(fromProvider, toProvider);

            toProvider.Save();

            context.User.SendMessage("Configuration was successfully migrated.");
        }

        private void CopyConfigElement(IConfigurationElement fromSection, IConfigurationElement toSection)
        {
            foreach (IConfigurationSection fromChild in fromSection.GetChildren())
            {
                IConfigurationSection toChild = toSection.CreateSection(fromChild.Key, fromChild.Type);

                if (fromChild.Type != SectionType.Object)
                    toChild.Set(fromChild.Get());
                else
                    CopyConfigElement(fromChild, toChild);
            }
        }

        private string GetConfigTypes(IConfiguration[] configProviders)
        {
            return "Available config types: " + string.Join(", ", configProviders.Select(c => c.Name).ToArray());
        }
    }
}