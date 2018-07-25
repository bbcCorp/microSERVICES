using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using System.Runtime.Serialization;

using app.model;

namespace app.data
{
    public class EventHandlerEntry<T> {
        public Guid EventHandlerID { get; set; }
        public AppEventType AppEventType { get; set; }

        // This should be an async Event handler
        public Func<AppEventArgs<T>, Task> EventHandler { get; set; }
        // public Action<AppEventArgs<T>> EventHandler { get; set; }

    }

    // Implementing IEvented interface for Repository
    public abstract class EventedRepository<T> : IEvented<T>
    {
        protected List<EventHandlerEntry<T>> allEventHandlers;

        public EventedRepository()
        {
            allEventHandlers = new List<EventHandlerEntry<T>>();
        }

        // 
        // All the event handlers are called asynchronously. We wait for all of them to complete
        // 
        private void __executeEventHandlers(IEnumerable<EventHandlerEntry<T>> eventhandlers, AppEventArgs<T> e)
        {
            
            List<Task> TaskList = new List<Task>();
            foreach(var eh in eventhandlers){
                TaskList.Add(  eh.EventHandler(e) );
            }      
            Task.WaitAll(TaskList.ToArray());      
        }

        public virtual void OnEvent(AppEventType appEventType, AppEventArgs<T> e)
        {
            this.__executeEventHandlers( 
                this.allEventHandlers.Where(ed => ed.AppEventType == appEventType), e);
        }

        public virtual void OnAnyEvent(AppEventArgs<T> e)
        {
            this.__executeEventHandlers(
                this.allEventHandlers.Where(ed => ed.AppEventType == AppEventType.Any), e);
        }

        // This function is used to subscribe to an event using an asynchrnous handler
        public virtual Guid Subscribe(AppEventType appEventType, Func<AppEventArgs<T>, Task> handler)
        {
            var newhandler = new EventHandlerEntry<T>();
            newhandler.EventHandlerID = Guid.NewGuid();
            newhandler.AppEventType = appEventType;
            newhandler.EventHandler = handler;

            allEventHandlers.Add(newhandler);

            return newhandler.EventHandlerID;
        }

        // Unsubscribe from event notification
        public virtual void UnSubscribe(Guid handlerID)
        {
            var handlerIdx = this.allEventHandlers.FindLastIndex(h => h.EventHandlerID == handlerID);
                       
            if(handlerIdx==-1){
                throw new ArgumentException($"Invalid handlerID:{handlerID}");
            }
            
            this.allEventHandlers.RemoveAt(handlerIdx);          
        }
    }
}
