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
	public struct HSteamUser : System.IEquatable<HSteamUser>, System.IComparable<HSteamUser> {
		public int m_HSteamUser;

		public HSteamUser(int value) {
			m_HSteamUser = value;
		}

		public override string ToString() {
			return m_HSteamUser.ToString();
		}

		public override bool Equals(object other) {
			return other is HSteamUser && this == (HSteamUser)other;
		}

		public override int GetHashCode() {
			return m_HSteamUser.GetHashCode();
		}

		public static bool operator ==(HSteamUser x, HSteamUser y) {
			return x.m_HSteamUser == y.m_HSteamUser;
		}

		public static bool operator !=(HSteamUser x, HSteamUser y) {
			return !(x == y);
		}

		public static explicit operator HSteamUser(int value) {
			return new HSteamUser(value);
		}

		public static explicit operator int(HSteamUser that) {
			return that.m_HSteamUser;
		}

		public bool Equals(HSteamUser other) {
			return m_HSteamUser == other.m_HSteamUser;
		}

		public int CompareTo(HSteamUser other) {
			return m_HSteamUser.CompareTo(other.m_HSteamUser);
		}
	}
}

#endif // !DISABLESTEAMWORKS
