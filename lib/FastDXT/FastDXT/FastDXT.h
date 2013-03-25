// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the FASTDXT_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// FASTDXT_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef FASTDXT_EXPORTS
#define FASTDXT_API __declspec(dllexport)
#else
#define FASTDXT_API __declspec(dllimport)
#endif

extern void initialize(void);

extern FASTDXT_API BOOL HasMMX;
extern FASTDXT_API BOOL HasSSE2;

extern "C" FASTDXT_API BOOL GetHasMMX(void);
extern "C" FASTDXT_API BOOL GetHasSSE2(void);

typedef unsigned char byte;
typedef unsigned short word;
typedef unsigned int dword;

extern "C" FASTDXT_API void CompressImageDXT1( const byte *inBuf, byte *outBuf, int width, int height );