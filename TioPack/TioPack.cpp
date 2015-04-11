// TioPack.cpp : Defines the entry point for the console application.
//

#include <windows.h>
#include <stdio.h>

typedef void(__cdecl *TioMsgCb)(const char *msg);
typedef void(__cdecl *TioPackFuncs)(TioMsgCb errorCb, TioMsgCb msgCb);
typedef void(__cdecl *TioPack)(int argc, char *argv[]);

static void printError(const char *msg) {
	printf("[ERROR] %s\n", msg);
}

static void printInfo(const char *msg) {
	printf("[INFO] %s\n", msg);
}

int main(int argc, char* argv[])
{
	auto lib = LoadLibrary("tio.dll");

	auto setFuncs = (TioPackFuncs) GetProcAddress(lib, "tio_pack_funcs");
	auto tioPack = (TioPack) GetProcAddress(lib, "tio_pack");

	setFuncs(printError, printInfo);

	tioPack(argc, argv);

	FreeLibrary(lib);

	return 0;
}

