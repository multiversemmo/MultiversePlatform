// SampleBridgeClient.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "Socket.h"
#include "MessageException.h"
#include "Thread.h"
#include "Message.h"
#include "Session.h"
#include "OA_Messages.h"

// HACK
long long mob_oid = 126048L;
long long player_oid = 97601L; //101101L;
IntVector mob_loc;


// Gets a quaternion that captures the rotation about the y axis
static Quaternion GetFacingFromDirection(const Vector &dir) {
	float theta = atan2(dir.x, dir.z);
	
	// theta is the rotation about the positive y axis
	// convert that to a quaternion
	float halfAngle = 0.5 * theta;
    float sinvalue = sin(halfAngle);
	
	Quaternion rv;
    rv.x = 0; 
    rv.y = sinvalue * 1;
    rv.z = 0; 
	rv.w = cos(halfAngle);
    return rv;
}

// Get the quaternion that represents the rotation required for
// an object at src_pos to look at dst_pos
static Quaternion GetFacingFromPosition(const IntVector &src_pos, const IntVector &dst_pos) {
	Vector facing;
	facing.x = dst_pos.x - src_pos.x;
	facing.y = dst_pos.y - src_pos.y;
	facing.z = dst_pos.z - src_pos.z;
	return GetFacingFromDirection(facing);
}


std::ostream& operator <<(std::ostream& os, const Vector& obj) {
	return os << "[" << obj.x << ", " << obj.y << ", " << obj.z << "]";
}

std::ostream& operator <<(std::ostream& os, const IntVector& obj) {
	return os << "[" << obj.x << ", " << obj.y << ", " << obj.z << "]";
}

std::ostream& operator <<(std::ostream& os, const Quaternion& obj) {
	return os << "[" << obj.x << ", " << obj.y << ", " << obj.z << ", " << obj.w << "]";
}

class UserMessageThread : public Thread {
private:
	Session *session;

public:
	UserMessageThread(Session *s);

	virtual void Run();
};

UserMessageThread::UserMessageThread(Session *s) {
	session = s;
}

void UserMessageThread::Run() {
	// int sub_id = in_msg->ReadInt32();
	std::map<int, Subscription *>::iterator iter;
	while (!session->ShuttingDown()) {
		MessageBuffer *in_msg = session->GetNextMessageBuffer();
		if (in_msg == NULL)
			continue; // we are probably shutting down. the while test should 
					  // cause us to stop
		unsigned char msg_code = in_msg->ReadByte();
		// assert(msg_code == SERVER_MESSAGE);
		// Normally, we would dispatch to that subscription, but for now, just do it inline
		std::string topic = in_msg->ReadString();
		// Move back to the beginning of the message (right before message type);
		in_msg->Reset();
		BaseMessage *message = NULL;
		if (topic == NewObjectMessage::GetMessageType()) {
			message = new NewObjectMessage();
		} else if (topic == FreeObjectMessage::GetMessageType()) {
			message = new FreeObjectMessage();
		} else if (topic == DirLocMessage::GetMessageType()) {
			message = new DirLocMessage();
		} else if (topic == PropertyMessage::GetMessageType()) {
			message = new PropertyMessage();
		} else if (topic == HotLoadBehaviorMessage::GetMessageType()) {
			message = new HotLoadBehaviorMessage();
		} else if (topic == SpawnedMessage::GetMessageType()) {
			message = new SpawnedMessage();
		} else if (topic == DespawnedMessage::GetMessageType()) {
			message = new DespawnedMessage();
		} else {
			std::cout << "Unhandled message topic: " << topic << std::endl;
			delete in_msg;
			continue;
		}
		message->ParseMessageBuffer(*in_msg);
		delete in_msg;
		session->subscription_monitor.Enter();
		try {
			for (iter = session->subscriptions.begin(); iter != session->subscriptions.end(); ++iter) {
				if ((*iter).second->filter->Matches(*message) &&
					(*iter).second->callback != NULL)
					(*iter).second->callback->HandleMessage(*message);
			}
		} catch (...) {
			session->subscription_monitor.Exit();
			delete message;
			throw;
		}
		session->subscription_monitor.Exit();
		delete message;
	}
}


class NewObjectMessageCallback : public MessageCallback {
public:
	Session *session;
	virtual void HandleMessage(BaseMessage &msg) const {
		std::cout << "In message handler for message with topic: " << msg.GetTopic() << std::endl;
		NewObjectMessage &message = dynamic_cast<NewObjectMessage &>(msg);
		std::cout << "  new_object_id: " << message.GetObjectOid() 
				  << "; object_type: " << message.GetObjectType() << std::endl;

		// MarsMob objects can respond to GetPropertyMessages
		// Most MVObject objects cannot.
		if (message.GetObjectType() != "MarsMob")
			return;

		// HACK
		//if (mob_oid == 0)
		//	mob_oid = message.GetObjectOid();
		//else if (player_oid == 0)
		//	player_oid = message.GetObjectOid();

		std::cout << "Requesting 'mvobj.userflag' property for MarsMob" << std::endl;

		GetPropertyMessage request_msg;
		request_msg.SetOid(message.GetObjectOid());
		request_msg.SetPropertyName("mvobj.userflag");
		BaseMessage *response = session->Request(&request_msg);
		if (response == NULL)
			return; // probably shutting down
		PropertyRespMessage *response_msg = dynamic_cast<PropertyRespMessage *>(response);
		std::cout << "mvobj.userflag = " << response_msg->GetPropertyValue() << std::endl;
		delete response;

		// Looks like this SetProperty mechanism involved getting
		// a response message
		SetPropertyMessage set_msg;
		set_msg.SetOid(message.GetObjectOid());
		set_msg.SetRequestResponse(true);
		set_msg.SetPropertyName("mvobj.brainstate");
		set_msg.SetPropertyValue("braindead.nocoffee");
		response = session->Request(&set_msg);
		if (response == NULL)
			return; // probably shutting down
		delete response;
	}
};


class FreeObjectMessageCallback : public MessageCallback {
public:
	virtual void HandleMessage(BaseMessage &msg) const {
		std::cout << "In message handler for message with topic: " << msg.GetTopic() << std::endl;
		FreeObjectMessage &message = dynamic_cast<FreeObjectMessage &>(msg);
		std::cout << "  free_object_id: " << message.GetObjectOid() 
				  << "; object_type: " << message.GetObjectType() << std::endl;

	}
};


class DirLocMessageCallback : public MessageCallback {
public:
	Session *session;
	virtual void HandleMessage(BaseMessage &msg) const {
		//std::cout << "In message handler for message with topic: " << msg.GetTopic() << std::endl;
		DirLocMessage &message = dynamic_cast<DirLocMessage &>(msg);
		//std::cout << "  object_id: " << message.GetOid() 
		//		  << "; object_dir: " << message.GetDir() 
		//		  << "; object_loc: " << message.GetLoc() << std::endl;
		// Hackery to store the location of the magic first object
		if (message.GetOid() == mob_oid) {
			mob_loc = message.GetLoc();
			std::cout << "Mob loc: " << mob_loc << std::endl;
		} else if (message.GetOid() == player_oid) {
			// if this is the second object that is moving, 
			// make the first object turn to face it
			std::cout << "Target loc: " << message.GetLoc() << std::endl;
			Quaternion q = GetFacingFromPosition(mob_loc, message.GetLoc());
			UpdateWorldNodeReqMessage turn_msg;
			turn_msg.SetOid(mob_oid);
			turn_msg.SetOrientation(q);
			session->WriteMessage(&turn_msg);
			//BaseMessage *response = session->Request(&turn_msg);
			//if (response == NULL)
			//	return; // probably shutting down
			//delete response;
		}
	}
};

class PropertyMessageCallback : public MessageCallback {
public:
	virtual void HandleMessage(BaseMessage &msg) const {
		std::cout << "In message handler for message with topic: " << msg.GetTopic() << std::endl;
		PropertyMessage &message = dynamic_cast<PropertyMessage &>(msg);
		std::cout << "  object_id: " << message.GetOid() << std::endl;
		std::map<std::string, std::string>::const_iterator iter;
		for (iter = message.properties.begin(); iter != message.properties.end(); ++iter)
			std::cout << "  '" << (*iter).first << "' => '" << (*iter).second << "'" << std::endl;
	}
};

class SpawnedMessageCallback : public MessageCallback {
public:
	virtual void HandleMessage(BaseMessage &msg) const {
		std::cout << "In message handler for message with topic: " << msg.GetTopic() << std::endl;
		SpawnedMessage &message = dynamic_cast<SpawnedMessage &>(msg);
		std::cout << " object_id: " << message.GetOid() << std::endl;
	}
};

class DespawnedMessageCallback : public MessageCallback {
public:
	virtual void HandleMessage(BaseMessage &msg) const {
		std::cout << "In message handler for message with topic: " << msg.GetTopic() << std::endl;
		DespawnedMessage &message = dynamic_cast<DespawnedMessage &>(msg);
		std::cout << " object_id: " << message.GetOid() << std::endl;
	}
};

void DoTest(Session *session) {
	// This sets up my subscription to GetPropertyMessage responses
	session->Startup();

	TopicFilter *filter1 = new TopicFilter();
	filter1->topic = NewObjectMessage::GetMessageType();
	NewObjectMessageCallback *callback1 = new NewObjectMessageCallback();
	callback1->session = session;

	TopicFilter *filter2 = new TopicFilter();
	filter2->topic = FreeObjectMessage::GetMessageType();
	FreeObjectMessageCallback *callback2 = new FreeObjectMessageCallback();

	TopicFilter *filter3 = new TopicFilter();
	filter3->topic = DirLocMessage::GetMessageType();
	DirLocMessageCallback *callback3 = new DirLocMessageCallback();
	callback3->session = session;
	
	TopicFilter *filter4 = new TopicFilter();
	filter4->topic = PropertyMessage::GetMessageType();
	PropertyMessageCallback *callback4 = new PropertyMessageCallback();

	TopicFilter *filter5 = new TopicFilter();
	filter5->topic = SpawnedMessage::GetMessageType();
	SpawnedMessageCallback *callback5 = new SpawnedMessageCallback();

	TopicFilter *filter6 = new TopicFilter();
	filter6->topic = DespawnedMessage::GetMessageType();
	DespawnedMessageCallback *callback6 = new DespawnedMessageCallback();

	Subscription *sub1 = session->CreateSubscription(filter1, callback1); 
	Subscription *sub2 = session->CreateSubscription(filter2, callback2); 
	Subscription *sub3 = session->CreateSubscription(filter3, callback3); 
	Subscription *sub4 = session->CreateSubscription(filter4, callback4); 
	Subscription *sub5 = session->CreateSubscription(filter5, callback5);
	Subscription *sub6 = session->CreateSubscription(filter6, callback6);

	// Sleep until we are ready to shutdown... for now, that is forever.
	for (;;) {
		// HACK
		//if (mob_oid != 0) {
		//	SetPropertyMessage set_msg;
		//	set_msg.SetOid(mob_oid);
		//	set_msg.SetPropertyName("mvobj.loopdata");
		//	set_msg.SetPropertyValue("loopdata.test");
		//	BaseMessage *response = session->Request(&set_msg);
		//	if (response == NULL)
		//		return; // probably shutting down
		//	delete response;
		//}
		Sleep(100);
	}

	session->CloseSubscription(sub1);
	session->CloseSubscription(sub2);
	session->CloseSubscription(sub3);
	session->CloseSubscription(sub4);
	session->CloseSubscription(sub5);
	session->CloseSubscription(sub6);

	session->Shutdown();
	// I probably need to interrupt these threads in many cases
}

int _tmain(int argc, _TCHAR* argv[])
{
	// Defaults (not really used since you must supply these)
	const char *bridge_server_host;    // cedeno-dxp.corp.multiverse.net
	unsigned short bridge_server_port; // 9757

	std::vector<std::string> args;
	char buf[1024];
	for (int i = 0; i < argc; ++i) {
		WideCharToMultiByte(CP_ACP, 0, argv[i], -1, buf, sizeof(buf), NULL, NULL);
		args.push_back(buf);
	}

	if (args.size() != 3) {
		std::cerr << "Usage: " << args[0] << " <hostname> <port>" << std::endl;
		return -1;
	}
	
	bridge_server_host = args[1].c_str();
	bridge_server_port = atoi(args[2].c_str());

	Session *session = new Session();

	// Startup the thread that I will use to process messages
	UserMessageThread *userThread = new UserMessageThread(session);
	userThread->Start();
	
	try {
		session->Connect(bridge_server_host, bridge_server_port);
		DoTest(session);
	} catch (const MessageException &e) {
		std::cerr << "Message Exception: " << e.Message() << std::endl;
	}

	userThread->Join();
	delete userThread;

	delete session;

    return 1; 
}



