#pragma once

#include "MessageException.h"

class __declspec(dllexport) Vector {
public:
	Vector() { x = 0; y = 0; z = 0; }
	float x;
	float y;
	float z;
};

class __declspec(dllexport) IntVector {
public:
	IntVector() { x = 0; y = 0; z = 0; }
	int x;
	int y;
	int z;
};


class __declspec(dllexport) Quaternion {
public:
	Quaternion() { x = 0; y = 0; z = 0; w = 1; }
    static Quaternion FromAngleAxis(float angle, const Vector &axis);
    static Quaternion RotateAroundYAxis(float angle);
    float x;
	float y;
	float z;
	float w;
};

// This class is the container for the data of the messages.
// The various message classes read and write to one of these
// buffer objects, which is then used to read or write to the
// network connection.
class __declspec(dllexport) MessageBuffer {
private:
	std::vector<unsigned char> buf;
	int read_offset;
	int mark_offset;

public:
	MessageBuffer();
	
	int GetLength() const;
	const unsigned char *GetData() const;
	void SetData(const unsigned char *d, int l);

	void Mark();
	void Reset();
	
	void WriteBool(bool data);
	void WriteByte(unsigned char data);
	void WriteInt32(int data);
	void WriteSingle(float data);
	void WriteInt64(long long data);
	void WriteString(const std::string &msg);
    void WriteVector(const Vector &vec);
    void WriteIntVector(const IntVector &vec);
    void WriteQuaternion(const Quaternion &quaternion);

	bool ReadBool();
	unsigned char ReadByte();
	int ReadInt32();
	float ReadSingle();
	long long ReadInt64();
	std::string ReadString();
	void ReadString(std::string &msg);
    void ReadVector(Vector &vec);
    void ReadIntVector(IntVector &vec);
    void ReadQuaternion(Quaternion &quaternion);

	bool PeekBool();
	unsigned char PeekByte();
	int PeekInt32();
	long long PeekInt64();
	std::string PeekString();

private:
	void WriteData(const unsigned char *data, int len);
	void ReadData(unsigned char *data, int len);
};

// Base class from which all of the standard messages are derived
class __declspec(dllexport) BaseMessage {
protected:
	// Parse the message buffer
	virtual void ParseMessage(MessageBuffer &msg) = 0;
	// Write our data to the message buffer
	virtual void WriteMessage(MessageBuffer &msg) const = 0;

public:
	virtual std::string GetTopic() const;

	// Populate our data fields based on the contents of the message buffer
	void ParseMessageBuffer(MessageBuffer &msg);
	// Generate a message buffer for sending this message
	// The caller is responsible for freeing the MessageBuffer
	MessageBuffer *GetMessageBuffer() const;
};

class __declspec(dllexport) OidMessage : public BaseMessage {
private:
	long long oid;
public:
	//OidMessage();
	//OidMessage(const OidMessage &other);

	long long GetOid() const;
	void SetOid(long long id);

	virtual void ParseMessage(MessageBuffer &msg);
	virtual void WriteMessage(MessageBuffer &msg) const;
};

class __declspec(dllexport) TimestampedOidMessage : public OidMessage {
private:
	long long timestamp;
public:
	//TimestampedOidMessage();
	//TimestampedOidMessage(const TimestampedOidMessage &other);

	long long GetTimestamp() const;

	virtual void ParseMessage(MessageBuffer &msg);
	virtual void WriteMessage(MessageBuffer &msg) const;
};

// Represents a new object notification (for any perceiver)
class __declspec(dllexport) NewObjectMessage : public OidMessage {
private:
	long long new_object_oid;
	std::string object_type;
public:
	//NewObjectMessage();
	//NewObjectMessage(const NewObjectMessage &other);

	// What message type is this message
	static std::string GetMessageType();
	virtual std::string GetTopic() const;

	std::string GetObjectType() const;
	long long GetObjectOid() const;

	virtual void ParseMessage(MessageBuffer &msg);
	virtual void WriteMessage(MessageBuffer &msg) const;
};

// Represents a free object notification (for any perceiver)
class __declspec(dllexport) FreeObjectMessage : public OidMessage {
	long long new_object_oid;
	std::string object_type;
public:
	//FreeObjectMessage();
	//FreeObjectMessage(const FreeObjectMessage &other);

	// What message type is this message
	static std::string GetMessageType();
	virtual std::string GetTopic() const;

	std::string GetObjectType() const;
	long long GetObjectOid() const;

	virtual void ParseMessage(MessageBuffer &msg);
	virtual void WriteMessage(MessageBuffer &msg) const;
};

class __declspec(dllexport) DirLocMessage : public TimestampedOidMessage {
	Vector dir;
	IntVector loc;
public:
	//DirLocMessage();
	//DirLocMessage(const DirLocMessage &other);

	// What message type is this message
	static std::string GetMessageType();
	virtual std::string GetTopic() const;

	Vector GetDir() const;
	void SetDir(const Vector &dir);
	IntVector GetLoc() const;
	void SetLoc(const IntVector &loc);

	virtual void ParseMessage(MessageBuffer &msg);
	virtual void WriteMessage(MessageBuffer &msg) const;
};

// The various response messages should extend this abstract class
class __declspec(dllexport) ResponseMessage : public BaseMessage {
public:
	//ResponseMessage();
	//ResponseMessage(const ResponseMessage &other);

	// What message type is this message
	static std::string GetMessageType();
	virtual std::string GetTopic() const;

	virtual void ParseMessage(MessageBuffer &msg) = 0;
	virtual void WriteMessage(MessageBuffer &msg) const = 0;
};

class __declspec(dllexport) GetPropertyMessage : public OidMessage {
	std::string property_name;
public:
	//GetPropertyMessage();
	//GetPropertyMessage(const GetPropertyMessage &other);

	// What message type is this message
	static std::string GetMessageType();
	virtual std::string GetTopic() const;

	std::string GetPropertyName() const;
	void SetPropertyName(const std::string &name);

	virtual void ParseMessage(MessageBuffer &msg);
	virtual void WriteMessage(MessageBuffer &msg) const;
};

class __declspec(dllexport) SetPropertyMessage : public OidMessage {
	std::string property_name;
	std::string property_value;
	bool request_response;
public:
	// What message type is this message
	static std::string GetMessageType();
	virtual std::string GetTopic() const;

	std::string GetPropertyName();
	void SetPropertyName(const std::string &name);

	std::string GetPropertyValue();
	void SetPropertyValue(const std::string &value);

	bool GetRequestResponse();
	void SetRequestResponse(const bool req);

	virtual void ParseMessage(MessageBuffer &msg);
	virtual void WriteMessage(MessageBuffer &msg) const;
};

// This class does not exist on the server, since the server just uses the 
// data object to contain the string that is the property value.  Since
// we don't have java serialization, we need to actually have a class for this.
class __declspec(dllexport) PropertyRespMessage : public ResponseMessage {
	std::string property_value;
public:
	static std::string GetMessageType();
	virtual std::string GetTopic() const;

	std::string GetPropertyValue() const;

	virtual void ParseMessage(MessageBuffer &msg);
	virtual void WriteMessage(MessageBuffer &msg) const;
};

class __declspec(dllexport) ObjInfoReqMessage : public OidMessage {
public:
	// What message type is this message
	static std::string GetMessageType();
	virtual std::string GetTopic() const;
};

class __declspec(dllexport) ObjInfoRespMessage : public ResponseMessage {
    long long oid;
    std::string name;
    IntVector loc;
    Quaternion orientation;
    Vector scale;
    int type;
    int followTerrain;

public:
	//ObjectInfoMessage();
	//ObjectInfoMessage(const ObjectInfoMessage &other);

	// What message type is this message
	static std::string GetMessageType();
	virtual std::string GetTopic() const;

    long long GetOid() const;
    std::string GetName() const;
    IntVector GetLoc() const;
    Quaternion GetOrientation() const;
    Vector GetScale() const;
    int GetIntegerType() const;
    int GetFollowTerrain() const;

	virtual void ParseMessage(MessageBuffer &msg);
	virtual void WriteMessage(MessageBuffer &msg) const;
};

class __declspec(dllexport) PropertyMessage : public OidMessage {
public:
	std::map<std::string, std::string> properties;

	// What message type is this message
	static std::string GetMessageType();
	virtual std::string GetTopic() const;

	virtual void ParseMessage(MessageBuffer &msg);
	virtual void WriteMessage(MessageBuffer &msg) const;
};

class __declspec(dllexport) CommandMessage : public OidMessage {
protected:
	virtual std::string GetCommand() const = 0;

public:
	std::map<std::string, std::string> properties;

	// What message type is this message
	static std::string GetMessageType();
	virtual std::string GetTopic() const;

	virtual void ParseMessage(MessageBuffer &msg);
	virtual void WriteMessage(MessageBuffer &msg) const;
};

class __declspec(dllexport) StopCommandMessage : public CommandMessage {
protected:
	virtual std::string GetCommand() const;
};

class __declspec(dllexport) GotoCommandMessage : public CommandMessage {
	IntVector destination;
	int speed;

protected:
	virtual std::string GetCommand() const;

public:
	IntVector GetDestination() const;
	void SetDestination(const IntVector &dest);
	int GetSpeed() const;
	void SetSpeed(int speed);

	virtual void WriteMessage(MessageBuffer &msg) const;
};

class __declspec(dllexport) FollowCommandMessage : public CommandMessage {
	long long target_oid;
	int speed;

protected:
	virtual std::string GetCommand() const;

public:
	int GetSpeed() const;
	void SetSpeed(int speed);
	long long GetTargetOid() const;
	void SetTargetOid(long long target_oid);

	virtual void WriteMessage(MessageBuffer &msg) const;
};

class __declspec(dllexport) UpdateWorldNodeReqMessage : public OidMessage {
	bool location_set;
	IntVector location;
	bool direction_set;
	Vector direction;
	bool orientation_set;
	Quaternion orientation;

public:
	UpdateWorldNodeReqMessage();

	// What message type is this message
	static std::string GetMessageType();
	virtual std::string GetTopic() const;

	IntVector GetLocation() const;
	void SetLocation(const IntVector &dir);
	Vector GetDirection() const;	
	void SetDirection(const Vector &dir);
	Quaternion GetOrientation() const;
	void SetOrientation(const Quaternion &orient);

	virtual void ParseMessage(MessageBuffer &msg);
	virtual void WriteMessage(MessageBuffer &msg) const;
};

class __declspec(dllexport) EventMessage : public OidMessage {
public:
	std::map<std::string, std::string> properties;

	// What message type is this message
	static std::string GetMessageType();
	virtual std::string GetTopic() const;

	std::string GetEventType() const;

	virtual void ParseMessage(MessageBuffer &msg);
	virtual void WriteMessage(MessageBuffer &msg) const;
};

class __declspec(dllexport) AutoAttackMessage : public OidMessage {
	long long target_oid;
	bool attack_status;

public:
	// What message type is this message
	static std::string GetMessageType();
	virtual std::string GetTopic() const;

	long long GetTargetOid() const;
	void SetTargetOid(long long target);
	bool GetAttackStatus() const;
	void SetAttackStatus(bool status);

	virtual void ParseMessage(MessageBuffer &msg);
	virtual void WriteMessage(MessageBuffer &msg) const;
};

class __declspec(dllexport) ComReqMessage : public OidMessage {
	int channel_id;
	std::string text;

public:
	// What message type is this message
	static std::string GetMessageType();
	virtual std::string GetTopic() const;

	int GetChannel() const;
	void SetChannel(int target);
	std::string GetText() const;
	void SetText(const std::string &msg);

	virtual void ParseMessage(MessageBuffer &msg);
	virtual void WriteMessage(MessageBuffer &msg) const;
};

class __declspec(dllexport) ComMessage : public OidMessage {
	int channel_id;
	std::string text;

public:
	// What message type is this message
	static std::string GetMessageType();
	virtual std::string GetTopic() const;

	int GetChannel() const;
	void SetChannel(int target);
	std::string GetText() const;
	void SetText(const std::string &msg);

	virtual void ParseMessage(MessageBuffer &msg);
	virtual void WriteMessage(MessageBuffer &msg) const;
};

class __declspec(dllexport) AnimationCommandReqMessage : public OidMessage {
	bool is_clear;
	std::string anim_name;
	bool is_looping;

public:
	AnimationCommandReqMessage();

	// What message type is this message
	static std::string GetMessageType();
	virtual std::string GetTopic() const;

	bool GetIsClear() const;
	void SetIsClear(bool clear);
	std::string GetAnimationName() const;
	void SetAnimationName(const std::string &anim);
	bool GetIsLooping() const;
	void SetIsLooping(bool looping);

	virtual void ParseMessage(MessageBuffer &msg);
	virtual void WriteMessage(MessageBuffer &msg) const;
};

class __declspec(dllexport) SpawnedMessage : public OidMessage {
public:
	// What message type is this message
	static std::string GetMessageType();
	virtual std::string GetTopic() const;
};

class __declspec(dllexport) DespawnedMessage : public OidMessage {
public:
	// What message type is this message
	static std::string GetMessageType();
	virtual std::string GetTopic() const;
};
// not needed: class to generate objects

