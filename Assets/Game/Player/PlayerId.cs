using System;

namespace Game
{
    public class PlayerId
    {
        private string _value;

        public static PlayerId Random()
        {
            return new PlayerId(Guid.NewGuid().ToString());
        }

        public PlayerId(string value)
        {
            _value = value;
        }

        public PlayerId(Photon.Realtime.Player player)
        {
            _value = player.UserId ?? player.ActorNumber.ToString();
        }

        public override string ToString()
        {
            return _value;
        }

        protected bool Equals(PlayerId other)
        {
            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PlayerId) obj);
        }

        public override int GetHashCode()
        {
            return (_value != null ? _value.GetHashCode() : 0);
        }
    }
}