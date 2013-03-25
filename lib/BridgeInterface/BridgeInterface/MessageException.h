#pragma once

class __declspec(dllexport) MessageException {
private:
	std::string msg;
public:
	MessageException(const char *message) {
		msg = message;
	}
	MessageException(const MessageException &other) {
		if (&other != this)
			msg = other.msg;
	}
	const std::string Message() const {
		return msg;
	}
};