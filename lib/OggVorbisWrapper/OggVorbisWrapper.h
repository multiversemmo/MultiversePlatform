// OggVorbisWrapper.h

#pragma once

using namespace System;
public value struct OggVorbisSoundData {
public: 
	int format;
	int size;
	int frequency;
	int error_code;
};
public ref class OggVorbisWrapper {
public:
	// Read the ogg data from the ogg file, and write the PCM data to the pcmStream
	static OggVorbisSoundData loadOGGFile(String ^fileName, System::IO::Stream ^pcmStream);
	// Read the ogg data from the oggStream, and write the PCM data to the pcmStream
	static OggVorbisSoundData loadOGGMemory(System::IO::Stream ^oggStream, System::IO::Stream ^pcmStream);
};


class OggVorbisHelper {
private:
	OggVorbis_File oggFile;

private:
	OggVorbisSoundData decodeOggData(System::IO::Stream ^pcmStream);
public:
	OggVorbisSoundData loadOggFile(String ^fileName, System::IO::Stream ^pcmStream);
	OggVorbisSoundData loadOggMemory(System::IO::Stream ^oggStream, System::IO::Stream ^pcmStream);
};