﻿
// RCG_AutoHeader
// to change the auto header please go to RCG_AutoHeader.cs
// Create time : 12/25 2024
// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2022 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// This file is automatically generated.
// Changes to this file will be reverted when you update Steamworks.NET

#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
	#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS

using System.Runtime.InteropServices;
using IntPtr = System.IntPtr;

namespace Steamworks {
	[System.Serializable]
	public struct SNetSocket_t : System.IEquatable<SNetSocket_t>, System.IComparable<SNetSocket_t> {
		public uint m_SNetSocket;

		public SNetSocket_t(uint value) {
			m_SNetSocket = value;
		}

		public override string ToString() {
			return m_SNetSocket.ToString();
		}

		public override bool Equals(object other) {
			return other is SNetSocket_t && this == (SNetSocket_t)other;
		}

		public override int GetHashCode() {
			return m_SNetSocket.GetHashCode();
		}

		public static bool operator ==(SNetSocket_t x, SNetSocket_t y) {
			return x.m_SNetSocket == y.m_SNetSocket;
		}

		public static bool operator !=(SNetSocket_t x, SNetSocket_t y) {
			return !(x == y);
		}

		public static explicit operator SNetSocket_t(uint value) {
			return new SNetSocket_t(value);
		}

		public static explicit operator uint(SNetSocket_t that) {
			return that.m_SNetSocket;
		}

		public bool Equals(SNetSocket_t other) {
			return m_SNetSocket == other.m_SNetSocket;
		}

		public int CompareTo(SNetSocket_t other) {
			return m_SNetSocket.CompareTo(other.m_SNetSocket);
		}
	}
}

#endif // !DISABLESTEAMWORKS
