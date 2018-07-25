using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace app.model
{
    public interface IEvented<T>
    {
        Guid Subscribe(AppEventType appEventType, Func<AppEventArgs<T>, Task> handler);
        void UnSubscribe(Guid handlerID);

        void OnEvent(AppEventType appEventType,  AppEventArgs<T> e);
        
        void OnAnyEvent(AppEventArgs<T> e);

    }

}