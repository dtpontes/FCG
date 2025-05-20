using System;
namespace FCG.Domain.Core.Events
{
    public abstract class Event : Message
    {
        public DateTime Timestamp { get; private set; }
        public bool Save { get; private set; }

        protected Event()
        {
            Timestamp = DateTime.Now;
            Save = true;
        }

        public void SetSave(bool save)
        {
            Save = save;
        }
    }
}
