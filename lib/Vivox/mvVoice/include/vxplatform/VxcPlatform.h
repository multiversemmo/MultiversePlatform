#ifndef __VXCPLATFORM_H
#define __VXCPLATFORM_H

#ifdef _MSC_VER
  #ifdef BUILDING_VIVOX_PLATFORM
    #define VXPLATFORM_DLLEXPORT __declspec(dllexport)
  #else
    #define VXPLATFORM_DLLEXPORT __declspec(dllimport)
  #endif
#else
  #define VXPLATFORM_DLLEXPORT __attribute__ ((visibility("default")))
#endif

namespace vxplatform {
    typedef unsigned long os_error_t;

    typedef void * os_thread_handle;
    typedef void * os_event_handle;
    typedef os_error_t (*thread_start_function_t)(void *);

    VXPLATFORM_DLLEXPORT os_error_t create_thread(thread_start_function_t pf, void * pArg, os_thread_handle *pHandle);
    VXPLATFORM_DLLEXPORT os_error_t delete_thread(os_thread_handle handle);
    VXPLATFORM_DLLEXPORT os_error_t close_thread_handle(os_thread_handle handle);

    VXPLATFORM_DLLEXPORT os_error_t create_event(os_event_handle *pHandle);
    VXPLATFORM_DLLEXPORT os_error_t set_event(os_event_handle handle);
    VXPLATFORM_DLLEXPORT os_error_t wait_event(os_event_handle handle, int timeout=-1);
    VXPLATFORM_DLLEXPORT os_error_t delete_event(os_event_handle handle);

	class VXPLATFORM_DLLEXPORT Lock
	{
		void *m_pImpl;
	public:
		Lock(void);
		~Lock(void);

		void Take();
		void Release();
    private:
        Lock(const Lock &);
	};
}



#endif
