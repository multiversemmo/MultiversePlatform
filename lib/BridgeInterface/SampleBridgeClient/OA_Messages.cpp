#include "stdafx.h"
#include <assert.h>
#include "Socket.h"
#include "Message.h"
#include "OA_Messages.h"
#include "Session.h"

std::string HotLoadBehaviorMessage::GetMessageType() {
	return "OA.HotLoadBehaviors";
}
std::string HotLoadBehaviorMessage::GetTopic() const {
	return HotLoadBehaviorMessage::GetMessageType();
}
void HotLoadBehaviorMessage::ParseMessage(MessageBuffer &msg) {
}

void HotLoadBehaviorMessage::WriteMessage(MessageBuffer &msg) const {
}
