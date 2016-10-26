﻿using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Buffers
{
    public sealed class OwnedArray<T> : OwnedMemory<T>
    {
        public new T[] Array => base.Array;

        public static implicit operator T[](OwnedArray<T> owner) {
            return owner.Array;
        }

        public static implicit operator OwnedArray<T>(T[] array) {
            return new OwnedArray<T>(array);
        }

        public OwnedArray(int length) : base(new T[length], 0, length)
        {}

        public OwnedArray(T[] array) : base(array, 0, array.Length)
        {}
    }

    public class OwnedNativeMemory : OwnedMemory<byte>
    {
        public OwnedNativeMemory(int length) : this(length, Marshal.AllocHGlobal(length))
        { }

        protected OwnedNativeMemory(int length, IntPtr address) : base(null, 0, length, address) { }

        public static implicit operator IntPtr(OwnedNativeMemory owner)
        {
            unsafe
            {
                return new IntPtr(owner.Pointer);
            }
        }

        ~OwnedNativeMemory()
        {
            Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (base.Pointer != IntPtr.Zero) {
                Marshal.FreeHGlobal(base.Pointer);
            }
            base.Dispose(disposing);
        }

        public new unsafe byte* Pointer => (byte*)base.Pointer.ToPointer();
    }

    // This is to support secnarios today covered by Memory<T> in corefxlab
    public class OwnedPinnedArray<T> : OwnedMemory<T>
    {
        private GCHandle _handle;

        public unsafe OwnedPinnedArray(T[] array, void* pointer, GCHandle handle = default(GCHandle)) :
            base(array, 0, array.Length, new IntPtr(pointer))
        {
            var computedPointer = new IntPtr(Unsafe.AsPointer(ref Array[0]));
            if (computedPointer != new IntPtr(pointer)) {
                throw new InvalidOperationException();
            }
            _handle = handle;
        }

        public unsafe OwnedPinnedArray(T[] array) : base(array, 0, array.Length, IntPtr.Zero)
        {
            _handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            base.Pointer = _handle.AddrOfPinnedObject();
        }

        public static implicit operator OwnedPinnedArray<T>(T[] array)
        {
            return new OwnedPinnedArray<T>(array);
        }

        public new unsafe byte* Pointer => (byte*)base.Pointer.ToPointer();
        public new T[] Array => base.Array;

        public unsafe static implicit operator IntPtr(OwnedPinnedArray<T> owner)
        {
            return new IntPtr(owner.Pointer);
        }

        public static implicit operator T[] (OwnedPinnedArray<T> owner)
        {
            return owner.Array;
        }

        protected override void Dispose(bool disposing)
        {
            if (_handle.IsAllocated) {
                _handle.Free();
            }
            base.Dispose(disposing);
        }
    }

    internal class OwnerEmptyMemory<T> : OwnedMemory<T>
    {
        public readonly static OwnedMemory<T> Shared = new OwnerEmptyMemory<T>();
        readonly static T[] s_empty = new T[0];

        public OwnerEmptyMemory() : base(s_empty, 0, 0) { }

        protected override void Dispose(bool disposing)
        {}
    }
}