// This is the main DLL file.

#include "stdafx.h"

#include <vector>
#include <iostream>

#include "stdlib.h"

#include "vorbis/vorbisfile.h"
#include "OggVorbisWrapper.h"

using namespace System::IO;
using namespace System::Runtime::InteropServices;

struct OggMemoryFile {
	unsigned char *ptr;
	unsigned char *base;
	int cnt;
};

static size_t read_func(void *ptr, size_t size, size_t nmemb, void *datasource) {
	OggMemoryFile *f = (OggMemoryFile *)datasource;
	int bytes = size * nmemb;
	if (f->ptr > f->base + f->cnt) {
		std::cerr << "Attempt to read outside of range" << std::endl;
		return -1;
	} else if (f->ptr + bytes > f->base + f->cnt) {
		nmemb = (f->base + f->cnt - f->ptr) / size;
		bytes = size * nmemb;
	}
	memcpy(ptr, f->ptr, bytes);
	f->ptr += bytes;
	return nmemb;
}

static int seek_func(void *datasource, ogg_int64_t offset, int whence) {
	OggMemoryFile *f = (OggMemoryFile *)datasource;
	unsigned char *p;
	switch (whence) {
		case SEEK_CUR:
			p = f->ptr + offset;
			break;
		case SEEK_END:
			p = f->base + f->cnt + offset;
			break;
		case SEEK_SET:
			p = f->base + offset;
			break;
		default:
			std::cerr << "Invalid whence flag: " << whence << std::endl;
			return -1;
	}
	if (p > f->base + f->cnt || p < f->base) {
		std::cerr << "Attempt to seek outside of range" << std::endl;
		return -1;
	}
	f->ptr = p;
	return 0;
}
static int close_func(void *datasource) {
	OggMemoryFile *f = (OggMemoryFile *)datasource;
	delete[] f->base;
	f->ptr = NULL;
	f->base = NULL;
	f->cnt = 0;
	return 0;
}
static long tell_func(void *datasource) {
	OggMemoryFile *f = (OggMemoryFile *)datasource;
	return f->ptr - f->base;
}

// Read a managed stream, and fill a native buffer
static void ReadStream(OggMemoryFile *f, Stream ^stream) {
	int bytesLeft = (int)stream->Length;
	f->base = new unsigned char[bytesLeft];
	f->cnt = bytesLeft;
	f->ptr = f->base;
	array<Byte> ^byteArray = gcnew array<Byte>(4096);
	while (bytesLeft > 0) {
		int bytesToRead = bytesLeft > 4096 ? 4096 : bytesLeft;
        int bytesRead = stream->Read(byteArray, 0, bytesToRead);
        bytesLeft -= bytesRead;
		// convert native pointer to System::IntPtr with C-Style cast
		Marshal::Copy(byteArray, 0, (IntPtr)f->ptr, bytesRead);
		f->ptr += bytesRead;
    }
	f->ptr = f->base;
}

OggVorbisSoundData OggVorbisWrapper::loadOGGFile(String ^fileName, Stream ^pcmStream) {
	OggVorbisHelper oggHelper;
	return oggHelper.loadOggFile(fileName, pcmStream);
}

OggVorbisSoundData OggVorbisWrapper::loadOGGMemory(System::IO::Stream ^oggStream, System::IO::Stream ^pcmStream) {
	OggVorbisHelper oggHelper;
	return oggHelper.loadOggMemory(oggStream, pcmStream);
}

OggVorbisSoundData OggVorbisHelper::loadOggFile(String ^fileName, Stream ^pcmStream) {
	char *fname = "test.ogg";
	FILE *f = fopen(fname, "rb");
	ov_open(f, &oggFile, NULL, 0);
	OggVorbisSoundData rv = decodeOggData(pcmStream);
	// Note that there is no need to call fclose() anymore once this is done.
	ov_clear(&oggFile);
	return rv;
}

OggVorbisSoundData OggVorbisHelper::loadOggMemory(Stream ^oggStream, Stream ^pcmStream) {
	
	OggMemoryFile memFile;
	ReadStream(&memFile, oggStream);
	ov_callbacks cb_struct;
	cb_struct.read_func = read_func;
	cb_struct.close_func = close_func;
	cb_struct.seek_func = seek_func;
	cb_struct.tell_func = tell_func;
	int rc = ov_open_callbacks(&memFile, &oggFile, NULL, 0, cb_struct);
	if (rc < 0)
		std::cerr << "Failure in ov_open_callbacks: " << rc << std::endl;
	OggVorbisSoundData rv = decodeOggData(pcmStream);
	ov_clear(&oggFile);
	return rv;
}

OggVorbisSoundData OggVorbisHelper::decodeOggData(Stream ^pcmStream) {
	const int AL_FORMAT_MONO16 = 4353;
	const int AL_FORMAT_STEREO16 = 4355;
	vorbis_info *pInfo;
	OggVorbisSoundData rv;
	char buf[4096];
	int endian = 0; // 0 for Little-Endian, 1 for Big-Endian
	int bitStream;
	
	// Get some information about the OGG file
	pInfo = ov_info(&oggFile, -1);

	// Check the number of channels... always use 16-bit samples
	if (pInfo->channels == 1)
		rv.format = AL_FORMAT_MONO16;
	else
		rv.format = AL_FORMAT_STEREO16;
	// end if

	// The frequency of the sampling rate
	rv.frequency = pInfo->rate;

	std::vector<char> buffer;
	int bytes = 0;
	for (;;) {
		// Read up to a buffer's worth of decoded sound data
		bytes = ov_read(&oggFile, buf, sizeof(buf), endian, 2, 1, &bitStream);
		if (bytes <= 0) {
			std::cerr << "Unexpected byte count: " << bytes << std::endl;
			break;
		}
		array<Byte>^ byteArray = gcnew array<Byte>(bytes);
		// convert native pointer to System::IntPtr with C-Style cast
		Marshal::Copy((IntPtr)buf, byteArray, 0, bytes);
		// Append to end of buffer
		pcmStream->Write(byteArray, 0, bytes);
	}

	rv.size = pcmStream->Length;
	return rv;
}
