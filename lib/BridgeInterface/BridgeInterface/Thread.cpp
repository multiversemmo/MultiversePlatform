#include "stdafx.h"
#include "Thread.h"
#include "MessageException.h"

static unsigned WINAPI ThreadProc(void *arg)
{
    Thread *thread = (Thread *)arg;
    thread->Run();

	_endthreadex(0);
    return 0;
}


Thread::Thread() : thread_handle(NULL), thread_id(0) {
}

Thread::~Thread() {
	Dispose();
}

void Thread::Start() {
	//_beginthread;
	// thread_handle = NULL;
	thread_handle = (HANDLE)_beginthreadex(NULL, 0, ThreadProc, (void *)this, 0, &thread_id);
}

void Thread::Dispose() {
	if (thread_handle != NULL) {
		CloseHandle(thread_handle);
		thread_handle = NULL;
	}
}

void Thread::Suspend() {
	if (SuspendThread(thread_handle) < 0)
		throw MessageException("Failed to suspend thread");
}

void Thread::Resume() {
	if (ResumeThread(thread_handle) < 0)
		throw MessageException("Failed to resume thread");
}

void Thread::Sleep(int milliseconds) {
	::Sleep(milliseconds);
}

void Thread::Join() {
	unsigned long rv = WaitForSingleObject(thread_handle, INFINITE);
	if (rv == WAIT_OBJECT_0)
		return;
	throw new MessageException("Failed to join thread");
}

bool Thread::Join(int timeout) {
	unsigned long rv = WaitForSingleObject(thread_handle, timeout);
	if (rv == WAIT_OBJECT_0)
		return true;	
	else if (rv == WAIT_TIMEOUT)
		return false;
	throw new MessageException("Failed to join thread");
}

CriticalSection::CriticalSection() {
	InitializeCriticalSection(&critical_section);
}

CriticalSection::~CriticalSection() {
	DeleteCriticalSection(&critical_section);
}

void CriticalSection::Lock() {
	EnterCriticalSection(&critical_section);
}

void CriticalSection::Unlock() {
	LeaveCriticalSection(&critical_section);
}

Mutex::Mutex() {
	mutex_handle = CreateMutex(NULL, false, NULL);
}

Mutex::~Mutex() {
	CloseHandle(mutex_handle);
}

void Mutex::Lock() {
	WaitForSingleObject(mutex_handle, INFINITE);
}

void Mutex::Unlock() {
	ReleaseMutex(mutex_handle);
}

Semaphore::Semaphore() {
	semaphore_handle = CreateSemaphore(NULL, 0, 0x7FFFFFFF, NULL);
}

Semaphore::~Semaphore() {
	CloseHandle(semaphore_handle);
}

int Semaphore::Release(int count) {
	long rv;
	ReleaseSemaphore(semaphore_handle, count, &rv);
	return rv;
}

Event::Event() {
	event_handle = CreateEvent(NULL, FALSE, FALSE, NULL);
}

Event::~Event() {
	CloseHandle(event_handle);
}

void Event::Wait() {
	WaitForSingleObject(event_handle, INFINITE);
}

ConditionVariable::ConditionVariable(Mutex *m) : mutex(m) {
	waiters_count = 0;
    was_broadcast = 0;
}

void ConditionVariable::Wait() {
	// Lock access to waiters_count
	waiters_count_lock.Lock();
	waiters_count++;
    waiters_count_lock.Unlock();

	// This call atomically releases the mutex and waits on the
	// semaphore until Pulse or PulseAll is called by another thread.
	SignalObjectAndWait(mutex->mutex_handle, semaphore.semaphore_handle, INFINITE, FALSE);

	// Lock access to waiters_count
	waiters_count_lock.Lock();
	waiters_count--;
	// Check to see if we're the last waiter after a broadcast
	int last_waiter = was_broadcast && waiters_count == 0;
	waiters_count_lock.Unlock();

	// If we're the last waiter thread during this particular broadcast
	// then let all the other threads proceed.
	if (last_waiter)
		// This call atomically signals the <waiters_done> event and waits until
		// it can acquire the <external_mutex>.  This is required to ensure fairness. 
		SignalObjectAndWait(waiters_done.event_handle, mutex->mutex_handle, INFINITE, FALSE);
	else
		// Always regain the external mutex since that's the guarantee we
		// give to our callers. 
		mutex->Lock();
}

void ConditionVariable::Pulse() {
	// Lock access to waiters_count and was_broadcast
	waiters_count_lock.Lock();
	bool have_waiters = waiters_count > 0;
	waiters_count_lock.Unlock();

	// If there aren't any waiters, then this is a no-op.  
	if (have_waiters)
		semaphore.Release(1);
}

void ConditionVariable::PulseAll() {
	// Lock access to waiters_count and was_broadcast
	waiters_count_lock.Lock();
	bool have_waiters = waiters_count > 0;

	if (have_waiters) {
		was_broadcast = true;
		// Wake up all the waiters atomically.
		semaphore.Release(waiters_count);
		waiters_count_lock.Unlock();
		// Wait for all the awakened threads to acquire the semaphore
		waiters_done.Wait();
		// clear our was_broadcast flag
		was_broadcast = false;
	} else
		waiters_count_lock.Unlock();
}

Monitor::Monitor() : condition_variable(&mutex) {
}

void Monitor::Enter() {
	mutex.Lock();
}

void Monitor::Exit() {
	mutex.Unlock();
}

void Monitor::Pulse() {
	condition_variable.Pulse();
}

void Monitor::PulseAll() {
	condition_variable.PulseAll();
}

void Monitor::Wait() {
	condition_variable.Wait();
}

ReaderWriterLock::ReaderWriterLock() : readers(0), writers(0), writers_pending(0) {
}

void ReaderWriterLock::AcquireReaderLock() {
	monitor.Enter();
	for (;;) {
		if (writers == 0 && writers_pending == 0) {
			readers++;
			monitor.Exit();
			return;
		} else {
			monitor.Wait();
		}
	}
}

void ReaderWriterLock::ReleaseReaderLock() {
	monitor.Enter();
	readers--;
	if (readers == 0)
		monitor.PulseAll();
	monitor.Exit();
}

void ReaderWriterLock::AcquireWriterLock() {
	monitor.Enter();
	for (;;) {
		if (readers == 0) {
			writers++;
			monitor.Exit();
			return;
		} else {
			writers_pending++;
			monitor.Wait();
			writers_pending--;
		}
	}
}

void ReaderWriterLock::ReleaseWriterLock() {
	monitor.Enter();
	writers--;
	if (writers == 0)
		monitor.PulseAll();
	monitor.Exit();
}