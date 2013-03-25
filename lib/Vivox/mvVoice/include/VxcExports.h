#ifndef __VXCEXPORTS_H
#define __VXCEXPORTS_H

#ifdef _MSC_VER
  // Note: this has been disabled so no-one tries to export C++ classes.
  //#ifdef BUILDING_VIVOXSDK
  //  #define VIVOXSDK_DLLEXPORT __declspec(dllexport)
  //#else
  //  #define VIVOXSDK_DLLEXPORT __declspec(dllimport)
  //#endif
  #define VIVOXSDK_DLLEXPORT
#else
  #define VIVOXSDK_DLLEXPORT __attribute__ ((visibility("default")))
#endif


#endif
