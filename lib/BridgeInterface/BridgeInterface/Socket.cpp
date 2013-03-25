#include "stdafx.h"
#include "Socket.h"

int Socket::wsa_error = 0;

__declspec(dllexport)
int Socket::Startup() {
	WORD winsock_version = MAKEWORD(2, 2);
	WSAData wsa_data;
	wsa_error = WSAStartup(winsock_version, &wsa_data);
	if (wsa_error != 0) {
		/* Tell the user that we could not find a usable */
		/* WinSock DLL.                                  */
		return -1;
	}
 
	/* Confirm that the WinSock DLL supports 2.2.        */
	/* Note that if the DLL supports versions later      */
	/* than 2.2 in addition to 2.2, it will still return */
	/* 2.2 in wVersion since that is the version we      */
	/* requested.                                        */
 	if (LOBYTE(wsa_data.wVersion) != 2 ||
		HIBYTE(wsa_data.wVersion) != 2) {
		/* Tell the user that we could not find a usable */
		/* WinSock DLL.                                  */
		WSACleanup();
		return -1;
	}
	return 0;
}

void Socket::Cleanup() {
	WSACleanup();
}

Socket::Socket() {
	sd = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
}

Socket::~Socket() {
	Dispose();
}

int Socket::Dispose() {
	int rv = 0;
	if (sd != NULL) {
		if (closesocket(sd) < 0)
			rv = -1;
		sd = NULL;
	}
	return rv;
}
int Socket::Connect(const char *hostname, unsigned short port) {
	unsigned long host_addr = ResolveHostname(hostname);
	if (host_addr == INADDR_NONE)
		return -1;
	
	struct sockaddr_in server_addr;
	/* Construct the server address structure */
    memset(&server_addr, 0, sizeof(server_addr)); /* Zero out structure */
    server_addr.sin_family      = AF_INET;               /* Internet address family */
    server_addr.sin_addr.s_addr = host_addr;             /* Server IP address */
    server_addr.sin_port        = htons(port);           /* Server port */
	
	if (connect(sd, (struct sockaddr *)&server_addr, sizeof(server_addr)) < 0) {
		return -1;
	}

	return 0;
}

int Socket::Write(const unsigned char *data, int offset, int len) {
	return send(sd, (const char *)data + offset, len, 0);
}
int Socket::Write(const unsigned char *data, int len) {
	return Write(data, 0, len);
}
int Socket::Read(unsigned char *buf, int offset, int len) {
	return recv(sd, (char *)buf + offset, len, 0);
}
int Socket::Read(unsigned char *buf, int len) {
	return Read(buf, 0, len);
}

int Socket::Close() {
	return closesocket(sd);
}

unsigned int Socket::ResolveHostname(const char *hostname) {
	struct hostent *host;            /* Structure containing host information */
	unsigned long host_addr = inet_addr(hostname);
	if (host_addr != INADDR_NONE)
		return host_addr;
	if ((host = gethostbyname(hostname)) == NULL)
        return INADDR_NONE;
	host_addr = *((unsigned long *)host->h_addr_list[0]);
	return host_addr;
}