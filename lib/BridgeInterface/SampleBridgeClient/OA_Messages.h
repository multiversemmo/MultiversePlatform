#pragma once

#include "MessageException.h"
#include "Message.h"

/** Add by JWT, should be the message that is called when hotloadbehaviors is called */
class HotLoadBehaviorMessage : public BaseMessage {
public:
	static std::string GetMessageType();
	virtual std::string GetTopic() const;
	virtual void ParseMessage(MessageBuffer &msg);
	virtual void WriteMessage(MessageBuffer &msg) const;
};

