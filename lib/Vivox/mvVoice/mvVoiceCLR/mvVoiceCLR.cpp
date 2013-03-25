// This is the main DLL file.
// mvVoiceCLR.cpp

#include "stdafx.h"

#include "mvVoiceCLR.h"
#include "mvVoiceCBL.h"
#include <iostream>
#include <string>

using namespace std;
using namespace System;
using namespace System::Runtime::InteropServices;

namespace mvVoiceCLR
{
  void MarshalString(System::String ^s, std::string& os) {
	  const char* chars = (const char*)(Marshal::StringToHGlobalAnsi(s)).ToPointer();
	  os = chars;
	  Marshal::FreeHGlobal(IntPtr((void*)chars));
  }
  System::String^ VoiceCLR::GetConnectorID()
  {
	  return gcnew System::String(mvVoiceCBL::VoiceCBL::GetConnectorID().c_str());
  }
  System::String^ VoiceCLR::GetAccountID()
  {
	  return gcnew System::String(mvVoiceCBL::VoiceCBL::GetAccountID().c_str());
  }
  System::String^ VoiceCLR::GetSessionID()
  {
	  return gcnew System::String(mvVoiceCBL::VoiceCBL::GetSessionID().c_str());
  }
  int VoiceCLR::Init(System::String^ ssServer)
  {
	std::string server = "";
	MarshalString(ssServer, server);
	return mvVoiceCBL::VoiceCBL::Init(server);
  }
  int VoiceCLR::Login(System::String ^ssConnectorID, System::String^ ssUsername, System::String^ ssPassword)
  {
	std::string connectorID = "";
	MarshalString(ssConnectorID, connectorID);
	std::string username = "";
	MarshalString(ssUsername, username);
	std::string password = "";
	MarshalString(ssPassword, password);
	return mvVoiceCBL::VoiceCBL::Login(connectorID, username, password);
  }
  int VoiceCLR::Call(System::String^ ssAccountID, System::String^ ssVoiceServer)
  {
	std::string accountID = "";
	MarshalString(ssAccountID, accountID);
	std::string voiceServer = "";
	MarshalString(ssVoiceServer, voiceServer);
	return mvVoiceCBL::VoiceCBL::Call(accountID, voiceServer);
  }
  int VoiceCLR::Hangup(System::String^ ssSessionID)
  {
	std::string sessionID = "";
	MarshalString(ssSessionID, sessionID);
	return mvVoiceCBL::VoiceCBL::Hangup(sessionID);
  }
  int VoiceCLR::Logout(System::String^ ssAccountID)
  {
	std::string accountID = "";
	MarshalString(ssAccountID, accountID);
	return mvVoiceCBL::VoiceCBL::Logout(accountID);
  }
  int VoiceCLR::Shutdown(System::String^ ssConnectorID)
  {
	std::string connectorID = "";
	MarshalString(ssConnectorID, connectorID);
	return mvVoiceCBL::VoiceCBL::Shutdown(connectorID);
  }
  int VoiceCLR::MicMute(System::String^ ssConnectorID, bool mute)
  {
	std::string connectorID = "";
	MarshalString(ssConnectorID, connectorID);
	return mvVoiceCBL::VoiceCBL::MicMute(connectorID, mute);
  }
}
