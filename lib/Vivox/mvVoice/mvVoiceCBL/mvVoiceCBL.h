// mvVoiceDLL.h

#include <string>

using namespace std;

namespace mvVoiceCBL
{
	class VoiceCBL
	{
	public:
		static __declspec(dllexport) std::string GetConnectorID();
		static __declspec(dllexport) std::string GetAccountID();
		static __declspec(dllexport) std::string GetSessionID();
		static __declspec(dllexport) int Init(std::string ssServer);
		static __declspec(dllexport) int Login(std::string ssConnectorID, std::string ssUsername, std::string ssPassword);
		static __declspec(dllexport) int Call(std::string ssAccountID, std::string ssVoiceServer);
		static __declspec(dllexport) int Hangup(std::string ssSessionID);
		static __declspec(dllexport) int Logout(std::string ssAccountID);
		static __declspec(dllexport) int Shutdown(std::string ssConnectorID);
		static __declspec(dllexport) int MicMute(std::string ssConnectorID, bool mute);
	};
}
