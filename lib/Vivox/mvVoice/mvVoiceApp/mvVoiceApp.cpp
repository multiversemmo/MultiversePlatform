// mvVoiceApp.cpp : main project file.

#include "stdafx.h"
#include <iostream>

using namespace std;
using namespace System;

int main(array<System::String ^> ^args)
{
	System::String^ connectorID;
	System::String^ accountID;
	System::String^ sessionID;
	char ch;

	cout << "Hello, World!" << endl;

	mvVoiceCLR::VoiceCLR::Init("http://www.vd1.vivox.com/api2");
	connectorID = mvVoiceCLR::VoiceCLR::GetConnectorID();
	mvVoiceCLR::VoiceCLR::Login(connectorID, "multiverse1", "multiverse");
	accountID = mvVoiceCLR::VoiceCLR::GetAccountID();
	mvVoiceCLR::VoiceCLR::Call(accountID, "sip:confctl-238@nkt.vivox.com");
	sessionID = mvVoiceCLR::VoiceCLR::GetSessionID();
	cout << "Hit ENTER to continue..." << endl;
	cin >> noskipws >> ch;

	mvVoiceCLR::VoiceCLR::Hangup(sessionID);
	mvVoiceCLR::VoiceCLR::Logout(accountID);
	mvVoiceCLR::VoiceCLR::Shutdown(connectorID);

	cout << "Hello, World!" << endl;

    return 0;
}
