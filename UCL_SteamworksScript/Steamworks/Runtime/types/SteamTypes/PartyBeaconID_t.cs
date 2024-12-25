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
	public struct PartyBeaconID_t : System.IEquatable<PartyBeaconID_t>, System.IComparable<PartyBeaconID_t> {
		public static readonly PartyBeaconID_t Invalid = new PartyBeaconID_t(0);
		public ulong m_PartyBeaconID;

		public PartyBeaconID_t(ulong value) {
			m_PartyBeaconID = value;
		}

		public override string ToString() {
			return m_PartyBeaconID.ToString();
		}

		public override bool Equals(object other) {
			return other is PartyBeaconID_t && this == (PartyBeaconID_t)other;
		}

		public override int GetHashCode() {
			return m_PartyBeaconID.GetHashCode();
		}

		public static bool operator ==(PartyBeaconID_t x, PartyBeaconID_t y) {
			return x.m_PartyBeaconID == y.m_PartyBeaconID;
		}

		public static bool operator !=(PartyBeaconID_t x, PartyBeaconID_t y) {
			return !(x == y);
		}

		public static explicit operator PartyBeaconID_t(ulong value) {
			return new PartyBeaconID_t(value);
		}

		public static explicit operator ulong(PartyBeaconID_t that) {
			return that.m_PartyBeaconID;
		}

		public bool Equals(PartyBeaconID_t other) {
			return m_PartyBeaconID == other.m_PartyBeaconID;
		}

		public int CompareTo(PartyBeaconID_t other) {
			return m_PartyBeaconID.CompareTo(other.m_PartyBeaconID);
		}
	}
}

#endif // !DISABLESTEAMWORKS
