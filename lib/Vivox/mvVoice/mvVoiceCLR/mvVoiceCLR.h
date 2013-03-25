// mvVoiceCLR.h

#pragma once

using namespace System;

namespace mvVoiceCLR {

	public ref class VoiceCLR
	{
	public:
		static System::String^ GetConnectorID();
		static System::String^ GetAccountID();
		static System::String^ GetSessionID();
		static int Init(System::String^ ssServer);
		static int Login(System::String^ ssConnectorID, System::String^ ssUsername, System::String^ ssPassword);
		static int Call(System::String^ ssAccountID, System::String^ ssVoiceServer);
		static int Hangup(System::String^ ssSessionID);
		static int Logout(System::String^ ssAccountID);
		static int Shutdown(System::String^ ssConnectorID);
		static int MicMute(System::String^ ssConnectorID, bool mute);
	};
}
