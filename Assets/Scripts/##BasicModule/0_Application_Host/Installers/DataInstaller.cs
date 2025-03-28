using VContainer;
using VContainer.Unity;
// using Unity.Assets.Scripts.Data;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine;
using Unity.Assets.Scripts.Data;
using Mono.Cecil;
using Unity.Assets.Scripts.Objects;

namespace Unity.Assets.Scripts.Module.ApplicationLifecycle.Installers
{



    public class DataInstaller : IModuleInstaller
    {
        public ModuleType ModuleType => ModuleType.GameData;


        public void Install(IContainerBuilder builder)
        {

            builder.Register<DataLoader>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<RemoteDataRepository>(Lifetime.Singleton).As<IDataRepository>();
            builder.Register<UserDataManager>(Lifetime.Singleton);
            builder.Register<CurrencyManager>(Lifetime.Singleton); 
            builder.Register<GameDataManager>(Lifetime.Singleton);
            
        }
    

    }
}
