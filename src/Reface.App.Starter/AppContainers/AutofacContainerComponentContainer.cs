﻿using Autofac;
using Reface.AppStarter.AutofacExt;
using Reface.AppStarter.Events;
using Reface.EventBus;
using System;

namespace Reface.AppStarter.AppContainers
{
    public class AutofacContainerComponentContainer : IComponentContainer
    {
        public IContainer Container { get; private set; }
        private readonly ContainerBuilder containerBuilder;
        private readonly TriggerComponentCreatingEventAutofacSource triggerComponentCreatingEventAutofacSource;

        public event EventHandler<ComponentCreatingEventArgs> ComponentCreating;

        public AutofacContainerComponentContainer(ContainerBuilder containerBuilder, TriggerComponentCreatingEventAutofacSource triggerComponentCreatingEventAutofacSource)
        {
            this.containerBuilder = containerBuilder;
            this.triggerComponentCreatingEventAutofacSource = triggerComponentCreatingEventAutofacSource;
            this.triggerComponentCreatingEventAutofacSource.ComponentCreating += (sender, e) =>
            {
                this.ComponentCreating?.Invoke(this, e);
            };
        }

        public IComponentContainer BeginScope(string scopeName)
        {
            return new LifetimescopeComponentContainer(this, this.Container.BeginLifetimeScope(scopeName));
        }

        public T CreateComponent<T>()
        {
            return this.Container.Resolve<T>();
        }

        public object CreateComponent(Type type)
        {
            return this.Container.Resolve(type);
        }

        public void Dispose()
        {
            this.Container.Dispose();
        }

        public void OnAppStarted(App app)
        {
            using (var scope = this.Container.BeginLifetimeScope("OnAppStarted"))
            {
                IEventBus eventBus = scope.Resolve<IEventBus>();
                eventBus.Publish(new AppStartedEvent(this, app));
            }
        }

        public void InjectProperties(object value)
        {
            this.Container.InjectProperties(value);
        }

        public void OnAppPrepair(App app)
        {
            this.containerBuilder.RegisterInstance(app)
                .SingleInstance();
            this.Container = containerBuilder.Build();
        }
    }
}