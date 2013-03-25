#pragma once

#include "Socket.h"
#include "Message.h"
#include "Thread.h"

enum MessageCode {
	SUBSCRIBE = 0,
	UNSUBSCRIBE = 1,
	SUBSCRIBE_RESPONSE = 2,
	SERVER_MESSAGE = 3
};

enum MessageFilterType {
	TOPIC = 0,
	OID = 1,
	RESPONSE = 2
};

class __declspec(dllexport) MessageFilter {
public:
	// Write our message to the buffer
	virtual void Write(MessageBuffer &buf) const;
	// Check to see if the message matches our filter
	virtual bool Matches(const BaseMessage &message) const;
};

class __declspec(dllexport) TopicFilter : public MessageFilter {
public:
	std::string topic;
	virtual void Write(MessageBuffer &buf) const;
	virtual bool Matches(const BaseMessage &message) const;
};

class __declspec(dllexport) OidFilter : public TopicFilter {
public:
	long long oid;
	virtual void Write(MessageBuffer &buf) const;
	virtual bool Matches(const BaseMessage &message) const;
};

class __declspec(dllexport) ResponseFilter : public TopicFilter {
public:
	virtual void Write(MessageBuffer &buf) const;
	virtual bool Matches(const BaseMessage &message) const;
};

class __declspec(dllexport) MessageCallback {
public:
	virtual void HandleMessage(BaseMessage &msg) const = 0;
};

class Session;

class __declspec(dllexport) MessageReadThread : public Thread {
private:
	Session *session;

public:
	MessageReadThread(Session *s);

	virtual void Run();
};

class __declspec(dllexport) Subscription {
public:
	Subscription(MessageFilter *filter, MessageCallback *callback);
	virtual ~Subscription();

	int sub_id;
	MessageFilter *filter;
	MessageCallback *callback;
};


class __declspec(dllexport) Session {
	friend class ResponseMessageCallback;
	friend class MessageReadThread;
public:
	Monitor subscription_monitor;
	Monitor response_monitor;
	std::map<int, Subscription *> pending_subscribes;
	std::map<int, Subscription *> subscriptions;
	std::set<int> pending_unsubscribes;
	
private:
	Monitor read_monitor;
	Monitor write_monitor;
	Monitor request_monitor;
	Monitor message_queue_monitor;
	std::string request_topic;
	// Queue of unprocessed messages.  Messages that are destined for
	// standard processing will be read from the socket, and put into
	// this queue.  The consumer of the queue will free them.
	std::list<MessageBuffer *> message_queue;
	// This is the subscription we will use to handle response messages
	Subscription *response_subscription;
	// Temporary storage for the response message.  This plays a similar role
	// to the message queue, but for response messages.
	BaseMessage *response_message;
	MessageReadThread *msgReader;

	int next_sub_id;
	Socket *sock;
	bool shutting_down;

	int WriteData(const unsigned char *data, int offset, int len);
	int ReadData(unsigned char *data, int offset, int len);

	void WriteMessage(const MessageBuffer *msg);
	MessageBuffer *ReadMessage();

	void Cleanup();
public:
	Session();
	~Session();
	void Connect(const char *hostname, unsigned short port);
	// This subscribes to response messages
	void Startup();
	// This unsubscribes to response messages
	void Shutdown();
	// This just gets a flag to indicate whether we are shutting down
	bool ShuttingDown();

	// This is the method that should be called by the user thread to
	// get the next queued user message.
	// The caller is responsible for freeing the returned message.
	MessageBuffer *GetNextMessageBuffer();

	// Blocking version of the subscribe call.  
	// The subscription, filter and callback end up in the subscriptions map.
	Subscription *CreateSubscription(MessageFilter *filter, MessageCallback *callback);
	// This does a blocking close on the subscription object, and frees it.
	void CloseSubscription(Subscription *sub);

	// Send a message
	void WriteMessage(const BaseMessage *msg);

	// Blocking version of Request.  This sends a request message, and blocks
	// while it waits for the response.  The caller is responsible for freeing
	// the returned message.
	BaseMessage *Request(const BaseMessage *request_msg);

    // Take the internal form of a message, convert it to on-the-wire
    // form, and enqueue it for the user thread as if it were re
    // received from the client.
    void QueueMessageForUserThread(const BaseMessage *msg);
};



