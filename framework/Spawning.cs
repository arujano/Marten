using Godot;

namespace Marten
{
    public interface ISpawnable
    {
        /// <summary>
        /// Sets this object's network owner.
        /// This method should be used to initialize `NetStorage<T>` objects.
        /// </summary>
        /// <param name="netOwner">The network owner of this object.</param>
        public abstract void SetNetOwner(NetOwner netOwner);
    }

    public partial class NetSpawner : Node
    {

    }
}