using System.Threading;

namespace AssetsTools.NET.Atomic.Helper
{
    public struct AtomicLong
    {
        private long internalLong;

        public long Get() => Volatile.Read(ref internalLong);
        public void Write(long value) => Volatile.Write(ref internalLong, value);
    }
}
