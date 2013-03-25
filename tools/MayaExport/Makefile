# Makefile for Maya-OGRE under Linux
# **make sure the environment variable MAYA_LOCATION is set**

CXXFLAGS	=-g -O2 -I${MAYA_LOCATION}/include -Imaya2ogre/include -Ishared/include -D_BOOL
LDFLAGS		=-lOpenMayaAnim
LD		=$(MAYA_LOCATION)/bin/mayald

MO_OBJS=maya2ogre/src/OgreMayaScene.o maya2ogre/src/maya2ogre.o
SHARED_OBJS=shared/src/OgreMayaMaterial.o\
	shared/src/OgreMayaOptions.o\
	shared/src/OgreMayaMesh.o\
	shared/src/OgreMayaSkeleton.o

MAYA2OGRE_OBJS=$(MO_OBJS) $(SHARED_OBJS)

m2o: $(MAYA2OGRE_OBJS)
	$(LD)   -o $@ $^  $(LDFLAGS)

clean:
	rm $(MO_OBJS) $(SHARED_OBJS)
