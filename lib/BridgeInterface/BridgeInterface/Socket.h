#pragma once

#include <winsock.h>

class __declspec(dllexport) Socket {
private:
	static int wsa_error;
public:
	static int Startup();
	static void Cleanup();

private:
	SOCKET sd;
protected:
	unsigned int ResolveHostname(const char *hostname);
public:
	Socket();
	~Socket();
	int Dispose();
	int Connect(const char *hostname, unsigned short port);
	int Write(const unsigned char *data, int len);
	int Write(const unsigned char *data, int offset, int len);
	int Read(unsigned char *buf, int len);
	int Read(unsigned char *buf, int offset, int len);
	int Close();
};
