
#include "stdafx.h"
#include "Socket.h"
#include "MessageException.h"
#include "Thread.h"
#include "Session.h"

MessageReadThread::MessageReadThread(Session *s) {
	session = s;
}

void MessageReadThread::Run() {
	// Read messages from our session, and dispatch them
	try {
		while (!session->ShuttingDown()) {
			MessageBuffer *in_msg = session->ReadMessage();
			in_msg->Mark();
			unsigned char msg_code = in_msg->ReadByte();
			switch (msg_code) {
				case SUBSCRIBE:
					std::cerr << "Got subscribe message from server" << std::endl;
					delete in_msg;
					break;
				case UNSUBSCRIBE:
					std::cerr << "Got unsubscribe message from server" << std::endl;
					delete in_msg;
					break;
				case SUBSCRIBE_RESPONSE: {
						// Sub response
						int sub_id = in_msg->ReadInt32();
						bool success = in_msg->ReadBool();
						delete in_msg;
						std::cout << "Got subscription response: " << sub_id << ", " << success << std::endl;
						session->subscription_monitor.Enter();
						try {
							if (success && session->pending_subscribes.find(sub_id) != session->pending_subscribes.end()) {
								// We added the subscription
								Subscription *sub = session->pending_subscribes[sub_id];
								sub->sub_id = sub_id;
								session->pending_subscribes.erase(sub_id);
								session->subscriptions[sub_id] = sub;
								session->subscription_monitor.PulseAll();
							} else if (!success && session->pending_unsubscribes.find(sub_id) != session->pending_unsubscribes.end()) {
								session->pending_unsubscribes.erase(sub_id);
								Subscription *sub = session->subscriptions[sub_id];
								session->subscriptions.erase(sub_id);
								delete sub;
								session->subscription_monitor.PulseAll();
							} else {
								std::cerr << "Invalid combination of success and states" << std::endl;
							}
						} catch (...) {
							session->subscription_monitor.Exit();
							throw;
						}
						session->subscription_monitor.Exit();
					}
					break;
				case SERVER_MESSAGE: {
						// Ideally, I could use the response_subscription filter
						// to check to see if the message matches, but since I 
						// don't yet have enough information to generate the 
						// BaseMessage object from the MessageBuffer object, I
						// have to pull the topic out directly.
						std::string topic = in_msg->ReadString();
						// Move back to the beginning of the message (right before message type);
						in_msg->Reset();
						if (topic == ResponseMessage::GetMessageType()) {
							BaseMessage *message = NULL;
							if (session->request_topic == ObjInfoReqMessage::GetMessageType()) 
								message = new ObjInfoRespMessage();
							else if (session->request_topic == SetPropertyMessage::GetMessageType())
								message = new PropertyRespMessage();
							else if (session->request_topic == GetPropertyMessage::GetMessageType())
								message = new PropertyRespMessage();
							else
								std::cerr << "Error: Got invalid response message: session->request_topic is " <<
                                          session->request_topic << std::endl;
							message->ParseMessageBuffer(*in_msg);
							delete in_msg;
							session->response_monitor.Enter();
							if (session->response_message != NULL) {
								std::cerr << "Warning, got response while there is one pending" << std::endl;
								delete message;
							}
							session->response_message = message;
							session->response_monitor.PulseAll();
							session->response_monitor.Exit();
						} else {
							// Message to be handled by the user thread
							session->message_queue_monitor.Enter();
							session->message_queue.push_back(in_msg);
							session->message_queue_monitor.PulseAll();
                            //std::cout << "After adding message '" << topic << "' in Run, queue size is " <<
                            //    session->message_queue.size() << std::endl;
							session->message_queue_monitor.Exit();
						}

                    }
					break;
				default:
					std::cerr << "Got unknown message code from server" << std::endl;
					delete in_msg;
					break;
			}
		}
	} catch (const MessageException &e) {
		std::cerr << "Message Exception: " << e.Message() << std::endl;
	}
}

Subscription::Subscription(MessageFilter *filter, MessageCallback *callback) {
	sub_id = -1;
	this->filter = filter;
	this->callback = callback;
}

Subscription::~Subscription() {
	delete filter;
	delete callback;
}

Session::Session() : sock(NULL), shutting_down(false), response_subscription(NULL), response_message(NULL), next_sub_id(1), msgReader(NULL) {
}

Session::~Session() {
	Cleanup();
}

bool Session::ShuttingDown() {
	// I could lock this with a critical section, but it's probably atomic
	return shutting_down;
}

void Session::Cleanup() {
	if (response_subscription != NULL) {
		CloseSubscription(response_subscription);
		response_subscription = NULL;
	}
	if (sock != NULL) {
		delete sock;
		sock = NULL;
	}
}

void Session::Connect(const char *hostname, unsigned short port) {
	Socket *s = new Socket();
	if (s->Connect(hostname, port) < 0) {
		delete s;
		throw MessageException("Failed to connect to server");
	}
	sock = s;
	
	// Read the session id from the stream
	unsigned char buf[4];
	if (ReadData(buf, 0, 4) != 4)
		throw MessageException("Unable to read data");
	unsigned long id_net = *((unsigned int *)buf);
	unsigned long session_id = ntohl(id_net);
}

void Session::Startup() {
	// assert(response_subscription == NULL);
	// Set up the messge reader thread
	msgReader = new MessageReadThread(this);
	msgReader->Start();
	// Ok, at this point the connection is established, but we 
	// want to create a special subscription for listening to 
	// response messages.
	ResponseFilter *response_filter = new ResponseFilter();
	response_filter->topic = "genericResp";
	// We should probably put our session id in here, but now we don't
	// response_filter->session_id = session_id;
    response_subscription = CreateSubscription(response_filter, NULL);
}

void Session::Shutdown() {
	if (response_subscription != NULL) {
		CloseSubscription(response_subscription);
		response_subscription = NULL;
	}
	
	// Perhaps we should attempt to close out all of our subscriptions,
	// but for now, we do not.

	// I could lock around this, but it's probably atomic
	shutting_down = true;
	if (msgReader != NULL) {
		// msgReader->Interrupt();
		msgReader->Join();
		delete msgReader;
		msgReader = NULL;
	}
}

int Session::WriteData(const unsigned char *data, int offset, int len) {
	while (offset < len && !ShuttingDown()) {
		int rv = sock->Write(data, offset, len - offset);
		if (rv <= 0)
			return rv;
		offset += rv;
	}
	return offset;
}

int Session::ReadData(unsigned char *data, int offset, int len) {
	while (offset < len && !ShuttingDown()) {
		int rv = sock->Read(data, offset, len - offset);
		if (rv <= 0)
			return rv;
		offset += rv;
	}
	return offset;
}

void Session::WriteMessage(const MessageBuffer *msg) {
	unsigned int msg_len = msg->GetLength();
	int len = 4;
	unsigned long msg_len_net = htonl(msg_len);
	const unsigned char *data = (unsigned char *)&msg_len_net;
	write_monitor.Enter();
	try {
		if (WriteData(data, 0, len) != len)
			throw MessageException("Unable to write data");
		len = msg_len;
		data = msg->GetData();
		if (WriteData(data, 0, len) != len)
			throw MessageException("Unable to write data");
	} catch (...) {
		write_monitor.Exit();
		throw;
	}
	write_monitor.Exit();
}

MessageBuffer *Session::ReadMessage() {
	unsigned char len_buf[4];
	unsigned char *buf = NULL;
	unsigned long msg_len;
	read_monitor.Enter();
	try {
		if (ReadData(len_buf, 0, 4) != 4)
			throw MessageException("Unable to read data");
		unsigned long msg_len_net = *((unsigned int *)len_buf);
		msg_len = ntohl(msg_len_net);
		buf = new unsigned char[msg_len];
		if (ReadData(buf, 0, msg_len) != msg_len)
			throw MessageException("Unable to read data");
	} catch (...) {
		read_monitor.Exit();
		if (buf != NULL)
			delete[] buf;
		throw;
	}
	read_monitor.Exit();

	MessageBuffer *rv = new MessageBuffer();
	try {
		rv->SetData(buf, msg_len);
	} catch (...) {
		delete[] buf;
		delete rv;
		throw;
	}
	delete[] buf;
	return rv;
}

MessageBuffer *Session::GetNextMessageBuffer() {
	MessageBuffer *rv = NULL;
	message_queue_monitor.Enter();
	while (!ShuttingDown()) {
		if (message_queue.size() != 0) {
			rv = message_queue.front();
			message_queue.pop_front();
			message_queue_monitor.Exit();
			return rv;
		} else 
			message_queue_monitor.Wait();
	}
	return NULL;
}

Subscription *Session::CreateSubscription(MessageFilter *filter, MessageCallback *callback) {
	int sub_id;
	subscription_monitor.Enter();
	sub_id = next_sub_id++;
	subscription_monitor.Exit();

	// Make a subscribe message
	MessageBuffer sub_msg;
	sub_msg.WriteByte(SUBSCRIBE);
	sub_msg.WriteInt32(sub_id);
	filter->Write(sub_msg);
	WriteMessage(&sub_msg);

	Subscription *sub = new Subscription(filter, callback);

	subscription_monitor.Enter();
	try {
		pending_subscribes[sub_id] = sub;
	} catch (...) {
		subscription_monitor.Exit();
		throw;
	}
	subscription_monitor.Exit();

	bool sub_pending = true;
	subscription_monitor.Enter();
	while (sub_pending && !ShuttingDown()) {
		try {
			sub_pending = (pending_subscribes.find(sub_id) != pending_subscribes.end());
		} catch (...) {
			subscription_monitor.Exit();
			throw;
		}
		if (sub_pending)
			subscription_monitor.Wait();
	}
	subscription_monitor.Exit();
	
	if (sub_pending)
		// We are shutting down
		return NULL;

	// Ok, it's not in the pending_subscribes list, so it should be in subscriptions
	subscription_monitor.Enter();
	try {
		// assert(subscriptions[sub_id] == sub);
		sub = subscriptions[sub_id];
	} catch (...) {
		subscription_monitor.Exit();
		throw;
	}
	subscription_monitor.Exit();

	return sub;
}

void Session::CloseSubscription(Subscription *sub) {
	if (sub == NULL)
		// act like delete, where we allow them to pass NULL, and do nothing
		return;

	int sub_id = sub->sub_id;

	// Make a new unsubscribe message
	MessageBuffer sub_msg;
	sub_msg.WriteByte(UNSUBSCRIBE);
	sub_msg.WriteInt32(sub_id);
	WriteMessage(&sub_msg);

	subscription_monitor.Enter();
	try {
		pending_unsubscribes.insert(sub_id);
	} catch (...) {
		subscription_monitor.Exit();
		throw;
	}
	subscription_monitor.Exit();

	bool unsub_pending = true;
	subscription_monitor.Enter();
	while (unsub_pending && !ShuttingDown()) {
		try {
			unsub_pending = (pending_unsubscribes.find(sub_id) != pending_unsubscribes.end());
		} catch (...) {
			subscription_monitor.Exit();
			throw;
		}
		if (unsub_pending)
			subscription_monitor.Wait();
	}
	subscription_monitor.Exit();
	// At this point the sub object has generally been deleted.
	// If we are shutting down, we may have aborted, in which case,
	// the sub object will still be in the subscription map 
	// (and will still need to be deleted)
}

void Session::WriteMessage(const BaseMessage *msg) {
	MessageBuffer *msg_buf = msg->GetMessageBuffer();
	this->WriteMessage(msg_buf);
	delete msg_buf;
}

BaseMessage *Session::Request(const BaseMessage *msg) {
	request_monitor.Enter();
	request_topic = msg->GetTopic();
    // std::cout << "In Session::Request, setting request_topic to " << request_topic << std::endl;
    response_monitor.Enter();
	if (response_message != NULL) {
		std::cerr << "Making request while there is a pending response" << std::endl;
		delete response_message;
	}
	response_message = NULL;
	MessageBuffer *msg_buf = msg->GetMessageBuffer();
	WriteMessage(msg_buf);
	delete msg_buf;
	while (!ShuttingDown()) {
		if (response_message == NULL)
			response_monitor.Wait();
		else {
			BaseMessage *rv = response_message;
			response_message = NULL;
			response_monitor.Exit();
			request_monitor.Exit();
			return rv;
		}
	}
	response_monitor.Exit();
	request_topic = "";
	request_monitor.Exit();
	return NULL;
}

void Session::QueueMessageForUserThread(const BaseMessage *msg) {
	if (!ShuttingDown()) {
        MessageBuffer *msg_buf = msg->GetMessageBuffer();
        // Message to be handled by the user thread
        message_queue_monitor.Enter();
        message_queue.push_back(msg_buf);
        //std::cout << "After adding message '" << msg->GetTopic() << "' in QueueMessageForUserThread, queue size is " <<
        //    message_queue.size() << std::endl;
        message_queue_monitor.PulseAll();
        message_queue_monitor.Exit();
    }
}

bool MessageFilter::Matches(const BaseMessage &message) const {
	throw MessageException("MessageFilter::Matches not implemented");
}

void MessageFilter::Write(MessageBuffer &buf) const {
	throw MessageException("MessageFilter::Write not implemented");
}

void TopicFilter::Write(MessageBuffer &buf) const {
	buf.WriteByte(TOPIC);   // filter_type = TOPIC
	buf.WriteString(topic);
}

bool TopicFilter::Matches(const BaseMessage &message) const {
	return (message.GetTopic() == topic);
}

bool OidFilter::Matches(const BaseMessage &message) const {
	if (!TopicFilter::Matches(message))
		return false;
	const OidMessage *oid_message_ptr = dynamic_cast<const OidMessage *>(&message);
	if (oid_message_ptr == NULL)
		return false;
	return (oid_message_ptr->GetOid() == oid);
}

void OidFilter::Write(MessageBuffer &buf) const {
	buf.WriteByte(OID);
	buf.WriteInt64(oid);
}

void ResponseFilter::Write(MessageBuffer &buf) const {
	buf.WriteByte(RESPONSE);   // filter_type = RESPONSE
	buf.WriteString(topic);
}

bool ResponseFilter::Matches(const BaseMessage &message) const {
	if (!TopicFilter::Matches(message))
		return false;
	// Ok, here we just assume it matches, and rely on the java code
	// to not send us anything that doesn't match.  This lets the server
	// not embed the session id in all the response messages.
	return true;
}
