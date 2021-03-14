using System;
using System.Reactive.Subjects;
using Wabbajack.App.Screens;

namespace Wabbajack.App.Services
{
    public class EventRouter
    {
        public IObservable<Event> Events => _events;
        private Subject<Event> _events = new();
        public EventRouter()
        {
            
        }

        public void NavigateTo<T>()
        {
            _events.OnNext(new NavigateToEvent {Screen = typeof(T)});
        }
    }

    public class Event
    {
        
    }

    public class NavigateToEvent : Event
    {
        public Type Screen { get; set; } = typeof(ModeSelection);
    }
}
