import clr
import System
import ClientAPI
ClientAPI.Log("Current Directory: %s" % System.Environment.CurrentDirectory)
clr.AddReferenceToFileAndPath("%s/mvVoiceCLR.dll" % System.Environment.CurrentDirectory)
import mvVoiceCLR

connectorID = ""
accountID = ""
sessionID = ""
server = "http://www.vd1.vivox.com/api2"
username = "multiverse1"
password = "multiverse"
channel = "sip:confctl-238@nkt.vivox.com"

def mvVoiceInit():
    global connectorID
    global accountID
    mvVoiceCLR.VoiceCLR.Init(server)
    connectorID = mvVoiceCLR.VoiceCLR.GetConnectorID()
    mvVoiceCLR.VoiceCLR.Login(connectorID, username, password)
    accountID = mvVoiceCLR.VoiceCLR.GetAccountID()
    mvVoiceCLR.VoiceCLR.MicMute(connectorID, True)

def mvVoiceOn():
    global sessionID
    mvVoiceCLR.VoiceCLR.Call(accountID, channel)
    sessionID = mvVoiceCLR.VoiceCLR.GetSessionID()

def mvVoiceOff():
    mvVoiceCLR.VoiceCLR.Hangup(sessionID)

def mvVoicePushToTalk(pushed):
    mvVoiceCLR.VoiceCLR.MicMute(connectorID, not pushed)
