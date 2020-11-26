
#define _CRT_SECURE_NO_WARNINGS
#define _WINSOCK_DEPRECATED_NO_WARNINGS
#pragma comment(lib, "ws2_32.lib")
#include <stdio.h>
#include <winsock2.h>
#include <WS2tcpip.h>

int analyze_file(SOCKET, char*);
int search_file(SOCKET, char*, int*);
void ip2string(unsigned int, char*);
