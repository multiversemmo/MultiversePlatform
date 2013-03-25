
#include "stdafx.h"
#include <assert.h>
#include "Socket.h"
#include "Message.h"
#include "Session.h"

Quaternion Quaternion::FromAngleAxis(float angle, const Vector &axis) {
    Quaternion quat;

    float halfAngle = 0.5 * angle;
    float sinvalue = sin(halfAngle);

    quat.w = cos(halfAngle);
    quat.x = sinvalue * axis.x; 
    quat.y = sinvalue * axis.y; 
    quat.z = sinvalue * axis.z; 

    return quat;
}

Quaternion Quaternion::RotateAroundYAxis(float angle) {
    Vector vec;
    vec.y = 1.0;
    return Quaternion::FromAngleAxis(angle, vec);
}

MessageBuffer::MessageBuffer() : read_offset(0), mark_offset(0) {
}

int MessageBuffer::GetLength() const {
	return buf.size();
}
const unsigned char *MessageBuffer::GetData() const {
	if (buf.size() > 0)
		return &buf.front();
	return NULL;
}
void MessageBuffer::SetData(const unsigned char *d, int l) {
	buf.resize(0);
	WriteData(d, l);
}

void MessageBuffer::Mark() {
	mark_offset = read_offset;
}

void MessageBuffer::Reset() {
	read_offset = mark_offset;
}

void MessageBuffer::WriteBool(bool data) {
	buf.push_back((data == true) ? 1 : 0);
}
void MessageBuffer::WriteByte(unsigned char data) {
	buf.push_back(data);
}
void MessageBuffer::WriteInt32(int data) {
	unsigned long data_net = htonl(data);
	unsigned char *ptr = (unsigned char *)&data_net;
	WriteData(ptr, 4);
}
void MessageBuffer::WriteSingle(float data) {
	unsigned long data_net = htonl(*(unsigned long *)&data);
	unsigned char *ptr = (unsigned char *)&data_net;
	WriteData(ptr, 4);
}
void MessageBuffer::WriteInt64(long long data) {
	unsigned long high = data >> 32;
	unsigned long low = (data & 0xFFFFFFFFl);
	WriteInt32(high);
	WriteInt32(low);
}
void MessageBuffer::WriteString(const std::string &msg) {
	WriteInt32(msg.length());
	WriteData((const unsigned char *)msg.data(), msg.length());
}

void MessageBuffer::WriteVector(const Vector &vec) {
    WriteSingle(vec.x);
    WriteSingle(vec.y);
    WriteSingle(vec.z);
}

void MessageBuffer::WriteIntVector(const IntVector &vec) {
    WriteInt32(vec.x);
    WriteInt32(vec.y);
    WriteInt32(vec.z);
}

void MessageBuffer::WriteQuaternion(const Quaternion &quaternion) {
    WriteSingle(quaternion.x);
    WriteSingle(quaternion.y);
    WriteSingle(quaternion.z);
    WriteSingle(quaternion.w);
}

bool MessageBuffer::ReadBool() {
	bool rv = (buf[read_offset] == 1) ? true : false;
	read_offset++;
	return rv;
}
unsigned char MessageBuffer::ReadByte() {
	unsigned char rv = buf[read_offset];
	read_offset++;
	return rv;
}
float MessageBuffer::ReadSingle() {
	unsigned long data_net;
	ReadData((unsigned char *)&data_net, 4);
	float tmp = data_net;
	unsigned long data = ntohl(data_net);
	return *(float *)&data;
}
int MessageBuffer::ReadInt32() {
	unsigned long data_net;
	ReadData((unsigned char *)&data_net, 4);
	unsigned long data = ntohl(data_net);
	return data;
}
long long MessageBuffer::ReadInt64() {
	if (buf.size() < read_offset + 8)
		throw MessageException("Invalid read off end of buffer");
	long high = ReadInt32();
	long low = ReadInt32();
	long long rv = high;
	rv = (rv << 32) | low;
	return rv;
}
std::string MessageBuffer::ReadString() {
	int len = ReadInt32();
	unsigned char *str_buf = new unsigned char[len];
	ReadData(str_buf, len);
	std::string rv((char *)str_buf, len);
	delete[] str_buf;
	return rv;
}

void MessageBuffer::ReadVector(Vector &vec) {
    vec.x = ReadSingle();
    vec.y = ReadSingle();
    vec.z = ReadSingle();
}

void MessageBuffer::ReadIntVector(IntVector &vec) {
    vec.x = ReadInt32();
    vec.y = ReadInt32();
    vec.z = ReadInt32();
}

void MessageBuffer::ReadQuaternion(Quaternion &quaternion) {
    quaternion.x = ReadSingle();
    quaternion.y = ReadSingle();
    quaternion.z = ReadSingle();
    quaternion.w = ReadSingle();
}

bool MessageBuffer::PeekBool() {
	int tmp = read_offset;
	bool rv = ReadBool();
	read_offset = tmp;
	return rv;
}
unsigned char MessageBuffer::PeekByte() {
	int tmp = read_offset;
	unsigned char rv = ReadByte();
	read_offset = tmp;
	return rv;
}
int MessageBuffer::PeekInt32() {
	int tmp = read_offset;
	int rv = ReadInt32();
	read_offset = tmp;
	return rv;
}
long long MessageBuffer::PeekInt64() {
	int tmp = read_offset;
	long long rv = ReadInt64();
	read_offset = tmp;
	return rv;
}
std::string MessageBuffer::PeekString() {
	int tmp = read_offset;
	std::string rv = ReadString();
	read_offset = tmp;
	return rv;
}

void MessageBuffer::WriteData(const unsigned char *data, int len) {
	int new_len = buf.size() + len;
	int offset = buf.size();
	buf.insert(buf.begin() + offset, data, data + len);
}
void MessageBuffer::ReadData(unsigned char *data, int len) {
	if (buf.size() < read_offset + len)
		throw MessageException("Invalid read off end of buffer");
	const unsigned char *ptr = &buf.front();
	memcpy(data, ptr + read_offset, len);
	read_offset += len;
}

void BaseMessage::ParseMessageBuffer(MessageBuffer &msg) {
	unsigned char msg_code = msg.ReadByte();
	assert(msg_code == SERVER_MESSAGE);
	std::string topic = msg.ReadString();
	ParseMessage(msg);
}

MessageBuffer *BaseMessage::GetMessageBuffer() const {
	MessageBuffer *buf = new MessageBuffer();
	try {
		buf->WriteByte(SERVER_MESSAGE);
		buf->WriteString(GetTopic());
		WriteMessage(*buf);
	} catch (...) {
		delete buf;
		throw;
	}
	buf->Reset();
	return buf;
}

std::string BaseMessage::GetTopic() const {
	throw MessageException("BaseMessage does not implement GetTopic");
}

long long OidMessage::GetOid() const {
	return oid;
}

void OidMessage::SetOid(long long id) {
	oid = id;
}

void OidMessage::ParseMessage(MessageBuffer &msg) {
	oid = msg.ReadInt64();
}

void OidMessage::WriteMessage(MessageBuffer &msg) const {
	msg.WriteInt64(oid);
}

long long TimestampedOidMessage::GetTimestamp() const {
	return timestamp;
}

void TimestampedOidMessage::ParseMessage(MessageBuffer &msg) {
	OidMessage::ParseMessage(msg);
	timestamp = msg.ReadInt64();
}

void TimestampedOidMessage::WriteMessage(MessageBuffer &msg) const {
	OidMessage::WriteMessage(msg);
	msg.WriteInt32(timestamp);
};

std::string NewObjectMessage::GetMessageType() {
	return "wrldMgr.newObject";
}

std::string NewObjectMessage::GetTopic() const {
	return NewObjectMessage::GetMessageType();
}

std::string NewObjectMessage::GetObjectType() const {
	return object_type;
}

long long NewObjectMessage::GetObjectOid() const {
	return new_object_oid;
}

void NewObjectMessage::ParseMessage(MessageBuffer &msg) {
	OidMessage::ParseMessage(msg);
	new_object_oid = msg.ReadInt64();
	object_type = msg.ReadString();
}

void NewObjectMessage::WriteMessage(MessageBuffer &msg) const {
	OidMessage::WriteMessage(msg);
	msg.WriteInt64(new_object_oid);
	msg.WriteString(object_type);
}


std::string FreeObjectMessage::GetMessageType() {
	return "wrldMgr.freeObject";
}

std::string FreeObjectMessage::GetTopic() const {
	return FreeObjectMessage::GetMessageType();
}

std::string FreeObjectMessage::GetObjectType() const {
	return object_type;
}

long long FreeObjectMessage::GetObjectOid() const {
	return new_object_oid;
}

void FreeObjectMessage::ParseMessage(MessageBuffer &msg) {
	OidMessage::ParseMessage(msg);
	new_object_oid = msg.ReadInt64();
	object_type = msg.ReadString();
}

void FreeObjectMessage::WriteMessage(MessageBuffer &msg) const {
	OidMessage::WriteMessage(msg);
	msg.WriteInt64(new_object_oid);
	msg.WriteString(object_type);
}


std::string DirLocMessage::GetMessageType() {
	return "wrldMsg.updateWnode";
}

std::string DirLocMessage::GetTopic() const {
	return DirLocMessage::GetMessageType();
}

Vector DirLocMessage::GetDir() const {
	return dir;
}

IntVector DirLocMessage::GetLoc() const {
	return loc;
}

void DirLocMessage::ParseMessage(MessageBuffer &msg) {
	TimestampedOidMessage::ParseMessage(msg);
	msg.ReadVector(dir);
	msg.ReadIntVector(loc);
}

void DirLocMessage::WriteMessage(MessageBuffer &msg) const {
	throw MessageException("DirLocMessage does not implement WriteMessage");
}

std::string ResponseMessage::GetMessageType() {
	return "genericResp";
}

std::string ResponseMessage::GetTopic() const {
	return ResponseMessage::GetMessageType();
}

std::string GetPropertyMessage::GetMessageType() {
	return "wrldMsg.getProp";
}

std::string GetPropertyMessage::GetTopic() const {
	return GetPropertyMessage::GetMessageType();
}

std::string GetPropertyMessage::GetPropertyName() const {
	return property_name;
}

void GetPropertyMessage::SetPropertyName(const std::string &name) {
	property_name = name;
}

void GetPropertyMessage::ParseMessage(MessageBuffer &msg) {
	OidMessage::ParseMessage(msg);
	property_name = msg.ReadString();
}

void GetPropertyMessage::WriteMessage(MessageBuffer &msg) const {
	OidMessage::WriteMessage(msg);
	msg.WriteString(property_name);
}

std::string SetPropertyMessage::GetMessageType() {
	return "wrldMsg.setProp";
}

std::string SetPropertyMessage::GetTopic() const {
	return SetPropertyMessage::GetMessageType();
}

std::string SetPropertyMessage::GetPropertyName() {
	return property_name;
}

void SetPropertyMessage::SetPropertyName(const std::string &name) {
	property_name = name;
}

std::string SetPropertyMessage::GetPropertyValue() {
	return property_value;
}

void SetPropertyMessage::SetPropertyValue(const std::string &value) {
	property_value = value;
}

bool SetPropertyMessage::GetRequestResponse() {
	return request_response;
}

void SetPropertyMessage::SetRequestResponse(const bool req) {
	request_response = req;
}

void SetPropertyMessage::ParseMessage(MessageBuffer &msg) {
	OidMessage::ParseMessage(msg);
	request_response = msg.ReadBool();
	property_name = msg.ReadString();
	property_value = msg.ReadString();
}

void SetPropertyMessage::WriteMessage(MessageBuffer &msg) const {
	OidMessage::WriteMessage(msg);
	msg.WriteBool(request_response);
	msg.WriteString(property_name);
	msg.WriteString(property_value);
}

std::string PropertyRespMessage::GetMessageType() {
	return "genericResp";
}

std::string PropertyRespMessage::GetTopic() const {
	return PropertyRespMessage::GetMessageType();
}

std::string PropertyRespMessage::GetPropertyValue() const{
	return property_value;
}

void PropertyRespMessage::ParseMessage(MessageBuffer &msg) {
	property_value = msg.ReadString();
}

void PropertyRespMessage::WriteMessage(MessageBuffer &msg) const {
	throw MessageException("PropertyRespMessage does not implement WriteMessage");
}

std::string ObjInfoReqMessage::GetMessageType() {
	return "wrldMgr.objInfoReq";
}

std::string ObjInfoReqMessage::GetTopic() const {
	return ObjInfoReqMessage::GetMessageType();
}

std::string ObjInfoRespMessage::GetMessageType() {
	return "genericResp";
}

std::string ObjInfoRespMessage::GetTopic() const {
	return ObjInfoRespMessage::GetMessageType();
}

long long ObjInfoRespMessage::GetOid() const {
    return oid;
}

std::string ObjInfoRespMessage::GetName() const{
    return name;
}

IntVector ObjInfoRespMessage::GetLoc() const{
    return loc;
}

Quaternion ObjInfoRespMessage::GetOrientation() const{
    return orientation;
}

Vector ObjInfoRespMessage::GetScale() const{
    return scale;
}

int ObjInfoRespMessage::GetIntegerType() const{
    return type;
}

int ObjInfoRespMessage::GetFollowTerrain() const{
    return followTerrain;
}

void ObjInfoRespMessage::ParseMessage(MessageBuffer &msg) {
	oid = msg.ReadInt64();
    name = msg.ReadString();
    msg.ReadIntVector(loc);
    msg.ReadQuaternion(orientation);
    msg.ReadVector(scale);
    type = msg.ReadInt32();
    followTerrain = msg.ReadInt32();
}

void ObjInfoRespMessage::WriteMessage(MessageBuffer &msg) const {
	throw MessageException("ObjInfoRespMessage does not implement WriteMessage");
	//msg.WriteInt64(oid);
	//msg.WriteString(name);
	//msg.WriteIntVector(loc);
	//msg.WriteQuaternion(orientation);
	//msg.WriteVector(scale);
	//msg.WriteInt32(type);
	//msg.WriteInt32(followTerrain);
}

std::string PropertyMessage::GetMessageType() {
	return "wrldMgr.property";
}

std::string PropertyMessage::GetTopic() const {
	return PropertyMessage::GetMessageType();
}

void PropertyMessage::ParseMessage(MessageBuffer &msg) {
	OidMessage::ParseMessage(msg);
	int prop_count = msg.ReadInt32();
	for (int i = 0; i < prop_count; ++i) {
		std::string property_name = msg.ReadString();
		std::string property_type = msg.ReadString();
		std::string property_value = msg.ReadString();
		properties[property_name] = property_value;
	}
}

void PropertyMessage::WriteMessage(MessageBuffer &msg) const {
	OidMessage::WriteMessage(msg);
	msg.WriteInt32(properties.size());
	std::map<std::string, std::string>::const_iterator iter = properties.begin();
	for (iter = properties.begin(); iter != properties.end(); ++iter) {
		msg.WriteString((*iter).first);
		msg.WriteString("S");
		msg.WriteString((*iter).second);
	}
}

std::string CommandMessage::GetMessageType() {
	return "behav.command";
}

std::string CommandMessage::GetTopic() const {
	return CommandMessage::GetMessageType();
}

void CommandMessage::ParseMessage(MessageBuffer &msg) {
	throw MessageException("CommandMessage does not implement ParseMessage");
}

void CommandMessage::WriteMessage(MessageBuffer &msg) const {
	OidMessage::WriteMessage(msg);
	msg.WriteString(GetCommand());
}

std::string StopCommandMessage::GetCommand() const {
	return "stop";
}

std::string GotoCommandMessage::GetCommand() const {
	return "goto";
}

IntVector GotoCommandMessage::GetDestination() const {
	return destination;
}

void GotoCommandMessage::SetDestination(const IntVector &dest) {
	destination = dest;
}

int GotoCommandMessage::GetSpeed() const {
	return speed;
}

void GotoCommandMessage::SetSpeed(int speed) {
	this->speed = speed;
}

void GotoCommandMessage::WriteMessage(MessageBuffer &msg) const {
	CommandMessage::WriteMessage(msg);
	msg.WriteIntVector(destination);
	msg.WriteInt32(speed);
}

std::string FollowCommandMessage::GetCommand() const {
	return "follow";
}

long long FollowCommandMessage::GetTargetOid() const {
	return target_oid;
}

void FollowCommandMessage::SetTargetOid(long long target) {
	target_oid = target;
}

int FollowCommandMessage::GetSpeed() const {
	return speed;
}

void FollowCommandMessage::SetSpeed(int speed) {
	this->speed = speed;
}

void FollowCommandMessage::WriteMessage(MessageBuffer &msg) const {
	CommandMessage::WriteMessage(msg);
	msg.WriteInt64(target_oid);
	msg.WriteInt32(speed);
}

UpdateWorldNodeReqMessage::UpdateWorldNodeReqMessage() {
	location_set = false;
	direction_set = false;
	orientation_set = false;
}

std::string UpdateWorldNodeReqMessage::GetMessageType() {
	return "wrldMsg.updateWnodeReq";
}

std::string UpdateWorldNodeReqMessage::GetTopic() const {
	return UpdateWorldNodeReqMessage::GetMessageType();
}

IntVector UpdateWorldNodeReqMessage::GetLocation() const {
	if (!location_set)
		throw MessageException("UpdateWorldNodeReqMessage::GetLocation called with uninitialized location");
	return location;
}

void UpdateWorldNodeReqMessage::SetLocation(const IntVector &loc) {
	location_set = true;
	location = loc;
}

Vector UpdateWorldNodeReqMessage::GetDirection() const {
	if (!direction_set)
		throw MessageException("UpdateWorldNodeReqMessage::GetDirection called with uninitialized direction");
	return direction;
}

void UpdateWorldNodeReqMessage::SetDirection(const Vector &dir) {
	direction_set = true;
	direction = dir;
}

Quaternion UpdateWorldNodeReqMessage::GetOrientation() const {
	if (!orientation_set)
		throw MessageException("UpdateWorldNodeReqMessage::GetOrientation called with uninitialized orientation");
	return orientation;
}

void UpdateWorldNodeReqMessage::SetOrientation(const Quaternion &orient) {
	orientation_set = true;
	orientation = orient;
}

void UpdateWorldNodeReqMessage::ParseMessage(MessageBuffer &msg) {
	OidMessage::ParseMessage(msg);
	location_set = msg.ReadBool();
	if (location_set)
		msg.ReadIntVector(location);
	direction_set = msg.ReadBool();
	if (direction_set)
		msg.ReadVector(direction);
	orientation_set = msg.ReadBool();
	if (orientation_set)
		msg.ReadQuaternion(orientation);
}

void UpdateWorldNodeReqMessage::WriteMessage(MessageBuffer &msg) const {
	OidMessage::WriteMessage(msg);
	msg.WriteBool(location_set);
	if (location_set)
		msg.WriteIntVector(location);
	msg.WriteBool(direction_set);
	if (direction_set)
		msg.WriteVector(direction);
	msg.WriteBool(orientation_set);
	if (orientation_set)
		msg.WriteQuaternion(orientation);
}

std::string EventMessage::GetMessageType() {
	return "behav.event";
}

std::string EventMessage::GetTopic() const {
	return EventMessage::GetMessageType();
}

std::string EventMessage::GetEventType() const {
	std::map<std::string, std::string>::const_iterator iter = properties.find("event");
	if (iter != properties.end())
		return (*iter).second;
	throw MessageException("Invalid EventMessage (no event property)");
}

void EventMessage::ParseMessage(MessageBuffer &msg) {
	OidMessage::ParseMessage(msg);
	int prop_count = msg.ReadInt32();
	for (int i = 0; i < prop_count; ++i) {
		std::string property_name = msg.ReadString();
		std::string property_value = msg.ReadString();
		properties[property_name] = property_value;
	}
}

void EventMessage::WriteMessage(MessageBuffer &msg) const {
	OidMessage::WriteMessage(msg);
	msg.WriteInt32(properties.size());
	std::map<std::string, std::string>::const_iterator iter = properties.begin();
	for (iter = properties.begin(); iter != properties.end(); ++iter) {
		msg.WriteString((*iter).first);
		msg.WriteString((*iter).second);
	}
}

std::string AutoAttackMessage::GetMessageType() {
	return "combat.autoAttack";
}

std::string AutoAttackMessage::GetTopic() const {
	return AutoAttackMessage::GetMessageType();
}

long long AutoAttackMessage::GetTargetOid() const {
	return target_oid;
}

void AutoAttackMessage::SetTargetOid(long long target) {
	target_oid = target;
}

bool AutoAttackMessage::GetAttackStatus() const {
	return attack_status;
}

void AutoAttackMessage::SetAttackStatus(bool status) {
	attack_status = status;
}

void AutoAttackMessage::ParseMessage(MessageBuffer &msg) {
	throw MessageException("AutoAttackMessage does not implement ParseMessage");
}

void AutoAttackMessage::WriteMessage(MessageBuffer &msg) const {
	OidMessage::WriteMessage(msg);
	msg.WriteInt64(target_oid);
	msg.WriteBool(attack_status);
}

std::string ComReqMessage::GetMessageType() {
	return "wrldMsg.comReq";
}

std::string ComReqMessage::GetTopic() const {
	return ComReqMessage::GetMessageType();
}

int ComReqMessage::GetChannel() const {
	return channel_id;
}

void ComReqMessage::SetChannel(int channel) {
	channel_id = channel;
}

std::string ComReqMessage::GetText() const {
	return text;
}

void ComReqMessage::SetText(const std::string &msg) {
	text = msg;
}

void ComReqMessage::ParseMessage(MessageBuffer &msg) {
	throw MessageException("ComReqMessage does not implement ParseMessage");
}

void ComReqMessage::WriteMessage(MessageBuffer &msg) const {
	OidMessage::WriteMessage(msg);
	msg.WriteInt32(channel_id);
	msg.WriteString(text);
}

std::string ComMessage::GetMessageType() {
	return "wrldMsg.com";
}

std::string ComMessage::GetTopic() const {
	return ComMessage::GetMessageType();
}

int ComMessage::GetChannel() const {
	return channel_id;
}

void ComMessage::SetChannel(int channel) {
	channel_id = channel;
}

std::string ComMessage::GetText() const {
	return text;
}

void ComMessage::SetText(const std::string &msg) {
	text = msg;
}

void ComMessage::ParseMessage(MessageBuffer &msg) {
	OidMessage::ParseMessage(msg);
	channel_id = msg.ReadInt32();
	text = msg.ReadString();
}

void ComMessage::WriteMessage(MessageBuffer &msg) const {
	throw MessageException("ComMessage does not implement WriteMessage");
}

AnimationCommandReqMessage::AnimationCommandReqMessage() {
	is_clear = false;
	is_looping = false;
}

std::string AnimationCommandReqMessage::GetMessageType() {
	return "anim.CMD_REQ";
}

std::string AnimationCommandReqMessage::GetTopic() const {
	return AnimationCommandReqMessage::GetMessageType();
}

bool AnimationCommandReqMessage::GetIsClear() const {
	return is_clear;
}

std::string AnimationCommandReqMessage::GetAnimationName() const {
	return anim_name;
}

void AnimationCommandReqMessage::SetAnimationName(const std::string &anim) {
	anim_name = anim;
}

void AnimationCommandReqMessage::SetIsLooping(bool looping) {
	is_looping = looping;
}

bool AnimationCommandReqMessage::GetIsLooping() const {
	return is_looping;
}

void AnimationCommandReqMessage::ParseMessage(MessageBuffer &msg) {
	OidMessage::ParseMessage(msg);
	is_clear = msg.ReadBool();
	anim_name = msg.ReadString();
	is_looping = msg.ReadBool();
}

void AnimationCommandReqMessage::WriteMessage(MessageBuffer &msg) const {
	OidMessage::WriteMessage(msg);
	msg.WriteBool(is_clear);
	msg.WriteString(anim_name);
	msg.WriteBool(is_looping);
}

std::string SpawnedMessage::GetMessageType() {
	return "wrldMgr.spawned";
}

std::string SpawnedMessage::GetTopic() const {
	return SpawnedMessage::GetMessageType();
}

std::string DespawnedMessage::GetMessageType() {
	return "wrldMgr.despawned";
}

std::string DespawnedMessage::GetTopic() const {
	return DespawnedMessage::GetMessageType();
}
