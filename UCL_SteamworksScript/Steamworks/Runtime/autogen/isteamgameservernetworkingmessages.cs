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
	public static class SteamGameServerNetworkingMessages {
		/// <summary>
		/// <para>/ Sends a message to the specified host.  If we don't already have a session with that user,</para>
		/// <para>/ a session is implicitly created.  There might be some handshaking that needs to happen</para>
		/// <para>/ before we can actually begin sending message data.  If this handshaking fails and we can't</para>
		/// <para>/ get through, an error will be posted via the callback SteamNetworkingMessagesSessionFailed_t.</para>
		/// <para>/ There is no notification when the operation succeeds.  (You should have the peer send a reply</para>
		/// <para>/ for this purpose.)</para>
		/// <para>/</para>
		/// <para>/ Sending a message to a host will also implicitly accept any incoming connection from that host.</para>
		/// <para>/</para>
		/// <para>/ nSendFlags is a bitmask of k_nSteamNetworkingSend_xxx options</para>
		/// <para>/</para>
		/// <para>/ nRemoteChannel is a routing number you can use to help route message to different systems.</para>
		/// <para>/ You'll have to call ReceiveMessagesOnChannel() with the same channel number in order to retrieve</para>
		/// <para>/ the data on the other end.</para>
		/// <para>/</para>
		/// <para>/ Using different channels to talk to the same user will still use the same underlying</para>
		/// <para>/ connection, saving on resources.  If you don't need this feature, use 0.</para>
		/// <para>/ Otherwise, small integers are the most efficient.</para>
		/// <para>/</para>
		/// <para>/ It is guaranteed that reliable messages to the same host on the same channel</para>
		/// <para>/ will be be received by the remote host (if they are received at all) exactly once,</para>
		/// <para>/ and in the same order that they were sent.</para>
		/// <para>/</para>
		/// <para>/ NO other order guarantees exist!  In particular, unreliable messages may be dropped,</para>
		/// <para>/ received out of order with respect to each other and with respect to reliable data,</para>
		/// <para>/ or may be received multiple times.  Messages on different channels are *not* guaranteed</para>
		/// <para>/ to be received in the order they were sent.</para>
		/// <para>/</para>
		/// <para>/ A note for those familiar with TCP/IP ports, or converting an existing codebase that</para>
		/// <para>/ opened multiple sockets:  You might notice that there is only one channel, and with</para>
		/// <para>/ TCP/IP each endpoint has a port number.  You can think of the channel number as the</para>
		/// <para>/ *destination* port.  If you need each message to also include a "source port" (so the</para>
		/// <para>/ recipient can route the reply), then just put that in your message.  That is essentially</para>
		/// <para>/ how UDP works!</para>
		/// <para>/</para>
		/// <para>/ Returns:</para>
		/// <para>/ - k_EREsultOK on success.</para>
		/// <para>/ - k_EResultNoConnection, if the session has failed or was closed by the peer and</para>
		/// <para>/   k_nSteamNetworkingSend_AutoRestartBrokenSession was not specified.  (You can</para>
		/// <para>/   use GetSessionConnectionInfo to get the details.)  In order to acknowledge the</para>
		/// <para>/   broken session and start a new one, you must call CloseSessionWithUser, or you may</para>
		/// <para>/   repeat the call with k_nSteamNetworkingSend_AutoRestartBrokenSession.  See</para>
		/// <para>/   k_nSteamNetworkingSend_AutoRestartBrokenSession for more details.</para>
		/// <para>/ - See ISteamNetworkingSockets::SendMessageToConnection for more possible return values</para>
		/// </summary>
		public static EResult SendMessageToUser(ref SteamNetworkingIdentity identityRemote, IntPtr pubData, uint cubData, int nSendFlags, int nRemoteChannel) {
			InteropHelp.TestIfAvailableGameServer();
			return NativeMethods.ISteamNetworkingMessages_SendMessageToUser(CSteamGameServerAPIContext.GetSteamNetworkingMessages(), ref identityRemote, pubData, cubData, nSendFlags, nRemoteChannel);
		}

		/// <summary>
		/// <para>/ Reads the next message that has been sent from another user via SendMessageToUser() on the given channel.</para>
		/// <para>/ Returns number of messages returned into your list.  (0 if no message are available on that channel.)</para>
		/// <para>/</para>
		/// <para>/ When you're done with the message object(s), make sure and call SteamNetworkingMessage_t::Release()!</para>
		/// </summary>
		public static int ReceiveMessagesOnChannel(int nLocalChannel, IntPtr[] ppOutMessages, int nMaxMessages) {
			InteropHelp.TestIfAvailableGameServer();
			if (ppOutMessages != null && ppOutMessages.Length != nMaxMessages) {
				throw new System.ArgumentException("ppOutMessages must be the same size as nMaxMessages!");
			}
			return NativeMethods.ISteamNetworkingMessages_ReceiveMessagesOnChannel(CSteamGameServerAPIContext.GetSteamNetworkingMessages(), nLocalChannel, ppOutMessages, nMaxMessages);
		}

		/// <summary>
		/// <para>/ Call this in response to a SteamNetworkingMessagesSessionRequest_t callback.</para>
		/// <para>/ SteamNetworkingMessagesSessionRequest_t are posted when a user tries to send you a message,</para>
		/// <para>/ and you haven't tried to talk to them first.  If you don't want to talk to them, just ignore</para>
		/// <para>/ the request.  If the user continues to send you messages, SteamNetworkingMessagesSessionRequest_t</para>
		/// <para>/ callbacks will continue to be posted periodically.</para>
		/// <para>/</para>
		/// <para>/ Returns false if there is no session with the user pending or otherwise.  If there is an</para>
		/// <para>/ existing active session, this function will return true, even if it is not pending.</para>
		/// <para>/</para>
		/// <para>/ Calling SendMessageToUser() will implicitly accepts any pending session request to that user.</para>
		/// </summary>
		public static bool AcceptSessionWithUser(ref SteamNetworkingIdentity identityRemote) {
			InteropHelp.TestIfAvailableGameServer();
			return NativeMethods.ISteamNetworkingMessages_AcceptSessionWithUser(CSteamGameServerAPIContext.GetSteamNetworkingMessages(), ref identityRemote);
		}

		/// <summary>
		/// <para>/ Call this when you're done talking to a user to immediately free up resources under-the-hood.</para>
		/// <para>/ If the remote user tries to send data to you again, another SteamNetworkingMessagesSessionRequest_t</para>
		/// <para>/ callback will be posted.</para>
		/// <para>/</para>
		/// <para>/ Note that sessions that go unused for a few minutes are automatically timed out.</para>
		/// </summary>
		public static bool CloseSessionWithUser(ref SteamNetworkingIdentity identityRemote) {
			InteropHelp.TestIfAvailableGameServer();
			return NativeMethods.ISteamNetworkingMessages_CloseSessionWithUser(CSteamGameServerAPIContext.GetSteamNetworkingMessages(), ref identityRemote);
		}

		/// <summary>
		/// <para>/ Call this  when you're done talking to a user on a specific channel.  Once all</para>
		/// <para>/ open channels to a user have been closed, the open session to the user will be</para>
		/// <para>/ closed, and any new data from this user will trigger a</para>
		/// <para>/ SteamSteamNetworkingMessagesSessionRequest_t callback</para>
		/// </summary>
		public static bool CloseChannelWithUser(ref SteamNetworkingIdentity identityRemote, int nLocalChannel) {
			InteropHelp.TestIfAvailableGameServer();
			return NativeMethods.ISteamNetworkingMessages_CloseChannelWithUser(CSteamGameServerAPIContext.GetSteamNetworkingMessages(), ref identityRemote, nLocalChannel);
		}

		/// <summary>
		/// <para>/ Returns information about the latest state of a connection, if any, with the given peer.</para>
		/// <para>/ Primarily intended for debugging purposes, but can also be used to get more detailed</para>
		/// <para>/ failure information.  (See SendMessageToUser and k_nSteamNetworkingSend_AutoRestartBrokenSession.)</para>
		/// <para>/</para>
		/// <para>/ Returns the value of SteamNetConnectionInfo_t::m_eState, or k_ESteamNetworkingConnectionState_None</para>
		/// <para>/ if no connection exists with specified peer.  You may pass nullptr for either parameter if</para>
		/// <para>/ you do not need the corresponding details.  Note that sessions time out after a while,</para>
		/// <para>/ so if a connection fails, or SendMessageToUser returns k_EResultNoConnection, you cannot wait</para>
		/// <para>/ indefinitely to obtain the reason for failure.</para>
		/// </summary>
		public static ESteamNetworkingConnectionState GetSessionConnectionInfo(ref SteamNetworkingIdentity identityRemote, out SteamNetConnectionInfo_t pConnectionInfo, out SteamNetConnectionRealTimeStatus_t pQuickStatus) {
			InteropHelp.TestIfAvailableGameServer();
			return NativeMethods.ISteamNetworkingMessages_GetSessionConnectionInfo(CSteamGameServerAPIContext.GetSteamNetworkingMessages(), ref identityRemote, out pConnectionInfo, out pQuickStatus);
		}
	}
}

#endif // !DISABLESTEAMWORKS
