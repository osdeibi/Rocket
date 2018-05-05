﻿using System;
using System.Collections.Generic;
using System.IO;
using Rocket.API;
using Rocket.API.Configuration;
using Rocket.API.DependencyInjection;
using Rocket.API.Eventing;
using Rocket.API.I18N;
using Rocket.API.Logging;
using Rocket.API.Plugins;
using Rocket.Core.Configuration;
using Rocket.Core.Plugins.Events;

namespace Rocket.Core.Plugins
{
    public abstract class Plugin : IPlugin, ITranslatable, IConfigurable
    {
        protected Plugin(IDependencyContainer container) : this(null, container) { }

        protected Plugin(string name, IDependencyContainer container)
        {
            Name = name ?? GetType().Name;
            Container = container.CreateChildContainer();
        }

        protected IDependencyContainer Container { get; }
        protected IEventManager EventManager => Container.Resolve<IEventManager>();
        protected ILogger Logger => Container.Resolve<ILogger>();

        protected virtual IPluginManager PluginManager => Container.Resolve<IPluginManager>("default_plugins");

        protected IRuntime Runtime => Container.Resolve<IRuntime>();

        protected IImplementation Implementation => Container.Resolve<IImplementation>();
        public IConfiguration Configuration { get; protected set; }

        public virtual object DefaultConfiguration
            => null;

        public string Name { get; }

        public virtual string WorkingDirectory { get; set; }

        public bool Activate()
        {
            if (IsAlive)
                return false;

            Logger.LogInformation($"Loading {Name}.");

            try
            {
                Load(false);
                IsAlive = true;
            }
            catch(Exception ex)
            {
                Logger.LogFatal($"Failed to load {Name}: ", ex);
            }
            return true;
        }

        public bool Deactivate()
        {
            if (!IsAlive)
                return false;

            Logger.LogInformation($"Unloading {Name}.");
            
            if (EventManager != null)
            {
                PluginDeactivateEvent loadedEvent = new PluginDeactivateEvent(PluginManager, this);
                EventManager.Emit(Runtime, loadedEvent);
            }
            try
            {
                OnDeactivate();
            }
            catch (Exception ex)
            {
                Logger.LogFatal($"An error occured on unloading {Name}: ", ex);
            }
    
            IsAlive = false;

            if (EventManager != null)
            {
                PluginDeactivatedEvent loadedEvent = new PluginDeactivatedEvent(PluginManager, this);
                EventManager.Emit(Runtime, loadedEvent);
            }

            return true;
        }

        public void Reload()
        {
            Deactivate();
            Configuration?.Reload();
            Translations?.Reload();
            Load(true);
        }

        public bool IsAlive { get; internal set; }

        public ITranslationLocator Translations { get; protected set; }
        public virtual Dictionary<string, string> DefaultTranslations => null;

        public void Load(bool isReload)
        {
            WorkingDirectory = Path.Combine(Path.Combine(Implementation.WorkingDirectory, "Plugins"), Name);

            if (EventManager != null)
            {
                PluginActivateEvent activateEvent = new PluginActivateEvent(PluginManager, this);
                EventManager.Emit(Runtime, activateEvent);
                if (activateEvent.IsCancelled)
                    return;
            }

            if (!Directory.Exists(WorkingDirectory))
                Directory.CreateDirectory(WorkingDirectory);

            if (DefaultConfiguration != null)
            {
                Configuration = Container.Resolve<IConfiguration>();
                Configuration.ConfigurationContext = new ConfigurationContext
                {
                    WorkingDirectory = WorkingDirectory,
                    ConfigurationName = Name + ".Configuration"
                }; 
                Configuration.Load(DefaultConfiguration);
            }

            if (DefaultTranslations != null)
            {
                Translations = Container.Resolve<ITranslationLocator>();
                Translations.Load(new ConfigurationContext
                {
                    WorkingDirectory = WorkingDirectory,
                    ConfigurationName = Name + ".Translations"
                }, DefaultTranslations);
            }

            OnActivate(isReload);
            IsAlive = true;

            if (EventManager != null)
            {
                PluginActivatedEvent activatedEvent = new PluginActivatedEvent(PluginManager, this);
                EventManager.Emit(Runtime, activatedEvent);
            }
        }

        public void RegisterCommandsFromObject(object o)
        {
            PluginManager p = PluginManager as PluginManager;
            p?.RegisterCommands(this, o);
        }

        protected virtual void OnActivate(bool isFromReload) { }
        protected virtual void OnDeactivate() { }

        public void Subscribe(IEventListener listener)
        {
            EventManager.AddEventListener(this, listener);
        }

        public void Subscribe<T>(EventCallback<T> callback) where T : IEvent
        {
            EventManager.Subscribe(this, callback);
        }

        public void Subscribe(string eventName, EventCallback callback)
        {
            EventManager.Subscribe(this, eventName, callback);
        }

        public void Emit(IEvent @event)
        {
            EventManager.Emit(this, @event);
        }
    }
}