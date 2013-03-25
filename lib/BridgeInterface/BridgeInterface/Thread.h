#pragma once

class __declspec(dllexport) Thread {
private:
	HANDLE thread_handle;
	unsigned int thread_id;
	
public:
	Thread();
	virtual ~Thread();

	void Start();
	void Join();
	bool Join(int milliseconds);
	void Suspend();
	void Resume();
//	void Interrupt();
	static void Sleep(int milliseconds);
	void Dispose();

	virtual void Run() = 0;
};

class __declspec(dllexport) CriticalSection {
	CRITICAL_SECTION critical_section;

public:
	CriticalSection();
	virtual ~CriticalSection();

	void Lock();
	void Unlock();
};

class __declspec(dllexport) Mutex {
	friend class ConditionVariable;
	HANDLE mutex_handle;
public:
	Mutex();
	virtual ~Mutex();

	void Lock();
	void Unlock();
};

class __declspec(dllexport) Semaphore {
	friend class ConditionVariable;
	HANDLE semaphore_handle;
public:
	Semaphore();
	~Semaphore();

	int Release(int count);
};

class __declspec(dllexport) Event {
	friend class ConditionVariable;
	HANDLE event_handle;
public:
	Event();
	~Event();

	void Wait();
};

class __declspec(dllexport) ConditionVariable {
    // Number of waiting threads
    int waiters_count;
    // Lock access to waiters_count
    CriticalSection waiters_count_lock;
    // Semaphore used to queue up threads waiting for the condition to
    // become signaled. 
    Semaphore semaphore;
    // An auto-reset event used by the broadcast/signal thread to wait
    // for all the waiting thread(s) to wake up and be released from the
    // semaphore. 
    Event waiters_done;
    // Keeps track of whether we were broadcasting or signaling.  This
    // allows us to optimize the code if we're just signaling.
    bool was_broadcast;

	// The external mutex that should be held before calls to 
	// Wait, Pulse, or PulseAll
	Mutex *mutex;

public:
	ConditionVariable(Mutex *m);

	void Wait();
	void Pulse();
	void PulseAll();
};

class __declspec(dllexport) Monitor {
	Mutex mutex;
	ConditionVariable condition_variable;

public:
	Monitor();

	void Enter();
	void Exit();

	void Wait();
	void Pulse();
	void PulseAll();
};


class __declspec(dllexport) ReaderWriterLock {
	int readers;
	int writers;
	int writers_pending;
	Monitor monitor;

public:
	ReaderWriterLock();
	void AcquireReaderLock();
	void ReleaseReaderLock();
	void AcquireWriterLock();
	void ReleaseWriterLock();
};