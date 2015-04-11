
#include <windows.h>
#include <stdio.h>

typedef void(__cdecl *TioMsgCb)(const char *msg);
typedef void(__cdecl *TioPackFuncs)(TioMsgCb errorCb, TioMsgCb msgCb);
typedef void(__cdecl *TioUnpack)(int argc, char *argv[]);

static void printError(const char *msg) {
	printf("[ERROR] %s", msg);
}

static void printInfo(const char *msg) {
	printf("[INFO] %s", msg);
}

int main(int argc, char* argv[])
{
	auto lib = LoadLibrary("tio.dll");

	auto setFuncs = (TioPackFuncs)GetProcAddress(lib, "tio_pack_funcs");
	auto tioUnpack = (TioUnpack)GetProcAddress(lib, "tio_unpack");

	setFuncs(printError, printInfo);

	tioUnpack(argc, argv);

	FreeLibrary(lib);

	return 0;
}

